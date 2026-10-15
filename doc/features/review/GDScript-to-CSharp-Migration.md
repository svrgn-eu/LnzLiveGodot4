# GDScript to C# (.NET) Migration

## 📖 Overview
Migrate the entire LnzLive codebase from GDScript (`.gd`) to C# (`.cs`) using Godot 4 (.NET), establishing strong static typing, improved IDE refactoring/debugging support, higher execution performance for file parsing and 3D pet generation, and a maintainable object-oriented architecture for future desktop pet features.

## 🎯 Goals & Use Cases
- **Goal 1:** Convert all existing GDScript data models, binary/text parsers (`.lnz`, `.bhd`, `.bdt`), 3D node controllers, and editor UI scripts to idiomatic C# (.NET 8+) classes.
- **Goal 2:** Replace loosely typed GDScript `Dictionary` and `Array` structures with strongly typed C# classes, structs, enums, and generic collections (`Dictionary<TKey, TValue>`, `List<T>`).
- **Goal 3:** Maintain 100% functional parity with the existing Godot 4 GDScript implementation (rendering, animation playback, LNZ editing, local storage management, and recoloring/mirroring tools).
- **Use Case A:** A developer modifying LNZ parsing or adding new Petz format features benefits from compile-time type safety, C# LINQ/binary readers, and full IDE autocompletion/refactoring.
- **Use Case B:** Loading, parsing, and scrubbing through frames of `.bhd`/`.bdt` animations and `.lnz` files executes with lower overhead in compiled C#.

## 👥 Target Audience / User Roles
- **Developers & Maintainers:** Contributors extending the desktop application, debugging `.lnz`/`.bhd`/`.bdt` parsers, and building new desktop pet systems in C#.
- **End Users (Petz Hexers):** Benefit from improved runtime stability, fewer dynamic typing runtime errors, and faster parsing/frame scrubbing.

## ⚙️ Functional Requirements
- [ ] **Requirement 1 (.NET Project Setup):** Initialize the `.csproj` and `.sln` files (`PetzRendering.csproj`) in `src/` targeting Godot.NET.Sdk for Godot 4.
- [ ] **Requirement 2 (Data Models & Enums):** Migrate all data classes in `src/data_classes/` (`BallData`, `AddBallData`, `LineData`, `PaintballData`, `Section`, `KeyBallsData`) to strongly typed C# classes/enums.
- [ ] **Requirement 3 (Binary & Text Parsers):** Migrate `BhdParser`, `BdtParser`, `LnzParser`, and `PetzTextureLoader` to C#, using `.NET` I/O or Godot `FileAccess`/`DirAccess` APIs while preserving `res://` and `user://` virtual path compatibility.
- [ ] **Requirement 4 (3D Rendering Nodes):** Migrate `Ball.gd`, `Line.gd`, `Paintball.gd`, and `rotator.gd` in `src/scripts/` to `Node3D`-derived C# scripts with `[Export]` properties and C# signals/events.
- [ ] **Requirement 5 (Pet Generator & Editor UI):** Migrate `dog_generator.gd` and all UI scripts under `src/scenes/editor/` (`FileTree`, `FPSLabel`, `FrameSlider`, `LnzTextEdit`, `PetViewContainer`, `PlayButton`, `ToolsMenu`, `unhandled_input_handler`) to C#.
- [ ] **Requirement 6 (Scene & Autoload Wiring):** Update `src/project.godot`, `src/Ball.tscn`, `src/Line.tscn`, `src/Paintball.tscn`, and `src/scenes/editor/editor.tscn` to reference the new `.cs` scripts and C# signal/method names, and remove obsolete `.gd` files once verified.

## 🎨 UI/UX Design & Interactions
- **No Visual Changes:** This is an architectural and language migration; the application layout, 3D viewport rendering, keyboard shortcuts, and tools menu interactions remain identical to the current desktop application.

## 💻 Technical Details
- **Project Configuration (`src/project.godot` & `src/PetzRendering.csproj`):**
  - `project/assembly_name="PetzRendering"` is already set under `[dotnet]` in `src/project.godot`.
  - Create `src/PetzRendering.csproj` using `Godot.NET.Sdk`.
