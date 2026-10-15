# LnzLive (Godot 4 Desktop Fork)

![LnzLive Screenshot](src/screenshot.png)

A native desktop fork of [mnemoli/LnzLive](https://github.com/mnemoli/LnzLive) — an interactive 3D previewer, editor, and rendering engine foundation for **Petz** (`Catz` / `Dogz`) `.lnz` files, migrated from **Godot 3** to **Godot 4**.

---

## Overview

**LnzLive** is a native desktop application (`v1.0.0`) that lets you load, inspect, edit, and preview Petz `.lnz` breed and pet files in real time. It accurately emulates the classic PF.Magic 2.5D/3D "Ballz & Linez" rendering pipeline—complete with ballz, addballz, tapered linez, surface-clipped paintballz, 256-color palette shifting, 8-bit indexed `.bmp` textures, and `.bhd`/`.bdt` skeletal animation playback.

### Key Achievements in This Fork

- **Desktop-First Application Scope** ([ADR-0001](doc/adr/0001-desktop-application-scope.md)):
  - Scoped specifically as a standalone native desktop application rather than a browser-based web export, providing unrestricted system clipboard access, local `user://` file storage, and a foundation for desktop pet features.
- **Godot 3 → Godot 4 Migration**:
  - Upgraded project configuration ([`src/project.godot`](src/project.godot), `config_version=5`, `gl_compatibility` renderer) and scene/resource formats (`format=3`).
  - Migrated all scripts to **GDScript 2.0** (`@export`, `@onready`, `Node3D` hierarchy, `ShaderMaterial.set_shader_parameter`, `DisplayServer.clipboard_set`, and `FileAccess`/`DirAccess` APIs).
  - Ported custom spatial shaders ([`ball.tres`](src/shaders/ball.tres), [`line.tres`](src/shaders/line.tres), [`paintball.tres`](src/shaders/paintball.tres)) to Godot 4's shader language, including Reverse-Z clip-space depth ordering for lines, `INV_VIEW_MATRIX` billboarding, `ALPHA_SCISSOR_THRESHOLD`, and nearest-neighbor palette/texture sampling.
- **UX & Editor Improvements**:
  - Replaced manual numeric animation entry with a dynamic **Animation Selection Dropdown** ([`AnimPicker.gd`](src/scenes/editor/AnimPicker.gd), [Feature Spec](doc/features/implemented/Animation-Selection-Dropdown.md)).
  - Fixed startup dialog/popup visibility for Godot 4 `Window` and `Popup` nodes in [`editor.tscn`](src/scenes/editor/editor.tscn).
  - Reorganized the repository into [`src/`](src/) (Godot 4 project) and [`doc/`](doc/) (technical documentation, format specifications, ADRs, and feature lifecycle tracking).

---

## Technical Documentation

Detailed documentation on the program's purpose, internal rendering pipeline, and Petz file formats is located in [`doc/`](doc/README.md):

- **[Documentation Index & Program Overview (`doc/README.md`)](doc/README.md)**
- **[System Architecture & 3D Rendering Pipeline (`doc/Architecture-and-Rendering.md`)](doc/Architecture-and-Rendering.md)** — Scene hierarchy, the 7-stage pet generation pipeline, and custom spatial shader math.
- **File Format Specifications (`doc/formats/`)**:
  - **[Petz `.lnz` Text Format Specification](doc/formats/LNZ-Format.md)** — Section-by-section syntax and field reference (`[Ballz Info]`, `[Add Ball]`, `[Linez]`, `[Paint Ballz]`, `[Move]`, `[Project Ball]`, extensions, and scales).
  - **[Petz `.bhd` & `.bdt` Binary Animation Formats](doc/formats/BHD-and-BDT-Formats.md)** — Byte-level layout of skeletal headers, frame offset tables, and signed binary frame transforms.
  - **[Petz 256-Color Palette & 8-Bit Indexed `.bmp` Textures](doc/formats/Palettes-and-BMP-Textures.md)** — `petzpalette.png` 10-shade color ramps, Z-depth shading, `transparent_color` modes, and custom 8bpp `.bmp` index loading.
- **Architecture Decision Records (`doc/adr/`)**:
  - **[ADR-0001: Target Native Desktop Application Scope Instead of Web Browser Export](doc/adr/0001-desktop-application-scope.md)**
- **Feature Specifications (`doc/features/`)**:
  - Implemented: **[Animation Selection Dropdown](doc/features/implemented/Animation-Selection-Dropdown.md)**
  - In Review: **[GDScript to C# (.NET) Migration](doc/features/review/GDScript-to-CSharp-Migration.md)**

---

## Project Structure

```text
├── doc/                                  # Project documentation, format specs, ADRs, and features
│   ├── README.md                         # Documentation index & program intention
│   ├── Architecture-and-Rendering.md     # 7-stage generation pipeline & shader documentation
│   ├── adr/                              # Architecture Decision Records
│   │   ├── _ADR_Template.md
│   │   └── 0001-desktop-application-scope.md
│   ├── bugs/                             # Bug tracking templates and solved reports
│   │   ├── _Bug_Template.md
│   │   └── solved/
│   ├── features/                         # Feature lifecycle directories
│   │   ├── _Feature_Template.md
│   │   ├── discarded/
│   │   ├── implemented/
│   │   │   └── Animation-Selection-Dropdown.md
│   │   ├── ongoing/
│   │   ├── review/
│   │   │   └── GDScript-to-CSharp-Migration.md
│   │   ├── reviewed/
│   │   └── working_on/
│   └── formats/                          # Detailed Petz file format specifications
│       ├── BHD-and-BDT-Formats.md
│       ├── LNZ-Format.md
│       └── Palettes-and-BMP-Textures.md
├── src/                                  # Godot 4 project root (v1.0.0)
│   ├── data_classes/                     # Parsers (.lnz, .bhd, .bdt, .bmp) and data models
│   ├── resources/                        # Example .lnz pets, .bhd/.bdt animations, palettes, and .bmp textures
│   ├── scenes/                           # Main editor scene (editor.tscn), UI scripts, and dog_generator.gd
│   ├── scripts/                          # 3D primitive controllers (Ball.gd, Line.gd, Paintball.gd, rotator.gd)
│   ├── shaders/                          # Custom spatial shaders (ball.tres, line.tres, paintball.tres)
│   ├── Ball.tscn                         # 3D Ball / Addball scene
│   ├── Line.tscn                         # 3D Line scene
│   ├── Paintball.tscn                    # 3D Paintball scene
│   ├── GUIDE.md                          # User guide for editor controls and hexing tools
│   ├── README.md                         # Godot project directory README
│   └── project.godot                     # Godot 4 engine configuration
└── README.md
```

---

## Getting Started

1. **Install Godot 4**: Download [Godot Engine 4.x](https://godotengine.org/download/).
2. **Import the Project**:
   - Open the Godot Project Manager.
   - Click **Import** and select [`src/project.godot`](src/project.godot).
3. **Run**: Press **F5** (or click the Play button in the top-right corner of the editor) to launch the desktop application (`res://scenes/editor/editor.tscn`).

---

## Usage & Controls

For full user instructions on editing `.lnz` files, keyboard shortcuts, local user storage (`%APPDATA%/Godot/app_userdata/PetzRendering`), and built-in hexing tools (Add Ball, Color Swap, Copy L to R, Move Head, etc.), see the [**Usage Guide (`src/GUIDE.md`)**](src/GUIDE.md).

### Quick Controls
- **Select Pet**: Double-click an entry under **Examples** or **Local Storage** in the left panel
- **Select Animation**: Choose an animation from the top-left **Anim** dropdown or scrub the frame slider
- **Rotate Pet**: Left-click + drag in the 3D view
- **Pan View**: Right-click + drag
- **Zoom**: Mouse wheel
- **Save & Refresh Preview**: `Ctrl + S`
- **Tools Menu**: `Ctrl + Space` in the pet view
- **Highlight Ball from LNZ**: Place cursor on a line in `[Ballz Info]` or `[Add Ball]` and press `Ctrl + Q`

---

## Credits & References

- **Original Project**: [mnemoli/LnzLive](https://github.com/mnemoli/LnzLive) by [mnemoli](https://github.com/mnemoli)
- **Wiki & Documentation**: [LnzLive on Petz Wiki](https://petz.miraheze.org/wiki/LnzLive)
- **Shader & Line Drawing Reference**: [Ratshack Godot Line Drawing Notes](https://ratshack.neocities.org/text/godot/linedrawing)
