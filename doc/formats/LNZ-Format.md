# Petz `.lnz` File Format Specification

The **`.lnz` ("Linez")** file format is a human-readable, INI-style plain-text format created by PF.Magic for the *Petz* series (*Dogz* and *Catz*). It defines how a breed or individual pet modifies a base skeletal model (`.bhd`/`.bdt`), including ball sizes, colors, textures, anatomical extensions, added balls, connecting lines, and surface paintballs.

This document specifies the exact structure and semantics of `.lnz` files as parsed by [`LnzParser`](../../src/data_classes/lnz_parser.gd) and manipulated by [`dog_generator.gd`](../../src/scenes/dog_generator.gd) and [`LnzTextEdit.gd`](../../src/scenes/editor/LnzTextEdit.gd).

---

## 1. General Syntax Rules

1. **Section Headers**: Sections begin with a bracketed header on its own line, e.g., `[Ballz Info]`, `[Add Ball]`, `[Linez]`.
2. **Comments**: Lines beginning with `;` or `#` (after indentation removal) are treated as comments and ignored during parsing.
   - **Special Breed Variant Marker (`#2`)**: In breed `.lnz` files that contain multiple coat/gender variations, a line beginning with `#2` marks the start of a secondary variation block. `LnzParser` stops reading the current section when it encounters `#2` so only the primary variation is loaded.
3. **Section Termination**: A section ends when the parser encounters an empty line, the start of a new `[Section]`, `#2`, or End-Of-File (EOF).
4. **Value Delimiters**: Numeric fields on a line may be separated by spaces, tabs, or commas. `LnzParser` extracts numeric tokens using the regular expression `[-\.\d]+` (and whitespace-delimited tokens in `[Texture List]` using `[\S]+`).

---

## 2. Global Configuration & Proportion Sections

### `[Species]`
Defines which base skeleton and animation set to use.
```ini
[Species]
2
```
| Value | Species | Base Skeleton & Animations | Base Ball Count |
| :--- | :--- | :--- | :--- |
| `1` | Cat (`KeyBallsData.Species.CAT`) | `CAT.bhd` / `CAT0.bdt`–`CAT462.bdt` | 67 (`0`–`66`) |
| `2` | Dog (`KeyBallsData.Species.DOG`) | `DOG.bhd` / `DOG0.bdt`–`DOG444.bdt` | 67 (`0`–`66`) |

*(Defaults to `2` if omitted.)*

---

### `[Texture List]`
Declares indexed `.bmp` textures that can be referenced by 0-based index (`texture_id`) in `[Ballz Info]`, `[Add Ball]`, and `[Paint Ballz]`.
```ini
[Texture List]
\ptzfiles\cat\ta\stripe3.bmp 1
\ptzfiles\cat\ta\stripe14.bmp 60 256 256
```
| Field | Type | Description |
| :--- | :--- | :--- |
| `filepath` | `String` | Original game path or filename. LnzLive strips directory prefixes via `.get_file()` and resolves the `.bmp` filename from `user://resources/textures/` or `res://resources/textures/`. |
| `transparent_color` | `int` | Controls palette recoloring/shifting behavior: `0` = raw texture colors (no shift); `1` = shift all pixels to match the ball's `color_index` ramp; `10..240` = only shift texture pixels whose palette index falls in `[transparent_color, transparent_color + 9]`. |
| `width`, `height` *(optional)* | `int` | Optional legacy dimensions (ignored by `LnzParser`; actual dimensions are read from the `.bmp` header). |

---

### `[Default Scales]`
Two integers on separate lines controlling overall pet size (`255` = $100\%$ normal scale).
```ini
[Default Scales]
200
195
```
1. **Line 1 (`scales.x` — Body/Position Scale)**: Multiplies all ball and addball 3D coordinates by `scales.x / 255.0`.
2. **Line 2 (`scales.y` — Ball Size Scale)**: Multiplies all ball and addball diameters by `scales.y / 255.0`.

---

### `[Leg Extension]`
Two integers on separate lines extending or shortening the front and back legs along the vertical ($Y$) axis.
```ini
[Leg Extension]
20
25
```
1. **Line 1 (`leg_extensions.x`)**: Front legs modifier.
2. **Line 2 (`leg_extensions.y`)**: Back legs modifier.

---

### `[Body Extension]`
Single integer extending or compacting the torso/spine length along the $Z$ axis.
```ini
[Body Extension]
-10
```
- Shifts hindquarter/tail balls along `position.z` by `body_extension * 2`, and the central spine/belly ball by `body_extension`.

---

