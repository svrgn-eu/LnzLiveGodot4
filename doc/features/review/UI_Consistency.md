# Feature / ADR: Consistent UI Looks & Behaviour

> **[MARK FOR REVIEW]** - This document is intended to be adjusted manually later on.

## 1. Context and Goals
This document serves as an Architecture Decision Record (ADR) and feature specification for UI Consistency across the game. The target is to establish a central, easy-to-apply standard for all in-game UIs, dedicated menus, pop-up menus, and individual UI elements (captions, buttons, images, etc.).

## 2. Current Implementation Review
Currently, the UI is centralized around a main theme: `src/UI/Themes/Default.tres`.

### 2.1 Theming & Styles
- **Buttons (Default):** Flat style with dark gray backgrounds, lighter gray borders, and off-white text. They include reactive states (Hover, Focus, Pressed) which brighten the border and text. *Note: We also want to use the 9-part split textures (NinePatch) we used for UI Panels to make the buttons look fancy.*
- **Panels:**
  - Standard Panels use a slightly transparent, desaturated dark blue background (`Color(0.067, 0.094, 0.153, 0.9)`) with a dark gray border (`Color(0.2, 0.2, 0.2, 1)`).
  - Menu Panels use a specialized textured NinePatchRect (`BgPanelNinepatchrect03.png`).
- **Progress Bars (Health):** Dark green background (`Color(0.066, 0.25, 0, 1)`) with a vibrant green fill (`Color(0.125, 0.48, 0, 1)`).
- **Special Effects:** Pop-up/Pause menus (e.g., `ScrPauseMenu.tscn`) use a combination of a full-screen blur shader background and a glitch effect shader overlay for a stylized, cyber/tactical feel.

### 2.2 Fonts & Font Sizes
The current UI relies primarily on the default system/Godot font with specific size variations defined in `Default.tres`:
- **ButtonTacticalAction:** Size 4 (Used in tactical HUD action menus like Move, Attack, End Turn).
- **LabelCallsign / MenuButton:** Size 16.
- **MenuCaption:** Size 20 with a black outline (outline size 1).

## 3. UI Consistency Guidelines

### 3.1 Main Colors & Theming
*(To be expanded/adjusted during review)*
- **Primary Backgrounds:** Desaturated Dark Blue (e.g., `#111827` / `Color(0.067, 0.094, 0.153)`).
- **Primary Borders:** Medium-Dark Grays (e.g., `#333333` / `Color(0.2, 0.2, 0.2)`).
- **Text Color:** Off-white for readability (`#DFDFDF` / `Color(0.875, 0.875, 0.875)`). Bright white (`#FFFFFF`) for active/pressed states.
- **Accent Colors:** Tactical Green for health and primary positive indicators.

### 3.2 Popup Colors
*(To be expanded/adjusted during review)*
- **Popup Backgrounds:** Should maintain the blurred backdrop style (`Color(0,0,0,0.5)` with blur shader) to separate them from gameplay visually.
- **Popup Panels:** Textured NinePatch backgrounds (`BgPanelNinepatchrect03.png`) or opaque dark gray flats with subtle borders.

### 3.3 Call to Action (CTA) Styles
*(To be detailed during review)*
- Primary CTAs (e.g., Confirm, End Turn, Attack) should use a distinct accent color (like the tactical green or a bright highlight) rather than standard gray to draw the user's eye.
- Hover states must prominently increase brightness or border thickness to clearly indicate interactivity.

### 3.4 Naming Conventions (Semantic Styling)
- **Usage-Based Naming:** Theme variations must be named for **what they are being used for**, rather than their visual properties (e.g., use `ButtonTacticalAction` instead of `ButtonSmallFont`). This ensures that if the visual style changes later, the name remains semantically accurate and components stay logically grouped.

## 4. UI Scope & Element Standards

### 4.1 In-Game UI (Tactical HUD)
- Keep elements compact and non-intrusive (e.g., `UiTacticalHud.tscn`).
- Use `ButtonTacticalAction` (Size 4) for space-saving grid/action buttons.
- Stick to screen edges and avoid obscuring the center isometric view.

### 4.2 Dedicated Menus (Main Menu, Character Screen)
- Use standard `MenuButton` (Size 16) and `MenuCaption` (Size 20, outlined) styling.
- Layouts should be centralized or follow a strict grid logic.

### 4.3 Pop-up Menus (Pause, Dialogs)
- Must pause background action where applicable (`process_mode = 3`).
- Utilize the established Blur + Glitch shaders to provide depth and atmospheric context.

### 4.4 Granular Elements
- **Captions:** Use outlines (`MenuCaption`) for readability against complex backgrounds. Captions need to be in capitilized letters and bold to signal the player the current options / where he or she is in the game.
- **Buttons:** Adhere strictly to the Hover/Normal/Pressed/Focus color progression defined in `Default.tres`. Incorporate the 9-part split textures for a fancier appearance, consistent with the UI Panels.
- **Images/Portraits:** Maintain consistent aspect ratios (e.g., `UiCharacterPortraitTile.tscn`) and frame them with standard panel borders.

## 5. Next Steps
[ ] Finalize and lock in specific hex codes for Main, Popup, and CTA colors.
[ ] Determine if a custom `.ttf`/`.otf` font should be imported to replace the default Godot font.
[ ] Migrate any hardcoded styles in scenes to `Default.tres` to enforce strict centralization.
