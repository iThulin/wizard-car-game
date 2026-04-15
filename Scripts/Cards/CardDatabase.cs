using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CardBlueprint
{
    public string Id;
    public CardSchool School;
    public CardRarity Rarity;
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
            // Normalize (trim + strip BOM) BEFORE parsing
            var rawClass = (r.Top.Class ?? "").Trim().Trim('\uFEFF');

            if (!Enum.TryParse<CardSchool>(rawClass, ignoreCase: true, out var school))
            {
                GD.PrintErr($"Unknown Class '{r.Top.Class}' (normalized '{rawClass}') for row top='{r.Top.Name}'. Skipping.");
                continue;
            }

            if (!Enum.TryParse<CardRarity>(r.Rarity, ignoreCase: true, out var rarity))
                rarity = CardRarity.Common;

            Blueprints.Add(new CardBlueprint
            {
                Id = $"{rawClass}:{r.Top.Name}|{r.Bottom.Name}",
                School = school,
                Rarity = rarity,  // <-- NEW
                Row = r
            });
        }

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

    public static List<Card> BuildWeightedDeck(CardSchool school, int count, int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var pool = Blueprints.Where(b => b.School == school).ToList();
        
        // Weight: Common=4, Uncommon=3, Rare=2, Legendary=1
        var weighted = new List<CardBlueprint>();
        foreach (var bp in pool)
        {
            int weight = bp.Rarity switch {
                CardRarity.Common => 4,
                CardRarity.Uncommon => 3,
                CardRarity.Rare => 2,
                CardRarity.Legendary => 1,
                _ => 4
            };
            for (int i = 0; i < weight; i++)
                weighted.Add(bp);
        }
        
        // Shuffle weighted pool and pick unique blueprints
        var picked = new HashSet<string>();
        var result = new List<Card>();
        
        for (int i = weighted.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (weighted[i], weighted[j]) = (weighted[j], weighted[i]);
        }
        
        foreach (var bp in weighted)
        {
            if (picked.Contains(bp.Id)) continue;
            picked.Add(bp.Id);
            result.Add(Instantiate(bp));
            if (result.Count >= count) break;
        }
        
        return result;
    }

    public static List<Card> GetDraftChoices(CardSchool school, int choices = 3, int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var pool = Blueprints.Where(b => b.School == school).ToList();
        
        // Roll rarity for each choice: 50% Common, 30% Uncommon, 15% Rare, 5% Legendary
        var result = new List<Card>();
        for (int i = 0; i < choices; i++)
        {
            double roll = rng.NextDouble();
            CardRarity target = roll < 0.50 ? CardRarity.Common
                            : roll < 0.80 ? CardRarity.Uncommon
                            : roll < 0.95 ? CardRarity.Rare
                            : CardRarity.Legendary;
            
            var candidates = pool.Where(b => b.Rarity == target).ToList();
            if (candidates.Count == 0) candidates = pool; // fallback
            
            var pick = candidates[rng.Next(candidates.Count)];
            result.Add(Instantiate(pick));
        }
        
        return result;
    }

    public static List<Card> GetShopInventory(CardSchool school, int size = 5, int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var pool = Blueprints.Where(b => b.School == school).ToList();
        
        // Shop always has: 2 Common, 1 Uncommon, 1 Rare, 1 random (weighted toward Uncommon+)
        var result = new List<Card>();
        var used = new HashSet<string>();
        
        void AddFromRarity(CardRarity r, int count)
        {
            var candidates = pool.Where(b => b.Rarity == r && !used.Contains(b.Id)).ToList();
            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                int idx = rng.Next(candidates.Count);
                used.Add(candidates[idx].Id);
                result.Add(Instantiate(candidates[idx]));
                candidates.RemoveAt(idx);
            }
        }
        
        AddFromRarity(CardRarity.Common, 2);
        AddFromRarity(CardRarity.Uncommon, 1);
        AddFromRarity(CardRarity.Rare, 1);
        
        // Last slot: weighted random
        double roll = rng.NextDouble();
        CardRarity bonus = roll < 0.30 ? CardRarity.Common
                        : roll < 0.65 ? CardRarity.Uncommon
                        : roll < 0.90 ? CardRarity.Rare
                        : CardRarity.Legendary;
        AddFromRarity(bonus, 1);
        
        return result;
    }
}