# Petz 256-Color Palette & 8-Bit Indexed `.bmp` Texture Format

A signature visual feature of the *Petz* engine is its **indexed 256-color palette system**, which enables dynamic recoloring of textured fur patterns and automatic depth-based shading (Z-shading) without traditional 3D lighting.

This document explains how `petzpalette.png`, [`PetzTextureLoader`](../../src/data_classes/petz_texture_loader.gd), and the spatial shaders ([`ball.tres`](../../src/shaders/ball.tres), [`paintball.tres`](../../src/shaders/paintball.tres), [`line.tres`](../../src/shaders/line.tres)) implement this system.

---

## 1. The 256-Color Petz Palette (`petzpalette.png`)

Located at `src/resources/textures/petzpalette.png`, the master palette is a **$256 \times 1$ pixel RGB texture** where each horizontal texel $x \in [0, 255]$ corresponds to a Petz color index.

### 1.1 Texel Lookup Formula
To sample exact texel centers without bilinear bleeding, the shaders use `filter_nearest, repeat_disable` and sample at:
$$u = \frac{\text{float}(\text{palette\_index}) + 0.5}{256.0}, \quad v = 0.0$$

### 1.2 10-Shade Color Ramps (`0`–`149`) vs. Fixed Colors (`150`–`255`)
In the standard Petz palette, indices `0` through `149` are organized into **15 contiguous color ramps of 10 shades each**, typically ordered from lightest (`base + 0`) to darkest (`base + 9`):

| Index Range | Typical Petz Color Ramp |
| :--- | :--- |
| `0`–`9` | White / Light Grey |
| `10`–`19` | Black / Charcoal |
| `20`–`29` | Mid Grey |
| `30`–`39` | Cream / Blonde |
| `40`–`49` | Orange / Ginger |
| `50`–`59` | Red / Rust |
| `60`–`69` | Brown / Chocolate |
| `70`–`79` | Tan / Fawn |
| `80`–`89` | Dusty Brown / Umber |
| `90`–`99` | Blue-Grey / Russian Blue |
| `100`–`149` | Additional Accent / Clothing / Eye Ramps |
| `150`–`255` | Non-shifting utility, UI, and accent colors |

Because indices `< 150` are arranged in blocks of 10:
- The **ramp base index** of any color `color_index` is:
  $$\text{modded\_color} = \text{color\_index} - (\text{color\_index} \bmod 10)$$
- The **shade offset** within a ramp (`0` to `9`) of any sampled texture pixel `palette_index` is:
  $$\text{shade\_offset} = \text{palette\_index} \bmod 10$$

---

## 2. 8-Bit Indexed `.bmp` Texture Format ([`PetzTextureLoader`](../../src/data_classes/petz_texture_loader.gd))

Petz fur and marking textures (such as `stripe3.bmp`, `plush.bmp`, `CALI2.bmp`) are stored as **uncompressed 8-bit-per-pixel (8bpp) indexed Windows Bitmap (`.bmp`)** files.

### 2.1 Why Standard Image Import Is Insufficient
When a standard game engine imports an 8-bit `.bmp` file, it immediately resolves each pixel's 8-bit index through the `.bmp` file's embedded color table into an `RGB8` color, **discarding the original palette index (`0`–`255`)**.

However, Petz textures rely on those exact integer indices (`0`–`255`) at runtime to decide:
1. Which pixels belong to the "transparent" (recolorable) color ramp (`transparent_index`).
2. Which shade offset (`0`–`9`) within the ball's target `color_index` ramp should be applied.

### 2.2 Binary `.bmp` Parsing in `PetzTextureLoader.load_petz_bmp`

[`PetzTextureLoader`](../../src/data_classes/petz_texture_loader.gd) parses the raw `.bmp` binary directly to preserve the 8-bit palette indices:

| Byte Offset | Type | Field | Expected / Usage |
| :--- | :--- | :--- | :--- |
| `0x00` (0) | `uint16` | `magic` | Must equal `0x4D42` (`"BM"` in little-endian). |
| `0x0A` (10) | `uint32` | `data_offset` | Byte offset where the raw pixel index array begins (after the BMP header and embedded 256-entry color table). |
| `0x12` (18) | `int32` | `width` | Texture width in pixels (typically `64`, `128`, or `256`). |
| `0x16` (22) | `int32` | `height` | Texture height in pixels. If `height > 0`, rows are stored **bottom-up** (`target_y = height - 1 - y`); if `height < 0`, rows are **top-down** (`target_y = y`). |
| `0x1C` (28) | `uint16` | `bpp` | Bits per pixel. Must equal `8` for indexed mode (otherwise falls back to `Image.load_from_file`). |
| `0x1E` (30) | `uint32` | `compression` | Must equal `0` (`BI_RGB` uncompressed). |

