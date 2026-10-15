# Advanced Debug Outputs

## 📖 Overview
A multi-level, toggleable debug overlay that provides developers and testers with real-time insights into game performance, entity states, and pathfinding. This feature is gated by the `EnableDebugTools` feature flag in the `FeatureService`.

## 🎯 Goals & Use Cases
- **Goal 1:** Provide varying levels of debug information to prevent screen clutter while still giving access to deep diagnostic data when needed.
- **Goal 2:** Centralize debug output into an easily accessible on-screen overlay.
- **Use Case A:** A developer is testing game performance and needs to quickly see FPS and Memory Usage. They press the debug toggle shortcut once to see basic metrics.
- **Use Case B:** A tester notices an AI character is stuck. They press the debug toggle shortcut multiple times to reach the "Paths & States" level to see the entity's current behavior state and planned navigation path.
- **Use Case C:** A level designer wants to verify the collision geometry. They cycle to the "Tilemap" debug level to visually see all non-walkable tiles.
- **Use Case D:** A designer wants to visualize the exact weapon range of a selected character on the grid to tune combat balance.

## 👥 Target Audience / User Roles
- Developers
- Testers
- Designers

## ⚙️ Functional Requirements
- [ ] Requirement 1: The debug output must only be available if `IsDebugToolsEnabled` is true in `FeatureService`.
- [ ] Requirement 2: The user must be able to toggle through different levels of debug output (e.g., Off -> Basic -> Entities/States -> Paths -> Off) using a single keypress.
- [ ] Requirement 3: **Basic Level** should show FPS, Frame Time, Memory Usage, and active Camera information.
- [ ] Requirement 4: **Entities Level** should show states of entities (e.g., Behavior Tree state, current animation/action).
- [ ] Requirement 5: **Paths Level** should display the current navigation path of the selected character and NPCs (either as text coords or visual lines).
- [ ] Requirement 6: **Tilemap Level** should display visual indicators on the grid for world state, such as highlighting non-walkable or colliding tiles.
- [ ] Requirement 7: **Combat Level** should visually display the ranges of the currently selected weapon for the active character, but also for other characters and NPCs. The Range of the selected character should be displayed brighter or more saturated than the others.
- [ ] Requirement 8: (Proposed Shortcut): The `F3` key should be used to cycle through these modes, replacing the current binary on/off toggle.

## 🎨 UI/UX Design & Interactions
- The debug overlay is rendered on a `CanvasLayer` (`ScrDebugOverlay`) to ensure it is always drawn on top of the game view.
- The text is aligned to the top-left corner with a semi-transparent dark background for readability. The respective page with Caption must be shown to help the tester orientate, e.g. "1/5 Basic", "2/5 Entities" etc. 
- **Interactions:** Pressing `F3` cycles through:
  1. `Off`: UI is hidden.
  2. `Level 1 (Basic)`: Shows FPS, Mem, Camera.
  3. `Level 2 (Entities)`: Adds Entity list with State/Position.
  4. `Level 3 (Verbose/Paths)`: Adds detailed pathfinding output and detailed states.
  5. `Level 4 (World/Tilemap)`: Adds visual debug drawing for the tilemap grid, specifically highlighting non-walkable/colliding tiles.
  6. `Level 5 (Combat/Ranges)`: Displays visual indicators for weapon ranges and sight radii for the currently selected character.

## 💻 Technical Details
- **Core Components:** `ScrDebugOverlay.cs` will be updated to handle multiple states rather than just `Visible = !Visible`.
- **Performance:** `Engine.GetFramesPerSecond()` and `OS.GetStaticMemoryUsage()` will be used for basic metrics.
- **Entity Data:** Modify the existing `CountEntities` method in `ScrDebugOverlay.cs` to extract state from `IsoCharacter` nodes (e.g., checking for LimboAI BTPlayer state or internal state enums).
- **Paths Data:** Extract planned path points from `NavigationAgent2D` or custom pathfinding logic on the entities.
- **Tilemap Data:** Query the active `TileMap` or AStar navigation grid to identify and draw overlays on non-walkable tiles using Godot's `_Draw()` function in a canvas item.
- **Combat Data:** Query the active character's equipped weapon data to determine the range and draw a radius or grid overlay representing the valid attack area.


## 🔗 Related Resources
- Existing `ScrDebugOverlay.cs`
- `FeatureService.cs` and `FeatureConfig.tres`

## 🎨 Color Palette & Coding Standards
When implementing the debug overlays, adhere to the following color palette to maintain consistency across the various debug levels:

