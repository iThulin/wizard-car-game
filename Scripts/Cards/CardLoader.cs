using Godot;
using System.Collections.Generic;

// ============================================================
// Entry point for loading JSON-defined cards.
//
// This is idempotent — safe to call from as many _Ready
// methods as you want. Subsequent calls after cards are loaded
// are a no-op.
// ============================================================

public static class CardLoaderV2
{
    private static bool _registered = false;

    public static void LoadCardsFromJson(string directoryPath)
    {
        // Self-guard — safe to call from multiple _Ready methods.
        if (CardDatabase.Blueprints.Count > 0)
            return;

        if (!_registered)
        {
            CardScriptRegistry.RegisterBuiltins();
            _registered = true;
        }

        var cards = JsonCardLoader.LoadAll(directoryPath);

        int added = 0;
        foreach (var c in cards)
        {
            CardDatabase.RegisterPrebuiltCard(c);
            added++;
        }

        GD.Print($"[CardLoaderV2] Registered {added} JSON cards into CardDatabase. " +
                 $"Total blueprints: {CardDatabase.Blueprints.Count}");
    }

    // Force-reload (use from dev tools, not gameplay)
    public static void Reload(string directoryPath)
    {
        CardDatabase.Blueprints.Clear();
        LoadCardsFromJson(directoryPath);
    }
}