- **Module-by-Module Migration Scope:**
  1. **Core Data & Enums (`src/data_classes/`):**
     - `section_enum.gd` → `SectionEnum.cs`
     - `ball_data.gd`, `addball_data.gd`, `line_data.gd`, `paintball_data.gd` → Strongly typed C# data models (`BallData.cs`, `AddBallData.cs`, `LineData.cs`, `PaintballData.cs`).
     - `key_balls_data.gd` → `KeyBallsData.cs` (registered as Autoload singleton in `project.godot`).
  2. **Parsers & Resource Loaders (`src/data_classes/`):**
     - `bhd_parser.gd` → `BhdParser.cs` (binary `.bhd` header and frame offset reader).
     - `bdt_parser.gd` → `BdtParser.cs` (binary `.bdt` frame transform parser).
     - `lnz_parser.gd` → `LnzParser.cs` (text `.lnz` section parser).
     - `petz_texture_loader.gd` → `PetzTextureLoader.cs` (8-bit indexed `.bmp` palette index texture loader).
  3. **3D Scene Scripts (`src/scripts/`):**
     - `Ball.gd` → `Ball.cs` (`Node3D`, updates `ShaderMaterial` parameters via `SetShaderParameter`, handles `Area3D` mouse/key selection and flash timer).
     - `Line.gd` → `Line.cs` (`Node3D`, updates line shader uniforms).
     - `Paintball.gd` → `Paintball.cs` (`Node3D`, updates paintball shader uniforms).
     - `rotator.gd` → `Rotator.cs` (`CameraHolder` rotation/pan/zoom controller).
  4. **Main Generator & Editor UI (`src/scenes/`):**
     - `dog_generator.gd` → `DogGenerator.cs` (orchestrates pet generation, extensions, projections, animations, and group visibility).
     - `editor/FileTree.gd` → `FileTree.cs`
     - `editor/LnzTextEdit.gd` & `src/scripts/TextEdit.gd` → `LnzTextEdit.cs`
     - `editor/PetViewContainer.gd` → `PetViewContainer.cs`
     - `editor/ToolsMenu.gd` → `ToolsMenu.cs`
     - `editor/FrameSlider.gd`, `editor/PlayButton.gd`, `editor/FPSLabel.gd`, `editor/unhandled_input_handler.gd` → `FrameSlider.cs`, `PlayButton.cs`, `FPSLabel.cs`, `UnhandledInputHandler.cs`.

## 🧪 Testing Plan
- **Build Verification:** Run `dotnet build` on `src/PetzRendering.csproj` with zero compilation errors or warnings.
- **Parser & Rendering Parity:**
  - Load both Dogz (`dachsund.lnz`, `scamp.lnz`) and Catz (`catz_base.lnz`, `cinn.lnz`) example files.
  - Verify balls, addballs, paintballs, lines, outlines, fuzz, and indexed `.bmp` textures render identically to the GDScript version.
- **Animation Playback:**
  - Play and scrub through animations using `FrameSlider` and `PlayButton`; switch animations and verify frame counts and ball transforms.
- **Interactive Editing & Tools:**
  - Test `Ctrl+S` save & refresh, `Ctrl+Q` ball flashing, `Select` mode hover/double-click/shortcut navigation (`B`/`Z`, `M`/`X`, `P`/`C`, `L`/`V`, `Delete`).
  - Test all `Ctrl+Space` Tools Menu actions: Color part/entire pet, Color Swap (`RecolorPopup`), Add Ball, Copy L to R, Move Head, and Copy ball colors to clipboard.
  - Test Local Storage file operations (Rename, Delete, Back up) and Local Textures loading from `user://resources/textures`.

## 🔗 Related Resources
- [`src/project.godot`](../../src/project.godot)
- [`src/scenes/dog_generator.gd`](../../src/scenes/dog_generator.gd)
- [`src/data_classes/lnz_parser.gd`](../../src/data_classes/lnz_parser.gd)
- [`src/scenes/editor/editor.tscn`](../../src/scenes/editor/editor.tscn)
