# Bug Report: Panning Causes Balls/Paintballs to Disappear and Pet to Be Cut Off

## 🐛 Bug Description
When panning the pet view (`PetViewContainer`) to the left (or beyond a certain distance in any direction from the viewport center), two visual rendering defects occur sequentially:
1. **Elements Disappear First (`Balls`, `Addballs`, `Paintballs`, and `Irises`):** Once a ball or paintball's center is panned more than `0.5` camera view-space units ($250\text{ px}$ in `SubViewport` coordinates) away from the viewport center (`|center_clip_space.x| > 0.5` or `|center_clip_space.y| > 0.5`), the billboard quad collapses to zero width/height and stops being drawn entirely, leaving only `Linez` (`Line.tscn`) visible.
2. **Pet Is Cut Off (`SubViewport` Boundary Clipping):** Panning slightly further causes the remaining geometry (`Linez`) to be sliced off along a hard vertical line at the edge of the fixed $1000 \times 1000\text{ px}$ `SubViewport`, which lies well inside the visible `PetViewContainer` panel (especially when zoomed out or on a wide editor window).

## 🔄 Steps to Reproduce
Steps to reproduce the behavior:
1. Launch **PetzRendering** and load any pet from the **Examples** tree (e.g., `snowball.lnz`).
2. Optionally zoom out once with the mouse wheel (`WheelDown`) or keep default zoom on a wide window.
3. Click and hold the **Right Mouse Button** (or **Middle Mouse Button**) in the center `PetViewContainer` and pan the pet toward the left side of the viewport.
4. Observe that as the pet crosses roughly $250\text{ px}$ to the left of the viewport center, all balls, addballs, irises, and paintballs vanish one by one as their centers cross the threshold, leaving only the `Linez` fur segments.
5. Continue panning slightly further left and observe the remaining `Linez` of the pet being sharply cut off along a vertical boundary well before reaching the left edge of `PetViewContainer`.

## 🛑 Expected Behavior
- All pet primitives (`Ball`, `Addball`, `Paintball`, `Iris`, and `Line`) should remain continuously rendered regardless of how far the pet is panned across the viewport.
- The pet should only be clipped by the actual visible borders of the `PetViewContainer` UI panel, not by an invisible interior boundary mid-panel.

## 📸 Screenshots / Video
![Pet panned to the left showing missing balls and vertical cutoff](../../img/Screenshot%202026-10-20%20panning-cutoff.png)

## 🖥️ Environment & Context
- **Environment:** Dev (`PetzRendering (DEBUG)`, Godot 4.6 .NET / C#)
- **OS:** Windows
- **Time of Issue:** 2026-10-20

## 📋 Additional Details & Root Cause Analysis

### 1. Root Cause of Disappearing Balls & Paintballs (Shader Quad Collapse)
In both [`src/shaders/ball.tres`](../../src/shaders/ball.tres) (lines 158–174) and [`src/shaders/paintball.tres`](../../src/shaders/paintball.tres) (lines 159–185), the vertex shader expands the billboard quad around `center_clip_space` using the sign of the **view-space** vertex coordinate `vert_in_clip_space` instead of the **local mesh** vertex coordinate `VERTEX`:

```glsl
// Construct camera-facing billboard matrix (strips local MeshInstance3D scale)
MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);

vec3 vert_in_clip_space = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
vec4 center_clip_space = MODELVIEW_MATRIX * vec4(vec3(0.0), 1.0);
...
float b = (float(ball_size) / 2.0 + 0.5 + float(fuzz_amount + 3)) / (VIEWPORT_SIZE.x / 2.0);
vert_in_clip_space.x = center_clip_space.x + b * sign(vert_in_clip_space.x);
vert_in_clip_space.y = center_clip_space.y + b * sign(vert_in_clip_space.y);
```

- Because `QuadMesh` has a default size of `1.0 x 1.0`, local `VERTEX.xy` is $(\pm 0.5, \pm 0.5)$, and `vert_in_clip_space.xy = center_clip_space.xy + VERTEX.xy`.
- When `|center_clip_space.x| < 0.5` (within $\pm 250\text{ px}$ of the camera center), the left vertices (`center_clip_space.x - 0.5 < 0`) and right vertices (`center_clip_space.x + 0.5 > 0`) have opposite signs, so `sign(vert_in_clip_space.x)` expands them to `center_clip_space.x - b` and `center_clip_space.x + b`.
- **As soon as `center_clip_space.x < -0.5` (or `> +0.5`, or `|center_clip_space.y| > 0.5`)**, both `center_clip_space.x - 0.5` and `center_clip_space.x + 0.5` have the **same sign** (`-1.0` when panned left). All four vertices of the quad are placed at `center_clip_space.x - b`, collapsing the quad to zero width (`0` area) so it is discarded by the rasterizer.
- By contrast, [`src/shaders/line.tres`](../../src/shaders/line.tres) (lines 134–136) checks `if (VERTEX.x < 0.0)` in local mesh space, which is why `Linez` remain visible after balls and paintballs disappear.
- **Proposed Fix Direction:** Use `sign(VERTEX.x)` and `sign(VERTEX.y)` (local mesh coordinates) instead of `sign(vert_in_clip_space.x)` and `sign(vert_in_clip_space.y)` in [`src/shaders/ball.tres`](../../src/shaders/ball.tres) and [`src/shaders/paintball.tres`](../../src/shaders/paintball.tres).

### 2. Root Cause of Hard Pet Cutoff (`SubViewport` Fixed Size & Scaling)
In [`src/Scenes/Editor/Editor.tscn`](../../src/Scenes/Editor/Editor.tscn) (lines 73–98) and [`src/UI/PetViewContainer.cs`](../../src/UI/PetViewContainer.cs) (lines 101–110, 195–215):
- `SubViewportContainer` and `SubViewport` have a hardcoded size of `1000 x 1000` pixels (`Camera3D` orthogonal `size = 2.0`, corresponding to view-space `[-1.0, +1.0]`).
- When zooming out via `WheelDown`, `_tex.Scale` is halved (`_tex.Scale /= 2.0f`), shrinking the `1000 x 1000` `SubViewportContainer` on screen (e.g., to `500 x 500` pixels at `Scale = 0.5`) inside a larger `PetViewContainer`.
- Even at `Scale = 1.0`, if `PetViewContainer` is wider than `1000 px` or when the pet approaches `center_clip_space.x = -1.0` ($500\text{ px}$ left of center in `SubViewport` space, which is only $250\text{ px}$ on screen at `Scale = 0.5`), the `SubViewport` frustum hard-clips all remaining geometry along its `1000 x 1000` border inside the middle of `PetViewContainer`.

## 🔗 Related Files
- [`src/shaders/ball.tres`](../../src/shaders/ball.tres)
- [`src/shaders/paintball.tres`](../../src/shaders/paintball.tres)
- [`src/shaders/line.tres`](../../src/shaders/line.tres)
- [`src/UI/PetViewContainer.cs`](../../src/UI/PetViewContainer.cs)
- [`src/Scenes/Editor/Editor.tscn`](../../src/Scenes/Editor/Editor.tscn)
