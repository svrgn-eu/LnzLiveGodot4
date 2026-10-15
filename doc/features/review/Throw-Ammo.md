# Ammo Exchange (Throwing)

## 📖 Overview
Characters can exchange ammo by throwing it to each other when they are in close proximity. This introduces dynamic resource management during combat and adds a layer of tactical positioning, accompanied by immersive throwing and catching animations.

## 🎯 Goals & Use Cases
- **Goal 1:** Allow characters to share resources seamlessly without navigating complex inventory menus.
- **Goal 2:** Encourage cooperative positioning and teamwork during tactical engagements.
- **Use Case A:** A front-line character runs out of ammo while under fire. A support character nearby throws a spare magazine to keep them in the fight without either having to move to the same exact tile.
- **Use Case B:** Pre-positioning resources before breaching a room, where characters toss ammo to ensure everyone is fully loaded.

## 👥 Target Audience / User Roles
- Players managing squads in tactical combat situations.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The system must allow a character to target a friendly unit within a specified range (e.g., 3-5 tiles) to initiate an ammo transfer.
- [ ] Requirement 2: The system must verify line of sight and an unobstructed path between the thrower and catcher.
- [ ] Requirement 3: The system must play a throwing animation for the sender and a catching animation for the receiver.
- [ ] Requirement 4: The system must handle cases where the catch fails or is interrupted (e.g., if the receiver is killed or moved mid-throw, the ammo drops on their tile).
- [ ] Requirement 5: The UI must clearly indicate valid targets for ammo exchange and highlight the throw trajectory.

## 🎨 UI/UX Design & Interactions
- **Interaction:** A dedicated "Throw Item/Ammo" action in the character's tactical menu or via a hotkey when targeting an ally.
- **Visuals:** An arcing trajectory line shows the path of the thrown ammo, turning green for valid targets and red if obstructed.
- **Feedback:** A floating UI text notification indicating the amount and type of ammo received, along with a sound effect for the catch.

## 💻 Technical Details
- **Game Objects / Logic:** Create a `ThrowAction` class in C# extending the base action system. Define a temporary projectile node (e.g., a RigidBody3D or Area3D) representing the thrown item with a calculated parabolic trajectory.
- **Animations:** Two new animations needed: `anim_throw_item` (for the sender) and `anim_catch_item` (for the receiver). Timing synchronization is required between the two animations and the projectile's flight time.
- **Inventory System:** Update `CharacterData.cs` and the underlying inventory logic to deduct the item from the sender upon initiating the throw, and add it to the receiver upon a successful catch.

## 🧪 Testing Plan
- Verify successful ammo transfer between adjacent units and units at the maximum throwing range.
- Test obstructed paths (walls, enemies, other allies) to ensure the throw is correctly blocked or marked invalid.
- Verify inventory quantities are accurately updated (no duplication or loss of ammo).
- Test edge cases, such as the receiver moving or being incapacitated while the ammo is in mid-air.

## 🔗 Related Resources
- [Inventory](../51-1 Inventory.md)
- [Characters](../13 Characters.md)