| Use Case | Color | Color Code (Godot C#) | Description |
| :--- | :--- | :--- | :--- |
| **Overlay Background** | Semi-transparent Black | `new Color(0, 0, 0, 0.5f)` | Background for the debug text overlay for readability. |
| **Debug Text (Default)** | White | `Colors.White` (`#FFFFFF`) | Standard text color for FPS, memory, and general information. |
| **Debug Text (Warning)** | Yellow | `Colors.Yellow` (`#FFFF00`) | Text color for warnings (e.g., low FPS, high memory). |
| **Debug Text (Error)** | Red | `Colors.Red` (`#FF0000`) | Text color for errors or critical states. |
| **Pathfinding (Valid Path)**| Orange | `Colors.Orange` (`#FFA500`) | Lines representing the current active path of an entity (Level 3). |
| **Tilemap (Non-walkable)** | Semi-transparent Red | `new Color(1.0f, 0, 0, 0.4f)` | Highlight for grid tiles that are non-walkable/colliding (Level 4). |
| **Combat (Selected Range)** | Bright Red | `Colors.Red` (`#FF0000`) | Weapon range of the currently active/selected character (Level 5). |
| **Combat (Other Range)** | Dark Red | `Colors.DarkRed` (`#8B0000`) | Weapon ranges for NPCs or non-selected characters (Level 5). |
| **Combat (Sight Radius)** | Dodger Blue | `Colors.DodgerBlue` (`#1E90FF`) | Vision/sight radius of characters (Level 5). |

## 📄 Page Info Distribution

To prevent the debug overlay text from stacking and cluttering the left side of the screen, the following distinct distribution of information per page is proposed. Instead of accumulating text from previous levels, the UI will only render text strictly relevant to the *current* page.

| Level | Page Name | Text Overlay Content | Visual Drawings (World Space) |
| :--- | :--- | :--- | :--- |
| **1/5** | Basic Performance | FPS, Memory Usage, Frame Time. | None |
| **2/5** | Camera & World | Camera Position, Zoom Level. | Tilemap non-walkable grid highlights. |
| **3/5** | Entities & States | List of active entities, Global Position, Basic State (Idle/Moving). | None |
| **4/5** | Pathfinding | BT States, Path Length, Target Coordinates. | Path interpolation lines (Orange). |
| **5/5** | Combat & Ranges | Selected character name, active weapon data summary. | Attack ranges (Red) and Sight radii (Blue). |

## 🛠️ Implementation Plan

Based on the existing codebase (specifically `ScrDebugOverlay.cs`, `IsoCharacter.cs`, and `FeatureService.cs`), the following steps outline how to implement the missing requirements:

### 1. Update `ScrDebugOverlay.cs` State Management
- **Enum Update**: Add `Tilemap` and `Combat` to the `DebugLevel` enum in `ScrDebugOverlay.cs` so it supports all 5 levels (plus `Off`).
- **Input Handling**: Register a new action in Godot's Input Map (e.g., `debug_toggle_overlay` mapped to `F3`). In `_Input(InputEvent @event)` or `_UnhandledInput`, use `Input.IsActionJustPressed("debug_toggle_overlay")` rather than hardcoding the key, and update the modulo arithmetic to cycle through 6 states: `_currentDebugLevel = (DebugLevel)(((int)_currentDebugLevel + 1) % 6);`.
- **Text Header Updates**: Update `UpdateDebugText()` to explicitly display the current page (e.g., "1/5 Basic", "4/5 World/Tilemap") in the `text` string, matching the requested UI design.

### 2. Implement Visual Debug Drawing
- **Override `_Draw`**: Add an `override void _Draw()` method to `ScrDebugOverlay.cs`.
- **Trigger Redraws**: Add `QueueRedraw()` inside the `_Process` loop so the drawing updates every frame while the overlay is visible.
- **Coordinate System**: Ensure drawing is correctly transformed to world coordinates, or adjust local drawing based on the `_camera` position, since `CanvasLayer` draws in screen space. You may need to use `_camera.GetCanvasTransform().AffineInverse()` for accurate world-space drawing.

### 3. Level 3: Paths Visualization
- In `_Draw()`, if the level is `Paths` (or higher), iterate through all `IsoCharacter` nodes.
- Read the public `IEnumerable<Vector2> CurrentPath` property from each `IsoCharacter`.
- If `character.IsMoving` is true and `CurrentPath` has points, convert the path to an array and use `DrawPolyline()` with `Colors.Orange` and a suitable line width.

### 4. Level 4: Tilemap Highlights
- Add a reference to the active `TileMap` (fetch it during `_Ready()` from `_levelContainer`).
- In `_Draw()`, if the level is `Tilemap`, get all used cells via `tileMap.GetUsedCells(layer)`.
- Check each cell's tile data (e.g., custom data layers for walkability). If non-walkable, calculate its world position and use `DrawRect()` with `new Color(1.0f, 0, 0, 0.4f)` to paint over it.

### 5. Level 5: Combat & Ranges Visualization
- **Selected Character Tracking**: Track the currently selected character by listening to `EventBus.Instance.CharacterSelected` in `ScrDebugOverlay` or polling an active combat manager.
- **Draw Radii**: In `_Draw()`, if the level is `Combat`:
  - Fetch the `CharacterData` for the selected character and other NPCs.
  - Draw the sight radius using `DrawCircle()` or `DrawArc()` with `Colors.DodgerBlue`.
  - Draw the attack range using `Colors.Red` for the active character and `Colors.DarkRed` for others.

## 🧪 Testing Plan
- Ensure pressing `F3` (or the mapped action `debug_toggle_overlay`) cycles correctly.
- Verify that if `EnableDebugTools` in `FeatureConfig.tres` is unchecked, the debug key does nothing.
- Confirm FPS and Memory metrics update in real-time.
- Spawn multiple entities and verify their states and paths are accurately reflected in higher debug levels.
