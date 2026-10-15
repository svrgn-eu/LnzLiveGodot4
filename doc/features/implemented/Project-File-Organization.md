# Feature: Godot Best Practices - Project File Organization

## Overview
Currently, the project follows a **Type-Based** file organization (`src/Scenes`, `src/Scripts`, `src/Data`). While this is common in some web development frameworks and older Unity projects, Godot's official best practice is **Feature-Based** organization.

In Godot, a scene (`.tscn`) and its attached script (`.cs` / `.gd`) are two halves of the same component. Keeping them separated by type increases cognitive load, causes "hunting" for related files, and prevents easy modularity (e.g., dropping a fully self-contained `StrayCat` folder into a new project).

The goal of this feature is to migrate the entire project into a Feature-Based folder architecture without breaking any `res://` paths or C# namespaces.

## Proposed Architecture Directory Structure
```text
src/
├── Core/
│   ├── Data/                  # Base data structures (Item, LevelDef, Slot)
│   ├── Pathfinding/           # AStar grids
│   ├── Services/              # ServiceLocator and IService implementations
│   └── SysMainGameWindow/     # Main game window scene and script
├── Entities/
│   ├── Character/             # Character scene, scripts, and specific data
│   └── StrayCat/              # Stray cat scene, scripts, and specific data
├── Levels/
│   └── Lvl01Warehouse/        # Level scenes and specific assets
└── UI/
    ├── Menus/                 # (Already migrated!)
    ├── CharacterScreen/       # ScrCharacterScreen.tscn & .cs
    ├── DebugOverlay/          # ScrDebugOverlay.tscn & .cs
    ├── TacticalHud/           # UiTacticalHud.tscn & .cs
    └── Components/            # Smaller UI elements like UiCharacterPortraitTile
```

## Detailed Migration Plan

### Step 1: Migrate Core Systems
Move the foundational systems into `src/Core/`.
1. Move `src/Scenes/SysMainGameWindow.tscn` and `src/Scripts/Core/SysMainGameWindow.cs` to `src/Core/SysMainGameWindow/`.
2. Move `src/Scripts/Core/Contracts/` and `src/Scripts/Core/Implementations/` to `src/Core/Services/`.
3. Move `src/Scripts/Core/Pathfinding/` to `src/Core/Pathfinding/`.
4. Move generic data structures from `src/Scripts/Data/` to `src/Core/Data/`.

### Step 2: Migrate Entities
Group actors and objects together in `src/Entities/`.
1. Move `src/Scenes/EntCharacter.tscn`, `src/Scripts/Core/IsoCharacter.cs` (if applicable), and `CharacterData.cs` to `src/Entities/Character/`.
2. Move `src/Scenes/EntStrayCat.tscn` and `src/Scripts/Data/StrayCatData.cs` to `src/Entities/StrayCat/`.

### Step 3: Migrate Levels
1. Move `src/Scenes/Lvl01Warehouse.tscn` to `src/Levels/Lvl01Warehouse/`.

### Step 4: Migrate Remaining UI
Move the rest of the loose UI elements into the structured `src/UI/` directory.
1. Move `src/Scenes/ScrDebugOverlay.tscn` and `src/Scripts/UI/ScrDebugOverlay.cs` to `src/UI/DebugOverlay/`.
2. Move `src/UI/ScrCharacterScreen.tscn` and `src/Scripts/UI/ScrCharacterScreen.cs` to `src/UI/CharacterScreen/`.
3. Move `src/UI/UiTacticalHud.tscn` and `src/Scripts/UI/UiTacticalHud.cs` to `src/UI/TacticalHud/`.
4. Move `src/UI/UiCharacterPortraitTile.tscn` and `src/Scripts/UI/UiCharacterPortraitTile.cs` to `src/UI/Components/CharacterPortraitTile/`.

### Step 5: Path and Namespace Resolution
* **Godot Paths:** Run an automated search-and-replace to update all `res://Scenes/...` and `res://Scripts/...` paths to their new feature locations in all `.tscn` and `project.godot` files.
* **C# Namespaces:** Refactor C# namespaces (e.g., `namespace Scripts.Core` to `namespace Core`, `namespace Scripts.UI` to `namespace UI`). Update all `using` statements globally to ensure the solution continues to compile flawlessly.

## Approval
Once this document is reviewed and approved, we will execute the migration scripts to safely restructure the project in a single sweep.
