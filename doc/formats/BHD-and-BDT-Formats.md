# Petz `.bhd` and `.bdt` Binary Animation Formats

While `.lnz` files define a pet's breed customizations (sizes, colors, addballs, lines, paintballs), the underlying **67-ball skeletal hierarchy, default ball diameters, and per-frame animation transforms** are stored in two paired binary formats extracted from the original *Petz* game data:

1. **`.bhd` (Ball Header)**: Master index file per species (`DOG.bhd`, `CAT.bhd`) containing default base ball sizes, animation frame counts, and byte offset tables.
2. **`.bdt` (Ball Data)**: Per-animation binary files (`DOG0.bdt`–`DOG444.bdt`, `CAT0.bdt`–`CAT492.bdt`) containing 3D positions and Euler rotations for every base ball across every frame of that animation.

All multi-byte integers in both formats are **little-endian**.

---

## 1. `.bhd` Format Specification ([`BhdParser`](../../src/data_classes/bhd_parser.gd))

Located in `src/resources/animations/DOG.bhd` and `src/resources/animations/CAT.bhd`.

### 1.1 Binary Layout

| Byte Offset | Type | Field Name | Description |
| :--- | :--- | :--- | :--- |
| `0x00` | `uint16` | `frames_offset` | Absolute byte offset in the `.bhd` file where the `uint32` frame offset table begins. |
| `0x02` | `uint32` | *(reserved)* | Header version / flags (skipped via `file.get_32()`). |
| `0x06` | `uint16` | `num_balls` | Number of base skeletal balls for this species (`67` for both Dog and Cat). |
| `0x08` | `uint8[30]` | *(reserved)* | 30 bytes of header metadata (skipped via `file.seek(file.get_position() + 30)`). |
| `0x26` | `uint16[num_balls]` | `ball_sizes` | Array of `num_balls` unsigned 16-bit integers defining the default unscaled pixel diameter for each base ball (`0` to `num_balls - 1`). |
| `0x26 + 2 * num_balls` | `uint16` | `animation_count` | Total number of animations defined for this species (`445` in `DOG.bhd`, `493` in `CAT.bhd`). |
| Next `2 * animation_count` bytes | `uint16[animation_count]` | `anim_end_indices` | Cumulative frame count table. Entry `i` stores the exclusive ending frame index `end` for animation `i` (up to `16,036` total frames across animations `0`–`492` in `CAT.bhd`). |
| `frames_offset` | `uint32[]` | `frame_file_offsets` | Contiguous array of 32-bit unsigned byte offsets pointing into the corresponding `<SPECIES><anim_index>.bdt` file. |

### 1.2 Resolving Animation Ranges & Frame Offsets

