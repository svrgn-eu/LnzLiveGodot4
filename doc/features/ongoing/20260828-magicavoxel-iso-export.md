# [Pipeline] MagicaVoxel ISO Export Settings

## 📖 Overview
Adjust the export settings for ISO in MagicaVoxel to support a new asset pipeline requirement. The goal is to smash all layers together and output a file of 32x64 px in eight directions.

## 🎯 Goals & Use Cases
- **Goal 1:** Smash all layers together during export.
- **Goal 2:** Output files at exactly 32x64 px resolution.
- **Goal 3:** Render the output in eight different directions (isometric/directional views).
- **Goal 4:** Support exporting different Character variations by enabling or disabling specific extra layers (e.g., "Extra: Cup Size", "Extra: Rectangular Body").
- **Goal 5:** Export all animations into a format that can be easily imported into Godot.
- **Use Case A:** An artist is exporting a voxel character or prop and needs it to perfectly fit into the 2D isometric engine with 8-directional sprites.
- **Use Case B:** An artist defines multiple body types or variations in a single `.vox` file using layers and wants to batch export all variations without manual toggling.

## 👥 Target Audience / User Roles
- 3D Artists / Voxel Artists
- Technical Artists managing the asset pipeline

## ⚙️ Functional Requirements
- [ ] Requirement 1: The export process must flatten ("smash") all MagicaVoxel layers into a single layer/object before or during export.
- [ ] Requirement 2: The output image dimensions must be precisely 32x64 px per frame/direction.
- [ ] Requirement 3: The export must generate 8 distinct directional views (N, NE, E, SE, S, SW, W, NW).
- [ ] Requirement 4: The process should support a configuration to selectively enable or disable layers (e.g., "Extra: Cup Size", "Extra: Rectangular Body") per export process to produce character variations. Those layers can be identified by the prefix "Extra".
- [ ] Requirement 5: Any hidden layers that are not part of the "Extras" (i.e., lacking the "Extra" prefix) must automatically be set to visible during export.
- [ ] Requirement 6: **Data Safety:** The automated script must *never* modify, overwrite, or delete layers in the original source `.vox` file. All modifications must happen on a temporary copy or purely in memory during the export process.
- [ ] Requirement 7: The process should ideally be automated or documented as a standard operating procedure (SOP) for external pipeline tools, seamlessly handling new "Extra" layers as they are added.
- [ ] Requirement 8: All animations must be exported into a format that can be easily imported into Godot (e.g., a sprite sheet or a format directly readable by Godot's importer).

## 🎨 UI/UX Design & Interactions
- Since this is an **External Pipeline** feature, this primarily involves configuring MagicaVoxel's internal render/export settings or using a wrapper script/tool to batch process the `.vox` files.
- The pipeline script or documentation should be easy to run for artists.

## 💻 Technical Details
- **Tooling:** MagicaVoxel (Export Settings, Project settings, or CLI if applicable).
- **Vendored Toolchain:** A portable, self-contained copy of MagicaVoxel with the pre-configured distinct export settings (like camera angle, resolution, and layer options) is located directly within the project folder at `tools\MagicaVoxel`. This guarantees that anyone running the pipeline uses the exact same version and configuration, avoiding inconsistencies.
- **External Scripts:** May require an external script (e.g., Python, ImageMagick) to crop/resize the output to exactly 32x64 px if MagicaVoxel cannot natively constrain the canvas size perfectly.
- **Target Implementation Directory:** All custom scripts, configuration files, and tools built for this export pipeline must reside in `helpers/MagicaVoxel Export`.
- **Pipeline Integration:** Document the exact camera angles and lighting setup in MagicaVoxel to ensure consistent 8-directional rendering.

### Proposed Approach for Character Variations
- **Pre-processing Script:** Since MagicaVoxel does not natively support batch exporting multiple layer visibility combinations in a single click, the pipeline should introduce an external script (e.g., using Python and a library like `py-vox-io`).
- **Configuration-Driven:** The script reads a configuration file (e.g., `variations.json`) that defines which layers should be active for which character variation. 
- **Temporary `.vox` Generation:** For each variation (e.g., "Female Base + Cup Size"), the script parses the master `.vox` file, makes any non-Extra hidden layers visible, drops or hides the unneeded "Extra" layers, and saves a temporary `.vox` file.
- **Batch Export:** The temporary `.vox` files are then fed into the automated export/render pipeline to generate the flattened 32x64 px 8-directional sprites. 
- **Scalability:** As new layers are added in MagicaVoxel, they can simply be referenced in the JSON configuration without changing the underlying pipeline code.

### Implementation Plan for Export & Settings Modifications
1. **Camera & Scene Configuration:**
   - Set the MagicaVoxel camera to **Orthogonal** projection to avoid perspective distortion.
   - Configure the viewport pitch and yaw to standard isometric angles (e.g., Pitch 30, Yaw 45) for the default forward-facing direction.
   - Adjust the camera scale/zoom so the character's bounding box fits neatly into a 32x64 space.
2. **Batch Export Automation (8-Directions):**
   - Develop a pipeline script (e.g., Python) that either interfaces with MagicaVoxel's CLI or uses a headless voxel renderer.
   - The script will iterate through the 8 directions by rotating the camera yaw in 45-degree increments (0, 45, 90, 135, 180, 225, 270, 315).
   - For each angle, trigger a 2D image export (PNG).
3. **Layer Flattening:**
   - During the temporary `.vox` generation step, the script will ensure non-Extra hidden layers are made visible, and merge all enabled layers (including the base body and selected "Extra" body types) into a single volume object.
4. **Post-Processing (Resolution Enforcement):**
   - Since MagicaVoxel's native export might include excess transparent padding, use ImageMagick or Python's Pillow library as a final step.
   - This script will trim the transparent pixels, center the sprite, and apply strict canvas boundaries to guarantee an exact 32x64 px output.
5. **Animation Export (Godot Integration):**
   - Package the exported animation frames into a sprite sheet or provide accompanying metadata to allow for easy and direct import into Godot (e.g., for use with `SpriteFrames` or `AnimationPlayer`).

## 🧪 Testing Plan
- Export a test character with multiple layers.
- Verify that the resulting sprites are flattened.
- Verify the output resolution is exactly 32x64 px.
- Verify all 8 directions are present and visually consistent.

## 🔗 Related Resources
- MagicaVoxel Documentation
- [30 Asset Pipeline.md](../../30 Asset Pipeline.md)
- [90-1 MagicaVoxel_Character_Pipeline.md](../../90-1 MagicaVoxel_Character_Pipeline.md)
