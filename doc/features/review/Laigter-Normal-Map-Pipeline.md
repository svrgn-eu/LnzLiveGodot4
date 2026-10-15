# Semi-Automated Normal Map Pipeline (Laigter + IsoVoxel)

## 📖 Overview
A semi-automated asset pipeline that extends the existing MagicaVoxel and IsoVoxel workflow. Triggered manually by a developer via script, it generates 2D normal maps for all isometric character and prop sprites, and automatically builds Godot `.tres` resource files (like `CanvasTexture`) to preserve animations and enable dynamic 2D lighting without requiring manual setup in the engine.

## 🎯 Goals & Use Cases
- **Goal 1:** Eliminate manual normal map baking from the art pipeline.
- **Goal 2:** Automatically generate normal maps (`_n.png`) alongside diffuse sprites.
- **Goal 3:** Keep the MagicaVoxel `.vox` file as the single source of truth without requiring intermediate `.obj` exports.
- **Use Case A:** An artist updates the `Female Base.vox` file in MagicaVoxel, runs the build script, and immediately receives both the updated isometric sprites and perfectly aligned normal maps.

## 👥 Target Audience / User Roles
- **Artists:** Want to focus strictly on voxel modeling in MagicaVoxel without getting bogged down by 3D rendering technicalities.
- **Developers:** Need automated scripts to ensure all generated sprites have corresponding normal map textures for dynamic lighting in engine.

## ⚙️ Functional Requirements
- [ ] Requirement 1: Provide a batch/build script (e.g., `build_sprites.bat`) that sequences IsoVoxel, Laigter CLI, and a `.tres` generation script.
- [ ] Requirement 2: The pipeline must recursively process output directories from IsoVoxel.
- [ ] Requirement 3: Laigter CLI must be configured with presets (e.g., a `.conf` file) to ensure consistent bump depth and lighting across all sprites.
- [ ] Requirement 4: The script must automatically generate Godot `CanvasTexture` `.tres` files linking both the diffuse and normal maps.
- [ ] Requirement 5: The script must preserve character animations, properly generating animated resources (e.g., `SpriteFrames`) that reference the paired `CanvasTexture` frames.

## 💻 Technical Details
### The Semi-Automated Flow
1. **Source of Truth:** MagicaVoxel (`.vox`) files stored in the workspace.
2. **IsoVoxel Pass:** A developer manually runs the build script, which invokes `IsoVoxel.exe <filename.vox>`. IsoVoxel processes the voxel data and spits out folders containing 2D `.png` isometric renders (diffuse maps) in multiple sizes and perspectives (including animation frames).
3. **Laigter Pass:** The script then invokes the Laigter tool located at `tools\laigter-windows\laigter.exe` in headless mode targeting the IsoVoxel output directories. 
   - Command structure: `tools\laigter-windows\laigter.exe --no-gui -d <directory_path> -n <output_path> -r my_preset.conf`
   - Laigter reads the shape and colors of the 2D sprites and algorithmically generates a normal map (saving it as `<filename>_n.png`).
4. **Godot .tres Generation:** A final processing step in the script creates a Godot `.tres` file (type `CanvasTexture`) for each individual image frame, linking the diffuse and normal maps (modeled directly after `src/assets/tiles/DemoCover01.tres`).
5. **SpriteFrames Assembly:** For animated characters, the script automatically assembles all the generated `CanvasTexture` `.tres` frames into a final Godot `SpriteFrames` resource (modeled after `src/assets/sprites/DemoSpriteFrames.tres`). This groups the directional frames and preserves the animation loop, so the asset is immediately ready to be dropped into a Godot scene with full lighting support.

### Configuration
Laigter uses a configuration file for its settings (bump scale, smooth, etc.). We will need to test and establish a standardized `.conf` preset for IsoTactics characters to ensure they look stylized and appropriate under the game's lighting conditions.

## 🧪 Testing Plan
- Create a test script with a sample `.vox` file (e.g., a character and a blocky prop).
- Verify that `laigter-cli` generates normal maps for all frames of animation.
- Import the resulting diffuse and normal map pairs into a Godot test scene.
- Apply a `PointLight2D` and verify that the normals react correctly to light direction (e.g., light from the right highlights the right side of the sprite).

## 🔗 Related Resources
- [Laigter Official Documentation & CLI Guide](https://bauxitedev.itch.io/laigter)
- [IsoVoxel Tool](../../tools/IsoVoxel/Usage.txt)
- [Skin Tones Guide](../33 Skin Tones.md) (Reference for voxel coloring that affects bump map generation).
