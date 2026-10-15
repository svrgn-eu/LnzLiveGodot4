# Futuristic Menu Button Implementation

## Overview
Based on a review of the button design proposals in the `Menu Button Styles` directory, the target UI aesthetic leans heavily into a sleek, cyberpunk-inspired pixel art style. The designs showcase:
- **Idle States:** Minimalist, grayscale base structures featuring crisp 1px traces, angled chamfers, cyber-brackets, and data nodes.
- **Hover & Active States:** Vibrant, neon recoloring (cyan, pink, purple, etc.) with dynamic glowing bloom effects that bleed slightly into the surrounding UI space. These states utilize varying structural accents like heavy-edge brackets, binary traces, and hex-tile nodes.

To accurately translate these pixel art mockups into Godot while maintaining asset flexibility and performance, we will follow a dynamic, engine-driven approach guided by our existing architecture.

## Architectural Alignment
This implementation plan strictly adheres to our Architectural Decision Records:
- **[ADR-0017: Pixel Art Style and Engine Effects](../../../adr/ADR-0017%20Pixel%20Art%20Style%20and%20Engine%20Effects.md):** Base assets will remain pure, flat grayscale pixel art. All coloring, glows, and effects will be applied dynamically in Godot to prevent asset bloat and allow for responsive runtime interaction.
- **[ADR-0014: UI Controller Naming Convention](../../../adr/ADR-0014%20UI%20Controller%20Naming%20Convention.md):** Any C# script handling these button states will be named appropriately using the `Scr[Name]Controller` pattern to clearly designate it as a UI controller for its respective scene.

## Implementation Steps

To implement these specific pixel art UI designs and futuristic glow effects in Godot, we will use a combination of 9-slice texturing for the structural frames and HDR modulation for the dynamic interactions.

### 1. Configure Pixel-Perfect Import
* **Texture Settings:** Select your UI sprite sheets in the FileSystem dock. In the Import dock, change the **Filter** mode to **Nearest** and click **Reimport**. Repeat this for any UI element to prevent the engine from applying bilinear filtering.
* **Verify:** Drag the imported sprite into a 2D scene; the 1px traces and data nodes should render perfectly crisp without any soft edges.

### 2. Set Up 9-Slice StyleBoxes
* **UI Theme:** Create a new `Theme` resource and assign it to your root UI `Control` node. Under the `Button` class, assign a `StyleBoxTexture` to the normal, hover, and pressed states. Load your respective idle and active sprite variations into each.
* **Texture Margins:** Expand the Texture Margin properties (Left, Top, Right, Bottom) and set them to the exact thickness of your corners (e.g., 3 pixels) to protect the angular chamfers and brackets from distorting.
* **Verify:** Resize the button in the 2D viewport; the horizontal and vertical lines should stretch, but the intricate corner details must remain exactly 1 to 3 pixels thick.

### 3. Centralize Styles via Theme Variations (No Code Required)
To make your button designs completely centralized and reusable, avoid C# scripting for hover colors and instead build them directly into the Godot Theme using **Type Variations**.
* **Create a Theme Variation:** Open your `Theme` resource. Add a new Type Variation (e.g., `CyberButton` or `PrimaryButton`) that inherits from `Button`.
* **StyleBox Modulation:** Assign your grayscale 9-slice `StyleBoxTexture` to the `normal`, `hover`, and `pressed` states. For the `hover` and `pressed` StyleBoxes, locate the **Modulate** property on the texture itself.
* **HDR Colors:** Click the color picker, toggle on **Raw** mode, and input your chosen neon color. Increase the RGB values above `1.0` (e.g., set Green to `1.5` instead of `1.0`) to push the color into the HDR range.
* **Apply to Nodes:** On any `Button` node in your UI, simply set its **Theme Type Variation** property to `CyberButton`. It will instantly inherit the exact pixel margins and neon hover colors centrally from the Theme.

### 4. Enable Futuristic Glow (Bloom)
* **WorldEnvironment:** Add a `WorldEnvironment` node to your main scene, create a new `Environment` resource, and set the **Background Mode** to **Canvas**. Enable **Glow**, set the **Blend Mode** to **Screen**, and lower the **HDR Threshold** to around `0.9`.
* **Verify:** Because your `StyleBoxTexture` modulate colors were pushed above `1.0` in the Theme, the idle grey borders will remain completely flat, while the active and hover states will automatically emit a soft, localized neon halo that bleeds slightly into the surrounding UI space.

## Next Steps

1. **Choose a Design:** Select one of the proposed button styles (e.g., Heavy-Edge Angled Chamfer or Cyber-Bracket) as the definitive starting template.
2. **Create Aseprite File:** Recreate the chosen base design in Aseprite. Ensure that the idle frame is purely grayscale. 
    * **Size Proposal:** Since the native game resolution is **640x360** (per `ADR-0011`), UI elements must be relatively compact. A standard primary menu button should have a final on-screen size of roughly **120x24** to **160x32** pixels. 
    * For the raw asset, your Aseprite template only needs to be large enough to contain the intricate corners for 9-slice rendering (e.g., **32x16** or **48x24** pixels). Godot will stretch the center to achieve the final width and height.
3. **Export:** Export the final asset from Aseprite as a PNG. Keep the image at its original 1x scale without adding any baked-in visual effects.
4. **Implement Steps in Godot:** Following the Implementation Steps outlined above, import the exported PNG into Godot, define it within a centralized Theme Variation (`CyberButton`), configure its 9-Slice margins, and bake the HDR neon colors directly into the `hover` and `pressed` StyleBoxes.