### `[Face Extension]`
Single integer extending or shortening the snout/muzzle along the $Z$ axis.
```ini
[Face Extension]
15
```
- Shifts snout, nose, jaw, and tongue balls (`face_ext`) by `position.z -= face_extension`.

---

### `[Ear Extension]`
Single integer percentage (`100` = $100\%$ normal ear length) scaling the vector from ear base balls to ear tip balls.
```ini
[Ear Extension]
125
```

---

### `[Head Enlargement]` & `[Feet Enlargement]`
Each section takes two integers on separate lines: a percentage multiplier and an additive size offset.
```ini
[Head Enlargement]
105
2

[Feet Enlargement]
100
0
```
1. **Line 1 (`enlargement.x`)**: Percentage scale (`100` = $100\%$). Scales both the relative distances between balls in the head/foot group and their diameters (`floor(size * (x / 100.0))`).
2. **Line 2 (`enlargement.y`)**: Additive diameter offset in pixels (`ball.size += y`).

---

### `[Omissions]`
Lists ball or addball numbers (one per line) that should be hidden (along with any `[Linez]` or `[Paint Ballz]` attached to them).
```ini
[Omissions]
43
44
45
```

---

## 3. Geometry & Appearance Sections

### `[Ballz Info]`
Defines appearance overrides and size adjustments for the **67 base skeletal balls** (`0` through `66`), ordered sequentially by line number (first line = ball `0`, second line = ball `1`, etc.).

```ini
[Ballz Info]
;color outCol spkl fuzz outln size group tex ;ballnum
15,    0,     0,   2,   -1,   0,   0,    -1  ;0 L ankle
```

| Col # | Field | Type | Description |
| :--- | :--- | :--- | :--- |
| 1 | `color` | `int` | Palette index (`0`–`255`) from `petzpalette.png`. |
| 2 | `outline_color` | `int` | Palette index (`0`–`255`) for the ball's outline. |
| 3 | `speckle` | `int` | Legacy speckle parameter (parsed, unused in shader). |
| 4 | `fuzz` | `int` | Edge fuzziness amount (clamped in LnzLive via `clamp(fuzz / 2, 0, 5)`). |
| 5 | `outline` | `int` | Outline style: `-1` = none, `0` = 1px left, `-2` = 1px right, `1` = dotted side, `-3` = nose highlight, `>= 2` = full outline of width `outline - 1` px. |
| 6 | `size` | `int` | **Additive** diameter modifier added to the base ball diameter from `.bhd`. |
| 7 | `group` | `int` | Color/texture group ID used by Petz breeding/color overrides. |
| 8 | `texture` | `int` | Index into `[Texture List]` (`-1` = no texture / solid color). |

> [!NOTE]
> Eye iris balls (Dog balls `14` & `38`; Cat balls `27` & `28`) are listed in `[Ballz Info]` like normal balls, but LnzLive automatically renders them as `Paintball` decals clipped to their parent eyeball (`8` & `32` for dogs; `14` & `15` for cats) with `z_add = 10`.

---

### `[Add Ball]`
Defines additional spheres parented to existing base balls (`0`–`66`). Addballs are assigned sequential ball numbers starting immediately after the last base ball (`67`, `68`, `69`, …).

```ini
[Add Ball]
;base  x    y    z   color outCol spkl fuzz group outln ballSize bodyArea addGroup tex
24,   -12, -8,  10,  35,   0,     0,   2,   0,    -1,   18,      0,       0,       -1
```

| Col # | Field | Type | Description |
| :--- | :--- | :--- | :--- |
| 1 | `base` | `int` | Parent base ball number (`0`–`66`). The addball inherits this ball's 3D position and rotation. |
| 2 | `x` | `int` | Local X offset from `base` (in Petz pixels). |
| 3 | `y` | `int` | Local Y offset from `base` (in Petz pixels; $+Y$ is down in Petz, inverted to $-Y$ in Godot). |
| 4 | `z` | `int` | Local Z offset from `base` (in Petz pixels). |
| 5 | `color` | `int` | Fill palette index (`0`–`255`). |
| 6 | `outline_color` | `int` | Outline palette index (`0`–`255`). |
| 7 | `speckle` | `int` | Legacy speckle value. |
| 8 | `fuzz` | `int` | Edge fuzziness (`clamp(fuzz / 2, 0, 5)`). |
| 9 | `group` | `int` | Color override group ID. |
| 10 | `outline` | `int` | Outline mode (`-1`, `0`, `-2`, `1`, `>= 2`). |
| 11 | `size` | `int` | **Absolute** diameter of the addball in pixels (before `[Default Scales]` adjustment). |
| 12 | `body_area` | `int` | Anatomical region tag. |
| 13 | `add_group` | `int` | Secondary group ID. |
| 14 | `texture` | `int` | Index into `[Texture List]` (`-1` = untextured). |

