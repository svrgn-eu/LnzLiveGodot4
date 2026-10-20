# Bug Report: Mouse Wheel Zoom Does Not Zoom Toward Mouse Pointer Location

## 🐛 Bug Description
When zooming in or out in the 3D pet view (`PetViewContainer`) using the mouse wheel (`WheelUp` / `WheelDown`), the view always zooms toward/away from the fixed center of the `SubViewportContainer` (`(500, 500)`) rather than zooming toward the point currently under the mouse cursor. If the pet has been panned away from the center or the user points the cursor at a specific body part (e.g., head, paw, or eye) to inspect it closely, scrolling the mouse wheel causes the target area to drift away from the mouse pointer.

## 🔄 Steps to Reproduce
Steps to reproduce the behavior:
1. Launch **PetzRendering** and load any pet from the **Examples** tree (e.g., `snowball.lnz` or `dachsund.lnz`).
2. Pan the pet slightly to the left or right using the **Right Mouse Button** (or hover the mouse cursor over an off-center part of the pet, such as the tip of the tail or ear).
3. Scroll the mouse wheel up (`WheelUp`) to zoom in or down (`WheelDown`) to zoom out while keeping the mouse cursor stationary on that feature.
4. Observe that the feature under the mouse cursor shifts away from the pointer because zooming scales around the viewport center instead of anchoring the world point under the cursor.

## 🛑 Expected Behavior
- Zooming in (`WheelUp`) or out (`WheelDown`) should anchor the point currently under the mouse pointer so that the world/viewport position beneath the cursor remains stationary under the cursor before and after the zoom step.
- 3D ball picking/raycasting (`Select` mode) and subsequent panning should remain accurately aligned with the rendered pet after zooming at any cursor position.

## 📸 Screenshots / Video
N/A (Interactive camera/viewport zoom behavior in `PetViewContainer`).

## 🖥️ Environment & Context
- **Environment:** Dev (`PetzRendering (DEBUG)`, Godot 4.6 .NET / C#)
- **OS:** Windows
- **Time of Issue:** 2026-10-20

## 📋 Additional Details & Root Cause Analysis

### Root Cause in `PetViewContainer.cs`
In [`src/UI/PetViewContainer.cs`](../../src/UI/PetViewContainer.cs) (lines 99–110), mouse wheel input is currently handled by scaling `_tex` (`SubViewportContainer`) around its geometric center (`_tex.Size / 2.0f`) without accounting for `mouseButtonEvent.Position` or adjusting `_camera.Transform.Origin`:

```csharp
else if (@event is InputEventMouseButton { Pressed: true } mouseButtonEvent)
{
	if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown && _tex != null)
	{
		_tex.PivotOffset = _tex.Size / 2.0f;
		_tex.Scale /= 2.0f;
	}
	else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp && _tex != null)
	{
		_tex.PivotOffset = _tex.Size / 2.0f;
		_tex.Scale *= 2.0f;
	}
```

1. **Ignored Cursor Offset:** `mouseButtonEvent.Position` (the mouse cursor position relative to `PetViewContainer`) is never read during `WheelUp` / `WheelDown`.
2. **Camera Translation Not Compensated:** When zoom scale changes from $S_{\text{old}}$ to $S_{\text{new}}$, a cursor offset $\Delta p = p_{\text{mouse}} - p_{\text{center}}$ (in `PetViewContainer` local pixels) corresponds to a change in world/camera offset of:
   $$\Delta \text{world} = \Delta p \times 0.002 \times \left(\frac{1}{S_{\text{old}}} - \frac{1}{S_{\text{new}}}\right)$$
   Because `_camera.Transform.Origin` is not shifted by this delta (taking into account the camera's $-X$ horizontal orientation in `Editor.tscn`, where panning shifts `t.Origin.X += motion.X * 0.001f / _tex.Scale.X` or `0.002f` pixel world size), zooming always pivots around `_camera.Transform.Origin` at the center of `PetViewContainer`.
3. **Raycasting & Panning Consistency Note:**
   - Notice also in [`src/UI/PetViewContainer.cs`](../../src/UI/PetViewContainer.cs) (lines 56–57) that panning uses `0.001f / _tex.Scale.X` while `PixelWorldSize` is `0.002f` (`Camera3D.Size = 2.0` over `1000 px`), and raycasting in lines 69–72 computes `var realCenter = Position + (Size / 2.0f);` where `motionEvent.Position` is already local to `PetViewContainer` (`Size / 2.0f`). Keeping `_tex.PivotOffset = _tex.Size / 2.0f` centered while translating `_camera.Transform.Origin` during zoom-to-cursor ensures that `_tex` stays centered and raycasting/panning remain consistent.

## 🔗 Related Files
- [`src/UI/PetViewContainer.cs`](../../src/UI/PetViewContainer.cs)
- [`src/Scenes/Editor/Editor.tscn`](../../src/Scenes/Editor/Editor.tscn)
