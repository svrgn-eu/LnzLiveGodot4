# Menu System Alpha

## 📖 Overview
Building upon the `MenuSystem-Prototype`, the Alpha version aims to transform the basic, functional UI into a visually appealing, polished, and fully featured menu system. The focus is on establishing the game's aesthetic, adding juiciness through animations and transitions, fully supporting gamepad/keyboard navigation, and introducing crucial player settings.

## 🎯 Approach
We will utilize Godot's `Theme` resources to standardize UI elements and `AnimationPlayer` or `Tween` for transitions and micro-interactions. The structural flow remains similar to the prototype, but screens will be visually overhauled, and an "Options/Settings" menu will be introduced to handle basic configuration.

## ⚙️ Functional & Visual Requirements

### 1. Global UI Polish & Theming
- [ ] **Design System:** Create a global Godot `Theme` resource (`MainTheme.tres`) defining custom fonts and standard UI elements.
- [ ] **Game-wide Color Palette:** Define a strict, standardized color palette (e.g., Primary, Secondary, Accent, Text, Background) to ensure visual consistency across all screens. Avoid default colors and establish a curated scheme that fits the tactical/isometric theme.
- [ ] **9-Slice Scalable UI:** Implement robust 9-slice textures (`StyleBoxTexture`) for all button states (Normal, Hover, Pressed, Disabled, Focus) and panels. This ensures UI elements maintain crisp, non-distorted corners and borders regardless of how they are resized for different text lengths or screen resolutions.
- [ ] **Background Art:** Source or create decent, high-quality background images for the menus. These should replace plain flat-color backgrounds, using evocative art (e.g., cityscapes, tactical maps, or blurred safehouse environments) to ground the player in the game's world.
- [ ] **Juiciness:** Add subtle sound effects (UI clicks, hovers) to all interactive elements.
- [ ] **Micro-animations:** Implement tween-based micro-animations for UI elements to signal common actions and states (e.g., subtle button scaling on hover, color shifting on focus, or slight bounces when opening panels). These small, polished movements are crucial for a premium feel.
- [ ] **Transitions:** Implement a `SceneTransitionManager` (or similar Autoload) to handle smooth fade-ins and fade-outs between scenes instead of abrupt cuts.
- [ ] **Navigation Polish:** Ensure 100% flawless gamepad and keyboard navigation. Godot's UI `focus_neighbor` properties must be correctly configured so players never get "stuck" without a mouse.
- [x] **Input Mapping:** Menu actions must use their own dedicated key bindings in the Godot Input Map, all prefixed with `ui_menu_` (e.g., `ui_menu_accept`, `ui_menu_cancel`, `ui_menu_up`, `ui_menu_down`, `ui_menu_tab_left`, `ui_menu_tab_right`). This prevents conflicts with gameplay actions and allows independent rebinding.

### 2. Screen Overhauls
- [ ] **Splash/Title Screen (`ScrSplashScreen.tscn`):**
  - Add an animated studio logo or game title reveal.
  - Implement a smooth "Press Any Key" pulsing effect.
  - [x] Display current game version in bottom-right corner adhering to SemVer 2 and build number (per [ADR-0018](../../adr/ADR-0018%20Semantic%20Versioning%20and%20Build%20Numbering.md)).
- [ ] **Main Menu (`ScrMainMenu.tscn`):**
  - Add a dynamic or stylized background (e.g., parallax scrolling art, subtle particle effects, or a static 3D scene acting as a backdrop).
  - Expand the "Continue" functionality to show the timestamp or details of the latest save file.
  - Add an "Options" button to access settings.
  - [x] Display current game version in bottom-right corner adhering to SemVer 2 and build number (per [ADR-0018](../../adr/ADR-0018%20Semantic%20Versioning%20and%20Build%20Numbering.md)).
- [ ] **Safehouse / Hub (`ScrSafehouseHub.tscn`):**
  - Visually upgrade from a simple menu list to a contextual hub (e.g., a stylized "computer terminal" interface, holographic map, or a view of a safehouse room).
  - Expand mission selection to show mission details, difficulty, and potential rewards.
- [ ] **Loading Screen (`ScrLoadingScreen.tscn`):**
  - Enhance the visual design beyond a basic bar.
  - Add random gameplay tips, lore snippets, or character art to keep the player engaged while the deterministic progress bar fills.
- [ ] **Pause Menu (`ScrPauseMenu.tscn`):**
  - Refine the glitch/blur shader for better visual quality.
  - Add an "Options" button to allow changing settings mid-game.

### 3. New Feature: Options / Settings Menu
- [ ] **Create `ScrOptionsMenu.tscn`**: Accessible from both the Main Menu and the Pause Menu.
- [ ] **Video Settings:** Add toggles for Fullscreen/Windowed, VSync, and Resolution dropdowns.
- [ ] **Audio Settings:** Add volume sliders for Master, Music, and SFX channels (utilizing Godot's AudioServer buses).
- [ ] **Persistence:** Save and load settings to a configuration file (e.g., `user://settings.cfg`) so preferences persist between game sessions.

## 🎨 UI/UX Design Goals
- **Premium Feel:** Avoid generic colors. Use harmonious, curated color palettes that fit the tactical/isometric theme.
- **Typography:** Use a strong, readable, and modern font for UI elements (e.g., from Google Fonts).
- **Clear State Representation:** Players must immediately know which button is currently focused, especially when using a controller. Use distinct "Focus" styles in the Godot Theme.

## 💻 Technical Details
- Utilize `Tween` nodes extensively in UI scripts for hover animations (e.g., slightly increasing `scale` or shifting `modulate` color over 0.1-0.2 seconds).
- Use Godot's `ConfigFile` class to manage saving and loading user preferences easily.
- Audio buses must be properly set up in the editor so the Volume sliders can correctly map to `AudioServer.SetBusVolumeDb()`.

## 🧪 Testing Plan
- Test all menus exclusively with a gamepad to verify focus routing.
- Change video and audio settings, restart the game, and verify the settings persist and apply on startup.
- Verify that transitioning from the Main Menu to the Hub to the Game (and back) feels seamless with fade effects.
