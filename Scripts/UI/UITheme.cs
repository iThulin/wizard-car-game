using Godot;

// ================================================================
// UITheme — Single source of truth for all UI colors, sizes, and
// spacing. Every UI class reads from here.
//
// DO NOT hardcode colors, font sizes, or animation durations
// anywhere else in the project. Add new tokens here instead.
//
// Dark Arcane visual identity:
//   - Deep purple-black backgrounds
//   - Violet/purple accents
//   - Gold for importance and titles
//   - Arcane blue for magic/mana
//   - High contrast text on dark surfaces
// ================================================================
public static class UITheme
{
    // ════════════════════════════════════════════════════════════
    // CORE PALETTE — base colors everything else derives from
    // ════════════════════════════════════════════════════════════

    // ── Backgrounds (darkest to lightest) ───────────────────────
    public static readonly Color BgDeep = new Color(0.08f, 0.06f, 0.10f, 1f); // #141018 — page bg
    public static readonly Color BgBase = new Color(0.12f, 0.09f, 0.16f, 1f); // #1E1728 — panels
    public static readonly Color BgRaised = new Color(0.15f, 0.12f, 0.20f, 1f); // #261E33 — cards, popups
    public static readonly Color BgOverlay = new Color(0.00f, 0.00f, 0.00f, 0.65f); // modal backdrop

    // ── Accent purple ────────────────────────────────────────────
    public static readonly Color Violet = new Color(0.48f, 0.31f, 0.75f, 1f); // #7A4FBF — primary accent
    public static readonly Color VioletDim = new Color(0.29f, 0.18f, 0.48f, 1f); // #4A2E7A — borders, secondary
    public static readonly Color VioletDark = new Color(0.16f, 0.10f, 0.26f, 1f); // #2A1A42 — deep tint

    // ── Gold (titles, important actions) ────────────────────────
    public static readonly Color Gold = new Color(0.91f, 0.78f, 0.29f, 1f); // #E8C84A
    public static readonly Color GoldDim = new Color(0.60f, 0.49f, 0.12f, 1f); // #9A7D1E
    public static readonly Color GoldDark = new Color(0.30f, 0.24f, 0.06f, 1f); // #4D3D0F

    // ── Magic blue (mana, arcane) ────────────────────────────────
    public static readonly Color ArcaneBlue = new Color(0.29f, 0.56f, 0.91f, 1f); // #4A8FE8
    public static readonly Color ArcaneBlueDim = new Color(0.18f, 0.33f, 0.55f, 1f); // #2E548C
    public static readonly Color ArcaneBlueDark = new Color(0.10f, 0.18f, 0.30f, 1f); // #1A2E4D

    // ── Semantic colors ──────────────────────────────────────────
    public static readonly Color Success = new Color(0.29f, 0.75f, 0.42f, 1f); // #4ABF6A
    public static readonly Color SuccessDim = new Color(0.18f, 0.45f, 0.26f, 1f); // #2E7342
    public static readonly Color Danger = new Color(0.75f, 0.29f, 0.29f, 1f); // #BF4A4A
    public static readonly Color DangerDim = new Color(0.60f, 0.40f, 0.40f, 1f); // #996666 — unaffordable tint
    public static readonly Color Warning = new Color(0.91f, 0.63f, 0.19f, 1f); // #E8A030
    public static readonly Color WarningDim = new Color(0.55f, 0.37f, 0.10f, 1f); // #8C5E19

    // ── Text ─────────────────────────────────────────────────────
    public static readonly Color TextPrimary = new Color(0.94f, 0.92f, 0.97f, 1f); // #F0EAF8 — warm off-white
    public static readonly Color TextSecondary = new Color(0.60f, 0.54f, 0.69f, 1f); // #9A8AB0 — muted lavender
    public static readonly Color TextDim = new Color(0.35f, 0.29f, 0.42f, 1f); // #5A4A6A — disabled/placeholder
    public static readonly Color TextOnDark = new Color(0.94f, 0.92f, 0.97f, 1f); // same as primary, named for clarity

