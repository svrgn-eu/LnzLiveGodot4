# LnzLive Documentation

Welcome to the technical documentation for **LnzLive (Godot 4 Desktop Fork)**. This directory contains comprehensive documentation on the program's purpose, internal architecture, 3D rendering pipeline, Petz file formats, architectural decision records (ADRs), and feature specifications.

---

## 1. Program Intention & Vision

**LnzLive** is a native desktop application built with **Godot 4** designed to parse, preview, animate, and edit **Petz** (`Catz` and `Dogz` by PF.Magic / Studio Mythos) `.lnz` character definition files in real time.

### Why LnzLive Exists
In the original 1990s/2000s *Petz* games, pets are not rendered using traditional 3D polygon meshes. Instead, they use a unique procedural 2.5D/3D rendering technique built from:
- **Ballz**: Screen-aligned 2D circles positioned in 3D space via skeletal animation frames.
- **Addballz**: Additional spheres parented to base skeletal balls to create custom anatomy, accessories, and breed details.
- **Linez**: Tapered 2D trapezoids/cones connecting pairs of balls to form limbs, torsos, tails, and ears.
- **Paintballz**: Decals (spots, patches, stripes, irises) projected onto the surface of a parent ball and clipped to its silhouette.

Historically, "hexers" (Petz modders and breed creators) had to edit `.lnz` text files in external tools (such as *LNZ Pro*), save them into binary `.pet` or `.bdt`/`.dll` resource files, launch the legacy Windows game, and wait for the pet to perform specific animations just to see how a single coordinate or color change looked.

**LnzLive solves this** by providing:
1. **Instant Live Previewing**: Edit `.lnz` code in a side-by-side text editor and press `Ctrl+S` to immediately regenerate the 3D pet in the viewport.
2. **Interactive 3D Inspection**: Rotate, pan, and zoom around the pet in 3D from any angle—even angles impossible to see in the original 2D-projected game—and scrub or play through all ~450+ skeletal animations frame-by-frame.
3. **Visual Picking & Navigation**: Hover or double-click directly on balls/addballs in the 3D viewport to jump straight to their corresponding `[Ballz Info]`, `[Add Ball]`, `[Move]`, `[Project Ball]`, or `[Linez]` definitions in the `.lnz` file (or press `Ctrl+Q` on a line of text to flash the ball in 3D).
4. **Automated Hexing Utilities**: Built-in tools for symmetrical mirroring (**Copy L to R**), batch recoloring (**Color Swap** and body-part recoloring), adding connected addballs, and moving head assemblies.
5. **Foundation for a Godot 4 Desktop Pet Engine**: By accurately implementing the Petz `.lnz`, `.bhd`, `.bdt`, and palette-indexed `.bmp` rendering pipeline in Godot 4, this project serves both as a standalone editor and as the core rendering engine for a modern Desktop Pet application.

---

## 2. Documentation Contents

### Architecture & Rendering
- [**System Architecture & 3D Rendering Pipeline**](Architecture-and-Rendering.md)
  - Complete walkthrough of the application architecture, the 7-stage pet generation pipeline in [`dog_generator.gd`](../src/scenes/dog_generator.gd), and the custom Godot 4 spatial shaders ([`ball.tres`](../src/shaders/ball.tres), [`paintball.tres`](../src/shaders/paintball.tres), [`line.tres`](../src/shaders/line.tres)).

### File Format Specifications
- [**`.lnz` Text Format Specification**](formats/LNZ-Format.md)
  - Detailed syntax and field-by-field reference for every section parsed and modified by LnzLive (`[Species]`, `[Texture List]`, `[Default Scales]`, `[Leg/Body/Face/Ear Extension]`, `[Head/Feet Enlargement]`, `[Omissions]`, `[Ballz Info]`, `[Add Ball]`, `[Move]`, `[Project Ball]`, `[Linez]`, and `[Paint Ballz]`).
- [**`.bhd` & `.bdt` Binary Animation Formats**](formats/BHD-and-BDT-Formats.md)
  - Byte-level specification of the binary skeletal header (`.bhd`) and frame transform data (`.bdt`) files, including endianness, offset tables, signed integer decoding, and coordinate space conversion.
- [**256-Color Palette & 8-Bit Indexed `.bmp` Textures**](formats/Palettes-and-BMP-Textures.md)
  - In-depth guide to `petzpalette.png`, 10-shade color ramps, Z-depth palette shifting, `transparent_color` modes, and the custom 8-bit uncompressed `.bmp` index loader ([`petz_texture_loader.gd`](../src/data_classes/petz_texture_loader.gd)).

### Project Governance & Planning
- **Architecture Decision Records (`doc/adr/`)**:
  - [ADR-0001: Target Native Desktop Application Scope Instead of Web Browser Export](adr/0001-desktop-application-scope.md)
  - [ADR Template](adr/_ADR_Template.md)
- **Features (`doc/features/`)**:
  - Implemented: [Animation Selection Dropdown](features/implemented/Animation-Selection-Dropdown.md)
  - In Review: [GDScript to C# (.NET) Migration](features/review/GDScript-to-CSharp-Migration.md)
  - [Feature Template](features/_Feature_Template.md)
- **Bug Reports (`doc/bugs/`)**:
  - [Bug Report Template](bugs/_Bug_Template.md)
