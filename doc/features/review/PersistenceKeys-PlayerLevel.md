# Player-Level Persistence Keys

## 📖 Overview
Introduce a system for Player-Level Persistence Keys to track game state, player choices, and progress throughout a campaign. This system will utilize string-based keys (or flags) stored within the player's active save file, enabling the game to "remember" stateful changes (e.g., opened chests, completed quests, defeated bosses) across game sessions and scene transitions.

## 🎯 Goals & Use Cases
- **Goal 1:** Provide a centralized, easily queryable dictionary of progression flags and state values tied to a specific player save file (as opposed to global meta-progression or settings).
- **Goal 2:** Allow scripts, dialogue trees, and interactive objects to easily set, read, and modify these keys at runtime.
- **Use Case A:** A player opens a unique treasure chest in a dungeon. The chest sets a key `"dungeon1_chest_gold" = true`. If the player leaves the level and returns, the chest checks this key on `_Ready()` and initializes itself as already opened.
- **Use Case B:** A dialogue system checks if `"quest_talked_to_mayor"` is true. If it is, the mayor gives a new dialogue line instead of repeating the introduction.
- **Use Case C:** Tracking integer values, such as `"factions_reputation_merchants" = 50`, to handle relationships and branching storyline outcomes.
- **Use Case D:** Persisting dynamic tactical state, such as current ammo counts, exact entity positions, and flags for which enemies are dead, ensuring a level reloads exactly as the player left it.

## 👥 Target Audience / User Roles
- **Game Designers / Writers:** Can use keys in dialogue trees, quest data, and level logic to create branching and reactive worlds.
- **Players:** Experience a persistent, stateful world that reacts to their past actions.
- **Developers:** Have a standardized, clean API to read/write state without creating bespoke C# variables for every single interactable object in the game.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The system must support storing Boolean (flags), Integer (counters/reputation), and String values mapped to unique string keys.
- [ ] Requirement 2: Keys must be scoped to the active player save/campaign, ensuring they do not bleed into other save slots or a "New Game".
- [ ] Requirement 3: Provide an `IPersistenceService` (injected via Dependency Injection) that wraps the raw dictionary with type-safe generic methods like `void SetValue<T>(string id, T value)`, `T GetValue<T>(string id, T defaultValue = default)`, and `bool HasKey(string id)`.
- [ ] Requirement 4: The system must serialize to and from JSON (or Godot's save format) efficiently when saving and loading the game.
- [ ] Requirement 5: Support a signal/event system (e.g., `OnKeyChanged(string id, variant newValue)`) so UI or in-game objects can react immediately when a key updates.

## 🎨 UI/UX Design & Interactions
- **Debug UI:** A developer console command or an in-game debug overlay to list all active keys, manually add/edit keys, or clear keys for testing purposes.
- **Save/Load Interactivity:** While the keys are invisible to the player, the save/load menu guarantees that the entire dictionary is captured instantly.

## 💻 Technical Details
- **Data Structure & Attachment:** A `Dictionary<string, object>` in C# to store the keys in memory. (Note: Using `object` or primitive C# types like `bool`/`int`/`string` is strongly preferred over Godot's `Variant` to ensure seamless serialization with standard libraries like `System.Text.Json`). This dictionary is directly attached as a property of the main `GameState` DTO defined in the Save System architecture.
- **Serialization:** Because the dictionary is attached to the `GameState` DTO, the `SaveService` naturally serializes it into the save file alongside other data. Upon loading, the dictionary is fully populated before the level scene is instantiated, ensuring objects can check their state (like their saved coordinates or if they should be dead) during `_Ready()`.
- **Integration with Data-Driven Design:** Mods and external JSON files (like dialogue or quest data) should be able to reference these keys as conditions (e.g., `"requires_key": "boss_defeated"`).

## 🏆 Best Practices (C# & Godot)
- **Avoid "Magic Strings":** Never type raw string keys (e.g., `"dungeon_chest_1"`) directly in game logic scripts. Instead, define them as constants in a centralized static class (e.g., `public static class PKeys { public const string DungeonChest1 = "dungeon_chest_1"; }`). This enables compiler checks, IDE refactoring, and prevents frustrating typo-induced bugs.
- **Strong Typing for Core State:** While a dynamic dictionary is perfect for flexible, mod-friendly flags (like quests or dialogue triggers), core system state (e.g., player inventory, character stats) should remain as strongly-typed POCOs (Plain Old C# Objects) inside the `GameState` DTO. Avoid dumping *everything* into the dictionary.
- **Save Versioning:** Include a reserved key (e.g., `"__save_version__"`) to track the game's schema version. This allows you to write migration scripts to safely upgrade old saves if key names change in future updates.
- **Type Safety via Generic Methods:** To mitigate the lack of type safety in a `Dictionary<string, object>`, hide the raw dictionary behind an `IPersistenceService` resolved via DI. By exposing generic methods (e.g., `GetValue<int>(PKeys.Gold, 0)`), you force callers to define the expected type, allowing the service to handle safe casting (or default fallbacks) internally.
- **Decoupled Node Logic:** Godot Nodes (`Node2D`, `CharacterBody2D`) should remain ignorant of the serialization process. They should resolve the `IPersistenceService` via Dependency Injection during `_Ready()` to retrieve their state, keeping game logic clean and separated from data storage.

## 🧪 Testing Plan
- Create a test scene with an interactable button that increments a `"times_pressed"` key.
- Save the game, reload the scene, and verify the counter resumes from the saved value.
- Test edge cases like requesting a key that hasn't been set yet (the system should safely return a default value like `false` or `0` without crashing).

## 🔗 Related Resources
- [Save System](file:///d:/Data/OneDrive/12%20Areas/Ideas/IsoTactics/doc/features/review/Save_System.md): For how the actual serialization and state persistence is managed at the architecture level.
