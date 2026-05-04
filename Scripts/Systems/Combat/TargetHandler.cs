using Godot;
using System;
using System.Collections.Generic;

// ============================================================
// Target Selectors — PHASE 2 UPDATE
// For player card-drops, OnCardDroppedOnTile bypasses these via
// TryCastWithTargets — but these need to work for everything else.
// ============================================================

public sealed class SelectUnitTarget : ITargetSelector
{
	public bool enemyOnly;
	public int range;
	public bool los;
	public bool friendlyOnly;

	public SelectUnitTarget(bool enemyOnly = true, int range = 4, bool los = true, bool friendlyOnly = false)
	{
		this.enemyOnly = enemyOnly;
		this.range = range;
		this.los = los;
		this.friendlyOnly = friendlyOnly;
	}

	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();

		if (s?.Grid == null || s.UnitsInPlay == null) return false;

		// Find the caster's unit to measure range from
		Unit casterUnit = null;
		if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
		else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;
		else
		{
			foreach (var u in s.UnitsInPlay)
				if (u != null && u.Name == caster?.Name) { casterUnit = u; break; }
		}

		if (casterUnit?.CurrentTile == null) return false;

		var center = casterUnit.CurrentTile.Axial;

		// Find the best target: closest valid unit
		Unit bestTarget = null;
		int bestDist = int.MaxValue;

		foreach (var unit in s.UnitsInPlay)
		{
			if (unit == null || !unit.Stats.IsAlive || unit.CurrentTile == null)
				continue;

			// Skip self
			if (unit == casterUnit) continue;

			// Filter by team
			if (enemyOnly && unit.TeamId == casterUnit.TeamId) continue;
			if (friendlyOnly && unit.TeamId != casterUnit.TeamId) continue;

			int dist = s.Grid.Distance(center, unit.CurrentTile.Axial);

			// Range check
			if (dist > range) continue;

			// Pick closest valid target
			if (dist < bestDist)
			{
				bestDist = dist;
				bestTarget = unit;
			}
		}

		if (bestTarget != null)
		{
			targets.Items.Add(bestTarget);
			return true;
		}

		return false;
	}
}

public sealed class SelectTileTarget : ITargetSelector
{
	public int range;

	public SelectTileTarget(int r = 4) { range = r; }

	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();

		if (s?.Grid == null) return false;

		// Find caster position
		Unit casterUnit = null;
		if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
		else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;

		if (casterUnit?.CurrentTile == null) return false;

		// Default: select the caster's own tile
		// (For player-initiated casts, this gets overridden by
		// TryCastWithTargets with the actual dropped-on tile)
		targets.Items.Add(casterUnit.CurrentTile);
		return true;
	}
}

public sealed class SelectAreaTarget : ITargetSelector
{
	public int Radius;
	public bool EnemiesOnly;
	public bool Tiles;

	public SelectAreaTarget(int r, bool enemiesOnly, bool tiles)
	{
		Radius = r;
		EnemiesOnly = enemiesOnly;
		Tiles = tiles;
	}

	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();

		if (s?.Grid == null) return false;

		// Find the center from caster's position
		Unit casterUnit = null;
		if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
		else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;
		else
		{
			foreach (var u in s.UnitsInPlay)
				if (u != null && u.Name == caster?.Name) { casterUnit = u; break; }
		}

		if (casterUnit?.CurrentTile == null) return false;

		var center = casterUnit.CurrentTile.Axial;

		foreach (var unit in s.UnitsInPlay)
		{
			if (unit == null || !unit.Stats.IsAlive || unit.CurrentTile == null)
				continue;

			// Skip allies if enemies_only
			if (EnemiesOnly && unit.TeamId == casterUnit.TeamId)
				continue;

			int dist = s.Grid.Distance(center, unit.CurrentTile.Axial);
			if (dist <= Radius)
				targets.Items.Add(unit);
		}

		return true;
	}
}

public sealed class SelectSelfTarget : ITargetSelector
{
	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();
		targets.Items.Add(caster);
		return true;
	}
}