### 2.3 Row Stride & Red-Channel Encoding
In the Windows `.bmp` specification, each scanline row is padded to a multiple of 4 bytes:
$$\text{row\_stride} = (\text{width} + 3) \ \& \ \sim 3$$

For each pixel `(x, y)`, `PetzTextureLoader` reads the raw byte `idx = row_bytes[x]` ($\in [0, 255]$) and stores it normalized in the **Red channel** of a Godot `Image.FORMAT_RGB8` texture:
$$\text{Color}\!\left(\frac{\text{idx}}{255.0},\ 0.0,\ 0.0,\ 1.0\right)$$

In the fragment shader, the original integer palette index (`0`–`255`) is recovered losslessly via:
```glsl
int palette_index = int(round(texture(ball_texture, uv).r * 255.0));
```

---

## 3. Transparent Color Modes & Z-Depth Shading

When a ball, addball, paintball, or line has a texture assigned (`has_texture == true`), its final pixel color depends on `color_index`, `transparent_index` (from `[Texture List]`), and its 3D depth relative to the pet's belly (`z_center_pet`).

### 3.1 `transparent_index` Modes
Assuming `color_index < 150`:
1. **`transparent_index == 0` (No Recoloring / Raw Texture)**:
   - Every pixel uses its raw `palette_index` directly from `petzpalette.png` without recoloring or Z-shifting.
2. **`transparent_index == 1` (Recolor & Z-Shift Entire Texture)**:
   - Every pixel in the texture is recolored into the ball's `color_index` 10-shade ramp and Z-shaded via `get_shifted_color(palette_index)`.
   - Commonly used for full-body fur/plush textures (`plush.bmp`, `hair10.bmp`) so a single greyscale-indexed texture can take on any coat color.
3. **`transparent_index > 1` (Selective Ramp Recoloring, e.g., `60`)**:
   - If the sampled texture pixel's `palette_index` falls within the 10-shade ramp `[transparent_index, transparent_index + 9]`, that pixel is treated as the "background fur" and is shifted to the ball's `color_index` ramp via `get_shifted_color(palette_index)`.
   - All other pixels outside `[transparent_index, transparent_index + 9]` keep their original texture color (e.g., preserving dark stripes in `stripe14.bmp` while letting the base coat color underneath change to match the ball's `color_index`).

*(If `color_index >= 150`, palette shifting is bypassed.)*

### 3.2 Z-Depth Palette Shifting (`get_shifted_color`)
To give flat 2D circles a sense of 3D volume, `get_shifted_color(int palette_index)` compares the primitive's view-space Z coordinate (`z_center_ball` or `z_center_line`) against the pet's belly view-space Z coordinate (`z_center_pet`):
```glsl
vec3 get_shifted_color(int palette_index) {
	int modded_color = color_index - int(mod(float(color_index), 10.0));
	int amount_to_shift_index = int(mod(float(palette_index), 10.0));
	// shift according to z
	if (z_center_ball < z_center_pet) {
		amount_to_shift_index = int(min(float(amount_to_shift_index + 2), 9.0));
	}
	else if (z_center_ball > z_center_pet) {
		amount_to_shift_index = int(max(float(amount_to_shift_index - 1), 0.0));
	}
	int new_palette_index = modded_color + amount_to_shift_index;
	vec3 new_color = texture(palette, vec2((float(new_palette_index) + 0.5) / 256.0, 0.0)).rgb;
	return new_color;
}
```
- **Farther from camera than belly (`z_center_ball < z_center_pet` in view space)**: Shifts the shade index **up by $+2$** (clamped to `9`), darkening balls on the far side of the body.
- **Closer to camera than belly (`z_center_ball > z_center_pet` in view space)**: Shifts the shade index **down by $-1$** (clamped to `0`), brightening balls on the near side of the body.
