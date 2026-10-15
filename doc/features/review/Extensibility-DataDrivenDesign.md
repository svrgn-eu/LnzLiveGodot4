# Extensibility & Data-Driven Design

## 📖 Overview
Introduce a robust data-driven architecture that allows developers and end-users to add new content—such as weapons, gear, character classes, and items—without requiring recompilation of the game's core C# codebase. This extensibility will form the basis of a modding system and streamline content creation.

## 🎯 Goals & Use Cases
- **Goal 1:** Move the definitions of in-game items, weapons, and gear from hardcoded C# scripts into external data files (e.g., JSON, CSV, or Godot Custom Resources).
- **Goal 2:** Create a modding API/loader that reads external files at runtime or startup.
- **Use Case A:** A designer wants to tweak the damage values of a specific sniper rifle without needing a programmer to recompile the game. They edit the `weapons.json` file instead.
- **Use Case B:** A player wants to install a user-made mod that adds new futuristic armor and a laser sword. They drop the mod folder into the `Mods/` directory, and the game loads the new gear automatically on startup.
- **Use Case C:** Future DLCs can simply provide a new bundle of data and assets, seamlessly integrating with the base game.

## 👥 Target Audience / User Roles
- **Game Designers:** Can balance and add new content using simple text editors or spreadsheet software.
- **Modders/Community:** Can expand the game's lifespan and ecosystem by creating custom content.
- **Developers:** Can iterate faster without waiting for code recompilation.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The system must load external data files (JSON/Godot Resources) representing game items, weapons, and gear.
- [ ] Requirement 2: The system must dynamically generate in-game objects (e.g., UI representations, stat modifiers) based on the loaded data.
- [ ] Requirement 3: Support a specific directory structure for loading (e.g., `res://Data/` for base game, `user://Mods/` for mods).
- [ ] Requirement 4: Implement a fallback mechanism if a data file is corrupted or missing critical fields.
- [ ] Requirement 5: Provide robustness and extensive logging. If a file cannot be loaded or parsed, the system should skip it safely without crashing, and log all useful and necessary output (e.g., line number, missing keys, stack trace) to help developers or modders fix the issue, to a dedicated log file, either one for all mods or one per mod.

## 🎨 UI/UX Design & Interactions
- **Mod Manager Menu:** A new UI screen on the main menu to enable, disable, and order installed mods.
- **Error Handling UI:** A notification or console log screen during startup to alert the user if a mod failed to load due to data formatting issues, see logging. An alert within the UI to see which mods could not load.

## 💻 Technical Details
- **Mod Packaging & Loading:** Mods will be placed in a fixed subdirectory, such as `user://mods/`. The engine must support loading mods both as packed `.zip` archives (for final distribution via `ProjectSettings.LoadResourcePack()`) and as unpacked file directories (for local development and debugging). Modders can test their raw files directly, and zip them using standard OS tools when ready to publish.
- **Mod Structure & Entry Point:** Whether packed in a ZIP or placed as an unpacked folder, the mod's contents must reside in a directory matching its name (e.g., `user://mods/MyModName/`). The main JSON entry point file must also share this name (e.g., `MyModName.json`). 
- **Data Format:** JSON or Godot `.tres` (Custom Resources). JSON is preferred for community modding as it's highly readable and doesn't require the Godot Engine to edit. 
- **Data Loader:** A C# Singleton/Autoload (e.g., `ModLoader.cs`) that scans the `user://mods/` directory during initialization, detecting both unpacked folders and `.zip` files, and parses the JSON entry points into C# classes/structs.
- **Asset Loading & Relative Paths:** To ensure assets load correctly regardless of whether the mod is zipped (mounted in `res://`) or unpacked (located in `user://`), all file paths inside the JSON **must be relative** (e.g., `"sprite": "./sprites/sword.png"`). The `ModLoader` will dynamically resolve these relative paths to the correct absolute Godot path based on the entry point's location before loading them with `ResourceLoader`.

## 🚀 Future Expansions (What else could be added this way?)
By adopting this extensible approach, the following systems could also be completely data-driven and moddable:
1. **Characters & Classes:** Base stats, growth curves, skill trees, and enemy types.
2. **Levels & Campaigns:** Custom maps, spawn points, and enemy compositions.
3. **Quests & Dialogue:** Entire storylines, branching dialogue trees, and mission objectives.
4. **Status Effects & Abilities:** Defining how much damage an effect does, its duration, and particle effects used.
5. **Localization:** Adding new community-translated languages by dropping in a localized JSON file.

## 🧪 Testing Plan
- Create a dummy mod folder containing a "Test Weapon" with exaggerated stats and unique artwork.
- Verify that the game parses the test weapon, adds it to the inventory, and applies correct stats.
- Test edge cases like invalid JSON formatting, missing required fields, and duplicate IDs, ensuring the game handles them gracefully without crashing.