    // ── Neutral ──────────────────────────────────────────────────
    public static readonly Color Neutral = new Color(0.40f, 0.36f, 0.46f, 1f); // mid purple-grey
    public static readonly Color NeutralDim = new Color(0.25f, 0.22f, 0.30f, 1f); // dark purple-grey

    // ════════════════════════════════════════════════════════════
    // SURFACE ALIASES — semantic names for common bg uses
    // ════════════════════════════════════════════════════════════
    public static readonly Color SurfaceLight = BgRaised;   // card panels, modal content
    public static readonly Color SurfaceMid = BgBase;     // panel backgrounds
    public static readonly Color SurfaceDark = BgDeep;     // page backgrounds
    public static readonly Color SurfaceOverlay = BgOverlay;  // modal backdrops

    // ════════════════════════════════════════════════════════════
    // CARD UI
    // ════════════════════════════════════════════════════════════
    public static readonly Color CardTopActive = new Color(1.15f, 1.05f, 0.85f, 1f); // warm highlight
    public static readonly Color CardBottomActive = new Color(0.90f, 0.85f, 1.20f, 1f); // cool highlight
    public static readonly Color CardDim = Neutral;
    public static readonly Color CardDragGhost = new Color(1.05f, 1.05f, 1.10f, 0.85f);

    // ── Rarity colors ────────────────────────────────────────────
    public static readonly Color RarityCommon = new Color(0.70f, 0.68f, 0.72f, 1f); // silver-grey
    public static readonly Color RarityUncommon = new Color(0.35f, 0.72f, 0.42f, 1f); // green
    public static readonly Color RarityRare = new Color(0.35f, 0.55f, 0.90f, 1f); // blue
    public static readonly Color RarityLegendary = new Color(0.72f, 0.35f, 0.90f, 1f); // purple

    // ════════════════════════════════════════════════════════════
    // HEALTH BAR / UNIT STATS
    // ════════════════════════════════════════════════════════════
    public static readonly Color HealthGreen = new Color(0.29f, 0.80f, 0.42f, 1f);
    public static readonly Color ManaBlue = ArcaneBlue;
    public static readonly Color ArmorGrey = new Color(0.55f, 0.55f, 0.62f, 1f);
    public static readonly Color ShieldPurple = new Color(0.50f, 0.25f, 0.85f, 0.75f);
    public const float HealthBarWidth = 1.6f;

    // ════════════════════════════════════════════════════════════
    // TILE HIGHLIGHTS (combat hex grid)
    // ════════════════════════════════════════════════════════════
    public static readonly Color TileHover = new Color(1.0f, 0.92f, 0.50f, 1f);
    public static readonly Color TileDragHover = new Color(0.90f, 0.80f, 0.25f, 1f);
    public static readonly Color TileMoveHighlight = new Color(0.30f, 0.65f, 1.00f, 1f);
    public static readonly Color TileDeployHighlight = new Color(0.30f, 1.00f, 0.40f, 1f);
    public static readonly Color TileTargetHighlight = new Color(1.00f, 0.38f, 0.38f, 1f);
    public static readonly Color TileRangeInterior = new Color(0.55f, 0.38f, 0.90f, 0.25f); // violet tint
    public static readonly Color TileRangeBorder = new Color(0.70f, 0.48f, 1.00f, 0.70f); // violet ring
    public static readonly Color TileGlyph = new Color(0.65f, 0.25f, 1.00f, 1f);

    // ════════════════════════════════════════════════════════════
    // COMBAT TILE BASE COLORS
    // ════════════════════════════════════════════════════════════
    public static readonly Color CombatTileGrass = new Color(0.35f, 0.60f, 0.35f, 1f);
    public static readonly Color CombatTileForest = new Color(0.18f, 0.42f, 0.20f, 1f);
    public static readonly Color CombatTileStone = new Color(0.45f, 0.45f, 0.50f, 1f);
    public static readonly Color CombatTileWater = new Color(0.20f, 0.42f, 0.80f, 1f);
    public static readonly Color CombatTileLava = new Color(0.85f, 0.28f, 0.08f, 1f);
    public static readonly Color CombatTileArcane = new Color(0.48f, 0.22f, 0.75f, 1f);
    public static readonly Color CombatTileIce = new Color(0.65f, 0.85f, 0.98f, 1f);

