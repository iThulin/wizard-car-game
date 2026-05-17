/// <summary>
/// Action point costs for all martial unit actions.
/// Single source of truth — both combat logic and UI read from here.
///
/// AP resets each turn. Martials spend AP to move, attack, switch stance, use items.
/// Wizards do not use AP — mana is their resource.
/// </summary>
public static class MartialAPCosts
{
    /// <summary>Cost to move one tile on normal terrain.</summary>
    public const int MoveNormal = 1;

    /// <summary>Cost to move one tile on difficult terrain (rubble, lava, etc.).</summary>
    public const int MoveDifficult = 2;

    /// <summary>Cost to make a melee attack (range 1).</summary>
    public const int AttackMelee = 2;

    /// <summary>Cost to make a ranged attack (range 2+).</summary>
    public const int AttackRanged = 3;

    /// <summary>Cost to switch stance (once per turn maximum).</summary>
    public const int SwitchStance = 1;

    /// <summary>Cost to use an item from inventory.</summary>
    public const int UseItem = 1;

    /// <summary>
    /// Get the attack AP cost based on effective attack range.
    /// </summary>
    public static int AttackCost(int effectiveRange)
        => effectiveRange > 1 ? AttackRanged : AttackMelee;

    /// <summary>
    /// Get the movement AP cost for a tile based on its MoveCost property.
    /// TileData.MoveCost is set to 2 for rubble, lava, and other difficult terrain
    /// via ApplyTerrainModifier — so we read directly from that rather than
    /// checking TerrainModifier string or a non-existent IsRubble property.
    /// </summary>
    public static int MoveCost(TileData tile)
    {
        if (tile == null) return MoveNormal;
        // MoveCost > 1 means difficult terrain (rubble, lava, scorched, etc.)
        return tile.MoveCost > 1 ? MoveDifficult : MoveNormal;
    }
}
