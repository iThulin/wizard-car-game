using Godot;

// ================================================================
// UITheme — Single source of truth for all UI colors, sizes, and
// spacing. Every UI class reads from here.
//
// DO NOT hardcode colors, font sizes, or animation durations
// anywhere else in the project. Add new tokens here instead.
//
// Visual identity:
//   Dark Arcane Field + Parchment UI Chrome
//
//   3D world (tiles, units, overworld map) — deep purple-black,
//   atmospheric and arcane.
//
//   UI chrome (hand area, panels, campus, modals, cards) — warm
//   parchment/tan. Cards feel native to the UI layer. High
//   contrast text on light surfaces.
//
//   Accents — violet borders, gold titles, arcane blue for mana.
//   These work on both dark and light surfaces.
// ================================================================
public static class UITheme
{
    // ════════════════════════════════════════════════════════════
    // CORE PALETTE
    // ════════════════════════════════════════════════════════════

    // ── World backgrounds (3D battlefield, overworld map) ────────
    // These stay dark — used for the game world, not UI chrome.
    public static readonly Color WorldDeep = new Color(0.08f, 0.06f, 0.10f, 1f); // #141018
    public static readonly Color WorldBase = new Color(0.12f, 0.09f, 0.16f, 1f); // #1E1728
    public static readonly Color WorldOverlay = new Color(0.00f, 0.00f, 0.00f, 0.65f);

    // ── UI chrome backgrounds (panels, cards, modals) ────────────
    // Warm parchment — the "card game layer" sitting on the world.
    public static readonly Color BgDeep = new Color(0.72f, 0.68f, 0.60f, 1f); // #B8AD99 — deep parchment
    public static readonly Color BgBase = new Color(0.82f, 0.78f, 0.70f, 1f); // #D1C7B3 — panel bg
    public static readonly Color BgRaised = new Color(0.90f, 0.87f, 0.80f, 1f); // #E6DEC9 — cards, popups
    public static readonly Color BgOverlay = new Color(0.00f, 0.00f, 0.00f, 0.65f); // modal backdrop (dark)

    // ── Accent purple ────────────────────────────────────────────
    public static readonly Color Violet = new Color(0.42f, 0.22f, 0.68f, 1f); // #6B38AD — deeper on light bg
    public static readonly Color VioletDim = new Color(0.28f, 0.15f, 0.45f, 1f); // #472673
    public static readonly Color VioletDark = new Color(0.16f, 0.10f, 0.26f, 1f); // #2A1A42

    // ── Gold ─────────────────────────────────────────────────────
    public static readonly Color Gold = new Color(0.72f, 0.55f, 0.08f, 1f); // #B88C14 — darker gold for light bg
    public static readonly Color GoldDim = new Color(0.50f, 0.38f, 0.05f, 1f); // #80610D
    public static readonly Color GoldDark = new Color(0.28f, 0.20f, 0.02f, 1f); // #473305

    // ── Magic blue ───────────────────────────────────────────────
    public static readonly Color ArcaneBlue = new Color(0.18f, 0.42f, 0.78f, 1f); // #2E6BC7 — deeper on light
    public static readonly Color ArcaneBlueDim = new Color(0.12f, 0.28f, 0.52f, 1f); // #1E4785
    public static readonly Color ArcaneBlueDark = new Color(0.08f, 0.16f, 0.30f, 1f); // #14284D

    // ── Semantic ─────────────────────────────────────────────────
    public static readonly Color Success = new Color(0.15f, 0.55f, 0.28f, 1f); // #268C47
    public static readonly Color SuccessDim = new Color(0.10f, 0.35f, 0.18f, 1f); // #1A592E
    public static readonly Color Danger = new Color(0.72f, 0.18f, 0.18f, 1f); // #B82E2E
    public static readonly Color DangerDim = new Color(0.82f, 0.55f, 0.50f, 1f); // #D18C80 — unaffordable tint
    public static readonly Color Warning = new Color(0.78f, 0.48f, 0.05f, 1f); // #C77A0D
    public static readonly Color WarningDim = new Color(0.88f, 0.68f, 0.45f, 1f); // #E0AD72

    // ── Text (on light parchment surfaces) ───────────────────────
    public static readonly Color TextPrimary = new Color(0.12f, 0.09f, 0.16f, 1f); // #1E1728 — near-black purple
    public static readonly Color TextSecondary = new Color(0.35f, 0.28f, 0.45f, 1f); // #594873 — mid purple
    public static readonly Color TextDim = new Color(0.55f, 0.50f, 0.62f, 1f); // #8C809E — muted purple-grey
    public static readonly Color TextOnDark = new Color(0.94f, 0.92f, 0.97f, 1f); // #F0EAF8 — for dark surfaces