    // ── Spawn zone tints ─────────────────────────────────────────
    public static readonly Color SpawnTintPlayer = new Color(0.25f, 0.75f, 1.00f, 1f);
    public static readonly Color SpawnTintEnemy = new Color(1.00f, 0.30f, 0.30f, 1f);
    public const float SpawnTintStrength = 0.30f;

    // ════════════════════════════════════════════════════════════
    // ELEMENT COLORS (attunement + tile imbue)
    // ════════════════════════════════════════════════════════════
    public static readonly Color ElementFire = new Color(0.95f, 0.38f, 0.08f, 1f);
    public static readonly Color ElementIce = new Color(0.50f, 0.82f, 1.00f, 1f);
    public static readonly Color ElementStorm = new Color(0.85f, 0.80f, 0.20f, 1f);
    public static readonly Color ElementEarth = new Color(0.62f, 0.45f, 0.22f, 1f);

    // ── Imbuement overlay tints ──────────────────────────────────
    public static readonly Color ElementTintFire = new Color(1.00f, 0.42f, 0.08f, 1f);
    public static readonly Color ElementTintFrost = new Color(0.60f, 0.88f, 1.00f, 1f);
    public static readonly Color ElementTintLightning = new Color(0.92f, 0.85f, 1.00f, 1f);
    public static readonly Color ElementTintEarth = new Color(0.72f, 0.52f, 0.22f, 1f);
    public static readonly Color ElementTintWater = new Color(0.28f, 0.58f, 0.95f, 1f);
    public static readonly Color ElementTintAir = new Color(0.82f, 0.95f, 0.85f, 1f);
    public static readonly Color ElementTintArcane = new Color(0.80f, 0.38f, 1.00f, 1f);
    public static readonly Color ElementTintShadow = new Color(0.45f, 0.18f, 0.52f, 1f);

    // ════════════════════════════════════════════════════════════
    // COMBAT UI
    // ════════════════════════════════════════════════════════════
    public static readonly Color EnemyHealthBar = Danger;
    public static readonly Color UnitBarSelected = new Color(0.29f, 0.48f, 0.85f, 0.55f);
    public static readonly Color UnitBarBorder = new Color(0.94f, 0.92f, 0.97f, 0.85f);
    public static readonly Color StatBarHealth = HealthGreen;
    public static readonly Color StatBarMove = new Color(0.85f, 0.72f, 0.20f, 1f);
    public static readonly Color StatBarMana = ArcaneBlue;

    public const int CombatStatLabelFontSize = 10;
    public const int EnemyRosterButtonWidth = 90;
    public const int EnemyRosterBarWidth = 80;
    public const int EnemyRosterBarHeight = 14;
    public const int UnitBarStatBarWidth = 80;
    public const int UnitBarStatBarHeight = 8;
    public const int MaxActionLogLines = 6;

    // ════════════════════════════════════════════════════════════
    // OVERWORLD
    // ════════════════════════════════════════════════════════════

    // ── Terrain ──────────────────────────────────────────────────
    public static readonly Color TerrainGrassland = new Color(0.38f, 0.65f, 0.28f, 1f);
    public static readonly Color TerrainForest = new Color(0.12f, 0.42f, 0.15f, 1f);
    public static readonly Color TerrainRoad = new Color(0.72f, 0.65f, 0.48f, 1f);
    public static readonly Color TerrainRuins = new Color(0.52f, 0.44f, 0.36f, 1f);
    public static readonly Color TerrainMountain = new Color(0.58f, 0.56f, 0.54f, 1f);
    public static readonly Color TerrainSwamp = new Color(0.32f, 0.44f, 0.22f, 1f);
    public static readonly Color TerrainArcaneGround = new Color(0.48f, 0.30f, 0.72f, 1f);
    public static readonly Color TerrainVolcanic = new Color(0.70f, 0.26f, 0.10f, 1f);
    public static readonly Color TerrainWater = new Color(0.18f, 0.44f, 0.78f, 1f);

