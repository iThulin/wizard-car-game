using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// ============================================================
// Card Library — Hearthstone-style collection viewer.
//
// Works both at runtime AND in the editor ([Tool] attribute).
// In-editor: loads JSON cards via CardLoaderV2 and spawns
// read-only CardUi instances so you can preview the full
// collection without running the game.
//
// Place at:  res://Scripts/UI/CardLibraryUi.cs
// Scene:     res://Scenes/UI/CardLibrary.tscn
// ============================================================

[Tool]
public partial class CardLibraryUi : Control
{
    // ── Exports ──────────────────────────────────────────────────────────
    [Export] public PackedScene CardUIScene;
    [Export] public string CardJsonDirectory = "res://Data/Cards";
    [Export] public string ReturnScenePath   = "res://Scenes/Campus/CampusScene.tscn";

    [Export(PropertyHint.Range, "2,10")]
    public int GridColumns = 5;

    [Export(PropertyHint.Range, "0.4,1.5,0.05")]
    public float CardScale = 0.75f;

    // Toggle this in the inspector to force-reload JSON cards and
    // rebuild the grid — useful when editing card JSON files.
    private bool _editorRefresh = false;
    [Export] public bool EditorRefresh
    {
        get => _editorRefresh;
        set
        {
            _editorRefresh = value;
            if (value && Engine.IsEditorHint())
                CallDeferred(nameof(DoEditorRebuild));
        }
    }

    // ── Node refs ────────────────────────────────────────────────────────
    private HBoxContainer _schoolTabs;
    private HBoxContainer _rarityTabs;
    private HBoxContainer _manaTabs;
    private LineEdit      _searchBox;
    private ScrollContainer _scroll;
    private GridContainer _cardGrid;
    private Label         _countLabel;
    private Button        _backButton;

    // ── Filter state ─────────────────────────────────────────────────────
    private CardSchool? _schoolFilter = null;
    private CardRarity? _rarityFilter = null;
    private int         _manaFilter   = -1;   // -1 = all
    private string      _searchText   = "";

    // ── Data ─────────────────────────────────────────────────────────────
    private List<CardBlueprint> _pool = new();

    // ═════════════════════════════════════════════════════════════════════
    //  Lifecycle
    // ═════════════════════════════════════════════════════════════════════

