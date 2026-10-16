# LnzLive (Godot 4 Desktop Fork)

![LnzLive Screenshot](src/screenshot.png)

A native desktop fork of [mnemoli/LnzLive](https://github.com/mnemoli/LnzLive) — an interactive 3D previewer, editor, and rendering engine foundation for **Petz** (`Catz` / `Dogz`) `.lnz` files, migrated from **Godot 3** to **Godot 4**.

---

## Overview & Program Intention

**LnzLive** is a native desktop application (`v1.0.0`) built with **Godot 4** designed to parse, preview, animate, and edit **Petz** (`Catz` and `Dogz` by PF.Magic / Studio Mythos) `.lnz` character definition files in real time.

### Why LnzLive Exists
In the original 1990s/2000s *Petz* games, pets are not rendered using traditional 3D polygon meshes. Instead, they use a unique procedural 2.5D/3D rendering technique built from:
- **Ballz**: Screen-aligned 2D circles positioned in 3D space via skeletal animation frames.
- **Addballz**: Additional spheres parented to base skeletal balls to create custom anatomy, accessories, and breed details.
- **Linez**: Tapered 2D trapezoids/cones connecting pairs of balls to form limbs, torsos, tails, and ears.
- **Paintballz**: Decals (spots, patches, stripes, irises) projected onto the surface of a parent ball and clipped to its silhouette.

Historically, "hexers" (Petz modders and breed creators) had to edit `.lnz` text files in external tools (such as *LNZ Pro*), save them into binary `.pet` or `.cat`/`.dog` DLL resource files, launch the legacy Windows game, and wait for the pet to perform specific animations just to see how a single coordinate or color change looked.

**LnzLive solves this** by providing:
1. **Instant Live Previewing**: Edit `.lnz` code in a side-by-side text editor and press `Ctrl+S` to immediately regenerate the 3D pet in the viewport.
2. **Interactive 3D Inspection**: Rotate, pan, and zoom around the pet in 3D from any angle—even angles impossible to see in the original 2D-projected game—and scrub or play through all skeletal animations frame-by-frame (`445` Dogz animations and `493` Catz animations).
3. **Visual Picking & Navigation**: Hover or double-click directly on balls/addballs in the 3D viewport to jump straight to their corresponding `[Ballz Info]`, `[Add Ball]`, `[Move]`, `[Project Ball]`, or `[Linez]` definitions in the `.lnz` file (or press `Ctrl+Q` on a line of text to flash the ball in 3D).
4. **Automated Hexing Utilities**: Built-in tools for symmetrical mirroring (**Copy L to R**), batch recoloring (**Color Swap** and body-part recoloring), adding connected addballs, and moving head assemblies.
5. **Foundation for a Godot 4 Desktop Pet Engine**: By accurately implementing the Petz `.lnz`, `.bhd`, `.bdt`, and palette-indexed `.bmp` rendering pipeline in Godot 4, this project serves both as a standalone editor and as the core rendering engine for a modern Desktop Pet application.

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

## Technical Documentation (`doc/`)

Comprehensive documentation on the program's internal architecture, 3D rendering pipeline, Petz file formats, architectural decision records (ADRs), and feature specifications is located in [`doc/`](doc/):

### Architecture & Rendering
- [**System Architecture & 3D Rendering Pipeline (`doc/Architecture-and-Rendering.md`)**](doc/Architecture-and-Rendering.md)
  - Complete walkthrough of the application architecture, the 7-stage pet generation pipeline in [`dog_generator.gd`](src/scenes/dog_generator.gd), and the custom Godot 4 spatial shaders ([`ball.tres`](src/shaders/ball.tres), [`paintball.tres`](src/shaders/paintball.tres), [`line.tres`](src/shaders/line.tres)).

### File Format Specifications (`doc/formats/`)
- [**Petz `.lnz` Text Format Specification (`doc/formats/LNZ-Format.md`)**](doc/formats/LNZ-Format.md)
  - Detailed syntax and field-by-field reference for every section parsed and modified by LnzLive (`[Species]`, `[Texture List]`, `[Default Scales]`, `[Leg/Body/Face/Ear Extension]`, `[Head/Feet Enlargement]`, `[Omissions]`, `[Ballz Info]`, `[Add Ball]`, `[Move]`, `[Project Ball]`, `[Linez]`, and `[Paint Ballz]`), plus `.lnz` file archetypes (Master, Breed, Kitten/Puppy, and Dumped Pet) and `#2`/`#1`/`##` variation blocks.
- [**Petz `.bhd` & `.bdt` Binary Animation Formats (`doc/formats/BHD-and-BDT-Formats.md`)**](doc/formats/BHD-and-BDT-Formats.md)
  - Byte-level specification of the binary skeletal header (`.bhd`) and frame transform data (`.bdt`) files (`445` Dogz and `493` Catz animations), including endianness, offset tables, signed integer decoding, and coordinate space conversion.
- [**Catz Skeletal Animations Catalog (`0`–`492`) (`doc/formats/Catz-Animations-Catalog.md`)**](doc/formats/Catz-Animations-Catalog.md)
  - Complete index of all 493 Catz animations (`CAT0.bdt`–`CAT492.bdt`), including frame counts, cumulative `First` and `Last` frame indices (`0`–`16035`), loop flags, and behavior descriptions.
- [**Petz 256-Color Palette & 8-Bit Indexed `.bmp` Textures (`doc/formats/Palettes-and-BMP-Textures.md`)**](doc/formats/Palettes-and-BMP-Textures.md)
  - In-depth guide to `petzpalette.png`, the 14 texturable 10-shade color ramps (`10`–`149`), Z-depth palette shifting, `transparent_color` modes, and the custom 8-bit uncompressed `.bmp` index loader ([`petz_texture_loader.gd`](src/data_classes/petz_texture_loader.gd)).
- [**Petz 4/5 Exact 256-Color Palette (`doc/formats/Petz-Color-Palette.md`)**](doc/formats/Petz-Color-Palette.md) & [**Workshop Color Chart (`doc/formats/Petz_Workshop_Color_Chart.png`)**](doc/formats/Petz_Workshop_Color_Chart.png)
  - Complete 256-entry (`0`–`255`) hex color lookup table and visual $16 \times 16$ index chart.
- [**Petz `.pet` Binary File Format v3 Specification (`doc/formats/PET-Format.md`)**](doc/formats/PET-Format.md)
  - Reverse-engineered specification of P.F. Magic `.pet` files (Load Info, Checksums, Saved Color Info, Embedded LNZ, Veterinary History, Genome Chromosomes/Alleles, Ancestry, and Thumbnail Bitmap).

### Project Governance & Planning
- **Architecture Decision Records (`doc/adr/`)**:
  - [ADR-0001: Target Native Desktop Application Scope Instead of Web Browser Export](doc/adr/0001-desktop-application-scope.md)
  - [ADR Template (`doc/adr/_ADR_Template.md`)](doc/adr/_ADR_Template.md)
- **Feature Specifications (`doc/features/`)**:
  - Implemented:
    - [Animation Selection Dropdown](doc/features/implemented/Animation-Selection-Dropdown.md)
  - In Review:
    - [Animation Linked Lists & Sequencer Window](doc/features/review/Animation-Linked-Lists-and-Sequencer.md)
    - [CI/CD Pipelines (GitHub Actions & GitLab CI) with FGVM](doc/features/review/CI-CD-GitHub-Actions-and-GitLab-Pipelines.md)
    - [GDScript to C# (.NET) Migration](doc/features/review/GDScript-to-CSharp-Migration.md)
    - [Godot Project Directory Reorganization & Standardization](doc/features/review/Godot-Project-Directory-Reorganization.md)
    - [Modern UI Overhaul & Theme System](doc/features/review/Modern-UI-Overhaul-and-Theme-System.md)
  - [Feature Template (`doc/features/_Feature_Template.md`)](doc/features/_Feature_Template.md)
- **Bug Reports (`doc/bugs/`)**:
  - [Bug Report Template (`doc/bugs/_Bug_Template.md`)](doc/bugs/_Bug_Template.md)

---

## Project Structure

```text
├── doc/                                  # Project documentation, format specs, ADRs, and features
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
│   │   │   ├── Animation-Linked-Lists-and-Sequencer.md
│   │   │   ├── CI-CD-GitHub-Actions-and-GitLab-Pipelines.md
│   │   │   ├── GDScript-to-CSharp-Migration.md
│   │   │   ├── Godot-Project-Directory-Reorganization.md
│   │   │   └── Modern-UI-Overhaul-and-Theme-System.md
│   │   ├── reviewed/
│   │   └── working_on/
│   └── formats/                          # Detailed Petz file format specifications & references
│       ├── BHD-and-BDT-Formats.md
│       ├── Catz-Animations-Catalog.md
│       ├── LNZ-Format.md
│       ├── PET-Format.md
│       ├── Palettes-and-BMP-Textures.md
│       ├── Petz-Color-Palette.md
│       └── Petz_Workshop_Color_Chart.png
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
└── README.md                             # Project root README & documentation index
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
- **Wiki & Documentation**: [LnzLive on Petz Wiki](https://petz.miraheze.org/wiki/LnzLive), [Petz Wiki: LNZ](https://petz.miraheze.org/wiki/LNZ), [Petz Wiki: Ballz](https://petz.miraheze.org/wiki/Ballz), [Petz Wiki: Hexing](https://petz.miraheze.org/wiki/Hexing)
- **Format & Animation References**: [Reflet's `.pet` File Format Breakdown](https://petzforum.proboards.com/thread/71837/full-breakdown-pet-file-format), [Secret Petz Catz Animations](https://secretpetz.neocities.org/Petz%20animations/cat), [Carolyn's Creations / Fantazzled](https://oasis.fantazzled.com/)
- **Shader & Line Drawing Reference**: [Ratshack Godot Line Drawing Notes](https://ratshack.neocities.org/text/godot/linedrawing)