    // ── Fog ──────────────────────────────────────────────────────
    public static readonly Color FogHidden = new Color(0.08f, 0.06f, 0.12f, 0.90f); // matches BgDeep
    public static readonly Color FogSilhouette = new Color(0.08f, 0.06f, 0.12f, 0.48f);
    public static readonly Color FogRevealed = new Color(0f, 0f, 0f, 0f);

    // ── POI markers ──────────────────────────────────────────────
    public static readonly Color POICombat = Danger;
    public static readonly Color POIRest = new Color(0.25f, 0.78f, 0.90f, 1f);
    public static readonly Color POIObjective = Gold;
    public static readonly Color POINarrative = Violet;
    public static readonly Color POINegotiation = new Color(0.20f, 0.70f, 0.52f, 1f);

    // ── Hex border ───────────────────────────────────────────────
    public static readonly Color HexBorderColor = new Color(0.18f, 0.14f, 0.22f, 0.70f);
    public const float HexBorderWidth = 1.5f;

    // ── Party token ──────────────────────────────────────────────
    public static readonly Color PartyTokenFill = Gold;
    public static readonly Color PartyTokenOutline = GoldDark;
    public const float PartyTokenRadius = 14f;
    public const float PartyTokenOutlineRadius = 17f;
    public const int PartyTokenSegments = 12;

    // ── Move highlights ───────────────────────────────────────────
    public static readonly Color MoveHighlightCheap = new Color(0.38f, 0.90f, 0.45f, 0.30f);
    public static readonly Color MoveHighlightModerate = new Color(0.90f, 0.80f, 0.25f, 0.30f);
    public static readonly Color MoveHighlightExpensive = new Color(0.90f, 0.40f, 0.25f, 0.30f);
    public static readonly Color MoveHighlightDefault = new Color(0.70f, 0.55f, 0.90f, 0.30f);

    // ── Overworld UI labels ───────────────────────────────────────
    public static readonly Color OverworldLowResourceWarning = new Color(0.90f, 0.38f, 0.38f, 1f);
    public static readonly Color OverworldInfoLabelTint = Gold;

    // ════════════════════════════════════════════════════════════
    // UNIT VISUALS
    // ════════════════════════════════════════════════════════════
    public static readonly Color[] EnemyUnitColors =
    {
        new Color(0.90f, 0.22f, 0.22f), // red
        new Color(0.95f, 0.52f, 0.08f), // orange
        new Color(0.75f, 0.18f, 0.88f), // purple
        new Color(0.18f, 0.75f, 0.88f), // cyan
        new Color(0.88f, 0.88f, 0.08f), // yellow
    };

    public static readonly Color SummonColorPillar = new Color(0.48f, 0.42f, 0.32f, 1f);
    public static readonly Color SummonColorFriendly = ArcaneBlue;
    public static readonly Color SummonColorEnemy = Danger;

    // ════════════════════════════════════════════════════════════
    // NARRATIVE ENCOUNTER PANEL
    // ════════════════════════════════════════════════════════════
    public static readonly Color NarrativeBackdrop = BgOverlay;
    public static readonly Color NarrativePanelBg = BgBase;
    public static readonly Color NarrativePanelBorder = VioletDim;
    public static readonly Color NarrativeResultBg = BgRaised;
    public static readonly Color NarrativeResultBorder = Violet;
    public static readonly Color NarrativeTitleColor = Gold;
    public static readonly Color NarrativeBodyColor = TextPrimary;
    public static readonly Color NarrativeResultColor = new Color(0.95f, 0.88f, 0.70f, 1f);

    public const int NarrativeTitleFontSize = 22;
    public const int NarrativeBodyFontSize = 16;
    public const int NarrativeResultFontSize = 15;
    public const int NarrativeChoiceFontSize = 15;
    public const int NarrativePanelCorner = 8;
    public const int NarrativeResultCorner = 4;

    // ════════════════════════════════════════════════════════════
    // NEGOTIATION PANEL
    // ════════════════════════════════════════════════════════════
    public static readonly Color NegotiationBg = BgDeep;
    public static readonly Color NegotiationResultBg = BgBase;
    public static readonly Color NegotiationResultBorder = Violet;
    public static readonly Color NegotiationTitleColor = Gold;
    public static readonly Color NegotiationNpcColor = TextSecondary;
    public static readonly Color NegotiationBodyColor = TextPrimary;
    public static readonly Color NegotiationHiddenTerm = TextDim;

