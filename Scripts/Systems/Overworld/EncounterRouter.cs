using Godot;
using System;

/// <summary>
/// Manages transitions between the overworld and combat encounters.
/// Uses full scene swap: saves overworld state, switches to combat scene,
/// then reloads overworld with restored state when combat ends.
/// 
/// This avoids all camera/viewport conflicts from additive loading.
/// </summary>
public partial class EncounterRouter : Node
{
    [Export] public string CombatScenePath = "res://Scenes/Combat/Battlefield.tscn";
    [Export] public string OverworldScenePath = "res://Scenes/Overworld/OverworldScene.tscn";

    public static EncounterRouter Instance { get; private set; }

    public bool HasPendingReturn { get; set; } = false;
    public bool CombatWon { get; set; }
    public int GoldReward { get; set; }
    public int DamageTaken { get; set; }

    public int SavedStepsRemaining;
    public int SavedCurrentHP;
    public int SavedGoldEarned;
    public int SavedEncountersWon;
    public Vector2I SavedPartyCoord;
    public Vector2I SavedCombatHexCoord;

    // ── Seed for deterministic map regeneration after combat ────────────
    public int SavedRunSeed;
    public bool HasSavedSeed = false;

    public System.Collections.Generic.Dictionary<Vector2I, OverworldHex.FogState> SavedFogStates = new();
    public System.Collections.Generic.Dictionary<Vector2I, bool> SavedPOIConsumed = new();

    public override void _Ready()
    {
        Instance = this;
        // This node persists across scene changes
        ProcessMode = ProcessModeEnum.Always;
    }

    /// <summary>
    /// Called by RunManager to start a combat encounter.
    /// Saves overworld state, then swaps to the combat scene.
    /// </summary>
    public void StartCombat(RunManager runManager, Vector2I combatHexCoord)
    {
        // ── Save overworld state ────────────────────────────────────────
        SavedStepsRemaining = runManager.StepsRemaining;
        SavedCurrentHP = runManager.CurrentHP;
        SavedGoldEarned = runManager.GoldEarned;
        SavedEncountersWon = runManager.EncountersWon;
        SavedPartyCoord = runManager.GetPartyCoord();
        SavedCombatHexCoord = combatHexCoord;

        // Save fog and POI state for every hex
        SavedFogStates.Clear();
        SavedPOIConsumed.Clear();
        var grid = runManager.GetGrid();
        foreach (var kvp in grid.Hexes)
        {
            SavedFogStates[kvp.Key] = kvp.Value.Fog;
            SavedPOIConsumed[kvp.Key] = kvp.Value.POIConsumed;
        }

        HasPendingReturn = false;

        GD.Print($"EncounterRouter: Saved overworld state. Party at {SavedPartyCoord}, " +
                 $"Steps: {SavedStepsRemaining}, HP: {SavedCurrentHP}");

        // ── Swap to combat scene ────────────────────────────────────────
        GetTree().ChangeSceneToFile(CombatScenePath);
    }

    /// <summary>
    /// Called by GameRunner (via signal) when combat ends.
    /// Stores the result and swaps back to the overworld.
    /// </summary>
    public void OnCombatFinished(bool playerWon)
    {
        CombatWon = playerWon;
        GoldReward = playerWon ? 30 + (int)(GD.Randf() * 50) : 0;
        DamageTaken = playerWon ? (int)(GD.Randf() * 20) : 30;
        HasPendingReturn = true;

        GD.Print($"EncounterRouter: Combat finished. Won: {playerWon}. " +
                 $"Returning to overworld in 2s...");

        // Brief delay so the player sees Victory/Defeat
        GetTree().CreateTimer(2.0f).Timeout += () =>
        {
            GetTree().ChangeSceneToFile(OverworldScenePath);
        };
    }
}