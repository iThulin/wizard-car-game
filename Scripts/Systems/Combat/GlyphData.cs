using System;

// ============================================================
// GlyphData.cs
//
// Purpose:        Data container for a triggered glyph placed on
//                 a hex tile. Fires its OnTrigger when an enemy
//                 enters the tile; the trigger logic is captured
//                 at placement time so subsequent state changes
//                 don't mutate it.
// Layer:          Data
// Collaborators:  TileData.cs (holds the Glyph ref),
//                 PlaceGlyphEffect (in Effect.cs — creator),
//                 Unit.cs (movement triggers Consumed-check)
// See:            README §5.4 — Place Glyph effect
// ============================================================

/// <summary>One triggered glyph on a hex tile. Owner team gates friendly-fire (allies don't trigger it). <see cref="OnTrigger"/> captures damage/status at placement time so subsequent caster-stat changes don't mutate the glyph's payload.</summary>
public sealed class GlyphData
{
    /// <summary>Display name of the unit that placed this glyph. Used for log attribution.</summary>
    public string OwnerId;

    /// <summary>Team ID of the placing unit. Units of the same team do not trigger.</summary>
    public int OwnerTeam;

    /// <summary>Game state snapshot at placement. Some glyph effects close over this so they can read state at trigger time.</summary>
    public GameState GameState;

    /// <summary>The trigger payload. Invoked when an enemy steps onto this tile.</summary>
    public Action<Unit, GameState> OnTrigger;

    /// <summary>True once triggered. Used to prevent double-firing.</summary>
    public bool Consumed;
}