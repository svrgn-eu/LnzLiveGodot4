# Level Data Resource (LevelDef)

## 📖 Overview
To centralize and manage level-specific configuration in IsoTactics, we use a dedicated `LevelDef` Resource in Godot. This resource stores all metadata and environment settings for a given level (e.g., level name, map scene to load, win/loss transitions, music playlist, ambient lighting, and weather). This allows the game to decouple logical level configuration from the hardcoded scene files and enables a flexible, data-driven approach to level loading and progression.

> **Note on Existing Implementation:** 
> The project already contains `Scripts.Data.LevelDef` which handles `LevelName`, `LevelScene`, `NextLevelWin`, and `NextLevelLoss`. We will expand this existing class rather than creating a new `LevelData` class.

## 🎯 Approach: Level Management in the Godot Editor
**Will there be one scene representing a level?**
Yes and no. There will be one `.tscn` (scene) representing the *structural/physical layout* (the TileMap, walls, spawn points). However, the *logical level* (what the player experiences as a mission) is represented by the `.tres` (Resource) file instance of `LevelDef`. 

This means you can have **multiple `LevelDef` resources pointing to the same `.tscn` scene**. 
- **Use Case A:** The `SceneManagementService` reads a `LevelDef` resource to transition into a new mission. It knows exactly which `.tscn` to load (`LevelScene`), what BGM to start playing, and what lighting overlay to apply.
- **Use Case B:** A designer wants to create a "Night" version of an existing warehouse map. They create a new `LevelDef` `.tres` file pointing to the same `.tscn` but with a different ambient lighting color, weather effect, and music track. They don't need to duplicate the `.tscn`.

### 🛠️ Editor Preview Tooling
**Is there a way to preview these LevelDef parameters directly in the Godot Editor?**
Yes, absolutely! Since Godot allows running code in the editor using the `[Tool]` attribute, we can build a previewer:
1. **LevelPreviewer Node**: Create a helper node (e.g., `LevelPreviewer.cs`) with the `[Tool]` attribute and place it in your `.tscn` map scenes.
2. **Exported Property**: Export a property for the `LevelDef` resource on this node.
3. **Live Updates**: In the setter for the `LevelDef` property, check if we are in the editor (`Engine.IsEditorHint()`). If we are, read the `AmbientLightColor`, `WeatherEffect`, etc. from the assigned `LevelDef` resource and apply them immediately to the scene's lighting and particle nodes.
4. **Workflow**: A level designer opens the base `.tscn` file, assigns "Level01_Night.tres" to the `LevelPreviewer` node, and the editor scene instantly darkens and turns on the rain particles. Assigning "Level01_Day.tres" instantly reverts it to daytime.

## 👥 Target Audience / User Roles
- **Level Designers:** To easily configure levels (music, weather, transitions) by editing `.tres` files in the Godot Inspector without digging into scene trees or scripts.
- **System Programmers:** To have a clean, unified data model for level loading handled by `GameManager` and `SceneManagementService`.

## ⚙️ Functional Requirements (Expanding LevelDef)
- [x] Use existing `LevelDef` C# class extending Godot `Resource` (with `[GlobalClass]`).
- [x] Support `LevelName` (string) for UI display.
- [x] Support `LevelScene` (`PackedScene`) to define the actual level layout/TileMap to instantiate.
- [x] Support `NextLevelWin` and `NextLevelLoss` for level transitions.
- [ ] Add `LevelDescription` (multiline string) for mission briefing screens.
- [ ] Add `MusicTrack` (`AudioStream`) or playlist data for the background music.
- [ ] Add `AmbientLightColor` (`Color`) to set the mood/time of day.
- [ ] Add `WeatherEffect` (enum/resource) to trigger weather particle systems like rain or fog.

## 🎨 UI/UX Design & Interactions
- `LevelDef` resources will be created and edited entirely within the Godot Inspector.
- The UI (e.g., Mission Select or Briefing Screen) will read the `LevelName` and `LevelDescription` directly from this resource before loading the map.

## 💻 Technical Details
- **Data Structure:** 
  - `src/Scripts/Data/LevelDef.cs`
  - Inherits from `Godot.Resource`.
  - New properties must use `[Export]` attributes.
- **Integration:** 
  - The level loading system (`GameManager` -> `SceneManagementService`) already accepts a `LevelDef` parameter and instantiates the `LevelScene`.
  - We need to update `SceneManagementService` (or a related environment manager) to read the new music/weather/lighting settings from `LevelDef` and apply them dynamically to the active scene or global environment after instantiation.

## 🧪 Testing Plan
- Create a mock `LevelDef` resource in the editor.
- Load the resource via the existing `GameManager.LoadLevel()` and verify that the correct scene instantiates and the correct background color/music plays based on the data.
- Verify that modifying the resource in the editor allows creating wildly different atmospheres (e.g., sunny vs raining) using the exact same underlying `.tscn` map file.

## 🔗 Related Resources
- Existing documentation on Level Loading and Game Progression.
- `src/Scripts/Data/LevelDef.cs`
- `src/Scripts/Core/GameManager.cs`
- `src/Scripts/Core/Implementations/SceneManagementService.cs`
