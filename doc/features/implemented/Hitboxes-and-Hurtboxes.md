# Feature: Hitboxes, Hurtboxes, and Screen Shake

## 📖 Overview
This feature introduces a structured approach to combat collision detection by separating Hitboxes and Hurtboxes. It builds upon the current physics implementation and adds Screen Shake effects to enhance the visual impact of combat events.

## 🎯 Goals & Use Cases
- **Goal 1:** Clearly separate physics collision (walls, navigation) from combat collision (bullets, melee hits).
- **Goal 2:** Provide a unified interface for receiving damage and impact events.
- **Goal 3:** Enhance game feel and feedback through Screen Shake when impacts occur.
- **Use Case A:** A bullet (Hitbox) collides with an enemy character (Hurtbox), triggering a damage event.
- **Use Case B:** An explosion occurs near the player, triggering a heavy screen shake effect to emphasize the blast.

## 👥 Target Audience / User Roles
- **Players:** Experience improved game feel and clear visual feedback during combat.
- **Designers:** Can easily configure damage zones and weak points on complex characters.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The current `CollisionShape2D` (used for environment physics) must remain separate from combat hit detection.
- [ ] Requirement 2: Characters must include a `Hurtbox` component (Area2D) specifically layered to receive combat events.
- [ ] Requirement 3: Projectiles and melee attacks must include a `Hitbox` component (Area2D) that detects overlaps with Hurtboxes.
- [ ] Requirement 4: A `ScreenShakeService` or component must be created to handle camera shake based on impact severity and distance.

## 🎨 UI/UX Design & Interactions
- Visual feedback is primarily conveyed through Screen Shake and hit reactions (e.g., character flashing or particle effects).
- Screen shake should have configurable intensity, duration, and decay.

## 💻 Technical Details
- **Current Implementation Review:** Existing Character scenes (e.g., `EntCharacter.tscn`) currently use standard `CollisionShape2D` for environment collisions and an `InteractableArea` for mouse interactions. True Hitboxes and Hurtboxes need to be added.
- **Hitbox:** An `Area2D` that carries damage data (Damage, Knockback, Status Effects) on the specific collision layer (e.g., "Hitboxes").
- **Hurtbox:** An `Area2D` that receives damage data on the collision mask (e.g., "Hurtboxes") and forwards it to the character's Health/Damage system.
- **Screen Shake:** A `Camera2D` component or global singleton that applies noise-based or random offsets to the camera position, diminishing over time.

## 🧪 Testing Plan
- Verify that bullets pass through environment physics shapes if intended, but stop at Hurtboxes.
- Ensure that overlapping Hitboxes and Hurtboxes accurately transmit damage.
- Test Screen Shake intensities to ensure they do not cause motion sickness and feel appropriate for the impact.
