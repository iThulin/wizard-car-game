using Godot;
using System;
using System.Collections.Generic;

// ============================================================
// RetargetEffect — Swaps the TargetSet mid-sequence
//
// Runs a new ITargetSelector to build a fresh TargetSet, then
// executes its child effect against those new targets. After
// the child resolves, restores the original targets so the
// rest of the sequence continues with the original targeting.
//
// JSON usage:
//   {
//     "type": "retarget",
//     "targeting": { "type": "aoe", "radius": 1, "enemies_only": true },
//     "do": { "type": "damage", "amount": 3 }
//   }
//
// Or with a sequence inside:
//   {
//     "type": "retarget",
//     "targeting": { "type": "aoe", "radius": 1, "enemies_only": true },
//     "do": {
//       "type": "sequence",
//       "steps": [
//         { "type": "move", "tiles": 1 },
//         { "type": "damage", "amount": 2 }
//       ]
//     }
//   }
//
// This enables patterns like:
//   step 1: { "type": "move", "tiles": 3 }          ← targets self
//   step 2: { "type": "retarget",                    ← switches to AoE
//             "targeting": { "type": "aoe", "radius": 1, "enemies_only": true },
//             "do": { "type": "move", "tiles": 1 }   ← pushes nearby enemies
//           }
// ============================================================

public sealed class RetargetEffect : EffectBase
{
	public ITargetSelector Targeter;
	public IEffect Child;

	public RetargetEffect(ITargetSelector targeter, IEffect child)
	{
		Targeter = targeter;
		Child = child;
	}

	public override IEnumerable<IEffect> Children
	{
		get { yield return Child; }
	}

	public override void Resolve(GameState s, Entity caster, TargetSet targets, EffectSnapshot snap)
	{
		var ctx = new PredicateContext
		{
			Game = s,
			Caster = caster,
			Targets = targets,
			Snapshot = snap
		};
		ResolveWithResult(ctx);
	}

	public override EffectResult ResolveWithResult(PredicateContext ctx)
	{
		// Save original targets
		var originalTargets = ctx.Targets;

		// Run the new targeter to build a fresh TargetSet
		TargetSet newTargets;
		if (Targeter != null && Targeter.Select(ctx.Game, ctx.Caster, out newTargets))
		{
			ctx.Game?.Log($"[Retarget] Switched to {newTargets.Items.Count} new target(s).");
			ctx.Targets = newTargets;
		}
		else
		{
			ctx.Game?.Log("[Retarget] Targeter found no targets. Skipping child effect.");
			return new EffectResult();
		}

		// Execute child effect with new targets
		EffectResult result;
		if (Child is EffectBase eb)
		{
			result = eb.ResolveWithResult(ctx);
		}
		else
		{
			Child.Resolve(ctx.Game, ctx.Caster, ctx.Targets, ctx.Snapshot);
			result = new EffectResult();
		}

		// Restore original targets for the rest of the sequence
		ctx.Targets = originalTargets;

		return result;
	}
}