In [`BhdParser._init`](../../src/data_classes/bhd_parser.gd#L22-L29), each animation `i` (`0 <= i < animation_count`) derives its slice of the `frame_file_offsets` table from the cumulative `anim_end_indices` array:
- For `i == 0`: `start_frame_index = 0`
- For `i > 0`: `start_frame_index = animation_ranges[i - 1].end`
- `end_frame_index = file.get_16()`
- **Number of frames in animation `i`**:
  $$\text{num\_of\_offsets} = \text{end\_frame\_index} - \text{start\_frame\_index}$$
- **Byte position in `.bhd` where animation `i`'s frame offsets begin**:
  $$\text{bhd\_table\_pos} = \text{frames\_offset} + (\text{start\_frame\_index} \times 4)$$

When [`BhdParser.get_frame_offsets_for(index)`](../../src/data_classes/bhd_parser.gd#L31-L46) is called for animation `index`:
1. It seeks to `animation_ranges[index].start` in the `.bhd` file.
2. It reads `num_of_offsets` consecutive `uint32` values.
3. Each `uint32` value is the exact byte offset of that frame inside `<SPECIES><index>.bdt` (e.g., `DOG0.bdt` or `CAT12.bdt`).

> [!NOTE]
> **Catz Animation Catalog (`0`–`492`)**: A complete human-readable index of all 493 Catz animations—including animation ID, hex ID (`0000`–`01EC`), frame count, cumulative `First` and `Last` frame indices (`0`–`16035`), loop flag, and behavior description—is available in [`Catz-Animations-Catalog.md`](Catz-Animations-Catalog.md). Note that `Catz-Animations-Catalog.md` lists inclusive `First` and `Last` frame indices (e.g., Anim `0`: `First = 0`, `Last = 20`), whereas `CAT.bhd` stores the exclusive upper bound `end = Last + 1` (`21`, giving `21 - 0 = 21` frame offsets).

---

## 2. `.bdt` Format Specification ([`BdtParser`](../../src/data_classes/bdt_parser.gd))

Located in `src/resources/animations/<SPECIES><anim_index>.bdt` (e.g., `DOG0.bdt` through `DOG444.bdt` and `CAT0.bdt` through `CAT492.bdt`).

### 2.1 Frame Binary Structure

For each `frame_offset` returned by `BhdParser.get_frame_offsets_for(anim_index)`, [`BdtParser`](../../src/data_classes/bdt_parser.gd) seeks to `frame_offset` in the `.bdt` file and reads:

#### Frame Header (14 Bytes)
| Relative Offset | Type | Field | Description |
| :--- | :--- | :--- | :--- |
| `+0x00` | `uint16[6]` | `_spec` | 6 unsigned 16-bit values (12 bytes total) representing frame bounding box / metadata. |
| `+0x0C` | `uint16` | `tag` | Frame tag / flags word (2 bytes). |

#### Per-Ball Transform Array (`num_balls` $\times$ 10 Bytes)
Immediately following the 14-byte frame header (`frame_offset + 14`) are `num_balls` (`67`) consecutive 10-byte records, one for each base ball `0` through `66`:

| Relative Offset (within ball record) | Type | Field | Description |
| :--- | :--- | :--- | :--- |
| `+0` | `int16` | `x` | Signed 16-bit X coordinate in Petz pixel space. |
| `+2` | `int16` | `y` | Signed 16-bit Y coordinate in Petz pixel space ($+Y$ points downward in Petz). |
| `+4` | `int16` | `z` | Signed 16-bit Z coordinate in Petz pixel space ($+Z$ points into/out of screen depending on camera). |
| `+6` | `int8` | `rot1` (`rot.x`) | Signed 8-bit Euler X rotation angle (in degrees). |
| `+7` | `int8` | `rot2` (`rot.y`) | Signed 8-bit Euler Y rotation angle (in degrees). |
| `+8` | `int8` | `rot3` (`rot.z`) | Signed 8-bit Euler Z rotation angle (in degrees). |
| `+9` | `int8` | `_tag3` | Signed 8-bit padding/status byte. |

### 2.2 Two's Complement Signed Decoding

Because Godot's `FileAccess.get_16()` and `FileAccess.get_8()` return unsigned integers (`0..65535` and `0..255`), [`BdtParser`](../../src/data_classes/bdt_parser.gd#L34-L40) converts them to signed two's complement values:
```gdscript
static func get_s16(file: FileAccess) -> int:
	var u = file.get_16()
	return u - 65536 if u >= 32768 else u

static func get_s8(file: FileAccess) -> int:
	var u = file.get_8()
	return u - 256 if u >= 128 else u
```

### 2.3 Coordinate Space Conversion to Godot 3D

In `dog_generator.gd`:
1. The raw `(x, y, z)` coordinates from `.bdt` are modified by `.lnz` `[Move]`, `[Leg/Body/Face/Ear Extension]`, `[Head/Feet Enlargement]`, and `[Default Scales]`.
2. When applied to 3D nodes, the vertical axis is inverted (`y *= -1.0`) because Petz screen/world space uses $+Y$ downward whereas Godot 3D uses $+Y$ upward.
3. Positions are multiplied by `pixel_world_size = 0.002` so that 1 Petz pixel equals `0.002` world units ($1\text{ px}$ in the $1000 \times 1000$ orthogonal viewport of width `2.0`).