public sealed class SelectGlobalTarget : ITargetSelector
{
	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();
		return true;
	}
}

public sealed class SelectByTagTarget : ITargetSelector
{
	public string tag;
	public bool enemyOnly;

	public SelectByTagTarget(string tag, bool enemyOnly = false)
	{
		this.tag = tag;
		this.enemyOnly = enemyOnly;
	}

	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();
		targets.Items.Add(tag);
		return true;
	}
}

public sealed class SelectNearestToTarget : ITargetSelector
{
	public int Range;

	public SelectNearestToTarget(int range = 3)
	{
		Range = range;
	}

	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();
		if (s?.Grid == null || s.UnitsInPlay == null) return false;

		// Find caster unit for team check
		Unit casterUnit = null;
		if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
		else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;
		else
		{
			foreach (var u in s.UnitsInPlay)
				if (u != null && u.Name == caster?.Name) { casterUnit = u; break; }
		}
		if (casterUnit == null) return false;

		// Build the "already hit" exclusion set from RetargetOrigin
		var excluded = new HashSet<Unit>();
		excluded.Add(casterUnit); // never chain to self

		Vector2I origin;

		if (s.RetargetOrigin != null && s.RetargetOrigin.Items.Count > 0)
		{
			// Use the previous target as the origin for "nearest"
			Unit prevTarget = ResolveUnit(s, s.RetargetOrigin.Items[0]);
			if (prevTarget?.CurrentTile != null)
			{
				origin = prevTarget.CurrentTile.Axial;
				excluded.Add(prevTarget); // don't bounce back
			}
			else
			{
				origin = casterUnit.CurrentTile?.Axial ?? default;
			}

			// Exclude ALL units in the previous target set
			foreach (var obj in s.RetargetOrigin.Items)
			{
				var u = ResolveUnit(s, obj);
				if (u != null) excluded.Add(u);
			}
		}
		else
		{
			// Fallback: nearest to caster
			if (casterUnit.CurrentTile == null) return false;
			origin = casterUnit.CurrentTile.Axial;
		}

		// Find nearest non-excluded enemy within range of origin
		Unit nearest = null;
		int nearestDist = int.MaxValue;

		foreach (var unit in s.UnitsInPlay)
		{
			if (unit == null || !unit.Stats.IsAlive || unit.CurrentTile == null)
				continue;
			if (unit.TeamId == casterUnit.TeamId) continue;
			if (excluded.Contains(unit)) continue;

			int dist = s.Grid.Distance(origin, unit.CurrentTile.Axial);
			if (dist > Range) continue;
			if (dist < nearestDist)
			{
				nearestDist = dist;
				nearest = unit;
			}
		}

		if (nearest != null)
		{
			targets.Items.Add(nearest);
			return true;
		}

		return false;
	}

	private static Unit ResolveUnit(GameState s, object obj)
	{
		if (obj is Unit u) return u;
		if (obj is TileData td) return td.Occupant;
		if (obj is HexTile tv) return s?.Grid?.GetTile(tv.Axial)?.Occupant;
		return null;
	}
}

public sealed class SelectAdjacentToTarget : ITargetSelector
{
    public bool IncludeTiles;

    public SelectAdjacentToTarget(bool includeTiles = true)
    {
        IncludeTiles = includeTiles;
    }

    public bool Select(GameState s, Entity caster, out TargetSet targets)
    {
        targets = new TargetSet();
        if (s?.Grid == null || s.RetargetOrigin == null) return false;

        // Find the center from the previous target
        Vector2I? center = null;
        foreach (var obj in s.RetargetOrigin.Items)
        {
            if (obj is Unit u && u.CurrentTile != null) { center = u.CurrentTile.Axial; break; }
            if (obj is TileData td) { center = td.Axial; break; }
            if (obj is HexTile tv) { center = tv.Axial; break; }
        }

        if (center == null) return false;

        foreach (var neighbor in s.Grid.GetNeighbors(center.Value))
        {
            var tile = s.Grid.GetTile(neighbor);
            if (tile == null) continue;

            if (IncludeTiles)
                targets.Items.Add(tile);
            else if (tile.Occupant != null && tile.Occupant.Stats.IsAlive)
                targets.Items.Add(tile.Occupant);
        }

        return targets.Items.Count > 0;
    }
}

