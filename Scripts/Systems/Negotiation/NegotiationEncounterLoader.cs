using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

// ============================================================
// NegotiationEncounterLoader.cs
//
// Purpose:        Loads NegotiationEncounterData from
//                 Data/Negotiations/*.json. Per-session cache.
// Layer:          Loader
// Collaborators:  NpcArchetype.cs (NegotiationEncounterData
//                 schema), NegotiationManager.cs (caller)
// See:            README §6 — Negotiation
// ============================================================

/// <summary>Lazy loader + per-session cache for negotiation encounter JSON. Each encounter file is read at most once per process.</summary>
public static class NegotiationEncounterLoader
{
    private const string DIR = "res://Data/Negotiations/";

    private static readonly Dictionary<string, NegotiationEncounterData> _cache = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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
        // Try region-specific commander first (high-stakes encounter)
        // Then fall back to generic merchant
        var candidates = new List<string>
        {
            $"{regionId}_commander",
            "generic_merchant",
        };

        // Randomize between available options so you don't always get the same one
        var available = new List<NegotiationEncounterData>();
        foreach (var id in candidates)
        {
            var data = Load(id);
            if (data != null) available.Add(data);
        }

        if (available.Count == 0) return null;
        return available[(int)(GD.Randi() % (uint)available.Count)];
    }

    public static void ClearCache() => _cache.Clear();
}