    // ── Tension bar ───────────────────────────────────────────────
    public static readonly Color TensionEmpty = NeutralDim;
    public static readonly Color TensionCordial = new Color(0.22f, 0.72f, 0.32f, 1f);
    public static readonly Color TensionStrained = Warning;
    public static readonly Color TensionHostile = Danger;

    // ── Zone labels ───────────────────────────────────────────────
    public static readonly Color ZoneCordialLabel = new Color(0.28f, 0.78f, 0.35f, 1f);
    public static readonly Color ZoneStrainedLabel = Warning;
    public static readonly Color ZoneHostileLabel = Danger;

    // ── Term icons ────────────────────────────────────────────────
    public static readonly Color TermFavorPlayer = Success;
    public static readonly Color TermAgainstPlayer = Danger;

    // ── Font sizes ────────────────────────────────────────────────
    public const int NegotiationTitleFontSize = 26;
    public const int NegotiationNpcFontSize = 18;
    public const int NegotiationBodyFontSize = 15;
    public const int NegotiationHeaderFontSize = 16;
    public const int NegotiationDetailFontSize = 14;
    public const int NegotiationSmallFontSize = 13;
    public const int NegotiationTinyFontSize = 11;
    public const int NegotiationResultFontSize = 18;
    public const int NegotiationActionFontSize = 16;
    public const int NegotiationTensionFontSize = 14;

    // ════════════════════════════════════════════════════════════
    // CAMPUS SCREEN
    // ════════════════════════════════════════════════════════════
    public static readonly Color CampusBg = BgDeep;
    public static readonly Color CampusTitleBarBg = BgBase;
    public static readonly Color CampusTitleBarBorder = VioletDim;
    public static readonly Color CampusTitleColor = Gold;
    public static readonly Color CampusSectionColor = Gold;
    public static readonly Color CampusSubtleText = TextSecondary;
    public static readonly Color CampusStubText = TextDim;
    public static readonly Color CampusSchoolDescText = TextSecondary;
    public static readonly Color CampusRunSuccess = Success;
    public static readonly Color CampusRunFail = Danger;
    public static readonly Color CampusNoRunText = TextDim;
    public static readonly Color CampusSlotSelected = new Color(0.55f, 0.90f, 0.55f, 1f);

    // ── Debug panel ───────────────────────────────────────────────
    public static readonly Color DebugPanelBg = new Color(0.14f, 0.08f, 0.08f, 1f);
    public static readonly Color DebugPanelBorder = new Color(0.55f, 0.25f, 0.25f, 1f);

    // ── Companion card ────────────────────────────────────────────
    public static readonly Color CompanionCardBg = BgRaised;
    public static readonly Color CompanionCardBorderActive = Success;
    public static readonly Color CompanionCardBorderInactive = NeutralDim;
    public static readonly Color CompanionSubText = TextSecondary;

    // ── Building card ─────────────────────────────────────────────
    public static readonly Color BuildingCardBg = BgRaised;
    public static readonly Color BuildingCardBorderBuilt = Violet;
    public static readonly Color BuildingCardBorderEmpty = NeutralDim;
    public static readonly Color BuildingCategoryText = TextSecondary;
    public static readonly Color BuildingActiveText = Success;
    public static readonly Color BuildingNextText = TextSecondary;
    public static readonly Color BuildingMaxText = Gold;

    // ── Font sizes ────────────────────────────────────────────────
    public const int CampusTitleFontSize = 30;
    public const int CampusTabFontSize = 16;
    public const int CampusSectionFontSize = 20;
    public const int CampusBodyFontSize = 15;
    public const int CampusSmallFontSize = 13;
    public const int CampusTinyFontSize = 12;
    public const int CampusStubFontSize = 14;
    public const int CampusSchoolFontSize = 13;
    public const int CampusNameFontSize = 15;
    public const int CampusBuildFontSize = 16;
    public const int CampusBuildSmallFontSize = 13;
    public const int CampusBuildTinyFontSize = 12;