public sealed class SelectConeTarget : ITargetSelector
{
	public int Range;
	public bool EnemiesOnly;

	public SelectConeTarget(int range = 3, bool enemiesOnly = true)
	{
		Range = range;
		EnemiesOnly = enemiesOnly;
	}

	// The 6 hex directions (axial, flat-top)
	private static readonly Vector2I[] HexDirs =
	{
		new Vector2I(1, 0),
		new Vector2I(1, -1),
		new Vector2I(0, -1),
		new Vector2I(-1, 0),
		new Vector2I(-1, 1),
		new Vector2I(0, 1)
	};

	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();
		if (s?.Grid == null || s.UnitsInPlay == null) return false;

		// Find caster unit
		Unit casterUnit = null;
		if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
		else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;
		else
		{
			foreach (var u in s.UnitsInPlay)
				if (u != null && u.Name == caster?.Name) { casterUnit = u; break; }
		}
		if (casterUnit?.CurrentTile == null) return false;

		var origin = casterUnit.CurrentTile.Axial;

		// Determine cone direction: toward the clicked target
		Vector2I aimTarget = origin;

		// Check RetargetOrigin first (set by RetargetEffect)
		if (s.RetargetOrigin != null && s.RetargetOrigin.Items.Count > 0)
		{
			var aimCoord = ResolveCoord(s, s.RetargetOrigin.Items[0]);
			if (aimCoord.HasValue) aimTarget = aimCoord.Value;
		}

		// If aim target is self, find nearest enemy as fallback
		if (aimTarget == origin)
		{
			int bestDist = int.MaxValue;
			foreach (var unit in s.UnitsInPlay)
			{
				if (unit == null || !unit.Stats.IsAlive || unit.CurrentTile == null) continue;
				if (unit.TeamId == casterUnit.TeamId) continue;
				int dist = s.Grid.Distance(origin, unit.CurrentTile.Axial);
				if (dist < bestDist)
				{
					bestDist = dist;
					aimTarget = unit.CurrentTile.Axial;
				}
			}
		}

		if (aimTarget == origin) return false; // nobody to aim at

		// Find the best hex direction toward the aim target
		int bestDirIdx = GetBestDirection(s, origin, aimTarget);

		// Build the cone: collect all hex coords in the cone shape
		var coneTiles = BuildConeCoords(origin, bestDirIdx, Range);

		// Find all units on those tiles
		foreach (var unit in s.UnitsInPlay)
		{
			if (unit == null || !unit.Stats.IsAlive || unit.CurrentTile == null)
				continue;
			if (unit == casterUnit) continue;
			if (EnemiesOnly && unit.TeamId == casterUnit.TeamId) continue;

			if (coneTiles.Contains(unit.CurrentTile.Axial))
				targets.Items.Add(unit);
		}

