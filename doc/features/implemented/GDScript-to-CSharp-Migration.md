# GDScript to C# (.NET) Migration

## 📖 Overview
Migrate the entire LnzLive codebase from GDScript (`.gd`) to C# (`.cs`) using Godot 4 (.NET), establishing strong static typing, improved IDE refactoring/debugging support, higher execution performance for file parsing and 3D pet generation, a clean Godot C# folder structure, and a maintainable service-oriented architecture powered by `Microsoft.Extensions.DependencyInjection`.

All work takes place in the dedicated Git branch `Feature/CSharpMigration` (to be created from `main`). You can run `dotnet build` on `src/PetzRendering.csproj` to verify compilation of the implementation.

## 🎯 Goals & Use Cases
- **Goal 1 (Idiomatic C# & .NET 10):** Convert all existing GDScript data models, binary/text parsers (`.lnz`, `.bhd`, `.bdt`), 3D node controllers, and editor UI scripts to idiomatic C# classes targeting **.NET 10 (`net10.0`)** (with .NET 8+ baseline compatibility).
- **Goal 2 (Strong Static Typing):** Replace loosely typed GDScript `Dictionary` and `Array` structures with strongly typed C# classes, records/structs, enums, and generic collections (`Dictionary<TKey, TValue>`, `List<T>`).
- **Goal 3 (100% Functional Parity):** Maintain full functional parity with the existing Godot 4 GDScript implementation (rendering, animation playback/selection, LNZ editing, local storage management, and recoloring/mirroring tools).
- **Goal 4 (Standardized Godot C# Folder Structure):** Rearrange the `src/` directory structure to follow Godot 4 C# conventions—separating scenes, models, parsers, services, 3D rendering nodes, and UI controllers into clean PascalCase folders aligned with C# namespaces.
- **Goal 5 (Service-Based Architecture with Microsoft DI):** Introduce a service-oriented architecture using the `Microsoft.Extensions.DependencyInjection` NuGet package, including a `LogService`, a `FeatureService` backed by settings-based configuration, and supporting domain/infrastructure services.
- **Use Case A:** A developer modifying LNZ parsing or adding new Petz format features benefits from compile-time type safety, C# LINQ/binary readers, and full IDE autocompletion/refactoring.
- **Use Case B:** Loading, parsing, and scrubbing through frames of `.bhd`/`.bdt` animations and `.lnz` files executes with lower overhead in compiled C#.
- **Use Case C:** Developers can register, configure, and inject decoupled services (logging, feature toggles, settings, file storage, species metadata) via standard .NET Dependency Injection rather than relying on tightly coupled global scripts.

## 👥 Target Audience / User Roles
- **Developers & Maintainers:** Contributors extending the desktop application, debugging `.lnz`/`.bhd`/`.bdt` parsers, and building new desktop pet systems in C#.
- **End Users (Petz Hexers):** Benefit from improved runtime stability, fewer dynamic typing runtime errors, and faster parsing/frame scrubbing.

## ⚙️ Functional Requirements
- [x] **Requirement 1 (Branch & .NET 10 Project Setup):** Create and check out the `Feature/CSharpMigration` Git branch. Initialize `src/PetzRendering.csproj` and `src/PetzRendering.sln` targeting `Godot.NET.Sdk` (matching `src/.fgvm-version` `4.7.1-stable`) with `<TargetFramework>net10.0</TargetFramework>` and add the `Microsoft.Extensions.DependencyInjection` NuGet package.
- [x] **Requirement 2 (Godot C# Folder Restructuring):** Reorganize `src/` to eliminate mixed asset/script folders and root-level scene clutter:
  - Group C# scripts into namespace-aligned folders (`src/Core/`, `src/Services/`, `src/Models/`, `src/Parsers/`, `src/Rendering/`, `src/UI/`).
  - Consolidate `.tscn` scenes under `src/Scenes/` (moving `Ball.tscn`, `Line.tscn`, `Paintball.tscn` from `src/` root and `RecolorLine.tscn` from `src/data_classes/`).
  - Keep `src/resources/` and `src/shaders/` paths stable so `res://resources/...` and `res://shaders/...` references continue to resolve seamlessly.
- [x] **Requirement 3 (Dependency Injection & Core Services):** Implement a DI bootstrapper Autoload (`ServiceBootstrapper` / `AppServices`) using `Microsoft.Extensions.DependencyInjection` and implement:
  - `ILogService` / `LogService`: Centralized structured logging wrapping Godot output (`GD.Print`, `GD.PushWarning`, `GD.PushError`) with configurable verbosity.
  - `ISettingsService` / `SettingsService` & `IFeatureService` / `FeatureService`: Settings-backed configuration (`user://settings.cfg` or `ProjectSettings`) and feature-flag evaluation.
  - `IKeyBallsService` / `KeyBallsService`: Injectable service providing species (`DOG`/`CAT`) and key-ball/body-part metadata (replacing the legacy `KeyBallsData` GDScript global singleton).
- [x] **Requirement 4 (Data Models & Enums):** Migrate all data classes from `src/data_classes/` (`BallData`, `AddBallData`, `LineData`, `PaintballData`, `Section`, `KeyBallsData`) into strongly typed C# classes/enums under `src/Models/`.
- [x] **Requirement 5 (Binary & Text Parsers):** Migrate `BhdParser`, `BdtParser`, `LnzParser`, and `PetzTextureLoader` into `src/Parsers/`, using `.NET` I/O or Godot `FileAccess`/`DirAccess` APIs while preserving `res://` and `user://` virtual path compatibility.
- [x] **Requirement 6 (3D Rendering Nodes & Generator):** Migrate `Ball.gd`, `Line.gd`, `Paintball.gd`, `rotator.gd`, and `dog_generator.gd` to C# scripts under `src/Rendering/` with `[Export]` properties and C# `[Signal]` events.
- [x] **Requirement 7 (Editor UI Controllers):** Migrate all UI scripts (`AnimPicker`, `FileTree`, `FPSLabel`, `FrameSlider`, `LnzTextEdit` / `TextEdit`, `PetViewContainer`, `PlayButton`, `ToolsMenu`, `unhandled_input_handler`) to C# under `src/UI/`.
- [x] **Requirement 8 (Scene & Autoload Wiring & Cleanup):** Update `src/project.godot` and all `.tscn` files to reference the relocated scenes, new `.cs` scripts, and C# signal/method names, and remove obsolete `.gd` and `.gd.uid` files once verified.

## 🎨 UI/UX Design & Interactions
- **No Visual Changes:** This is an architectural and language migration; the application layout, 3D viewport rendering, animation picker, keyboard shortcuts, and tools menu interactions remain identical to the current desktop application.

## 💻 Technical Details
- **Git Branch & Build Verification:**
  - Branch name: `Feature/CSharpMigration`
  - Verification command: `dotnet build src/PetzRendering.csproj`
- **Project Configuration (`src/project.godot` & `src/PetzRendering.csproj`):**
  - `project/assembly_name="PetzRendering"` is already configured under `[dotnet]` in `src/project.godot`.
  - Create `src/PetzRendering.csproj` using `Godot.NET.Sdk/4.7.1` (matching `.fgvm-version`), targeting `<TargetFramework>net10.0</TargetFramework>` with `<Nullable>enable</Nullable>` and `<LangVersion>latest</LangVersion>`.
  - Add `<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.*" />`.
- **Standardized Folder & Namespace Structure (`src/`):**
  ```text
  src/
  ├── Core/                        # Namespace: PetzRendering.Core
  │   └── ServiceBootstrapper.cs   # Godot Autoload initializing IServiceProvider
  ├── Services/                    # Namespace: PetzRendering.Services
  │   ├── ILogService.cs
  │   ├── LogService.cs
  │   ├── ISettingsService.cs
  │   ├── SettingsService.cs
  │   ├── IFeatureService.cs
  │   ├── FeatureService.cs
  │   ├── IKeyBallsService.cs
  │   └── KeyBallsService.cs
  ├── Models/                      # Namespace: PetzRendering.Models
  │   ├── Section.cs
  │   ├── BallData.cs
  │   ├── AddBallData.cs
  │   ├── LineData.cs
  │   └── PaintballData.cs
  ├── Parsers/                     # Namespace: PetzRendering.Parsers
  │   ├── BhdParser.cs
  │   ├── BdtParser.cs
  │   ├── LnzParser.cs
  │   └── PetzTextureLoader.cs
  ├── Rendering/                   # Namespace: PetzRendering.Rendering
  │   ├── DogGenerator.cs
  │   ├── Ball.cs
  │   ├── Line.cs
  │   ├── Paintball.cs
  │   └── Rotator.cs
  ├── UI/                          # Namespace: PetzRendering.UI
  │   ├── AnimPicker.cs
  │   ├── FileTree.cs
  │   ├── FPSLabel.cs
  │   ├── FrameSlider.cs
  │   ├── LnzTextEdit.cs
  │   ├── PetViewContainer.cs
  │   ├── PlayButton.cs
  │   ├── ToolsMenu.cs
  │   └── UnhandledInputHandler.cs
  ├── Scenes/
  │   ├── Rendering/
  │   │   ├── Ball.tscn
  │   │   ├── Line.tscn
  │   │   └── Paintball.tscn
  │   └── Editor/
  │       ├── Editor.tscn
  │       └── RecolorLine.tscn
  ├── resources/                   # Unchanged (preserves res://resources/... paths)
  └── shaders/                     # Unchanged (preserves res://shaders/... paths)
  ```
- **Service Architecture (`Microsoft.Extensions.DependencyInjection`):**
  - **`ServiceBootstrapper` (`Node` Autoload):** Builds the `ServiceCollection` and exposes `IServiceProvider Services { get; }` in `_EnterTree()` before scene nodes run `_Ready()`.
  - **`ILogService` / `LogService`:** Provides `LogDebug`, `LogInfo`, `LogWarning`, and `LogError` methods, formatting timestamps/categories and routing to `GD.Print` / `GD.PushWarning` / `GD.PushError`.
  - **`IFeatureService` / `FeatureService` & `ISettingsService` / `SettingsService`:** Loads/saves application and feature settings (e.g., from `user://settings.cfg` with sensible defaults) and exposes strongly typed feature checks (`IsEnabled(string featureKey)` / typed options).
  - **`IKeyBallsService` / `KeyBallsService`:** Holds species state and static/instance lookup tables for Dogz/Catz key balls, symmetry mappings, and body part categories previously stored in `key_balls_data.gd`.
- **Module-by-Module Migration Scope:**
  1. **Core Data & Enums (`src/data_classes/` → `src/Models/` & `src/Services/`):**
     - `section_enum.gd` → `Models/Section.cs`
     - `ball_data.gd`, `addball_data.gd`, `line_data.gd`, `paintball_data.gd` → `Models/BallData.cs`, `Models/AddBallData.cs`, `Models/LineData.cs`, `Models/PaintballData.cs`
     - `key_balls_data.gd` → `Services/KeyBallsService.cs` (`IKeyBallsService`)
  2. **Parsers & Resource Loaders (`src/data_classes/` → `src/Parsers/`):**
     - `bhd_parser.gd` → `Parsers/BhdParser.cs` (binary `.bhd` header and frame offset reader)
     - `bdt_parser.gd` → `Parsers/BdtParser.cs` (binary `.bdt` frame transform parser)
     - `lnz_parser.gd` → `Parsers/LnzParser.cs` (text `.lnz` section parser)
     - `petz_texture_loader.gd` → `Parsers/PetzTextureLoader.cs` (8-bit indexed `.bmp` palette index texture loader)
  3. **3D Scene Scripts (`src/scripts/` & `src/scenes/` → `src/Rendering/`):**
     - `dog_generator.gd` → `Rendering/DogGenerator.cs` (orchestrates pet generation, extensions, projections, animations, and group visibility)
     - `Ball.gd` → `Rendering/Ball.cs` (`Node3D`, updates `ShaderMaterial` parameters via `SetShaderParameter`, handles `Area3D` mouse/key selection and flash timer)
     - `Line.gd` → `Rendering/Line.cs` (`Node3D`, updates line shader uniforms)
     - `Paintball.gd` → `Rendering/Paintball.cs` (`Node3D`, updates paintball shader uniforms)
     - `rotator.gd` → `Rendering/Rotator.cs` (`CameraHolder` rotation/pan/zoom controller)
  4. **Editor UI Scripts (`src/scenes/editor/` & `src/scripts/` → `src/UI/`):**
     - `editor/AnimPicker.gd` → `UI/AnimPicker.cs`
     - `editor/FileTree.gd` → `UI/FileTree.cs`
     - `editor/LnzTextEdit.gd` & `src/scripts/TextEdit.gd` → `UI/LnzTextEdit.cs`
     - `editor/PetViewContainer.gd` → `UI/PetViewContainer.cs`
     - `editor/ToolsMenu.gd` → `UI/ToolsMenu.cs`
     - `editor/FrameSlider.gd`, `editor/PlayButton.gd`, `editor/FPSLabel.gd`, `editor/unhandled_input_handler.gd` → `UI/FrameSlider.cs`, `UI/PlayButton.cs`, `UI/FPSLabel.cs`, `UI/UnhandledInputHandler.cs`

## 🧪 Testing Plan
- **Build Verification:** Run `dotnet build src/PetzRendering.csproj` on the `Feature/CSharpMigration` branch and verify zero compilation errors or warnings on `.NET 10`.
- **DI & Services Verification:**
  - Verify `ServiceBootstrapper` initializes the `Microsoft.Extensions.DependencyInjection` container on startup and resolves `ILogService`, `IFeatureService`, `ISettingsService`, and `IKeyBallsService`.
  - Verify `FeatureService` reads/persists setting-based configuration and `LogService` outputs formatted logs.
- **Parser & Rendering Parity:**
  - Load both Dogz (`dachsund.lnz`, `scamp.lnz`) and Catz (`catz_base.lnz`, `cinn.lnz`) example files.
  - Verify balls, addballs, paintballs, lines, outlines, fuzz, and indexed `.bmp` textures render identically to the GDScript version.
- **Animation Playback & Selection:**
  - Play and scrub through animations using `FrameSlider` and `PlayButton`; switch animations via `AnimPicker` and verify frame counts and ball transforms.
- **Interactive Editing & Tools:**
  - Test `Ctrl+S` save & refresh, `Ctrl+Q` ball flashing, `Select` mode hover/double-click/shortcut navigation (`B`/`Z`, `M`/`X`, `P`/`C`, `L`/`V`, `Delete`).
  - Test all `Ctrl+Space` Tools Menu actions: Color part/entire pet, Color Swap (`RecolorPopup`), Add Ball, Copy L to R, Move Head, and Copy ball colors to clipboard.
  - Test Local Storage file operations (Rename, Delete, Back up) and Local Textures loading from `user://resources/textures`.

## 🔗 Related Resources
- [`src/project.godot`](../../src/project.godot)
- [`src/.fgvm-version`](../../src/.fgvm-version)
- [`src/scenes/dog_generator.gd`](../../src/scenes/dog_generator.gd)
- [`src/data_classes/lnz_parser.gd`](../../src/data_classes/lnz_parser.gd)
- [`src/scenes/editor/editor.tscn`](../../src/scenes/editor/editor.tscn)
