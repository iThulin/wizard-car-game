using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CardBlueprint
{
    public string Id;
    public CardSchool School;
    public RowCsvData Row;
}

public static class CardDatabase
{
    public static readonly List<CardBlueprint> Blueprints = new();

    public static void LoadFromCsv(string path)
    {
        var rows = CardCsvReader.Load(path, out var diags);
        foreach (var d in diags) GD.PrintErr(d);

        var classValues = rows
            .Select(r => (r.Top.Class ?? "").Replace("\uFEFF","<BOM>").Trim())
            .Distinct()
            .ToList();

        GD.Print($"Distinct class[0] values: {string.Join(" | ", classValues)}");

        Blueprints.Clear();
        foreach (var r in rows)
        {
            // ✅ normalize (trim + strip BOM) BEFORE parsing
            var rawClass = (r.Top.Class ?? "").Trim().Trim('\uFEFF');

            if (!Enum.TryParse<CardSchool>(rawClass, ignoreCase: true, out var school))
            {
                GD.PrintErr($"Unknown Class '{r.Top.Class}' (normalized '{rawClass}') for row top='{r.Top.Name}'. Skipping.");
                continue;
            }

            Blueprints.Add(new CardBlueprint
            {
                Id = $"{rawClass}:{r.Top.Name}|{r.Bottom.Name}",
                School = school,
                Row = r
            });
        }

        // ✅ PLACE THE COUNTS BLOCK HERE (after Blueprints are built)
        var counts = Blueprints
            .GroupBy(b => b.School)
            .Select(g => $"{g.Key}:{g.Count()}")
            .ToList();

        GD.Print("Blueprint counts => " + string.Join(", ", counts));

        GD.Print($"CardDatabase loaded {Blueprints.Count} blueprints from {path}");
    }

    // Always returns a NEW runtime Card instance (unique InstanceId)
    public static Card Instantiate(CardBlueprint bp)
        => CardCsvCompiler.Compile(bp.Row);

    public static List<Card> BuildRandomDeck(CardSchool school, int count, int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        var pool = Blueprints.Where(b => b.School == school).ToList();
        if (pool.Count < count)
        {
            GD.PrintErr($"Not enough cards for {school}: {pool.Count} available, need {count}.");
            return new List<Card>();
        }

        // Shuffle and pick
        for (int i = 0; i < pool.Count; i++)
        {
            int j = rng.Next(i, pool.Count);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Take(count).Select(Instantiate).ToList();
    }

    public static Card RandomCard(Random rng, Func<CardBlueprint, bool> filter = null)
    {
        var pool = (filter == null) ? Blueprints : Blueprints.Where(filter).ToList();
        if (pool.Count == 0) return null;

        var pick = pool[rng.Next(pool.Count)];
        return Instantiate(pick);
    }
}