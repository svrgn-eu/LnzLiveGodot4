# Animation Selection Dropdown

## 📖 Overview
Replace (or augment) the current free-text `AnimPicker` (`LineEdit`) in the top toolbar of the pet view with a dropdown selector (`OptionButton`) so users can easily browse and select animations for the loaded pet without needing to know or manually type raw animation index numbers.

## 🎯 Goals & Use Cases
- **Goal 1:** Allow users to discover and switch between available `.bhd`/`.bdt` animations directly from a dropdown menu in the 3D pet view toolbar.
- **Goal 2:** Prevent out-of-range or non-numeric input errors by constraining selection to valid animation indices for the currently loaded species (`CAT` or `DOG`).
- **Use Case A:** A user loads an example or custom `.lnz` pet and wants to preview how their ball/line/paintball edits look across different poses and animations by clicking the dropdown and picking an animation.
- **Use Case B:** A user switches from a Dogz `.lnz` file (445 animations) to a Catz `.lnz` file (463 animations) and the dropdown automatically updates its list of available animations to match the loaded species.

## 👥 Target Audience / User Roles
- **Petz Hexers / Breed Creators:** Users editing `.lnz` files in LnzLive who need to inspect pet proportions, ball placements, and line connections across multiple animations and frames.

## ⚙️ Functional Requirements
- [x] **Requirement 1:** The system must populate the animation dropdown dynamically whenever a pet is generated and `bhd_loaded(num_of_animations)` is emitted by `dog_generator.gd`.
- [x] **Requirement 2:** The dropdown must list all valid animation indices (`0` to `num_of_animations - 1`) for the active species (`DOG.bhd` or `CAT.bhd`).
- [x] **Requirement 3:** Selecting an item from the dropdown must immediately call `set_animation(index)` on the pet generator node, resetting the `FrameSlider` to frame `0` and updating the 3D pet view.
- [x] **Requirement 4:** If the currently selected animation index exceeds the animation count of a newly loaded species, the selection must clamp or reset to `0`.

## 🎨 UI/UX Design & Interactions
- **Location:** Top toolbar inside `PetViewContainer/VBoxContainer/HBoxContainer` (to the left of `FrameSlider`, `Play` button, and `FPSLabel`).
- **Control Type:** Godot `OptionButton` replacing the unlabeled `AnimPicker` (`LineEdit`).
- **Interaction Flow:**
  1. Before a pet is loaded, the dropdown is either disabled or shows a placeholder (e.g., `"Anim: -"`).
  2. Once a pet is loaded, the dropdown displays the current animation (e.g., `"Anim 0"` or `"0"`) and enables selection.
  3. Clicking the dropdown opens a scrollable popup list of available animations. Selecting an entry immediately updates the pet pose and frame slider range.

## 💻 Technical Details
- **Scene Modifications (`src/scenes/editor/editor.tscn`):**
  - Replace `AnimPicker` (`LineEdit`) under `SceneRoot/HSplitContainer/HSplitContainer/PetViewContainer/VBoxContainer/HBoxContainer` with an `OptionButton` (or attach a dedicated script `AnimPicker.gd`).
  - Connect `PetRoot/Node`'s existing `bhd_loaded(num_of_animations)` signal to populate the `OptionButton` items.
  - Connect the `OptionButton`'s `item_selected(index: int)` signal to `PetRoot/Node.set_animation(index)` (or `_on_AnimPicker_item_selected`).
- **Generator Logic (`src/scenes/dog_generator.gd`):**
  - `bhd_loaded(num_of_animations)` is already emitted in `init_ball_data(species)` (`bhd.animation_ranges.size()`).
  - Add or update the handler to accept the `int` index from `item_selected` directly via `set_animation(index)`.
- **Data Classes (`src/data_classes/bhd_parser.gd`, `src/data_classes/bdt_parser.gd`):**
  - No schema changes required; optional future enhancement could map animation indices to human-readable Petz action names.

## 🧪 Testing Plan
- **Initial State:** Launch the application and verify the dropdown is visible, properly sized in the top bar, and does not throw errors when clicked before a pet is loaded.
- **Loading a Pet:** Double-click a Dogz example (e.g., `dachsund.lnz`) and verify the dropdown populates with `0`–`444` (`DOG.bhd` animation count).
- **Switching Animations:** Select several animations from the dropdown (first, middle, last) and verify both the 3D pet view and `FrameSlider.max_value` update accurately, and `Play` animates through the frames.
- **Species Switching:** Load a Catz example (e.g., `catz_base.lnz`) after a Dogz example and verify the dropdown updates to the Catz animation count without duplicating entries.

## 🔗 Related Resources
- [`src/scenes/editor/editor.tscn`](../../src/scenes/editor/editor.tscn)
- [`src/scenes/dog_generator.gd`](../../src/scenes/dog_generator.gd)
- [`src/data_classes/bhd_parser.gd`](../../src/data_classes/bhd_parser.gd)
