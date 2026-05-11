using System.Collections.Generic;

/// <summary>
/// Schema for a single narrative encounter event.
/// Loaded from Data/Encounters/*.json.
/// </summary>
public class NarrativeEncounterData
{
    // ── Identity ────────────────────────────────────────────────────────
    public string Id = "";
    public string Title = "";
    public string Body = "";

    // ── Context filters ──────────────────────────────────────────────────
    // Empty list = matches any. Non-empty = only matches listed values.
    public List<string> TerrainTags = new();
    public List<string> RegionTags = new();

    // ── Choices ──────────────────────────────────────────────────────────
    public List<EncounterChoice> Choices = new();
}

/// <summary>
/// A single choice option within a narrative encounter.
/// </summary>
public class EncounterChoice
{
    public string Label = "";
    public string ResultText = "";

    // Outcomes (positive or negative)
    public int GoldDelta = 0;
    public int HPDelta = 0;
    public int StepDelta = 0;

    // Phase 3+ tracking
    public List<string> SetFlags = new();

    // Phase 3+ gating
    public string RequiredFlag = "";
    public string RequiredSchool = "";
    public int RequiredGold = 0;
}