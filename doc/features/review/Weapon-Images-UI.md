# Weapon Images in HUD

## 📖 Overview
This feature allows weapon and equipment images to be associated with `Item` data objects and displayed dynamically in the tactical UI. Specifically, the equipped primary/secondary weapon will be shown immediately below the character's portrait slot on the HUD, providing clear visual feedback of the active loadout.

## 🎯 Goals & Use Cases
- **Goal 1:** Give `Item` objects visual representation outside of 2D models.
- **Goal 2:** Display the currently equipped item (e.g., weapon) below the character's portrait in the HUD.
- **Use Case A:** A player equips a "Plasma Rifle" via the inventory menu. When returning to the tactical view, a high-resolution icon of the Plasma Rifle appears beneath the character's portrait.
- **Use Case B:** The player switches between their primary weapon and a secondary sidearm. The UI instantly updates the weapon image below the portrait to reflect the newly active item.

## 👥 Target Audience / User Roles
- UI/UX Designers, Developers, and 2D Artists.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The base `Item` class must be updated to include a `Texture2D` property (e.g., `ItemIcon` or `WeaponImage`).
- [ ] Requirement 2: The HUD UI must include a `TextureRect` element positioned underneath the `PortraitPanel`.
- [ ] Requirement 3: The UI must subscribe to the character's `Inventory` state to detect changes in `EquippedItems`.
- [ ] Requirement 4: When the character equips a new item in a designated "Weapon" or "Active Hand" `Slot`, the UI `TextureRect.Texture` must be updated to that item's `ItemIcon`.
- [ ] Requirement 5: If the active weapon slot is empty, the `TextureRect` should either be hidden or display a default "empty hand" icon.

## 🎨 UI/UX Design & Interactions
- **Placement:** Positioned immediately underneath the `PortraitAnimation` node in the lower HUD layout.
- **Visual Style:** The weapon image should have a subtle background panel (e.g., a semi-transparent dark box with a border) to separate it from the main portrait.
- **Feedback:** When weapons are swapped, play a short UI animation (like a quick scale up and fade in) along with an "equip" sound effect.

## 💻 Technical Details
- **Data Update:** 
  - Open `Item.cs` and add `[Export] public Texture2D ItemIcon { get; set; }`.
- **UI Logic:**
  - In the character HUD script (e.g., `CharacterHUD.cs`), hold a reference to the `CharacterData` and its `Inventory` component.
  - Listen for an `ItemEquipped` or `ActiveWeaponChanged` signal from the `Inventory`.
  - Fetch the active item: `Item activeWeapon = Inventory.EquippedItems[WeaponSlot]`.
  - Update the UI: `WeaponIconTextureRect.Texture = activeWeapon.ItemIcon`.
- **Scaling:**
  - Ensure the `TextureRect` has its `ExpandMode` set to `Ignore Size` and `StretchMode` set to `Keep Aspect Centered` to uniformly scale all item icons without stretching them.

## 🧪 Testing Plan
- Give a character two different weapons in their inventory, both with distinct `ItemIcon` textures assigned.
- Switch the active weapon using a hotkey or inventory screen and verify the UI updates immediately underneath the portrait.
- Unequip the weapon and verify the image defaults back to an empty state or disappears.
- Check various screen resolutions to ensure the weapon icon scales correctly alongside the portrait.

## 🔗 Related Resources
- [Animation Integration](./Animation-Integration.md)
- [Inventory System](../51-1 Inventory.md)
