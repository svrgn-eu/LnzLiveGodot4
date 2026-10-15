# Character Animation Integration

## 📖 Overview
This feature describes the comprehensive implementation and usage of the three core animation layers (`PortraitAnimation`, `CharacterAnimation`, and `IsoAnimation`) defined within the `CharacterData` and `AnimalData` resources. It outlines how these `SpriteFrames` are dynamically loaded and managed across various game systems, ensuring consistent visual representation of characters and animals in UI, tactical gameplay, and standard views.

## 🎯 Goals & Use Cases
- **Goal 1:** Clearly separate animation concerns (UI vs. 2D flat vs. Isometric 3D) to allow modular visual representation.
- **Goal 2:** Create a standardized way for systems to query and play the correct animation state for a given entity.
- **Use Case A:** An `IsoCharacter` moves across the grid in combat. The system checks the movement direction and plays the correct directional walk cycle from the `IsoAnimation` `SpriteFrames`.
- **Use Case B:** The player opens the character's inventory screen. The `CharacterAnimation` idle cycle plays to show a detailed, non-isometric view of the character.
- **Use Case C:** A character takes damage or speaks during a dialogue event. The HUD displays their `PortraitAnimation`, triggering a "talking" or "hurt" animation to reflect their status.

## 👥 Target Audience / User Roles
- Developers implementing UI, character logic, and rendering.
- Technical artists setting up `SpriteFrames` for new characters and animals.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The `IsoCharacter` and `IsoAnimal` nodes must automatically load their respective `IsoAnimation` `SpriteFrames` into an `AnimatedSprite3D` (or isometric 2D equivalent) upon instantiation.
- [x] Requirement 2: The UI system must be able to bind a component to a character's `PortraitAnimation` and react to state changes (e.g., taking damage triggers the "hurt" portrait animation).
- [ ] Requirement 3: A generic `AnimationController` component should be created to manage state transitions (Idle, Walk, Run, Shoot, Hurt, Die) and sync them with the appropriate `SpriteFrames`.
- [ ] Requirement 4: The system must support directional animations for `IsoAnimation` (e.g., mapping movement vectors to 4 or 8-way directional strings like `walk_ne`).
- [ ] Requirement 5: Fallback handling must be in place if a specific animation state is missing from the assigned `SpriteFrames` (e.g., reverting to `idle` and logging a warning).

## 🎨 UI/UX Design & Interactions
- **HUD Portraits:** The bottom or side screen tactical HUD will host an `AnimatedSprite2D` or custom UI control that runs the `PortraitAnimation`. It should subtly bob or play idle loops to feel alive.
- **Inventory/Roster Screen:** A dedicated UI panel will use the `CharacterAnimation` to show a higher-resolution, flat 2D version of the character rotating or idling while managing gear.
- **Tactical Grid:** The `IsoAnimation` will handle the actual game board representation, requiring smooth frame transitions between moving, aiming, throwing, and shooting in isometric space.

## 💻 Technical Details
- **Animation Controller:** Implement a script (e.g., `CharacterAnimationController.cs`) that listens to signals from `CharacterData` and the character's state machine. 
  - It will require a reference to the main visual node and trigger `Play(animName)`.
- **Directional Mapping (IsoAnimation):**
  - Calculate the angle of movement or aiming.
  - Convert the angle into a directional suffix (e.g., `_n`, `_ne`, `_e`).
  - Concatenate the state with the suffix (e.g., `run` + `_ne` = `run_ne`) and play it from the loaded `IsoAnimation`.
- **UI Binding:**
  - Create a custom `Control` node (e.g., `PortraitPanel.cs`) that takes a `CharacterData` or `AnimalData` object and dynamically instantiates an `AnimatedSprite2D` using the assigned `PortraitAnimation` frames.

## 🧪 Testing Plan
- Instantiate a character and an animal on the test map and verify that their `IsoAnimation` correctly maps to movement directions.
- Open the UI and verify the `PortraitAnimation` properly cycles its idle state.
- Trigger a "Take Damage" event and verify that both the `IsoAnimation` and the `PortraitAnimation` switch to their respective "hurt" frames simultaneously.
- Ensure that if an animation is not defined, the game does not crash but logs a warning and successfully plays the fallback animation.

## 🔗 Related Resources
- [Characters](../13 Characters.md)
