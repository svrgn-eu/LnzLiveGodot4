# Feature: Character Creator Script (Tool / Editor Utility)

## 📖 Overview
Character scenes in the project (such as `EntCharacter.tscn`) have grown complex, comprising numerous nested nodes (Visuals, Sensors, StateManagement, Inventory, Audio, InteractableArea, etc.). This feature introduces an Editor Script or Tool to automate the creation and configuration of new character scenes to ensure consistency and save time.

## 🎯 Goals & Use Cases
- **Goal 1:** Automate the assembly of a new character scene with all necessary core components.
- **Goal 2:** Prevent human error and missing nodes when creating new enemies or NPCs.
- **Use Case A:** A designer needs to create a new "Sniper" enemy type and uses the script to generate the base scene structure in one click.

## 👥 Target Audience / User Roles
- **Designers / Developers:** Use the tool in the Godot Editor to quickly bootstrap new characters.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The script must run in the Godot Editor (e.g., as an `EditorScript` or a custom dock/inspector plugin).
- [ ] Requirement 2: It must generate a root `CharacterBody2D` with the standard `IsoCharacter.cs` script attached.
- [ ] Requirement 3: It must instantiate the entire node hierarchy (Visuals, Sensors, StateManagement with StateChart, Audio, InteractableArea, Hurtbox, Hitbox) matching `EntCharacter.tscn`. Before implementing, please analyze this scene in order to not miss anything. Also, please create a new ADR afterwards to specify the needed components and their dedication.
- [ ] Requirement 4: It must allow specifying base attributes (e.g., sprite frames, collision shapes) upon creation.

## 🎨 UI/UX Design & Interactions
- This is a developer-facing tool. It could be accessed via a custom menu under "Project > Tools" or a dock panel in the editor.
- The UI should have fields for "Character Name" and "Sprite Frames" and a "Generate Character" button.

## 💻 Technical Details
- **Base Scene Structure:** Based on `EntCharacter.tscn`, the script needs to build:
  - `CollisionShape2D` (physics)
  - `Visuals` (AnimatedSprite2D, ShadowSprite, AnimationPlayer)
  - `Sensors` (LOSOrigin, VisionRaycast)
  - `StateManagement` (StateChart, BTPlayer, Inventory)
  - `Audio` (SFXPlayer, BarkPlayer)
  - `InteractableArea`
  - `Hurtbox` (Area2D on Hurtboxes layer for receiving combat damage)
  - `Hitbox` (Optional Area2D on Hitboxes layer for melee combat)
- **Scripting:** Implement using C# `EditorScript` or `EditorPlugin`. Use `new Node()` and `AddChild()`, then set `Owner` properly so the scene can be saved.

## 🧪 Testing Plan
- Generate a character using the script and compare its node tree against `EntCharacter.tscn`.
- Run the game with the generated character to ensure all references (like node paths) are correctly established.
