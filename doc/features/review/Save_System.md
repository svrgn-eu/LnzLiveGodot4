# Save System

## 📖 Overview
A robust system for persisting game state, allowing players to save their progress, character details, inventory, and current level, and resume later. It handles the serialization of Godot C# objects into a persistent storage format (e.g., JSON) and vice versa.

## 🎯 Goals & Use Cases
- **Goal 1:** Provide a reliable way to serialize and deserialize game state.
- **Goal 2:** Ensure decoupling between game logic (Nodes) and game state (Data Models).
- **Use Case A:** A player pauses the game and clicks "Save Game". The current tactical map state, character stats, and inventory are persisted to disk.
- **Use Case B:** A player launches the game and selects "Load Game" from the main menu to restore their previous session.

## 👥 Target Audience / User Roles
- **Players:** Want seamless, fast, and uncorrupted save/load experiences.
- **Developers:** Need an easy-to-use API to mark data for saving and to hook into the save/load lifecycle.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The system must define clear C# Data Transfer Objects (DTOs) representing the game state without Godot Node dependencies.
- [ ] Requirement 2: The system must use `System.Text.Json` (or Godot's `Json`) to serialize these DTOs to the `user://` directory.
- [ ] Requirement 3: The system must provide a `SaveService` (injected via DI) that orchestrates gathering data from the game world.
- [ ] Requirement 4: On load, the system must instantiate or update game world objects (like the tactical map and characters) using the deserialized state.

## 🎨 UI/UX Design & Interactions
- **Location:** Accessible from the Main Menu ("Load Game") and Pause Menu ("Save Game" / "Load Game").
- **Interactions:** 
  - A "Save Slots" UI presenting multiple save files with timestamps, play time, and current level preview.
  - A subtle notification or "Saving..." icon indicating auto-saves or manual saves completing.

## 💻 Technical Details
### How Godot Handles Saves (Generally)
Godot provides several built-in ways to handle saves:
1. **FileAccess & JSON:** Reading/writing strings to the `user://` directory using `FileAccess` and converting data with Godot's `Json` helper.
2. **Custom Resources (`.tres` / `.res`):** Creating scripts that inherit from `Resource` and using `ResourceSaver` and `ResourceLoader`. This is easy for Godot-centric structures but can be risky for complex C# architectures or backward compatibility (especially cross-version).
3. **ConfigFile:** Best for simple key-value settings, not complex structured game states.

### Specific Implementation for IsoTactics (Godot C#)
Since this is a C# project adhering to strict architecture guidelines, we should rely on standard C# patterns:
- **State Separation:** Game state (health, inventory, position) should be stored in plain C# objects (POCOs/DTOs) rather than relying on reading properties directly from `Node2D`/`Node3D` at save time. 
- **Serialization:** Use standard `System.Text.Json` for strong typing, fast serialization, and robust attribute support (e.g., `[JsonIgnore]`). This avoids dynamic typing issues with Godot's dictionary-based JSON parser.
- **Save Service:** A dedicated `ISaveService` class that manages reading/writing to `user://saves/` via dependency injection.
- **ISaveable Interface:** Game entities (like the Player Manager or Level Manager) implement an `ISaveable<TState>` interface to populate their state when requested by the `SaveService`, and consume state when loading.

## 🧪 Testing Plan
- Create test scenes to verify serialization/deserialization of complex states (e.g., characters with specific items).
- Test edge cases: loading a save file from an older game version (versioning the save file), handling corrupted save files, and ensuring the `user://` quota isn't exceeded.

## 🔗 Related Resources
- [Godot Docs: Saving and reading data](https://docs.godotengine.org/en/stable/tutorials/io/saving_games.html)
- [System.Text.Json documentation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)
- [Player-Level Persistence Keys](file:///d:/Data/OneDrive/12%20Areas/Ideas/IsoTactics/doc/features/review/PersistenceKeys-PlayerLevel.md): For tracking granular player progression states and narrative flags.