    // ── Neutral ──────────────────────────────────────────────────
    public static readonly Color Neutral = new Color(0.55f, 0.50f, 0.60f, 1f); // mid purple-grey
    public static readonly Color NeutralDim = new Color(0.68f, 0.64f, 0.72f, 1f); // light purple-grey

    // ════════════════════════════════════════════════════════════
    // SURFACE ALIASES
    // ════════════════════════════════════════════════════════════
    public static readonly Color SurfaceLight = new Color(0.96f, 0.94f, 0.88f, 1f); // #F5F0E0 — card halves
    public static readonly Color SurfaceMid = BgBase;
    public static readonly Color SurfaceDark = BgDeep;
    public static readonly Color SurfaceOverlay = BgOverlay;

    // ════════════════════════════════════════════════════════════
    // CARD UI
    // ════════════════════════════════════════════════════════════
    public static readonly Color CardTopActive = new Color(1.10f, 1.00f, 0.80f, 1f); // warm gold tint
    public static readonly Color CardBottomActive = new Color(0.85f, 0.82f, 1.10f, 1f); // cool violet tint
    public static readonly Color CardDim = new Color(0.70f, 0.66f, 0.72f, 1f); // muted for unaffordable
    public static readonly Color CardDragGhost = new Color(1.00f, 0.98f, 0.92f, 0.88f);

    // ── Rarity colors (work on parchment) ────────────────────────
    public static readonly Color RarityCommon = new Color(0.42f, 0.40f, 0.45f, 1f); // dark grey
    public static readonly Color RarityUncommon = new Color(0.12f, 0.48f, 0.22f, 1f); // forest green
    public static readonly Color RarityRare = new Color(0.15f, 0.35f, 0.72f, 1f); // deep blue
    public static readonly Color RarityLegendary = new Color(0.48f, 0.12f, 0.68f, 1f); // deep purple

    // ── Card borders ───────────────────────────────────────────────
    public const int CardBorderWidth = 4; // thicker border for cards
    public const int CardCornerRadius = 10;

    // ════════════════════════════════════════════════════════════
    // HEALTH BAR / UNIT STATS
    // ════════════════════════════════════════════════════════════
    public static readonly Color HealthGreen = new Color(0.18f, 0.68f, 0.32f, 1f);
    public static readonly Color ManaBlue = ArcaneBlue;
    public static readonly Color ArmorGrey = new Color(0.52f, 0.52f, 0.58f, 1f);
    public static readonly Color ShieldPurple = new Color(0.45f, 0.20f, 0.80f, 0.80f);
    public const float HealthBarWidth = 1.6f;

    // ════════════════════════════════════════════════════════════
    // TILE HIGHLIGHTS (combat hex grid — on dark 3D world)
    // ════════════════════════════════════════════════════════════
    public static readonly Color TileHover = new Color(1.0f, 0.92f, 0.50f, 1f);
    public static readonly Color TileDragHover = new Color(0.90f, 0.80f, 0.25f, 1f);
    public static readonly Color TileMoveHighlight = new Color(0.30f, 0.65f, 1.00f, 1f);
    public static readonly Color TileDeployHighlight = new Color(0.30f, 1.00f, 0.40f, 1f);
    public static readonly Color TileTargetHighlight = new Color(1.00f, 0.38f, 0.38f, 1f);
    public static readonly Color TileRangeInterior = new Color(0.70f, 0.48f, 1.00f, 0.70f);
    public static readonly Color TileRangeBorder = new Color(0.55f, 0.38f, 0.90f, 0.25f);
    public static readonly Color TileGlyph = new Color(0.65f, 0.25f, 1.00f, 1f);

    // ════════════════════════════════════════════════════════════
    // COMBAT TILE BASE COLORS (dark world — unchanged)
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
    // ELEMENT COLORS
    // ════════════════════════════════════════════════════════════
    public static readonly Color ElementFire = new Color(0.88f, 0.32f, 0.05f, 1f);
    public static readonly Color ElementIce = new Color(0.35f, 0.68f, 0.90f, 1f);
    public static readonly Color ElementStorm = new Color(0.72f, 0.65f, 0.08f, 1f);
    public static readonly Color ElementEarth = new Color(0.52f, 0.38f, 0.15f, 1f);

