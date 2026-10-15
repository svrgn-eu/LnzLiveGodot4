# Feature: Entity Tags

## 📖 Overview
The "Tag" feature introduces a flexible, modular system for assigning traits, conditions, or properties to any game entity (characters, NPCs, items). A Tag acts as a data object that holds a description and defines a specific mechanical effect (e.g., altering stats, applying buffs/debuffs, or changing behaviors). This system allows for highly dynamic interactions without hardcoding specific states into every entity class.

## 🎯 Goals & Use Cases
- **Goal 1:** Provide a unified, scalable system for applying effects, traits, and states to entities.
- **Goal 2:** Decouple specific mechanical effects (like "Poisoned", "Heavy", "Flammable") from the core entity logic to make the game data-driven.
- **Use Case A:** A character walks into a fire hazard. They receive a "Burning" tag which applies damage over time (an alteration effect).
- **Use Case B:** A player equips a weapon that has a "Lightweight" tag. The tag passively alters the character's base movement speed and is marked as a Positive effect.
- **Use Case C:** An NPC has a "Cowardly" tag that modifies their AI behavior tree to prioritize fleeing when health is low.
- **Use Case D:** A character is generated with an "Origin" group type. They are assigned a tag with the caption "Veteran Fighter" from this group, applying unique combat modifiers.

## 📝 Examples of Tag Groups

Here are a few examples of how different Group Types can contain context-specific Captions and Effects:

### Group Type: "Origin"
Tags that represent a character's backstory or defining innate traits.
- **Caption:** "Veteran Fighter" 
  - *Polarity:* Positive
  - *Description:* Hardened by years of combat.
  - *Effect:* +10% max health, +5% accuracy.
- **Caption:** "Easy to Manipulate"
  - *Polarity:* Negative
  - *Description:* Gullible and easily swayed by others.
  - *Effect:* -15% resistance to mental attacks/debuffs.

### Group Type: "Status Effect"
Temporary conditions applied during combat or exploration.
- **Caption:** "Burning"
  - *Polarity:* Negative
  - *Description:* Engulfed in flames.
  - *Effect:* Takes 5 damage per turn, illuminates the surrounding area.
- **Caption:** "Adrenaline Rush"
  - *Polarity:* Positive
  - *Description:* Pumped up and ready to sprint.
  - *Effect:* +2 Movement Range for 1 turn.

### Group Type: "Equipment Trait"
Inherent properties of items, armor, or weapons.
- **Caption:** "Lightweight"
  - *Polarity:* Positive
  - *Description:* Exceptionally easy to carry and maneuver.
  - *Effect:* +1 Movement Speed when equipped.
- **Caption:** "Cumbersome"
  - *Polarity:* Negative
  - *Description:* Heavy, awkward, and restrictive.
  - *Effect:* -1 Action Point per turn.

## 👥 Target Audience / User Roles
- **Players:** Experience deep, systemic gameplay where tags interact with each other and the environment.
- **Designers:** Can easily create new mechanics and status effects by composing tags in the Godot Editor without needing to write new C# classes for every state.

