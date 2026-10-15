# Character Details Screen

## 📖 Overview
A dedicated UI screen that provides players with a comprehensive overview of a selected character. This screen consolidates the character's high-resolution visual representation, equipped loadout, and vital statistics into a single, easily readable interface.
The header is on top, the full character view on the left hand side and stats and inventory items are to the right.
The screen itself can be found in "src/UI/CharacterScreen.tscn". It can be displayed in game when the PLayer hits the kay associated with it.

## 🎯 Goals & Use Cases
- **Goal 1:** Give players a centralized location to inspect character capabilities and current status.
- **Goal 2:** Showcase the high-resolution `CharacterAnimation` (from CharacterData.CharacterAnimation) alongside detailed equipment and weapon information.
- **Use Case A:** Before engaging in a tough battle, the player opens the character screen to review a unit's movement speed, equipped weapon, and current health pool.
- **Use Case B:** During the squad management phase, a player opens this screen to inspect exactly what weapon and armor a character is currently wielding.

## 👥 Target Audience / User Roles
- Players managing their squad's composition, stats, and tactical approach.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The screen must display the character's full-body `CharacterAnimation` playing its idle loop.
- [ ] Requirement 2: The screen must pull and display the character's core stats from `CharacterData` (e.g., `Callsign`, `Health`/`MaxHealth`, `MovementSpeed`).
- [ ] Requirement 3: The screen must query the character's `Inventory` to display the currently equipped weapon, including its name and `ItemIcon`.
- [x] Requirement 4: The screen should be accessible via a hotkey or a dedicated UI button when a character is selected on the tactical grid.
- [ ] Requirement 5: The UI must be responsive and correctly scale to different screen resolutions.

## 🎨 UI/UX Design & Interactions
- **Layout Structure:** 
  - **Left Panel:** A large, prominent viewport displaying the looping `CharacterAnimation`.
  - **Top Right Panel:** Character Header (Callsign) and core bar stats (Health bar, Movement Speed) display current value and max value.
  - **Bottom Right Panel:** Equipment/Loadout section highlighting the active weapon with its `ItemIcon` and basic damage stats.
- **Interaction:** A clear "Close" or "Back" button to return to the tactical view. The game should pause while this screen is open to allow players to read safely.
- **Visual Style:** Should match the tactical HUD's aesthetic (e.g., dark semi-transparent panels, clean sans-serif typography, high contrast for readability).

## 💻 Technical Details
- **UI Scene:** Create a new `Control` node scene (e.g., `CharacterScreen.tscn`) using `HBoxContainer` and `VBoxContainer` for the layout.
- **Data Binding:** The root script `CharacterScreen.cs` will accept a `CharacterData` reference upon opening.
  - It will assign `CharacterData.CharacterAnimation` to a central `AnimatedSprite2D` or custom UI control.
  - It will populate text labels mapping to `CharacterData.Callsign`, `CharacterData.Health`, etc.
  - It will access `CharacterData.Inventory.EquippedItems` to find the active weapon and assign its `ItemIcon` to a `TextureRect`.
- **State Management:** The screen must subscribe to `HealthChanged` signals while open to reflect real-time updates (if unpaused), and cleanly unsubscribe when closed to prevent memory leaks.

## 🧪 Testing Plan
- Open the character screen for a fully equipped character and verify that all visuals (animation, weapon icon) and text stats accurately reflect the data.
- Open the screen for a character with an empty weapon slot to ensure it handles missing equipment gracefully (displaying an "Empty Hand" placeholder without crashing).
- Resize the game window to ensure the layout remains properly anchored and readable.

## 🔗 Related Resources
- [Animation Integration](./review/Animation-Integration.md)
- [Weapon Images UI](./Weapon-Images-UI.md)
