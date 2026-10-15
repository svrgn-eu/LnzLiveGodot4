# Feature: Highlight Selected Character

## 📖 Overview
In an isometric tactical game, it is crucial for the player to instantly recognize which character is currently selected. This feature introduces a visual indicator (such as a shader outline, a selection ring underneath the character, or a pulsing effect) to clearly distinguish the active character in the game view.

## 🎯 Goals & Use Cases
- **Goal 1:** Improve player situational awareness by clearly indicating the currently selected unit.
- **Goal 2:** Provide a visually pleasing and non-intrusive selection effect that fits the game's aesthetic.
- **Use Case A:** A player clicks on a character in the game view. The character immediately receives a glowing outline or selection ring to show they are now active.
- **Use Case B:** A player selects a character via the UI (e.g., character portrait). The corresponding character in the isometric world highlights accordingly.

## 👥 Target Audience / User Roles
- **Players:** Will have an easier time managing multiple units and understanding who they are commanding.

## ⚙️ Functional Requirements
- [x] Requirement 1: The system must listen to the `CharacterSelected` (or equivalent) signal from the `EventBus`.
- [x] Requirement 2: The visual highlight must be applied to the newly selected character. This highlight must only be applicable to a player-controlled character and specifically affect `EntCharacter.tscn`.
- [x] Requirement 3: The visual highlight must be removed from the previously selected character.
- [x] Requirement 4: The highlight effect should be a recognizable indicator (e.g., shader outline, glowing selection ring sprite, or pulsing color modulate). It should be recognizable but leave the game scene readable, so an indicator at the character's feet should be ideal.

## 🎨 UI/UX Design & Interactions
- **Visuals:** A subtle glowing ring at the character's feet or a shader-based colored outline around the character sprite.
- **Interaction:** The effect appears instantly upon selection and disappears when deselected or when another character is selected. The mode of selection (via portrait or click in the game world) does not matter.

## 🖼️ Asset Requirements (Sprite Indicator)
If the sprite indicator approach is chosen, the asset must meet the following requirements:
- **Isometric Perspective:** The ring must be drawn as an isometric ellipse (typically a 2:1 width-to-height ratio) to properly match the game's grid projection.
- **Color Versatility:** The sprite should be drawn in pure white or grayscale with transparency. This allows the engine's `modulate` property to easily tint it dynamically (e.g., green for the active player character). Note that this indicator is exclusively for player-controlled characters (NPCs and enemies do not use selection indicators).
- **Dimensions:** The sprite should roughly match the width of a single isometric tile or the character's base, avoiding overlap into adjacent cells.
- **Pivot/Origin:** The visual center of the ring must align perfectly with the sprite's origin point so it anchors exactly to the character's root position (feet).
- **Animation:** The "glow" effect can be achieved via engine tweens (modulating opacity), making a static single-frame image sufficient. This should be optional.

## 💻 Technical Details
- **Core Components:** 
  - `IsoEntity.cs` or `IsoCharacter.cs`: Needs logic to toggle the selection visual on and off. It must verify the character is player-controlled before allowing the highlight.
  - `EventBus.cs`: Used to broadcast `CharacterSelected` and track the active entity.
- **Implementation Options:**
  1. **Shader Material:** Apply a custom 2D outline shader to the `AnimatedSprite2D`.
  2. **Sprite Indicator:** Add a `Sprite2D` node (e.g., `SelectionRing`) to the `Visuals` node that is toggled visible/invisible.
- **Determining Player Control (Proposals):**
  - **Option 1 (Faction/Team Enum - Recommended):** Add a `Team` or `Faction` property (e.g., `Team.Player`, `Team.Enemy`) to the character's base stats. The character only highlights if its team is `Team.Player`. This is the most scalable approach for tactical games.
  - **Option 2 (Component-Based):** Check for the presence of a specific node, such as a `PlayerInputController`, on the entity. If it exists, it's a player character.
  - **Option 3 (Simple Flag):** Add an exported boolean flag (e.g., `[Export] public bool IsPlayerControlled`) directly on the `EntCharacter.tscn` root script.

## 🧪 Testing Plan
- Select a character by clicking them and verify the highlight appears.
- Select a different character and verify the first character's highlight disappears while the new one appears.
- Verify that UI selection triggers the same in-world visual highlight.

## 🔗 Related Resources
- Existing `EventBus.cs` implementation.
- Current character scenes (`EntCharacter.tscn`, `EntStrayCat.tscn`).
