using Godot;

/// <summary>
/// Data passed from the overworld to the combat scene.
/// EncounterRouter sets Current before the scene transition.
/// CombatManager reads it at spawn time.
/// Follows the same static-carrier pattern as NegotiationContext.
/// </summary>
public class EncounterContext
{
    // ── Overworld source context ─────────────────────────────────────────
    public OverworldHex.POIType SourcePOI;
    public OverworldHex.TerrainType SourceTerrain;

    // ── Legacy fields (kept for compatibility) ───────────────────────────
    public int EnemyCount = 3;
    public int PlayerCount = 2;

    // ── Results (filled after combat completes) ───────────────────────────
    public bool PlayerWon;
    public int GoldReward;
    public int DamageTaken;
}

/// <summary>
/// Static carrier — set by EncounterRouter before scene swap,
/// read by CombatManager at spawn time.
/// </summary>
public static class EncounterContextCarrier
{
    public static EncounterDefinition Current { get; private set; } = null;
    public static bool HasEncounter => Current != null;

    public static void Set(EncounterDefinition def) => Current = def;
    public static void Clear() => Current = null;
}