		s.Log($"[ConeTarget] Direction {bestDirIdx}, range {Range}: found {targets.Items.Count} target(s).");
		return true;
	}

	
	private static Vector2I? ResolveCoord(GameState s, object obj)
	{
		if (obj is Unit u && u.CurrentTile != null) return u.CurrentTile.Axial;
		if (obj is TileData td) return td.Axial;
		if (obj is HexTile tv) return tv.Axial;
		return null;
	}

	/// <summary>
	/// Find which of the 6 hex directions best points toward the target.
	/// </summary>
	private int GetBestDirection(GameState s, Vector2I from, Vector2I to)
	{
		int bestIdx = 0;
		int bestDist = int.MaxValue;

		for (int i = 0; i < 6; i++)
		{
			// Step one tile in this direction and see how close it gets to target
			var stepped = from + HexDirs[i];
			int dist = s.Grid.Distance(stepped, to);
			if (dist < bestDist)
			{
				bestDist = dist;
				bestIdx = i;
			}
		}

		return bestIdx;
	}

	/// <summary>
	/// Build the set of hex coordinates that form the cone.
	/// 
	/// The cone has a "forward" direction and expands at each step:
	///   Step 1 (distance 1 from origin): just the forward hex
	///   Step 2 (distance 2): forward + one hex on each side
	///   Step 3 (distance 3): forward + two hexes on each side
	///
	/// The "side" hexes are found by taking the two directions
	/// adjacent to the forward direction and stepping along them.
	/// </summary>
	private HashSet<Vector2I> BuildConeCoords(Vector2I origin, int dirIdx, int range)
	{
		var coords = new HashSet<Vector2I>();

		// The forward direction and its two neighbors (left and right of cone)
		var forward = HexDirs[dirIdx];
		var leftDir = HexDirs[(dirIdx + 5) % 6];  // one step counterclockwise
		var rightDir = HexDirs[(dirIdx + 1) % 6];  // one step clockwise

		for (int step = 1; step <= range; step++)
		{
			// The center tile of this row: step tiles forward
			var center = origin + forward * step;

			// The spread at this distance: (step - 1) tiles to each side
			int spread = step - 1;

			for (int side = -spread; side <= spread; side++)
			{
				Vector2I tile;
				if (side == 0)
				{
					tile = center;
				}
				else if (side < 0)
				{
					// Left side: step along leftDir from center
					tile = center + leftDir * Math.Abs(side);
				}
				else
				{
					// Right side: step along rightDir from center
					tile = center + rightDir * side;
				}

				coords.Add(tile);
			}
		}

		return coords;
	}
}

public sealed class SelectRingTarget : ITargetSelector
{
    public int Radius;
    public bool IncludeTiles;

    public SelectRingTarget(int radius = 2, bool includeTiles = true)
    {
        Radius = radius;
        IncludeTiles = includeTiles;
    }

    public bool Select(GameState s, Entity caster, out TargetSet targets)
    {
        targets = new TargetSet();
        if (s?.Grid == null) return false;

        // Center on the previous target (the tile that was clicked)
        Vector2I? center = null;

        if (s.RetargetOrigin != null && s.RetargetOrigin.Items.Count > 0)
        {
            foreach (var obj in s.RetargetOrigin.Items)
            {
                if (obj is TileData td) { center = td.Axial; break; }
                if (obj is HexTile tv) { center = tv.Axial; break; }
                if (obj is Unit u && u.CurrentTile != null) { center = u.CurrentTile.Axial; break; }
            }
        }

        // Fallback to caster position
        if (center == null)
        {
            Unit casterUnit = null;
            if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
            else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;
            if (casterUnit?.CurrentTile != null)
                center = casterUnit.CurrentTile.Axial;
        }

        if (center == null) return false;

        // Get tiles at exactly Radius distance (the ring)
        foreach (var kvp in s.Grid.Tiles)
        {
            if (s.Grid.Distance(center.Value, kvp.Key) != Radius) continue;

            if (IncludeTiles)
                targets.Items.Add(kvp.Value);
            else if (kvp.Value.Occupant != null && kvp.Value.Occupant.Stats.IsAlive)
                targets.Items.Add(kvp.Value.Occupant);
        }

        return targets.Items.Count > 0;
    }
}

public sealed class SelectLineTarget : ITargetSelector
{
	public int Length;
	public bool EnemiesOnly;
	public bool IncludeTiles; // if true, returns TileData; if false, returns Units

	public SelectLineTarget(int length = 2, bool enemiesOnly = true, bool includeTiles = false)
	{
		Length = length;
		EnemiesOnly = enemiesOnly;
		IncludeTiles = includeTiles;
	}

	private static readonly Vector2I[] HexDirs =
	{
		new Vector2I(1, 0),  new Vector2I(1, -1), new Vector2I(0, -1),
		new Vector2I(-1, 0), new Vector2I(-1, 1), new Vector2I(0, 1)
	};

