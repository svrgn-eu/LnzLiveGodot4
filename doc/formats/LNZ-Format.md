# Petz `.lnz` File Format Specification

The **`.lnz` ("Linez")** file format is a human-readable, INI-style plain-text format created by PF.Magic for the *Petz* series (*Dogz* and *Catz*). It defines how a breed or individual pet modifies a base skeletal model (`.bhd`/`.bdt`), including ball sizes, colors, textures, anatomical extensions, added balls, connecting lines, and surface paintballs.

This document specifies the exact structure and semantics of `.lnz` files as parsed by [`LnzParser`](../../src/data_classes/lnz_parser.gd) and manipulated by [`dog_generator.gd`](../../src/scenes/dog_generator.gd) and [`LnzTextEdit.gd`](../../src/scenes/editor/LnzTextEdit.gd).

---

## 1. General Syntax Rules

1. **Section Headers**: Sections begin with a bracketed header on its own line, e.g., `[Ballz Info]`, `[Add Ball]`, `[Linez]`.
2. **Comments & Breed Variation Markers**: Lines beginning with `;` or `#` (after indentation removal) are treated as comments and ignored during parsing.
   - **Breed Variation Blocks (`#N`, `#2`, `#1`, `##`)**: In breed `.lnz` files (such as [`src/resources/cinn.lnz`](../../src/resources/cinn.lnz), [`chichi.lnz`](../../src/resources/chichi.lnz), or raw `.cat`/`.dog` breed resources like `B+W Shorthair/BW`), coat, marking, and clothing variations are grouped using `#` directives and closed with `##`.
   - **Current `LnzParser` `#2` Termination Rule & Caveat**: `LnzParser` stops reading the current section when it encounters a line beginning with `#2` (`line.begins_with("#2")`). In files where `#1` precedes `#2` (e.g., `cinn.lnz`), this loads variation `#1` and skips `#2`. However, in raw breed files where `#2` is used as the outer header declaring a 2-variation block before `#1` (e.g., `#2;face streak` followed by `#1` in the original *B&W Shorthair* `BW` resource), breaking on `#2` stops parsing `[Paint Ballz]` early and omits the subsequent `#1` markings (such as the B&W Shorthair face streak and chest patch). Adopted/dumped pet `.lnz` files (e.g., [`src/resources/pouncer.lnz`](../../src/resources/pouncer.lnz)) have their variation blocks already flattened by the game and load all markings cleanly.
3. **Section Termination**: A section ends when the parser encounters an empty line, the start of a new `[Section]`, `#2`, or End-Of-File (EOF).
4. **Value Delimiters**: Numeric fields on a line may be separated by spaces, tabs, or commas. `LnzParser` extracts numeric tokens using the regular expression `[-\.\d]+` (and whitespace-delimited tokens in `[Texture List]` using `[\S]+`). Extra trailing columns or inline comments (e.g., `eBall_ankleL,` or `; 0`) are ignored once the expected column keys for a section have been read.

---

## 2. Global Configuration & Proportion Sections

### `[Species]`
Defines which base skeleton and animation set to use.
```ini
[Species]
2
```
| `.lnz` `[Species]` Value | `.pet` Binary `Load Info` Value | Species | Base Skeleton & Animations | Base Ball Count |
| :--- | :--- | :--- | :--- | :--- |
| `1` | `0` | Cat (`KeyBallsData.Species.CAT`) | `CAT.bhd` / `CAT0.bdt`–`CAT492.bdt` (493 animations) | 67 (`0`–`66`) |
| `2` | `1` | Dog (`KeyBallsData.Species.DOG`) | `DOG.bhd` / `DOG0.bdt`–`DOG444.bdt` (445 animations) | 67 (`0`–`66`) |