## ⚙️ Functional Requirements
- [ ] Requirement 1: Any derived `IsoEntity` (characters, NPCs) **and any `Item`** must have a unified way to store and manage active tags (e.g., a plain C# `TagContainer` class). Note: `Item` is a `Resource`, not an `IsoEntity`, so the container must not depend on `IsoEntity`.
- [ ] Requirement 2: A Tag must be defined as an object containing at least a **Caption** (e.g., "Veteran Fighter"), a **Group Type** (e.g., "Origin"), a **Description**, and an **Effect** (the logic that alters values).
- [ ] Requirement 3: The system must support the dynamic addition and removal of tags during gameplay.
- [ ] Requirement 4: Tag effects must be able to alter entity values cleanly (e.g., temporarily modifying `MovementSpeed` without permanently destroying the base value).
- [ ] Requirement 5: Tags must be assignable to **Groups** by defining a **Type** (e.g., "Origin") to give them semantic meaning, while their specific identity is defined by a **Caption** (e.g., "Veteran Fighter", "hard to kill", or "easy to manipulate").
- [ ] Requirement 6: Tags must have a polarity/alignment property (e.g., Positive, Negative, Neutral) to clearly mark whether their effect is a buff or a debuff.
- [] Requirement 7: Tags must be persistable. The game must be shipped with a Tag database which is adjustable without altering the game files, I need to be able to extend this database using the Feature "Extensibility-DataDrivenDesign.md". Also, tags assigned to characters need to be persisted with that character using persistance keys, see feature "PersistenceKeys-PlayerLevel.md".
- [] Requirement 8: Tags must be shown in th eUI in character info screens or when hovering a different character in the tactical view

## 🎨 UI/UX Design & Interactions
- **Visuals:** Tags on a character should be visible in the Tactical HUD or Character Screen, represented by small icons. Their polarity should be visually distinct (e.g., Positive tags have a green tint/up-arrow, Negative tags have a red tint/down-arrow).
- **Interactions:** Hovering over a tag icon in the UI displays a tooltip showing the tag's Caption (e.g., "Veteran Fighter"), its Group Type (e.g., "Origin"), its Description, its Polarity (Positive/Negative), and what it is currently altering.

## 💻 Technical Details (Proposals for Review)
Here are three proposed architectures for how Tags can alter and influence values, evaluated against the current codebase and Requirements 4, 7 and 8:

### Current Codebase Constraints
- **Stats live in data resources, not entities.** `CharacterData` and `AnimalData` are separate `Resource` classes with no shared base. `IsoEntity` holds no stats.
- **Stats are read directly.** `MovementSpeed` is read straight from `Data` in `IsoCharacter._PhysicsProcess`, `IsoAnimal` and `ScrCharacterScreen`. There is no "effective value" lookup yet, so Req 4 requires rerouting these call sites.
- **`Item` is a `Resource`**, not an `IsoEntity`.
- **Persistence:** Per [Save_System](Save_System.md) and [PersistenceKeys-PlayerLevel](PersistenceKeys-PlayerLevel.md), core state (stats, inventory) belongs in strongly-typed POCOs inside the `GameState` DTO, serialized with `System.Text.Json`. The key dictionary is meant for flags/counters.
- **Extensibility:** Per [Extensibility-DataDrivenDesign](Extensibility-DataDrivenDesign.md), content should be loadable from JSON (`res://Data/` for base game, `user://mods/` for mods) without recompilation. Code-bearing tags cannot be defined in a mod.

### Proposal A: Godot Resource / Strategy Pattern
- **Concept:** Create a base `EntityTag` C# class that inherits from `Godot.Resource`.
- **Structure:**
  - `string Caption` (e.g., "Veteran Fighter")
  - `string Description`
  - `string GroupType` (e.g., "Origin", "StatusEffect", "Equipment")
  - `TagAlignment Polarity` (Enum: Positive, Negative, Neutral)
  - `virtual void OnTagAdded(IsoEntity target)`
  - `virtual void OnTagRemoved(IsoEntity target)`
- **How it alters values:** `OnTagAdded` might subscribe to a damage calculation event or directly apply a buff. `OnTagRemoved` reverses the change. This integrates well with the Godot Inspector.
- **Drawbacks (updated):** Every new behavior needs a new C# subclass, so it cannot satisfy Req 7 (moddable DB without altering game files). Directly applying buffs risks permanently altering base values (violates Req 4). The `IsoEntity` parameter excludes items. Not recommended as the primary approach.

### Proposal B: Stat Modifiers (Data-Driven Approach)
- **Concept:** Tags contain no executable code. They are purely data definitions containing lists of modifiers (e.g., `StatType.MovementSpeed`, `ModifierType.Multiply`, `Value: 1.2`).
- **How it alters values:** Current values are calculated dynamically by iterating through all active tags in the `TagContainer` and applying the modifiers on the fly (e.g., `Tags.Resolve(StatType.MovementSpeed, Data.MovementSpeed)`). Base values are never touched, so removal always reverts precisely.
- **Fit:** Serializes trivially to JSON, is moddable via the Extensibility feature, and works for entities and items alike.
- **Drawback:** Cannot express behavioral tags (e.g., "Burning" damage over time, "Cowardly" AI) on its own.

### Proposal C: Component / Node-Based Tags
- **Concept:** Tags are actual `Node`s added as children to the entity in the scene tree.
- **How it alters values:** A `BurningTagNode` uses Godot's built-in `_Process(double delta)` to apply damage over time. This makes time-based or heavily animated tags easy to implement, but might clutter the scene tree if entities have dozens of tags.
- **Drawbacks (updated):** Nodes cannot be defined in JSON/mods and conflict with the "state in POCOs, not Nodes" rule of the Save System. Items are not in the scene tree. Not recommended.

### Recommendation: Hybrid of B with Optional Behavior Handlers
Use Proposal B as the foundation and add an optional `BehaviorId` string to the tag definition for tags that need logic (Burning, Cowardly).

- **`TagDefinition`** (plain C# class, loaded from the JSON Tag database):
  - `string Id` (unique, stable, used for persistence and mod overrides)
  - `string Caption`, `string GroupType`, `string Description`, `TagAlignment Polarity`
  - `List<StatModifier> Modifiers` (`StatType`, `ModifierType`, `Value`)
  - `string BehaviorId` (optional; resolved to a registered C# handler such as `IBurningTagBehavior`; unknown IDs are logged and ignored, not fatal)
  - `int DefaultDuration` (optional; 0 = permanent)
- **`TagInstance`** (runtime/persisted state): `string TagId`, `int RemainingDuration`, `int Stacks`, `string SourceId`. Required because status effects like "Adrenaline Rush" (1 turn) and "Burning" (per turn) need per-entity runtime state.
- **`TagContainer`** (plain C# class, owned by entities and items): add/remove/query tags, `Resolve(StatType, baseValue)`, and a `TagsChanged` event for the UI.
- **Open question:** The "turn" used for durations is not defined anywhere in `doc/`, and the current movement code runs on `_PhysicsProcess`. Decide whether durations are turns, ticks, or seconds before implementing `RemainingDuration`.

### Tag Database & Persistence (Req 7)
- **Tag database:** JSON files in `res://Data/Tags/` (base game), with mods adding or overriding tags from `user://mods/<ModName>/` through the `ModLoader` described in the Extensibility feature. Invalid entries are skipped and logged, following that feature's robustness requirement.
- **Character persistence:** Save `List<TagInstance>` (tag IDs only, not full definitions) as a strongly-typed property of the character in the `GameState` DTO. Do **not** store these in the persistence-key dictionary. That dictionary is meant for flags and counters, per its best practices.
- **Persistence keys usage:** Keys are appropriate for tag-related *events* (e.g., `"origin_tag_assigned_<characterId>" = true` so an Origin is only rolled once), accessed through `IPersistenceService`.
- **Load behavior:** If a saved tag ID no longer exists in the database (e.g., a mod was removed), skip it and log a warning instead of failing the load.

### Storage Format Recommendation: Plain JSON Files (not SQLite)
**Recommendation:** Store the tag database as plain JSON files, loaded once at startup into an in-memory `Dictionary<string, TagDefinition>` (owned by a `TagDatabaseService`, registered via DI). Do not use an SQLite instance.

| Criterion | Plain JSON files | SQLite |
|---|---|---|
| Edit without altering game files / extend via mods | ✅ Any text editor; drop a file into `user://mods/` | ❌ Needs a DB tool; mods would have to ship binary DB files or SQL scripts |
| Matches Extensibility feature (JSON, relative paths, `ModLoader`) | ✅ Same pipeline, no extra code path | ❌ Second, parallel loading mechanism |
| Merging base game + several mods | ✅ Load files in order, later ID overrides earlier | ⚠️ Needs custom merge/migration logic |
| Error reporting (line number, missing keys, per-mod log) | ✅ Parser reports exact position; bad entry is skipped | ⚠️ Whole-DB corruption or schema mismatch is harder to localize |
| Version control / diffing / review | ✅ Readable diffs | ❌ Binary blob |
| Dependencies | ✅ `System.Text.Json` is already the standard in this project | ❌ Adds a native library and Godot export/platform concerns (per-platform binaries) |
| Query power / scale | ⚠️ Linear scans, but a dictionary lookup by ID is enough | ✅ Overkill for hundreds of tags |

**Why not SQLite:** A tag database is small (likely hundreds of entries), read-only at runtime and fully held in memory after startup. SQLite's strengths (large datasets, complex queries, transactions) are not needed. Its costs (binary format, native dependencies, awkward modding) work directly against Req 7.

**Suggested layout:**
- Base game (shipped, read-only): `res://Data/Tags/*.json`, one file per group type (e.g., `origin.json`, `status_effects.json`, `equipment_traits.json`).
- Mods (user-editable, writable): `user://mods/<ModName>/tags/*.json`, referenced from the mod's entry point `<ModName>.json`.
- Override rule: tags are keyed by `Id`. A mod entry with an existing `Id` replaces the base definition; a new `Id` adds a tag. Prefix mod IDs with the mod name (e.g., `mymod:fireproof`) to avoid collisions.

**Example file (`origin.json`):**
```json
{
  "schemaVersion": 1,
  "tags": [
    {
      "id": "origin.veteran_fighter",
      "caption": "Veteran Fighter",
      "groupType": "Origin",
      "polarity": "Positive",
      "description": "Hardened by years of combat.",
      "modifiers": [
        { "stat": "MaxHealth", "type": "PercentAdd", "value": 0.10 },
        { "stat": "Accuracy", "type": "PercentAdd", "value": 0.05 }
      ]
    }
  ]
}
```
- Include a `schemaVersion` so the format can be migrated later, mirroring the save-versioning practice in the Persistence Keys feature.
- Icons are referenced by relative path (e.g., `"icon": "./icons/burning.png"`), resolved by the `ModLoader` as described in the Extensibility feature.
- **Note:** `Accuracy` is only an example. Only stats that exist on `CharacterData`/`AnimalData` (currently `Health`, `MaxHealth`, `MovementSpeed`) can be modified until further stats are added.

**Persistence boundary:** Savegames never contain tag definitions, only `TagInstance` data referencing tag IDs, so editing the JSON files (balancing) takes effect on the next load without invalidating saves. Keep tag IDs stable once released.

**When to revisit:** If the tag count grows into the tens of thousands or you need complex runtime queries, consider building an indexed in-memory structure first. Only then evaluate SQLite.

### UI Integration (Req 8)
- **Character screen:** `ScrCharacterScreen` currently uses plain `Label`s. Add a tag icon row (e.g., an `HFlowContainer`) populated from the selected character's `TagContainer`. This is shown independently of the hover feature flag.
- **Hover in tactical view:** Hover is not implemented yet. `IsoCharacter` and `IsoAnimal` only handle left-clicks in `OnInteractableAreaInputEvent`. Connect `MouseEntered` / `MouseExited` on the interactable `Area2D` (ideally in shared `IsoEntity` code to avoid duplicating the handler) and raise an event through `EventBus`. A tooltip UI component then displays the tags.
- **Tooltip content:** Caption, Group Type, Description, Polarity and the current modifiers, per the UI/UX section above.

### Implementation Steps (Feature Service Integration)
To ensure the behavior of showing tags on hover is controllable via the global feature toggle system, the following steps must be implemented:
1. **Extend Feature Configuration:** In `src/Core/Services/Implementations/FeatureConfig.cs`, add a new boolean flag inside a fitting `[ExportCategory]` (e.g., "Gameplay Mechanics"): `[Export] public bool ShowTagsOnHover { get; set; } = true;`. The default of `true` means `res://FeatureConfig.tres` only needs editing if you want to turn it off (the `.tres` stores only non-default values).
2. **Update the Interface:** Add `bool IsTagHoverEnabled { get; }` to `src/Core/Services/Contracts/IFeatureService.cs` (named consistently with `IsMusicEnabled`, `IsCombatPauseEnabled`).
3. **Implement Service Logic:** In `src/Core/Services/Implementations/FeatureService.cs`:
   - Map the flag in the constructor: `this.FeatureFlags["ShowTagsOnHover"] = config.ShowTagsOnHover;`.
   - **Also add it to the fallback `else` branch** (`= true`). `IsFeatureEnabled` returns `false` for unknown keys, so omitting this silently disables the feature if the `.tres` is missing.
   - Implement `IsTagHoverEnabled` to return `this.IsFeatureEnabled("ShowTagsOnHover")`, following the existing property pattern.
4. **Control the UI:** In the hover handler / tooltip controller introduced under *UI Integration*, resolve `IFeatureService` from the `ServiceLocator` and only show tag tooltips if `IsTagHoverEnabled` returns `true`.

## 🧪 Testing Plan
- Create a stat-altering tag, add it to a character, and verify the stat correctly changes.
- Remove the tag and verify the stat returns precisely to its original value.
- Add a tag to an item and verify the item's properties are influenced.
- Save and load a character with tags and verify the tag IDs and remaining durations are restored.
- Remove a tag from the database (simulating a removed mod), load a save referencing it, and verify the game logs a warning and loads without crashing.
- Toggle `ShowTagsOnHover` off and verify the hover tooltip no longer appears while the character screen still shows tags.

## 🔗 Related Resources
- `IsoEntity.cs` (Base class that will receive the tags).
- `CharacterData.cs` (Stats that will be modified).