    public override void _Ready()
    {
        WireNodes();
        EnsureCardsLoaded();
        _pool = CardDatabase.Blueprints;

        BuildSchoolTabs();
        BuildRarityTabs();
        BuildManaTabs();

        if (_searchBox != null)
        {
            _searchBox.PlaceholderText = "Search cards\u2026";
            _searchBox.TextChanged += OnSearchChanged;
        }

        if (_backButton != null && !Engine.IsEditorHint())
            _backButton.Pressed += () => GetTree().ChangeSceneToFile(ReturnScenePath);

        if (_cardGrid != null)
            _cardGrid.Columns = GridColumns;

        RebuildGrid();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Node wiring — uses short paths matching the .tscn below
    // ═════════════════════════════════════════════════════════════════════

    private void WireNodes()
    {
        _schoolTabs = GetNodeOrNull<HBoxContainer>("Margin/VBox/FilterBar/SchoolTabs");
        _rarityTabs = GetNodeOrNull<HBoxContainer>("Margin/VBox/FilterBar/RarityTabs");
        _manaTabs   = GetNodeOrNull<HBoxContainer>("Margin/VBox/FilterBar/ManaRow/ManaTabs");
        _searchBox  = GetNodeOrNull<LineEdit>     ("Margin/VBox/FilterBar/ManaRow/SearchBox");
        _countLabel = GetNodeOrNull<Label>        ("Margin/VBox/FilterBar/ManaRow/CountLabel");
        _scroll     = GetNodeOrNull<ScrollContainer>("Margin/VBox/Scroll");
        _cardGrid   = GetNodeOrNull<GridContainer>("Margin/VBox/Scroll/CardGrid");
        _backButton = GetNodeOrNull<Button>       ("Margin/VBox/TopBar/BackButton");
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Card loading — works in editor AND at runtime
    // ═════════════════════════════════════════════════════════════════════

    private void EnsureCardsLoaded()
    {
        // CardLoaderV2.LoadCardsFromJson is idempotent.
        CardLoaderV2.LoadCardsFromJson(CardJsonDirectory);
    }

    private void DoEditorRebuild()
    {
        GD.Print("[CardLibrary] Editor rebuild triggered.");
        CardLoaderV2.Reload(CardJsonDirectory);
        _pool = CardDatabase.Blueprints;
        BuildSchoolTabs();   // counts may have changed
        RebuildGrid();
        _editorRefresh = false;
        NotifyPropertyListChanged();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Filter tab builders
    // ═════════════════════════════════════════════════════════════════════

    private void BuildSchoolTabs()
    {
        if (_schoolTabs == null) return;
        ClearChildren(_schoolTabs);

        MakeTab(_schoolTabs, "All", _schoolFilter == null, () =>
        {
            _schoolFilter = null;
            SyncRadio(_schoolTabs, 0);
            RebuildGrid();
        });

        foreach (CardSchool school in Enum.GetValues(typeof(CardSchool)))
        {
            var s = school;
            int count = _pool.Count(b => b.School == s);
            // Always show the tab even if count is 0 — the designer
            // may be about to add cards for that school.
            MakeTab(_schoolTabs, $"{s} ({count})", false, () =>
            {
                _schoolFilter = s;
                // Let the radio helper figure out which index was pressed.
                RebuildGrid();
            });
        }
    }

    private void BuildRarityTabs()
    {
        if (_rarityTabs == null) return;
        ClearChildren(_rarityTabs);

        MakeTab(_rarityTabs, "All", true, () =>
        {
            _rarityFilter = null;
            RebuildGrid();
        });

        foreach (CardRarity r in Enum.GetValues(typeof(CardRarity)))
        {
            var rr = r;
            MakeTab(_rarityTabs, r.ToString(), false, () =>
            {
                _rarityFilter = rr;
                RebuildGrid();
            });
        }
    }

    private void BuildManaTabs()
    {
        if (_manaTabs == null) return;
        ClearChildren(_manaTabs);

        MakeTab(_manaTabs, "All", true, () =>
        {
            _manaFilter = -1;
            RebuildGrid();
        });

        for (int m = 0; m <= 5; m++)
        {
            int captured = m;
            string label = m < 5 ? m.ToString() : "5+";
            MakeTab(_manaTabs, label, false, () =>
            {
                _manaFilter = captured;
                RebuildGrid();
            });
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Tab / button helpers
    // ═════════════════════════════════════════════════════════════════════

    private static void MakeTab(HBoxContainer parent, string text,
                                bool active, Action onPress)
    {
        var btn = new Button
        {
            Text = text,
            ToggleMode = true,
            ButtonPressed = active,
            FocusMode = FocusModeEnum.None,
            CustomMinimumSize = new Vector2(0, 28)
        };

        var normal  = FlatBox(new Color(0.12f, 0.12f, 0.18f));
        var pressed = FlatBox(new Color(0.30f, 0.30f, 0.42f));
        var hover   = FlatBox(new Color(0.18f, 0.18f, 0.26f));

        btn.AddThemeStyleboxOverride("normal",  normal);
        btn.AddThemeStyleboxOverride("pressed", pressed);
        btn.AddThemeStyleboxOverride("hover",   hover);
        btn.AddThemeFontSizeOverride("font_size", 12);

        btn.Pressed += () =>
        {
            // Radio behaviour
            foreach (var child in parent.GetChildren())
                if (child is Button other && other != btn)
                    other.ButtonPressed = false;
            btn.ButtonPressed = true;
            onPress?.Invoke();
        };

        parent.AddChild(btn);
    }

    private static StyleBoxFlat FlatBox(Color bg)
    {
        var sb = new StyleBoxFlat { BgColor = bg };
        sb.SetCornerRadiusAll(5);
        sb.SetContentMarginAll(6);
        return sb;
    }

    private static void SyncRadio(HBoxContainer bar, int activeIndex)
    {
        int i = 0;
        foreach (var child in bar.GetChildren())
        {
            if (child is Button btn)
                btn.ButtonPressed = (i == activeIndex);
            i++;
        }
    }

    private static void ClearChildren(Node parent)
    {
        foreach (Node c in parent.GetChildren())
            c.QueueFree();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Search
    // ═════════════════════════════════════════════════════════════════════

    private void OnSearchChanged(string newText)
    {
        _searchText = (newText ?? "").Trim().ToLower();
        RebuildGrid();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Grid rebuild
    // ═════════════════════════════════════════════════════════════════════

    private void RebuildGrid()
    {
        if (_cardGrid == null || CardUIScene == null) return;

        ClearChildren(_cardGrid);

        var filtered = _pool.Where(PassesFilter).ToList();

        // Sort: school → rarity → top-half name
        filtered.Sort((a, b) =>
        {
            int cmp = a.School.CompareTo(b.School);
            if (cmp != 0) return cmp;
            cmp = a.Rarity.CompareTo(b.Rarity);
            if (cmp != 0) return cmp;
            string na = a.Prebuilt?.TopHalf?.Name ?? "";
            string nb = b.Prebuilt?.TopHalf?.Name ?? "";
            return string.Compare(na, nb, StringComparison.OrdinalIgnoreCase);
        });

        foreach (var bp in filtered)
        {
            var card = CardDatabase.Instantiate(bp);
            if (card == null) continue;

            var cardUi = CardUIScene.Instantiate<CardUi>();
            cardUi.SetCard(card);

            // ── Library-mode: disable all mouse interaction ──────────
            DisableMouseRecursive(cardUi);

            // Scale to fit the grid nicely
            float s = CardScale;
            cardUi.Scale = new Vector2(s, s);
            cardUi.CustomMinimumSize = new Vector2(200 * s, 300 * s);

            _cardGrid.AddChild(cardUi);
        }

        if (_countLabel != null)
            _countLabel.Text = $"{filtered.Count} / {_pool.Count}";

        // Scroll back to top whenever filters change
        if (_scroll != null)
            _scroll.ScrollVertical = 0;
    }

    /// <summary>
    /// Recursively sets MouseFilter = Ignore on every Control
    /// descendant so the card can't drag, lift, breathe, or
    /// trigger hover effects while in library view.
    /// </summary>
    private static void DisableMouseRecursive(Control root)
    {
        root.MouseFilter = MouseFilterEnum.Ignore;
        foreach (var child in root.GetChildren())
            if (child is Control c)
                DisableMouseRecursive(c);
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Filter logic
    // ═════════════════════════════════════════════════════════════════════

    private bool PassesFilter(CardBlueprint bp)
    {
        if (_schoolFilter.HasValue && bp.School != _schoolFilter.Value)
            return false;

        if (_rarityFilter.HasValue && bp.Rarity != _rarityFilter.Value)
            return false;

        if (_manaFilter >= 0)
        {
            int topMana = bp.Prebuilt?.TopHalf?.ManaCost ?? 0;
            if (_manaFilter < 5 && topMana != _manaFilter) return false;
            if (_manaFilter >= 5 && topMana < 5)           return false;
        }

        if (!string.IsNullOrEmpty(_searchText))
        {
            var top = bp.Prebuilt?.TopHalf;
            var bot = bp.Prebuilt?.BottomHalf;
            string haystack = string.Join(" ",
                top?.Name           ?? "", top?.RulesText                   ?? "",
                top?.ChannelVariant?.RulesText ?? "",
                bot?.Name           ?? "", bot?.RulesText                   ?? "",
                bot?.ChannelVariant?.RulesText ?? "",
                bp.School.ToString(), bp.Rarity.ToString()
            ).ToLower();

            if (!haystack.Contains(_searchText))
                return false;
        }

        return true;
    }
}