	public bool Select(GameState s, Entity caster, out TargetSet targets)
	{
		targets = new TargetSet();
		if (s?.Grid == null) return false;

		Unit casterUnit = null;
		if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
		else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;
		else
		{
			foreach (var u in s.UnitsInPlay)
				if (u != null && u.Name == caster?.Name) { casterUnit = u; break; }
		}
		if (casterUnit?.CurrentTile == null) return false;

		var origin = casterUnit.CurrentTile.Axial;

		// Determine direction from RetargetOrigin or nearest enemy
		Vector2I aimTarget = origin;
		if (s.RetargetOrigin != null && s.RetargetOrigin.Items.Count > 0)
		{
			if (s.RetargetOrigin.Items[0] is Unit u && u.CurrentTile != null)
				aimTarget = u.CurrentTile.Axial;
			else if (s.RetargetOrigin.Items[0] is TileData td)
				aimTarget = td.Axial;
			else if (s.RetargetOrigin.Items[0] is HexTile tv)
				aimTarget = tv.Axial;
		}

		if (aimTarget == origin)
		{
			int bestDist = int.MaxValue;
			foreach (var unit in s.UnitsInPlay)
			{
				if (unit == null || !unit.Stats.IsAlive || unit.CurrentTile == null) continue;
				if (unit.TeamId == casterUnit.TeamId) continue;
				int dist = s.Grid.Distance(origin, unit.CurrentTile.Axial);
				if (dist < bestDist) { bestDist = dist; aimTarget = unit.CurrentTile.Axial; }
			}
		}

		if (aimTarget == origin) return false;

		// Find best hex direction
		int bestIdx = 0;
		int bestD = int.MaxValue;
		for (int i = 0; i < 6; i++)
		{
			int dist = s.Grid.Distance(origin + HexDirs[i], aimTarget);
			if (dist < bestD) { bestD = dist; bestIdx = i; }
		}

		// Walk the line and collect targets
		for (int step = 1; step <= Length; step++)
		{
			var coord = origin + HexDirs[bestIdx] * step;
			var tile = s.Grid.GetTile(coord);
			if (tile == null) continue;

			if (IncludeTiles)
			{
				targets.Items.Add(tile);
			}

			if (tile.Occupant != null && tile.Occupant.Stats.IsAlive)
			{
				if (EnemiesOnly && tile.Occupant.TeamId == casterUnit.TeamId)
					continue;
				if (!IncludeTiles) // avoid duplicates
					targets.Items.Add(tile.Occupant);
			}
		}

		return true;
	}
}

public sealed class SelectElementTileTarget : ITargetSelector
{
    public string Element;
    public int Range;

    public SelectElementTileTarget(string element, int range = 6)
    {
        Element = element;
        Range = range;
    }

    public bool Select(GameState s, Entity caster, out TargetSet targets)
    {
        targets = new TargetSet();
        if (s?.Grid == null) return false;

        Unit casterUnit = null;
        if (caster == s.PlayerA) casterUnit = s.PlayerUnit;
        else if (caster == s.PlayerB) casterUnit = s.EnemyUnit;
        if (casterUnit?.CurrentTile == null) return false;

        var center = casterUnit.CurrentTile.Axial;

        TileElementType needed = Element.ToLowerInvariant() switch
        {
            "fire"  => TileElementType.Fire,
            "ice"   => TileElementType.Frost,
            "storm" => TileElementType.Lightning,
            "stone" => TileElementType.Earth,
            _ => TileElementType.None
        };

        // Find nearest fire tile in range as auto-select fallback
        // (player card drop overrides this with actual tile)
        TileData best = null;
        int bestDist = int.MaxValue;

        foreach (var kvp in s.Grid.Tiles)
        {
            var tile = kvp.Value;
            if (tile?.ElementType != needed) continue;
            int dist = s.Grid.Distance(center, kvp.Key);
            if (dist > Range) continue;
            if (dist < bestDist) { bestDist = dist; best = tile; }
        }

        if (best != null)
        {
            targets.Items.Add(best);
            return true;
        }

        return false;
    }
}