    // ── Imbuement overlay tints (on dark tiles — unchanged) ──────
    public static readonly Color ElementTintFire = new Color(1.00f, 0.42f, 0.08f, 1f);
    public static readonly Color ElementTintFrost = new Color(0.60f, 0.88f, 1.00f, 1f);
    public static readonly Color ElementTintLightning = new Color(0.92f, 0.85f, 1.00f, 1f);
    public static readonly Color ElementTintEarth = new Color(0.72f, 0.52f, 0.22f, 1f);
    public static readonly Color ElementTintWater = new Color(0.28f, 0.58f, 0.95f, 1f);
    public static readonly Color ElementTintAir = new Color(0.82f, 0.95f, 0.85f, 1f);
    public static readonly Color ElementTintArcane = new Color(0.80f, 0.38f, 1.00f, 1f);
    public static readonly Color ElementTintShadow = new Color(0.45f, 0.18f, 0.52f, 1f);

    // ════════════════════════════════════════════════════════════
    // COMBAT UI (parchment panel over dark world)
    // ════════════════════════════════════════════════════════════
    public static readonly Color EnemyHealthBar = Danger;
    public static readonly Color UnitBarSelected = new Color(0.42f, 0.22f, 0.68f, 0.20f); // violet tint on parchment
    public static readonly Color UnitBarBorder = Violet;
    public static readonly Color StatBarHealth = HealthGreen;
    public static readonly Color StatBarMove = new Color(0.72f, 0.55f, 0.08f, 1f);
    public static readonly Color StatBarMana = ArcaneBlue;

    public const int CombatStatLabelFontSize = 10;
    public const int EnemyRosterButtonWidth = 90;
    public const int EnemyRosterBarWidth = 80;
    public const int EnemyRosterBarHeight = 14;
    public const int UnitBarStatBarWidth = 80;
    public const int UnitBarStatBarHeight = 8;
    public const int MaxActionLogLines = 6;

    // ════════════════════════════════════════════════════════════
    // OVERWORLD (2D map — dark world, dark HUD labels)
    // ════════════════════════════════════════════════════════════

    // ── Terrain (dark world — unchanged) ─────────────────────────
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
    public static readonly Color FogHidden = new Color(0.08f, 0.06f, 0.12f, 0.90f);
    public static readonly Color FogSilhouette = new Color(0.08f, 0.06f, 0.12f, 0.48f);
    public static readonly Color FogRevealed = new Color(0f, 0f, 0f, 0f);

    // ── POI markers ──────────────────────────────────────────────
    public static readonly Color POICombat = new Color(0.90f, 0.22f, 0.22f, 1f);
    public static readonly Color POIRest = new Color(0.25f, 0.78f, 0.90f, 1f);
    public static readonly Color POIObjective = new Color(1.00f, 0.85f, 0.20f, 1f);
    public static readonly Color POINarrative = new Color(0.70f, 0.40f, 1.00f, 1f);
    public static readonly Color POINegotiation = new Color(0.20f, 0.80f, 0.58f, 1f);

    // ── Hex border ───────────────────────────────────────────────
    public static readonly Color HexBorderColor = new Color(0.18f, 0.14f, 0.22f, 0.70f);
    public const float HexBorderWidth = 1.5f;

    // ── Party token ──────────────────────────────────────────────
    public static readonly Color PartyTokenFill = new Color(1.00f, 0.85f, 0.20f, 1f); // bright gold on dark map
    public static readonly Color PartyTokenOutline = new Color(0.25f, 0.18f, 0.05f, 1f);
    public const float PartyTokenRadius = 14f;
    public const float PartyTokenOutlineRadius = 17f;
    public const int PartyTokenSegments = 12;

    // ── Move highlights ───────────────────────────────────────────
    public static readonly Color MoveHighlightCheap = new Color(0.38f, 0.90f, 0.45f, 0.30f);
    public static readonly Color MoveHighlightModerate = new Color(0.90f, 0.80f, 0.25f, 0.30f);
    public static readonly Color MoveHighlightExpensive = new Color(0.90f, 0.40f, 0.25f, 0.30f);
    public static readonly Color MoveHighlightDefault = new Color(0.70f, 0.55f, 0.90f, 0.30f);

    // ── Overworld HUD (dark backing, light text) ──────────────────
    public static readonly Color OverworldHudBg = new Color(0.08f, 0.06f, 0.12f, 0.82f);
    public static readonly Color OverworldHudBorder = new Color(0.30f, 0.20f, 0.45f, 0.80f);
    public static readonly Color OverworldLowResourceWarning = new Color(0.90f, 0.38f, 0.38f, 1f);
    public static readonly Color OverworldInfoLabelTint = new Color(1.00f, 0.88f, 0.50f, 1f);

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
    public static readonly Color SummonColorFriendly = new Color(0.25f, 0.55f, 0.90f, 1f);
    public static readonly Color SummonColorEnemy = new Color(0.88f, 0.22f, 0.22f, 1f);

