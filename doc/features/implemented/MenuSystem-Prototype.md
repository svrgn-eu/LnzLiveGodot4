# Simple Menu System Prototype

## 📖 Overview
To provide a complete flow for the game prototype, we need a simple menu system that acts as the primary entry point for the player. The system will consist of an initial Splash/Title Screen (displaying the game title and technology credits), a Main Menu (with options to continue, start a new game, or quit), an intermediate Headquarters / Safehouse Hub (for mission selection and eventually crew management), and a Pause Menu accessible during gameplay (with options to resume or return to the main menu/hub). This establishes the foundational scene management and game state flow, faithfully recreating the intended flow outlined in the game's core concepts (`10 Concept.md`).

## 🎯 Approach
The menu system will be built using Godot's Control nodes (UI) and integrate with the existing `GameManager` and `SceneManagementService` to handle transitions between the menu scenes and the actual game levels.

We will have five primary UI scenes:
1. **ScrSplashScreen.tscn**: The scene loaded upon launching the game. It displays the title and credits for the technologies used, waiting for a key press to transition to the Main Menu.
2. **ScrMainMenu.tscn**: The main entry scene. It contains "Continue", "New Game", and "Quit" buttons.
3. **ScrSafehouseHub.tscn**: The intermediate base of operations. Loaded after starting a new game or continuing. In the prototype, this will act primarily as a Mission Selection screen to launch the tactical gameplay map.
4. **ScrPauseMenu.tscn**: A canvas layer/control scene that can be instantiated or toggled active during gameplay when the player presses the designated pause action (e.g., Escape). It pauses the game tree and provides "Resume" and "Return to Hub/Menu" buttons. A base version is already implemented.
5. **ScrLoadingScreen.tscn**: An intermediary scene used when transitioning from the Safehouse Hub to a tactical level. It masks the background loading process (via Godot's threaded resource loader) to prevent the main thread from freezing and provides progress feedback to the player.

## 👥 Target Audience / User Roles
- **Players:** To navigate into the game experience and exit gracefully.
- **Developers:** To have a structured way to test the full game loop (Boot -> Menu -> Gameplay -> Menu) rather than always starting directly in a gameplay scene.

## ⚙️ Functional Requirements
- [x] **Splash/Title Screen:**
  - Create `ScrSplashScreen.tscn`.
  - Display the game title prominently.
  - Display text/credits acknowledging the technologies used (e.g., Godot Engine, C#, etc.).
  - Listen for "Any Key" or a specific "Confirm" action to transition to the Main Menu.
- [x] **Main Menu Scene:**
  - Create `ScrMainMenu.tscn`.
  - Add a "Continue" button. This should only be active/visible if a save game exists, and it should transition to the `ScrSafehouseHub.tscn` (loading the saved state).
  - Add a "New Game" button that transitions to the `ScrSafehouseHub.tscn` with a fresh state.
  - Add a "Quit" button that terminates the application (`GetTree().Quit()`).
- [x] **Safehouse / Mission Select Hub:**
  - Create `ScrSafehouseHub.tscn`.
  - Add a "Mission Board" UI that allows the player to select a mission.
  - Add a "Launch Mission" button that uses the `GameManager` to load the selected `LevelDef` (the prototype level).
  - Add a "Return to Main Menu" or "Save & Quit" button.
- [x] **Loading Screen:**
  - Create `ScrLoadingScreen.tscn`.
  - Provide visual feedback (e.g., progress bar, spinning icon) so the game doesn't appear frozen. **Note:** The loading progress is deterministic; Godot's background threaded resource loader (`ResourceLoader.LoadThreadedGetStatus`) provides a float from 0.0 to 1.0 that accurately drives a progress bar.
  - Intercept the transition from the Safehouse Hub to the tactical level by asynchronously loading the heavy scene data (`SysMainGameWindow` / `LevelDef`) on a background thread.
  - Automatically transition to the gameplay scene once loading is complete.
- [x] **Pause Menu Scene (Adapt Existing):**
  - Move the existing `src/UI/ScrPauseMenu.tscn` to `src/Scenes/Menus/ScrPauseMenu.tscn` and adapt it.
  - Wire up the existing "Resume" button to unpause the game and hide the menu.
  - Repurpose the "Quit to Desktop" button (or add a new button) to "Abort Mission / Return to Hub" which unpauses the game, cleans up the current level, and loads `ScrSafehouseHub.tscn` (or the Main Menu depending on failure state design).
  - Listen for a global "Pause" input action (e.g., 'Esc' key or Start button on a gamepad).
  - When paused, set `GetTree().Paused = true` and show the menu.
- [x] **Game State Integration:**
  - Ensure the `GameManager` properly handles the state transitions between "Menu" and "Gameplay".
  - Prevent the Pause menu from being triggered while already in the Main Menu.

## 🎨 UI/UX Design & Interactions
- **Input & Navigation:** All menus must be fully navigable via Mouse, Keyboard (e.g., Arrow Keys, Enter), and Controller (e.g., D-Pad, Face Buttons). These navigational actions should be mapped to the corresponding key bindings in the Godot project's Input Map (like `ui_up`, `ui_down`, `ui_accept`, `ui_cancel`).
- **Splash/Title Screen:** A static or gently animated screen with the title in the center, technology credits at the bottom, and a "Press Any Key" prompt blinking.
- **Main Menu:** A simple, clean layout with the game title at the top center and the buttons arranged vertically in the center of the screen.
- **Safehouse Hub:** A menu representing the HQ. For the prototype, a minimalist layout containing a list of available missions (the prototype level) and a Launch button.
- **Loading Screen:** A visually distinct transition screen. It should feature a dark background, a stylistic spinning icon or progress bar in the bottom right corner, and optionally display gameplay tips or lore snippets while the player waits.
- **Pause Menu:** Leverages the existing `ScrPauseMenu.tscn` which already includes a blur shader background, glitch shader effects, and vertical buttons ("Resume", "Options", "Quit to Desktop").

## 💻 Technical Details
- **Data Structure & Files:**
  - All menu UI scenes will be organized under `src/Scenes/Menus/`.
  - `src/Scenes/Menus/ScrSplashScreen.tscn`
  - `src/Scenes/Menus/ScrMainMenu.tscn`
  - `src/Scenes/Menus/ScrSafehouseHub.tscn`
  - `src/Scenes/Menus/ScrPauseMenu.tscn` (Moved from `src/UI/`)
  - All menu scripts will be organized under `src/Scripts/Menus/`.
  - `src/Scripts/Menus/SplashScreenController.cs`
  - `src/Scripts/Menus/MainMenuController.cs`
  - `src/Scripts/Menus/SafehouseHubController.cs`
  - `src/Scripts/Menus/ScrPauseMenu.cs` (Moved from `src/Scripts/UI/`)
- **Integration:**
  - `MainMenuController` handles transitioning to the Hub state.
  - `SafehouseHubController` will need a reference to the prototype `LevelDef` (or a list of them) to pass to `GameManager.LoadLevel()` when a mission is launched.
  - `ScrPauseMenu` already has `process_mode = 3` (Always) on its root node, so it is correctly configured to receive input to unpause while the tree is paused.
  - The Pause Menu could be a global AutoLoad (singleton) canvas layer that is always available but only active/visible during gameplay state, or it can be instantiated by the `GameManager` when a level is loaded.

## 🧪 Testing Plan
- Launch the game from the Godot editor starting with `ScrSplashScreen.tscn`. Verify the title and credits appear.
- Press any key and verify it transitions cleanly to `ScrMainMenu.tscn`.
- Verify the "Continue" button is disabled (or hidden) if no save data exists.
- Create dummy save data, verify "Continue" becomes active, and clicking it transitions to `ScrSafehouseHub.tscn`.
- Click "New Game" and verify it transitions to `ScrSafehouseHub.tscn`.
- From the Hub, click "Launch Mission" and verify the prototype tactical level loads successfully.
- Press 'Esc' during gameplay and verify the game pauses (characters stop moving) and the existing `ScrPauseMenu` appears with blur/glitch effects.
- Click "Resume" and verify gameplay continues normally.
- Press 'Esc' again, click "Abort Mission" (or Return), and verify the level is unloaded and the `ScrSafehouseHub.tscn` is displayed again.
- Return to Main Menu and click "Quit", verifying the application closes.

## 🔗 Related Resources
- `src/Scripts/Core/GameManager.cs`
- `src/Scripts/Core/Implementations/SceneManagementService.cs`
- Existing `LevelDef` infrastructure.
