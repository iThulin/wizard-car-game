using System;
using System.Collections.Generic;

// ============================================================
// Base class for all effects. This is the ONLY EffectBase in the
// project — the scripting system extends it rather than replacing it.
//
// Leaf effects (DealDamage, Move, etc.) inherit from this and
// override Resolve. They may ALSO override ResolveWithResult if
// they want to report data back to downstream conditionals
// (e.g. DealDamageEffect reports WasLethal).
// ============================================================

public abstract class EffectBase : IEffect
{
	protected string[] _tags = Array.Empty<string>();
	public string[] Tags => _tags;

	public IEffect WithTag(string t)
	{
		_tags = new[] { t };
		return this;
	}

	// Default: leaf effect, no children. Composite effects override.
	public virtual IEnumerable<IEffect> Children => Array.Empty<IEffect>();

	// Old entry point — kept for compatibility with your stack code
	// (RulesManager still calls this through the IEffect interface).
	public abstract void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap);

	// New entry point. Default wraps the old Resolve so legacy
	// effects keep working without needing to override.
	// Effects that want to report data (lethal damage, targets hit,
	// spawned entities) should override this.
	public virtual EffectResult ResolveWithResult(PredicateContext ctx)
	{
		Resolve(ctx.Game, ctx.Caster, ctx.Targets, ctx.Snapshot);
		return new EffectResult();
	}
}

// ============================================================
// Leaf effects
// ============================================================

public sealed class DealDamageEffect : EffectBase
{
	public int Amount;
	public DealDamageEffect(int a) { Amount = a; }

	public override void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap)
	{
		int hit = 0;

		s.Log($"[DealDamageEffect] resolving for Amount={Amount}");
		if (targets == null)
		{
			s.Log("targets == null");
			return;
		}

		s.Log($"targets.Items.Count={targets.Items.Count}");

		foreach (var obj in targets.Items)
		{
			s.Log($"  item: {(obj == null ? "null" : obj.GetType().Name)}");

			if (obj is Unit u)
				s.Log($"    -> Unit: {u.Name} HP {u.Stats.Health}/{u.Stats.MaxHealth}");

			if (obj is TileData td)
				s.Log($"    -> TileData: {td.Axial} occupant={(td.Occupant != null ? td.Occupant.Name : "null")}");

			if (obj is HexTile tile)
				s.Log($"    -> TileView: {tile.Axial}");
		}

		foreach (var obj in targets.Items)
		{
			if (obj is Unit u)
			{
				u.ApplyDamage(Amount);
				s.Log($"HIT unit {u.Name}");
				hit++;
			}
			else if (obj is TileData td && td.Occupant != null)
			{
				td.Occupant.ApplyDamage(Amount);
				s.Log($"HIT tile occupant {td.Occupant.Name} on {td.Axial}");
				hit++;
			}
			else if (obj is HexTile tileView)
			{
				var tileData = ResolveTileDataFromView(s, tileView);
				if (tileData != null && tileData.Occupant != null)
				{
					tileData.Occupant.ApplyDamage(Amount);
					s.Log($"HIT tile occupant {tileData.Occupant.Name} on {tileData.Axial}");
					hit++;
				}
			}
		}

		s.Log($"Resolve: Deal {Amount} damage to {hit} target(s).");
	}

	// NEW: reports damage + lethality so 'was_lethal' predicate works.
	// This is what makes Bone Shatter's "if lethal summon skeleton" functional.
	public override EffectResult ResolveWithResult(PredicateContext ctx)
	{
		int totalDamage = 0;
		bool lethal = false;
		int hit = 0;

		if (ctx.Targets == null) return new EffectResult();

		foreach (var obj in ctx.Targets.Items)
		{
			Unit victim = null;
			if (obj is Unit u) victim = u;
			else if (obj is TileData td && td.Occupant != null) victim = td.Occupant;
			else if (obj is HexTile tileView)
			{
				var tileData = ResolveTileDataFromView(ctx.Game, tileView);
				if (tileData != null) victim = tileData.Occupant;
			}

			if (victim != null)
			{
				int hpBefore = victim.Stats.Health;
				victim.ApplyDamage(Amount);
				totalDamage += Amount;
				hit++;
				if (hpBefore > 0 && victim.Stats.Health <= 0) lethal = true;
			}
		}

		ctx.Game?.Log($"Resolve: Deal {Amount} damage to {hit} target(s). lethal={lethal}");
		return new EffectResult { DamageDealt = totalDamage, WasLethal = lethal, TargetsHit = hit };
	}

	private TileData ResolveTileDataFromView(GameState s, HexTile tileView)
	{
		if (tileView == null) return null;
		var grid = s?.Grid;
		if (grid == null)
		{
			s?.Log("ResolveTileDataFromView: could not find HexGridManager.");
			return null;
		}
		return grid.GetTile(tileView.Axial);
	}
}

public sealed class DashEffect : EffectBase
{
	public int Tiles;
	public DashEffect(int t) { Tiles = t; }
	public override void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap)
	{
		s.Log($"Resolve: Move {Tiles} tile(s).");
	}
}

public sealed class GiveShieldEffect : EffectBase
{
	public int Shield;
	public GiveShieldEffect(int v) { Shield = v; }
	public override void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap)
	{
		s.Log($"Resolve: Gain {Shield} shield.");
	}
}

public sealed class DrawCardsEffect : EffectBase
{
	public int Count;
	public DrawCardsEffect(int n) { Count = n; }
	public override void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap)
	{
		s.Draw(caster, Count);
		s.Log($"Resolve: Draw {Count}.");
	}
}

public sealed class SummonEffect : EffectBase
{
	public string UnitKind;
	public int Count;
	public SummonEffect(string kind, int count) { UnitKind = kind; Count = count; }
	public override void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap)
	{
		s.Log($"Resolve: Summon {Count}x {UnitKind}.");
	}
}

public sealed class NoOpEffect : EffectBase
{
	public string Text;
	public NoOpEffect(string t) { Text = t; }
	public override void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap)
	{
		s.Log($"Resolve: NoOp ({Text}).");
	}
}