*(Defaults to `2` in `LnzParser` if omitted. Note that in binary `.pet` files, the `Load Info` header encodes `Species` as `0 = Catz` and `1 = Dogz`, whereas the embedded `.lnz` text section uses `1 = Catz` and `2 = Dogz`; see [`PET-Format.md`](PET-Format.md#L93-L98).)*

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
37
65
66
67
68
```
> [!NOTE]
> In standard Catz `.lnz` files (see [`catz_base.lnz`](../../src/resources/catz_base.lnz#L151-L164)), `[Omissions]` routinely hides base ball `37` (`eBall_nose`, replaced by nose addballs `77`–`79`), orientation/translation base balls `65` (`eBall_zorient`) and `66` (`eBall_ztrans`), and utility alignment addballs `67`–`76` (*"utility addballz for Andrew"* used for mounting, toy stealing, digging, and pillow alignment), so visible breed addballs begin at index **`77`**.

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
Defines additional spheres parented to existing base balls (`0`–`66`). Addballs are assigned sequential ball numbers starting immediately after the last base ball (`67`, `68`, `69`, …). In binary `.pet` files, up to `512` total balls (`67` base + `445` addballs) are supported (see [`PET-Format.md`](PET-Format.md#L405-L444)).

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
| 15 *(optional)* | *(extra)* | `int` | Present in dumped `.pet` `.lnz` files (e.g., `-1` in `pouncer.lnz`); ignored by `LnzParser`. |

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
| 9–10 *(optional)* | *(extra)* | `int` | Present in dumped `.pet` `.lnz` files (e.g., `0 0` or `-1 0` in `pouncer.lnz`); ignored by `LnzParser`. |

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
| 11 | `texture` | `int` | Index into `[Texture List]` (`-1` = untextured) when `[Lnz Version] 2` is used. *(In legacy `.lnz` files without `[Lnz Version] 2`, the original game interprets this value as a base ball ID whose texture is copied; see [Petz Wiki: LNZ](https://petz.miraheze.org/wiki/LNZ).)* |
| 12 *(optional)* | `anchored` | `int` | Optional anchor flag (`0` or `1`). |

---

## 4. `.lnz` File Archetypes & Additional Petz Sections

As documented in the [Petz Wiki LNZ Specification](https://petz.miraheze.org/wiki/LNZ) and [`PET-Format.md`](PET-Format.md), `.lnz` data in the *Petz* ecosystem exists in four distinct forms:

1. **Species Master `.lnz` (`CATMASTER.lnz` / `DOGMASTER.lnz`)**:
   - Extracted from `Catz 5 Rez.dll` / `Dogz 5 Rez.dll`.
   - Defines only species-level defaults (`[Species]`, `[Eyes]`, `[Key Balls]`, `[Whiskers]`, `[Z Shade Slope]`, `[Head Rotation Limits]`, `[Head Tilt Limits]`, `[Num Ballz]`, and base paw-pad `[Fur Markings]`). Contains **no** `[Ballz Info]`, `[Add Ball]`, or `[Linez]`—it cannot be rendered standalone without a breed `.lnz`.
2. **Adult Breed `.lnz` (e.g., `BW` inside `B+W Shorthair.cat` or [`cinn.lnz`](../../src/resources/cinn.lnz))**:
   - References `[Default Linez File] \ptzfiles\cat\catmaster.lnz` and `[Little one] bwkit.lnz`, defines full `[Ballz Info]`, `[Add Ball]`, `[Linez]`, and `[Paint Ballz]`, and uses `#N` / `#2` / `#1` / `##` variation blocks for randomized Adoption Center traits.
3. **Puppy / Kitten `.lnz` (e.g., `BWKIT` inside `B+W Shorthair.cat`)**:
   - References the adult breed file in `[Default Linez File]` (`\ptzfiles\cat\bw\bw.lnz`) and overrides only juvenile proportions (`[Default Scales]`, `[Head/Feet Enlargement]`, `[Body/Leg/Face/Ear Extension]`, `[Project Ball]`, `[Ball Size Override]`, and `[Fuzz Override]`).
4. **Adopted Pet / Dumped `.lnz` (e.g., [`pouncer.lnz`](../../src/resources/pouncer.lnz) or embedded `.pet` `LNZ Info`)**:
   - Self-contained `.lnz` files (see [`PET-Format.md`](PET-Format.md)) where master defaults, breed definitions, and random-seed variation blocks have already been merged and flattened into concrete `[Ballz Info]`, `[Add Ball]`, `[Linez]`, and `[Paint Ballz]` tables.

### Unparsed Engine & Breeding Sections Reference
The following `.lnz` sections appear in original *Petz* files for in-game audio, breeding genetics, weight biorhythms, and clothing, and are safely ignored by `LnzParser`:
- **Inheritance & Metadata**: `[Default Linez File]`, `[Little One]`, `[Sounds]`, `[Breed Name]`, `[Lnz Version]`, `[Num Ballz]`, `[Head Shot]` (Pick-A-Pet portrait pose; frame numbers $> 492$ crash original *Catz*), `[256 Eyelid Color]`.
- **Skeletal & Rendering Hints**: `[Eyes]`, `[Key Balls]`, `[Whiskers]`, `[Default Linez Thickness]`, `[Z Shade Slope]`, `[Default Glue Ball]`, `[Circle Render Mode]`, `[Line Render Mode]`, `[Draw Linez Before Ballz]`, `[Draw Small Balls]`, `[Head Rotation Limits]`, `[Head Tilt Limits]`.
- **Variations, Biorhythms & Genetics**: `[Thin/Fat]` (ball size deltas at min/max fullness/fatness), `[Ball Size Override]`, `[Color Info Override]`, `[Outline Color Override]`, `[Fuzz Override]`, `[Fur Color Areas]`, `[Fur Markings]`, `[Fur Pattern Balls]`, `[Marking Factor]`, `[Spot Factor]`, `[Force To Female]`, `[Force To Male]`.
- **Clothing**: `[Add Clothing]`, `[Adjust Clothing]`, `[Flat Clothing]`.

