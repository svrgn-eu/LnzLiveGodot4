# LnzLive (Godot 4 Desktop Application)

![screenshot](screenshot.png)

Interactive desktop 3D previewer and editor for **Petz** `.lnz` files, built with **Godot 4 (.NET / C#)**.

## Requirements

- **Godot Editor**: **Godot 4.7.1 (.NET / Mono Edition)** (`4.7.1-stable-mono`, pinned in [`.fgvm-version`](.fgvm-version)) or newer 4.7.x+ .NET build. *The standard GDScript-only Godot editor is not supported.*
- **.NET SDK**: **.NET 10 SDK (`net10.0`)** to build [`PetzRendering.csproj`](PetzRendering.csproj).

## Documentation & Guides

- **[Root README](../README.md)** — Project overview, structure, and setup instructions
- **[Usage Guide (`GUIDE.md`)](GUIDE.md)** — Editor controls, keyboard shortcuts, and hexing tools
- **[Technical Documentation Index (`../doc/README.md`)](../doc/README.md)**:
  - [System Architecture & 3D Rendering Pipeline](../doc/Architecture-and-Rendering.md)
  - [Petz `.lnz` Format Specification](../doc/formats/LNZ-Format.md)
  - [Petz `.bhd` & `.bdt` Binary Animation Formats](../doc/formats/BHD-and-BDT-Formats.md)
  - [Petz 256-Color Palette & 8-Bit Indexed `.bmp` Textures](../doc/formats/Palettes-and-BMP-Textures.md)
  - [ADR-0001: Native Desktop Application Scope](../doc/adr/0001-desktop-application-scope.md)

## Status

This desktop application is in active development. If you encounter bugs or would like to contribute, please raise [an issue](https://github.com/svrgn-eu/LnzLiveGodot4/issues) or open a pull request.
