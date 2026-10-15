# In-Game Tutorial

## 📖 Overview
A comprehensive, interactive in-game tutorial designed to introduce players to the core mechanics of IsoTactics without overwhelming them. It integrates seamlessly into the first level of the game, guiding players through movement, combat, line-of-sight, cover usage, and mission types using contextual prompts, highlighting, and controlled scenarios.

## 🎯 Goals & Use Cases
- **Goal 1:** Teach the player basic navigation, camera controls, and character movement on an isometric grid.
- **Goal 2:** Introduce combat mechanics, shooting dynamics, weapon usage, cover systems, and line-of-sight rules.
- **Goal 3:** Familiarize the player with different mission types and their specific objectives.
- **Use Case A:** A new player starts the game for the first time and needs to understand how to move their unit to a destination.
- **Use Case B:** A player needs to learn how to select a weapon, utilize cover, and execute a shot against an enemy.
- **Use Case C:** A player is introduced to specific mission objectives, such as interacting with elements or reaching extraction points.

## 👥 Target Audience / User Roles
- **New Players:** Unfamiliar with isometric tactical RPG mechanics.
- **Returning Players:** Needing a refresher on game-specific mechanics (e.g., throwing ammo, stray cat interactions, dynamic cover).

## ⚙️ Functional Requirements
- [ ] The system must restrict player actions to the current tutorial step (e.g., disable attacking when teaching movement).
- [ ] The system must dynamically highlight relevant grid tiles, cover positions, or UI elements.
- [ ] The system must detect when the player has successfully completed a step to advance the tutorial sequence.
- [ ] The system must display contextual pop-up dialogues or tooltips explaining the current objective.
- [ ] The system must allow skipping the tutorial from the pause menu, granting immediate full control.

## 🎨 UI/UX Design & Interactions
- **Tutorial Prompts:** A semi-transparent overlay dialogue box positioned near the bottom or top of the screen (to avoid obstructing the action). Contains character portraits, instructional text, and a "Continue" button.
- **UI Highlighting:** When teaching UI usage, the background dims slightly while the target UI element (e.g., Weapon Selection button, Mission Objective tracker) glows with a pulsing outline.
- **In-Game Indicators:**
  - **Movement Range:** Specific tiles will flash to indicate where the player should move.
  - **Cover Indicators:** Shield icons (full or half) appear over objects providing cover.
  - **Target Selection:** An animated crosshair appears over the enemy unit the player is supposed to attack, showing line-of-sight percentage.
  - **Camera Pan:** The camera automatically and smoothly pans to points of interest before returning control to the player.

## 📝 Step-by-Step Tutorial Sequence

### Phase 1: Basics and Navigation
1. **Camera Movement:**
   - **UI:** Prompt "Use W,A,S,D or drag the Middle Mouse Button to pan the camera."
   - **Action:** Player must move the camera a certain distance.
   - **Completion:** Camera moves beyond a threshold.
2. **Unit Selection & Movement:**
   - **UI:** The "Select Unit" UI is highlighted. Prompt: "Left-click your character to select them."
   - **Action:** Player selects the unit.
   - **In-Game:** A specific objective tile glows green.
   - **Action:** Player right-clicks the glowing tile.
   - **Completion:** Unit successfully navigates to the tile via pathfinding.

### Phase 2: Combat, Cover, and Shooting
3. **Understanding Cover:**
   - **In-Game:** The player is prompted to move towards a low wall. A half-shield icon highlights the cover.
   - **UI:** Prompt: "Positioning is key. Move behind the highlighted wall to gain cover, reducing incoming damage and enemy accuracy."
   - **Action:** Player moves the unit to the designated cover tile.
4. **Selecting a Weapon and Shooting:**
   - **UI:** Dim the screen; highlight the Weapon UI slot. Prompt: "Click here to equip your firearm."
   - **Action:** Player clicks the weapon UI icon.
   - **In-Game:** An enemy drone or dummy appears. A line-of-sight indicator connects the player's unit to the enemy, showing a clear shot.
   - **UI:** Prompt: "Your line of sight is clear. Hover over the target to check your accuracy, then click to shoot."
   - **Action:** Player shoots and destroys the target.
5. **Flanking and Line of Sight:**
   - **In-Game:** A new enemy appears behind heavy cover.
   - **UI:** Prompt: "The enemy is well-protected. A direct shot will likely miss. Move to the flanking position to bypass their cover!"
   - **Action:** Player navigates to an exposed angle (highlighted tile) and successfully shoots the enemy.

### Phase 3: Mission Types and Objectives
6. **Retrieval / Interaction Objective:**
   - **In-Game:** A glowing data drive appears on a terminal.
   - **UI:** Prompt: "Missions have different goals. For this Retrieval mission, interact with the terminal to secure the data."
   - **Action:** Player moves to the terminal and clicks to interact.
7. **Throwing Items / Distractions:**
   - **UI:** Highlight the item inventory UI. Prompt: "Sometimes a stealthy approach is needed. Throw a rock to distract the guard."
   - **In-Game:** Throw trajectory arc is shown.
   - **Action:** Player executes the throw action, clearing the path.
8. **Extraction (Escort / Elimination):**
   - **In-Game:** An extraction zone appears (e.g., a glowing helicopter pad).
   - **UI:** Prompt: "Objective complete! In Extraction or Escort missions, you must reach the designated zone to secure victory. Move there now."
   - **Action:** Player reaches the extraction zone, completing the tutorial.

## 💻 Technical Details
- **Frontend Components:**
  - `TutorialManager.gd`: A state machine (or Behavior Tree) controlling the active tutorial step, listening for specific signals (e.g., `unit_moved`, `target_eliminated`, `objective_reached`).
  - `UI_HighlightOverlay.tscn`: A control node that uses a shader to darken the screen while masking out the specific UI rect to highlight.
- **Backend/Systems:**
  - Integration with the Mission Controller or Game State Manager to inject artificial pauses or restrict available actions during the tutorial.
  - Input filtering: Temporarily block abilities or actions that aren't part of the current step to prevent players from entering an invalid state.

## 🧪 Testing Plan
- Test if the player can "break" the tutorial by performing unexpected actions (e.g., trying to attack when asked to move). Ensure the input is safely ignored.
- Verify that UI highlighting correctly adapts to different screen resolutions and aspect ratios.
- Ensure that skipping the tutorial properly cleans up all tutorial UI elements and restores normal game state.

## 🔗 Related Resources
- Existing features: `Weapon-Images-UI.md`, `Throw-Ammo.md`, `Animation-Integration.md`.