---

### `[Move]`
Applies static local positional offsets to base skeletal balls (`0`–`66`) along a reference ball's coordinate axes. Multiple `[Move]` lines for the same ball accumulate in order.

```ini
[Move]
;ball  x    y    z   (optional: relative_to_ball)
24     0   -5    4   24
```

| Col # | Field | Type | Description |
| :--- | :--- | :--- | :--- |
| 1 | `base` | `int` | Base ball number (`0`–`66`) to move. |
| 2 | `x` | `int` | Offset along X axis. |
| 3 | `y` | `int` | Offset along Y axis. |
| 4 | `z` | `int` | Offset along Z axis. |
| 5 *(optional)* | `relative_to` | `int` | Base ball number whose animated Euler rotation orients the `(x, y, z)` move vector (defaults to `base` if omitted). |

---

### `[Project Ball]`
Moves a ball (`projected`) along the 3D vector connecting an anchor ball (`base`) and the target ball (`projected`). Processed in listed order after base/addball positions are finalized.

```ini
[Project Ball]
;base  projected  amount
12     20         130
```

| Col # | Field | Type | Description |
| :--- | :--- | :--- | :--- |
| 1 | `base` | `int` | Anchor ball number whose position stays fixed. |
| 2 | `projected` | `int` | Target ball number to reposition along the ray `base -> projected`. |
| 3 | `amount` | `int` | Percentage distance (`100` = no change, `< 100` = closer to `base`, `> 100` = farther from `base`). |

---

### `[Linez]`
Defines tapered connecting lines (limbs, torso segments, ears, tail) between pairs of balls or addballs.

```ini
[Linez]
;start end fuzz color l_color r_color s_thick e_thick
0,     12, 2,   -1,   0,      0,      100,    100
```

| Col # | Field | Type | Description |
| :--- | :--- | :--- | :--- |
| 1 | `start` | `int` | Starting ball or addball number. |
| 2 | `end` | `int` | Ending ball or addball number. |
| 3 | `fuzz` | `int` | Horizontal scanline fuzziness (`clamp(fuzz / 2, 0, 5)`). |
| 4 | `color` | `int` | Fill palette index (`-1` = inherit `start` ball's `color_index`). Always inherits `start` ball's texture. |
| 5 | `l_color` | `int` | Left-edge 1px outline palette index (`-1` = no left outline / inherit `start` color). |
| 6 | `r_color` | `int` | Right-edge 1px outline palette index (`-1` = no right outline / inherit `start` color). |
| 7 | `start_thickness` | `int` | Thickness at `start` ball as a percentage of `start.ball_size` (`100` = $100\%$). |
| 8 | `end_thickness` | `int` | Thickness at `end` ball as a percentage of `end.ball_size` (`100` = $100\%$). |

---

### `[Paint Ballz]`
Defines circular decals (spots, patches, muzzle markings) anchored to the surface of a base ball or addball and clipped to the parent ball's silhouette.

```ini
[Paint Ballz]
;base  size%  x       y       z       color outCol fuzz outln group tex (anchored)
2,     45,   -0.412,  0.150, -0.898,  15,   -1,    2,   -1,   0,    -1,  0
```

| Token # | Field | Type | Description |
| :--- | :--- | :--- | :--- |
| 1 | `base` | `int` | Parent ball or addball number onto which the paintball is projected. |
| 2 | `diameter_percent` | `int` | Diameter of the paintball as a percentage of the parent ball's diameter (`base_ball.size * (diameter_percent / 100.0)`). |
| 3 | `x` | `float` | Normalized X direction component on the parent ball's sphere surface. |
| 4 | `y` | `float` | Normalized Y direction component on the parent ball's sphere surface. |
| 5 | `z` | `float` | Normalized Z direction component on the parent ball's sphere surface. |
| 6 | `color` | `int` | Palette index (`0`–`255`). |
| 7 | `outline_color` | `int` | Outline palette index (`-1` is normalized to `0` by `LnzParser`). |
| 8 | `fuzz` | `int` | Edge fuzziness (`clamp(fuzz / 2, 0, 5)`). |
| 9 | `outline` | `int` | Outline mode (`-1` = none, `0`, `-2`, `1`, `>= 2`). |
| 10 | *(group)* | `int` | Skipped/unused group token (`split_line[9]`). |
| 11 | `texture` | `int` | Index into `[Texture List]` (`-1` = untextured). |
| 12 *(optional)* | `anchored` | `int` | Optional anchor flag (`0` or `1`). |
