# Target Native Desktop Application Scope Instead of Web Browser Export

* **Status:** accepted
* **Date:** 2026-10-15
* **Authors:** efilnukefesin

## Context and Problem Statement

The upstream project ([mnemoli/LnzLive](https://github.com/mnemoli/LnzLive)) was originally designed in Godot 3 primarily as a web-based tool running in desktop browsers via HTML5/WebGL export, with a standalone Windows executable offered only as a fallback. Browser execution introduced several constraints—such as clipboard permission restrictions when pasting `.lnz` data, limited local filesystem access for custom `.bmp` textures and backups, and WebGL/WASM quirks in Godot 4. As part of migrating the project to Godot 4 and evolving it toward a full desktop pet/editor project, how should the target platform scope be defined?

## Decision Drivers

* Full, unrestricted access to the system clipboard (pasting `.lnz` sections and copying ball color overrides).
* Direct access to the local filesystem for loading/saving `.lnz` files, backups, and custom `.bmp` textures without browser sandbox limitations.
* Native desktop windowing and rendering capabilities in Godot 4 (essential for desktop pet features, transparent viewports, and native UI dialogs).
* Simplified maintenance and shader/rendering compatibility without HTML5/WebGL browser workarounds.

## Considered Options

* **Option 1: Native Desktop Application only** (Windows/Linux/macOS via Godot 4 desktop export)
* **Option 2: Web-first application** (HTML5/WebAssembly in the browser, matching upstream)
* **Option 3: Dual-target** (maintain both Web/HTML5 and Native Desktop builds)

## Decision Outcome

Chosen option: **"Option 1: Native Desktop Application only"**, because targeting the desktop directly removes browser sandbox limitations (clipboard, local storage, file I/O) and aligns with the project's Godot 4 architecture and desktop pet goals.

### Positive Consequences

* Reliable system clipboard integration (`DisplayServer.clipboard_set` and pasting into `TextEdit`) across all supported desktop environments.
* Straightforward local file management in Godot's `user://` data directory (e.g., `%APPDATA%/Godot/app_userdata/PetzRendering`) and future native file dialogs.
* No need to maintain HTML5 export templates, SharedArrayBuffer headers, or browser-specific WebGL shader workarounds.
* Enables native desktop window features (multi-window support, transparent background overlays, and higher rendering performance).

### Negative Consequences

* Users must download and run the desktop executable (or run the project via Godot 4) rather than opening a URL in a browser.