    // ── Card library ──────────────────────────────────────────────
    public static readonly Color LibraryTabNormal = BgBase;
    public static readonly Color LibraryTabPressed = new Color(0.25f, 0.18f, 0.38f, 1f);
    public static readonly Color LibraryTabHover = BgRaised;

    public const int LibraryTabFontSize = 12;
    public const int LibraryTabHeight = 28;
    public const float LibraryCardWidth = 200f;
    public const float LibraryCardHeight = 300f;
    public const float LibraryGridSpacing = 12f;

    // ════════════════════════════════════════════════════════════
    // FONT SIZES — shared scale
    // ════════════════════════════════════════════════════════════
    public const int FontSizeSmall = 13;
    public const int FontSizeNormal = 16;
    public const int FontSizeMedium = 20;
    public const int FontSizeLarge = 26;
    public const int FontSizeTitle = 32;

    // ── 3D Label sizes ────────────────────────────────────────────
    public const int Label3DUnit = 18;
    public const int Label3DSmall = 24;
    public const int Label3DHealth = 28;
    public const int Label3DGlyph = 64;

    // ── Overworld label sizes ─────────────────────────────────────
    public const int OverworldUIFontSize = 18;
    public const int OverworldCostLabelFontSize = 14;

    // ── Attunement bar ────────────────────────────────────────────
    public const int AttunementPanelWidth = 252;
    public const int AttunementBarHeight = 12;
    public const int AttunementBarMax = 4;

    // ── Pause button ──────────────────────────────────────────────
    public const int FontSizePauseButton = 18;

    // ════════════════════════════════════════════════════════════
    // SPACING / LAYOUT
    // ════════════════════════════════════════════════════════════
    public const int PaddingSmall = 4;
    public const int PaddingNormal = 8;
    public const int PaddingLarge = 16;
    public const int BorderWidth = 2;    // reduced from 3 — cleaner at dark palette
    public const int CornerRadius = 5;
    public const int CornerRadiusLg = 8;

    // ════════════════════════════════════════════════════════════
    // ANIMATION DURATIONS (seconds)
    // ════════════════════════════════════════════════════════════
    public const float AnimFast = 0.12f;
    public const float AnimNormal = 0.20f;
    public const float AnimSlow = 0.35f;
    public const float AnimPulse = 0.50f;

    // ════════════════════════════════════════════════════════════
    // HAND / CARD LAYOUT
    // ════════════════════════════════════════════════════════════
    public const float HandArcRadiusScale = 2.3f;
    public const float HandArcCenterYScale = 0.6f;
    public const float HandArcMaxSpanDeg = 30f;
    public const float HandArcMinSpanDeg = 1f;
    public const float HandArcStepPerCard = 5f;
    public const float HandNeighborPushPx = 18f;

    public const float DiscardAnimDropScale = 0.3f;
    public const float DiscardAnimDuration = 0.28f;
    public const float DiscardFadeDuration = 0.22f;
    public const float DiscardEndScale = 0.85f;

    // ════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════

    public static Color GetRarityColor(CardRarity rarity) => rarity switch
    {
        CardRarity.Common => RarityCommon,
        CardRarity.Uncommon => RarityUncommon,
        CardRarity.Rare => RarityRare,
        CardRarity.Legendary => RarityLegendary,
        _ => RarityCommon
    };

    public static StyleBoxFlat MakePanelStyle(
        Color bg, Color border,
        bool topCorners = true, bool bottomCorners = true)
    {
        var s = new StyleBoxFlat();
        s.BgColor = bg;
        s.BorderColor = border;
        s.SetBorderWidthAll(BorderWidth);
        if (topCorners)
        {
            s.CornerRadiusTopLeft = CornerRadius;
            s.CornerRadiusTopRight = CornerRadius;
        }
        if (bottomCorners)
        {
            s.CornerRadiusBottomLeft = CornerRadius;
            s.CornerRadiusBottomRight = CornerRadius;
        }
        return s;
    }

    public static StyleBoxFlat MakeBadgeStyle(Color bg)
    {
        var s = new StyleBoxFlat { BgColor = bg };
        s.SetCornerRadiusAll(CornerRadiusLg);
        return s;
    }
}