    // ════════════════════════════════════════════════════════════
    // NARRATIVE ENCOUNTER PANEL (parchment modal)
    // ════════════════════════════════════════════════════════════
    public static readonly Color NarrativeBackdrop = BgOverlay;
    public static readonly Color NarrativePanelBg = BgBase;
    public static readonly Color NarrativePanelBorder = Violet;
    public static readonly Color NarrativeResultBg = BgRaised;
    public static readonly Color NarrativeResultBorder = VioletDim;
    public static readonly Color NarrativeTitleColor = Gold;
    public static readonly Color NarrativeBodyColor = TextPrimary;
    public static readonly Color NarrativeResultColor = new Color(0.28f, 0.18f, 0.08f, 1f); // warm dark brown

    public const int NarrativeTitleFontSize = 22;
    public const int NarrativeBodyFontSize = 16;
    public const int NarrativeResultFontSize = 15;
    public const int NarrativeChoiceFontSize = 15;
    public const int NarrativePanelCorner = 8;
    public const int NarrativeResultCorner = 4;

    // ════════════════════════════════════════════════════════════
    // NEGOTIATION PANEL (parchment)
    // ════════════════════════════════════════════════════════════
    public static readonly Color NegotiationBg = BgDeep;
    public static readonly Color NegotiationResultBg = BgRaised;
    public static readonly Color NegotiationResultBorder = Violet;
    public static readonly Color NegotiationTitleColor = Gold;
    public static readonly Color NegotiationNpcColor = TextSecondary;
    public static readonly Color NegotiationBodyColor = TextPrimary;
    public static readonly Color NegotiationHiddenTerm = TextDim;

    // ── Tension bar ───────────────────────────────────────────────
    public static readonly Color TensionEmpty = NeutralDim;
    public static readonly Color TensionCordial = new Color(0.15f, 0.58f, 0.28f, 1f);
    public static readonly Color TensionStrained = Warning;
    public static readonly Color TensionHostile = Danger;

    // ── Zone labels ───────────────────────────────────────────────
    public static readonly Color ZoneCordialLabel = new Color(0.12f, 0.52f, 0.22f, 1f);
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
    // CAMPUS SCREEN (parchment)
    // ════════════════════════════════════════════════════════════
    public static readonly Color CampusBg = WorldBase;
    public static readonly Color CampusTitleBarBg = new Color(0.10f, 0.08f, 0.14f, 1f); // slightly lighter than world
    public static readonly Color CampusTitleBarBorder = new Color(0.42f, 0.22f, 0.68f, 1f); // = Violet

    public static readonly Color CampusTitleColor = Gold;          // already gold, fine
    public static readonly Color CampusSectionColor = Gold;          // already gold, fine
    public static readonly Color CampusSubtleText = TextOnDark;    // was TextSecondary
    public static readonly Color CampusStubText = new Color(0.60f, 0.55f, 0.68f, 1f); // light dim on dark
    public static readonly Color CampusSchoolDescText = TextOnDark;  // was TextSecondary
    public static readonly Color CampusNoRunText = new Color(0.55f, 0.50f, 0.62f, 1f);
    public static readonly Color CampusRunSuccess = Success;
    public static readonly Color CampusRunFail = Danger;
    public static readonly Color CampusSlotSelected = new Color(0.80f, 0.92f, 0.80f, 1f); // light green tint

    // ── Debug panel ───────────────────────────────────────────────
    public static readonly Color DebugPanelBg = new Color(0.88f, 0.75f, 0.72f, 1f); // pink-parchment
    public static readonly Color DebugPanelBorder = new Color(0.65f, 0.18f, 0.18f, 1f);

    // ── Companion card ────────────────────────────────────────────
    public static readonly Color CompanionCardBg = SurfaceLight;
    public static readonly Color CompanionCardBorderActive = Success;
    public static readonly Color CompanionCardBorderInactive = Neutral;
    public static readonly Color CompanionSubText = TextSecondary;

    // ── Building card ─────────────────────────────────────────────
    public static readonly Color BuildingCardBg = SurfaceLight;
    public static readonly Color BuildingCardBorderBuilt = Violet;
    public static readonly Color BuildingCardBorderEmpty = Neutral;
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
    public static readonly Color LibraryTabPressed = new Color(0.65f, 0.55f, 0.80f, 1f); // violet-tinted parchment
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
    public const int BorderWidth = 2;
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
