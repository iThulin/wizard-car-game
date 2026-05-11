using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Loads NegotiationEncounterData from Data/Negotiations/*.json.
/// </summary>
public static class NegotiationEncounterLoader
{
    private const string DIR = "res://Data/Negotiations/";

    private static readonly Dictionary<string, NegotiationEncounterData> _cache = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
    };

    public static NegotiationEncounterData Load(string id)
    {
        if (_cache.TryGetValue(id, out var cached)) return cached;

        string path = $"{DIR}{id}.json";
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"NegotiationEncounterLoader: No file at {path}");
            return null;
        }

        try
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null) return null;

            var data = JsonSerializer.Deserialize<NegotiationEncounterData>(
                file.GetAsText(), JsonOptions);

            if (data != null) _cache[id] = data;
            GD.Print($"NegotiationEncounterLoader: Loaded '{id}'");
            return data;
        }
        catch (Exception e)
        {
            GD.PrintErr($"NegotiationEncounterLoader: Error loading {id}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Pick a random negotiation encounter appropriate for a terrain type.
    /// </summary>
    public static NegotiationEncounterData PickForTerrain(string terrain, string regionId)
    {
        // Phase 2: simple pool. Phase 3: terrain/region affinity system.
        var candidates = new List<string>();

        // Always try region-specific encounters first
        candidates.Add($"{regionId}_merchant");
        candidates.Add($"{regionId}_commander");
        candidates.Add("generic_merchant");

        foreach (var id in candidates)
        {
            var data = Load(id);
            if (data != null) return data;
        }

        return null;
    }

    public static void ClearCache() => _cache.Clear();
}