# Fractured Arcana

A tactical roguelite card game built in Godot 4 / C#. You play as a wizard leading a guild, exploring fog-of-war hex maps, resolving encounters through tactical card combat or negotiation, and building out a persistent campus between runs. Each wizard school has a distinct combat identity, exploration passive, and negotiation style.

**Current development phase: Phase 3 — Horizontal Content**
One school (Elementalist) is fully playable. Core systems (save/load, overworld, campus, negotiation skeleton, companion framework) are in place. Phase 3 widens content to all six schools, multiple regions, and full campus buildings.

---

## Table of Contents

1. [Prerequisites & Setup](#1-prerequisites--setup)
2. [Project Structure](#2-project-structure)
3. [Architecture Overview](#3-architecture-overview)
4. [How to Add Content](#4-how-to-add-content)
   - [Adding a Card](#41-adding-a-card)
   - [Adding a Region](#42-adding-a-region)
   - [Adding a Narrative Encounter](#43-adding-a-narrative-encounter)
   - [Adding a Building](#44-adding-a-building)
   - [Adding a Companion](#45-adding-a-companion)
5. [Systems Reference](#5-systems-reference)
6. [Known Gotchas & Recurring Bugs](#6-known-gotchas--recurring-bugs)
7. [Godot 4.6 Compatibility Rules](#7-godot-46-compatibility-rules)
8. [Git Conventions](#8-git-conventions)

---

## 1. Prerequisites & Setup

### Requirements

- **Godot 4.6.1** (exact version — do not use 4.5 or any 4.7 build; compat mode rules are version-specific)
- **.NET 8 SDK** (not .NET 6 or .NET 9)
- A C# IDE: Rider or VS Code with the C# Dev Kit extension

### Opening the Project

1. Clone the repo.
2. Open **Godot 4.6.1**.
3. Click **Import** and select the repo root folder (the one containing `project.godot`).
4. Do **not** open the `.sln` in Godot. Use the `.sln` only from your IDE for code editing.
5. On first open, Godot will build the C# project. If it fails, verify your .NET 8 SDK is on `PATH`.

### Building & Running

- Press **F5** in Godot to run, or use the play button in the top-right toolbar.
- The entry scene is `res://Scenes/Campus/CampusScene.tscn`.
- Save data is stored in `user://saves/` (Godot's user data folder, not the repo).

---

## 2. Project Structure

```
FracturedArcana/
├── project.godot
├── FracturedArcana.csproj
├── FracturedArcana.sln
│
├── Data/                          # JSON content files (no code here)
│   ├── Cards/                     # One .json file per card
│   ├── Regions/                   # One .json file per region
│   ├── Encounters/                # Narrative encounter pools per region
│   ├── Buildings/                 # One .json file per campus building
│   └── Companions/                # One .json file per companion (Phase 3+)
│
├── Scenes/                        # Godot .tscn scene files
│   ├── Campus/                    # CampusScene.tscn
│   ├── Combat/                    # Main combat scene
│   ├── Negotiation/               # NegotiationScene.tscn
│   ├── Cards/                     # CardUI.tscn (card visual prefab)
│   └── UI/                        # CardLibrary.tscn, etc.
│
└── Scripts/                       # All C# source files
    ├── Cards/                     # CardDatabase.cs, card data models
    ├── Data/                      # Save/load, data models, loaders
    │   ├── GuildSaveData.cs       # Central save data structure
    │   ├── SaveManager.cs         # JSON serialization, slot management
    │   ├── RegionDefinition.cs    # Region data model
    │   ├── RegionLoader.cs        # Loads Data/Regions/*.json
    │   ├── NarrativeEncounterLoader.cs
    │   ├── BuildingDatabase.cs
    │   └── Companion.cs
    ├── Models/                    # Shared data-model classes (no Godot deps)
    ├── Overworld/                 # CampusScreen.cs, overworld systems
    ├── Systems/
    │   ├── Combat/                # GameRunner, HexGridManager, ElementalAttunement
    │   └── Negotiation/           # NegotiationManager
    ├── UI/                        # CardLibraryUi, SchoolAttunementUI, etc.
    └── Loaders/                   # JsonCardLoader, CardScriptRegistry
```

> **Data vs Scripts/Data**: `Data/` contains JSON files authored as game content. `Scripts/Data/` contains C# classes that load and represent that data at runtime. If you're adding content, you're in `Data/`. If you're changing data structures, you're in `Scripts/Data/`.

---

## 3. Architecture Overview

The game has four layers that flow top-to-bottom. Understanding this prevents you from accidentally bypassing a layer.

```
┌─────────────────────────────────────────────────────┐
│  DATA LAYER                                         │
│  Data/Cards/*.json  Data/Regions/*.json  etc.       │
│  Plain JSON. No code. Authorable without building.  │
└───────────────┬─────────────────────────────────────┘
                │ loaded by
┌───────────────▼─────────────────────────────────────┐
│  REGISTRY / LOADER LAYER                            │
│  CardScriptRegistry  JsonCardLoader  RegionLoader   │
│  NarrativeEncounterLoader  BuildingDatabase         │
│  Parses JSON → C# objects. Caches results.          │
│  Effects, targeters, predicates registered here.    │
└───────────────┬─────────────────────────────────────┘
                │ populates
┌───────────────▼─────────────────────────────────────┐
│  RUNTIME LAYER                                      │
│  GameState  GameRunner  HexGridManager              │
│  DeckManager  UnitDeckData  ElementalAttunement     │
│  GuildSaveData  SaveManager                         │
│  Live game state. No JSON parsing here.             │
└───────────────┬─────────────────────────────────────┘
                │ drives
┌───────────────▼─────────────────────────────────────┐
│  UI LAYER                                           │
│  CombatUI  SchoolAttunementUI  CardLibraryUi        │
│  DeckUiManager  CampusScreen  NegotiationManager    │
│  Reads runtime state. Never touches JSON directly.  │
└─────────────────────────────────────────────────────┘
```

### Key Systems at a Glance

| System | Class(es) | What it does |
|--------|-----------|--------------|
| **Card loading** | `JsonCardLoader`, `CardScriptRegistry` | Reads card JSON, compiles effect chains via registry pattern |
| **Card effects** | `IEffect`, `SequenceEffect`, `ConditionalEffect`, etc. | Leaf and composite effect tree evaluated at cast time |
| **Targeting** | `ITargetSelector` + implementations | Range, cone, line, ring, AoE, element-tile selectors |
| **Combat loop** | `GameRunner` | Turn management, unit selection, card play, win/lose |
| **Hex grid** | `HexGridManager` | Map generation, terrain, tile data, pathfinding |
| **School mechanic** | `ISchoolAttunement`, `ElementalAttunement` | Per-school class mechanic (charges, thresholds, burst) |
| **Deck management** | `DeckManager`, `UnitDeckData` | Per-unit draw pile/hand/discard; swaps on unit select |
| **Persistence** | `GuildSaveData`, `SaveManager` | 3-slot JSON save with versioning + migration |
| **Overworld** | `OverworldHexGrid`, `FogOfWarManager`, `POIGenerator` | Fog-of-war exploration, step budget, POI placement |
| **Negotiation** | `NegotiationManager` | Standalone scene; tension meter, 8 token types |
| **Campus** | `CampusScreen`, `BuildingDatabase` | Building construction/upgrade, school building unlocks |

---

## 4. How to Add Content

For most content types, no code changes are required — you author a JSON file and the loader picks it up automatically.

---

### 4.1 Adding a Card

Create a new file in `Data/Cards/`. Filename should be `schoolname_cardname.json` (all lowercase, underscores).

**Full card schema:**

```json
{
  "id": "elementalist_example_card",
  "name": "Example Card",
  "school": "Elementalist",
  "rarity": "Common",
  "top": {
    "name": "Top Half Name",
    "mana": 2,
    "speed": "Sorcery",
    "rules_text": "Deal 4 damage to target.",
    "tags": ["fire"],
    "targeting": { "type": "range", "range": 3 },
    "effect": { "type": "damage", "amount": 4 },
    "requires": ["fire_tile"],
    "channel": {
      "name": "Top Half Name (Channel)",
      "mana": 2,
      "speed": "Reaction",
      "rules_text": "Channel: Deal 6 damage instead.",
      "targeting": { "type": "range", "range": 3 },
      "effect": { "type": "damage", "amount": 6 }
    }
  },
  "bottom": {
    "name": "Bottom Half Name",
    "mana": 1,
    "speed": "Instant",
    "rules_text": "Gain 2 armor.",
    "tags": [],
    "targeting": { "type": "self" },
    "effect": { "type": "armor", "amount": 2 }
  }
}
```

**Field reference:**

| Field | Type | Notes |
|-------|------|-------|
| `id` | string | Unique. Used for deck references. |
| `school` | string | Must match `CardSchool` enum exactly: `Elementalist`, `Arcanist`, `Necromancer`, `Enchanter`, `Artificer`, `Chronomancer`, `Tinker` |
| `rarity` | string | `Common`, `Uncommon`, `Rare`, `Legendary` |
| `mana` | int | Mana cost of this half |
| `speed` | string | `Sorcery`, `Instant`, `Reaction` |
| `tags` | string[] | Element tags for attunement: `"fire"`, `"ice"`, `"storm"`, `"stone"` |
| `requires` | string[] | Pre-cast validation: `"fire_tile"`, `"ice_tile"`, `"stone_tile"`, `"empty_tile"` |
| `channel` | object | Optional. The channeled version of this half (different effect, same structure). |

**Common targeting types:**

| Type | Parameters | Description |
|------|-----------|-------------|
| `self` | — | Caster only |
| `range` | `"range": n` | Single target within n tiles |
| `aoe` | `"radius": n` | All units within radius |
| `line` | `"length": n` | All tiles in a line from caster |
| `cone` | `"length": n` | Cone in a direction |
| `ring` | `"radius": n` | Tiles at exactly radius distance |
| `adjacent` | — | All adjacent tiles |
| `nearest_to_target` | `"range": n` | Selects the closest unit to the primary target |
| `element_tile` | `"element": "fire"`, `"range": n` | Selects an imbued tile of a given element |

**Common effect types:**

| Type | Key parameters | Notes |
|------|---------------|-------|
| `damage` | `"amount": n` | Direct damage |
| `heal` | `"amount": n` | Restore HP |
| `armor` | `"amount": n` | Grant armor |
| `shield` | `"amount": n` | Grant shield |
| `self_damage` | `"amount": n` | Damage the caster |
| `mana_gain` | `"amount": n` | Restore mana |
| `apply_status` | `"status": "burn"/"slow"/"stun"/"root"` | Apply a status effect |
| `imbue_tile` | `"element": "fire"` | Imbue the target tile |
| `push` | `"tiles": n`, `"collision_damage": m` | Push target away |
| `push_damage` | `"tiles": n`, `"damage_per_tile": m` | Push + damage per tile moved |
| `sequence` | `"steps": [...]` | Execute multiple effects in order |
| `conditional` | `"if": {predicate}`, `"then": {effect}`, `"else": {effect}` | Branch on a condition |
| `retarget` | `"selector": {targeter}`, `"effect": {effect}` | Switch target mid-sequence |
| `summon` | `"unit_id": "..."` | Summon a unit (via GameState delegate) |

For complex multi-step effects, combine `sequence` + `retarget`. See existing Elementalist cards for examples.

---

### 4.2 Adding a Region

Create a file in `Data/Regions/regionid.json`. The `id` field must match the filename.

```json
{
  "id": "haunted_ruins",
  "displayName": "Haunted Ruins",
  "description": "Ancient walls saturated with necromantic residue.",
  "schoolAffinity": "Necromancer",
  "atmosphere": "Hostile",
  "gridWidth": 15,
  "gridHeight": 15,
  "stepBudget": 20,
  "combatPOICount": 10,
  "restPOICount": 3,
  "narrativePOICount": 5,
  "negotiationPOICount": 0,
  "hasRiver": false,
  "hasMountainRange": true,
  "hasRoads": false,
  "riverCrossingCount": 0,
  "enemyDifficultyMult": 1.2,
  "goldRewardMult": 1.1,
  "biomes": [
    {
      "name": "Central Ruins",
      "centerQ": 7,
      "centerR": 7,
      "radius": 4,
      "primaryTerrain": "Ruins",
      "secondaryTerrain": "Grassland"
    }
  ]
}
```

Biomes are optional — omitting `biomes` uses the default layout. `schoolAffinity` is currently flavor; it will gate school bonuses in Phase 3.

---

### 4.3 Adding a Narrative Encounter

Narrative encounters live in `Data/Encounters/`. Files are named `{regionid}_encounters.json` for region-specific pools, or `generic_encounters.json` for the shared fallback pool that loads in every region.

```json
[
  {
    "id": "ruins_echo_01",
    "title": "The Whispering Wall",
    "terrain": "Ruins",
    "isOneShot": true,
    "flavorText": "A crumbling wall bears faint inscriptions. As {wizard_name} approaches, the markings begin to glow.",
    "choices": [
      {
        "id": "study",
        "label": "Study the inscriptions",
        "outcome": "You decipher a fragment of an old ward. Gain 1 lore entry.",
        "effects": [{ "type": "add_lore", "entryId": "old_ward_fragment" }]
      },
      {
        "id": "destroy",
        "label": "Shatter the wall",
        "outcome": "The stone crumbles, revealing a hidden cache.",
        "effects": [{ "type": "gold", "amount": 30 }]
      },
      {
        "id": "leave",
        "label": "Leave it alone",
        "outcome": "Some things are better undisturbed.",
        "effects": []
      }
    ]
  }
]
```

Variable substitution in `flavorText`: `{wizard_name}`, `{companion_name}`, `{region}` are replaced at runtime.

---

### 4.4 Adding a Building

Create a file in `Data/Buildings/buildingid.json`.

```json
{
  "id": "training_grounds",
  "name": "Training Grounds",
  "category": "Core",
  "schoolAffinity": "",
  "description": "A yard where wizards and companions hone their skills between runs.",
  "tiers": [
    {
      "tier": 1,
      "displayName": "Basic Training Grounds",
      "goldCost": 80,
      "requirements": [],
      "passiveEffects": [{ "type": "run_start_buff", "stat": "mana", "amount": 1 }],
      "activeEffects": []
    },
    {
      "tier": 2,
      "displayName": "Advanced Training Grounds",
      "goldCost": 150,
      "requirements": ["training_grounds:1"],
      "passiveEffects": [{ "type": "run_start_buff", "stat": "mana", "amount": 2 }],
      "activeEffects": []
    }
  ]
}
```

`category` options: `Core`, `Magic`, `Economy`, `Reputation`, `School`. School buildings should set `schoolAffinity` to the matching school name and are automatically unlocked when that school's wizard or companion joins.

---

### 4.5 Adding a Companion

Create a file in `Data/Companions/companionid.json`.

```json
{
  "id": "elara_stormcaller",
  "name": "Elara Stormcaller",
  "school": "Elementalist",
  "personalityTrait": "Reckless",
  "backstory": "A former guild enforcer who burned her charter after a mission gone wrong.",
  "recruitmentCost": 120,
  "unlockCondition": "Complete 3 runs in the Frontier Wilds",
  "contributedCardIds": [
    "elementalist_chain_lightning",
    "elementalist_frost_nova",
    "elementalist_mana_surge"
  ]
}
```

`contributedCardIds` must match `id` fields of card JSON files. These cards are added to the wizard's deck during combat when this companion is in the active party.

---

## 5. Systems Reference

### Elemental Attunement (Elementalist)

The Elementalist tracks four elemental counters: **Fire, Ice, Storm, Earth**. Opposition pairs are Fire/Ice and Storm/Earth — casting one reduces the opposing counter by 1. All counters decay by 1 at the start of each turn (minimum 0).

| Charges | Tier | Effect |
|---------|------|--------|
| 1 | Minor | +1 bonus damage on spells of that element |
| 2 | Imbue | Auto-imbue the target tile with that element |
| 3 | Enhanced | Enhanced effect: Fire=Burn, Ice=Slow, Storm=Chain, Earth=Armor |
| 4 | Burst | Big AoE, then counter resets to 0 |

Attunement is read from the card's `tags` field at cast time. A card tagged `["fire", "ice"]` increments both counters.

### Save System

Three save slots. Saves automatically after every run and every campus change. Schema versioning is in `SaveManager.cs` — add a migration case there when you change `GuildSaveData`.

Save files are stored at `user://saves/slot_0.json` through `slot_2.json`. On Mac: `~/Library/Application Support/Godot/app_userdata/FracturedArcana/saves/`. On Windows: `%APPDATA%/Godot/app_userdata/FracturedArcana/saves/`.

### Per-Unit Deck Management

Each unit owns a `UnitDeckData` (draw pile, hand, discard). `DeckManager` acts as a view controller; calling `SetActiveDeck()` swaps the visible hand to the selected unit's deck. This is triggered automatically in `GameRunner` when a player unit is selected.

### Persistent Effects

Effects that last across turns (e.g., `MaelstromEffect`, `AvatarAuraEffect`) are stored in `GameState.ActiveEffects` and ticked each turn. Do not store persistent state inside the effect object itself; hang it off `GameState`.

### Summons

Summon effects fire a `GameState.OnSummonRequested` delegate rather than accessing the scene tree directly. `GameRunner` subscribes to this delegate and handles the actual instantiation. If you're writing a new summon effect, follow the same delegate pattern — never call `AddChild` from inside an effect class.

---

## 6. Known Gotchas & Recurring Bugs

These have caused real bugs before. Read them once and remember them.

### JSON Property Names

The JSON loaders use `JsonNamingPolicy.CamelCase`. Your JSON keys must be camelCase equivalents of the C# field names.

| ❌ Wrong | ✅ Correct | Where it bites |
|---------|-----------|----------------|
| `"targetting"` | `"targeting"` | Card JSON |
| `"amount"` | `"tiles"` | Push effects — the field is `tiles`, not `amount` |
| `"Elementalist"` | `"elementalist"` | Effect type keys are lowercased in the registry |
| `"Stone"` | `"stone"` | Element tags are lowercase strings |
| `"rules_text"` | — | This one uses snake_case and is matched explicitly — keep it |

When a card silently fails to load or an effect does nothing, your first check should be a JSON property name typo.

### Missing `using System;`

Any C# file that uses `Action<T>`, `Dictionary<,>`, `Math`, `Func<>`, or `Enum` needs `using System;` at the top. Godot's template files don't always include it. The error manifests as a cryptic type-not-found compile error, not a hint about the missing using.

### Effect Types Must Be Registered

If you add a new effect class, it does nothing until you register it in `CardScriptRegistry.RegisterBuiltins()` in `JsonCardLoader.cs`. The loader will print `[CardLoader] Unknown effect type 'your_type'. Using EmptyEffect.` if you forget.

### Card School Must Match Enum Exactly

The `school` field in card JSON is parsed against the `CardSchool` enum. Valid values: `Elementalist`, `Arcanist`, `Necromancer`, `Enchanter`, `Artificer`, `Chronomancer`, `Tinker`. Case-sensitive. A mismatch silently defaults to `Tinker`.

---

## 7. Godot 4.6 Compatibility Rules

These rules exist specifically for Godot 4.6.1 Mac + Windows cross-platform compatibility. Violating them causes crashes on one platform that don't appear on the other.

| Rule | What to do | What NOT to do |
|------|-----------|----------------|
| **Code-built UIs** | `CallDeferred(nameof(BuildUI))` from `_Ready()` | Build child nodes directly inside `_Ready()` |
| **Root Control anchors** | Set anchor presets in the `.tscn` editor | Set `AnchorRight`/`AnchorBottom` in C# code |
| **ScrollContainer children** | Use manual button + `Visible` tabs | Don't use `ScrollContainer` as a direct `TabContainer` child |
| **MarginContainer in ScrollContainer** | Set `SizeFlagsVertical = ShrinkBegin` | Default `Fill` sizing (causes layout collapse) |
| **Cameras** | `CallDeferred("make_current")` | Call `MakeCurrent()` directly in `_Ready()` |
| **Adding to scene root** | `CallDeferred("add_child", node)` | Call `AddChild()` on root directly |

---

## 8. Git Conventions

### Branching

- `main` is always playable. Every commit on `main` should leave the game in a runnable state.
- Feature branches for anything non-trivial: `feature/necromancer-school`, `fix/attunement-decay-bug`, `data/frontier-wilds-encounters`.
- Merge via pull request so both contributors are aware of changes before they land on `main`.

### Commit Message Prefixes

```
[feat]     New gameplay feature or system
[fix]      Bug fix
[data]     JSON content only (cards, regions, encounters, buildings)
[refactor] Code restructure, no behavior change
[ui]       UI-only change
[docs]     README, comments, documentation
[chore]    Build config, .gitignore, project settings
```

Examples:
```
[data] Add Necromancer starter deck (12 cards)
[feat] Implement FogOfWarManager with 3 hex states
[fix] Attunement counter not decaying on enemy turn
[refactor] Flatten Scripts/Scripting/Loader to Scripts/Loaders
```

### Ownership

For Phase 3 development, parallel work is safest split by layer:

- **Code/systems work** (new school mechanics, UI, new effect types) → conflict risk is low if you're in different script files
- **Content work** (JSON card files, region files, encounter pools) → almost zero conflict risk; these are independent files

If you're both touching the same C# file, communicate before starting. `GameRunner.cs` and `JsonCardLoader.cs` are the highest-traffic files.
