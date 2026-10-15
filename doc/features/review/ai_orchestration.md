# AI Implementation & Orchestration Architecture

## 📖 Overview
The AI in the game utilizes a hybrid architecture: high-level decision making and Behavior Trees (BTs) are handled via **LimboAI** (in GDScript), while the heavy lifting (pathfinding, grid movement, and physics) is executed natively in **C#**. 

This document defines the multi-layered orchestration of AI logic across the game board, ensuring complex behaviors for enemies, civilians, animals, and non-controlled player squad members.

## 🏗️ The 3 Layers of AI Coordination

To prevent individual AI agents from acting stupidly in isolation (e.g., all enemies moving to the same cover spot), the AI is divided into three distinct hierarchical layers.

### 1. Global AI Manager (The Orchestrator)
**Scope:** Map-wide.
**Responsibilities:**
- Acts as the central brain of the tactical map.
- Monitors the global "Heat" or "Alertness" level of the map.
- Caches global A* pathfinding grids and line-of-sight (LoS) calculations to prevent redundant processing by individual entities.
- Broadcasts global events (e.g., "Explosion at X,Y", "Alarm Triggered") down to the Specific Managers.
- Handles the "Possession" logic (routing input to the currently selected player character).

### 2. Specific Faction Managers
**Scope:** Group / Faction level.
**Responsibilities:**
These managers sit between the Global Manager and the individual entities to coordinate group tactics.
- **Squad Manager:** Manages the non-controlled player characters. If the player is controlling Character A, this manager dictates the formation and follow-distance of Characters B, C, and D.
- **Enemy Manager (The Tactician):** Assigns roles dynamically during combat. If three enemies see the player, the Enemy Manager assigns one to provide "Suppressing Fire", one to "Flank Left", and one to "Hunker Down". It ensures enemies don't steal each other's cover.
- **Civilian Manager (The Crowd Controller):** When panic ensues, this manager designates global "Safe Zones" (e.g., map edges) and directs the flow of fleeing civilians so they don't all clump in one corner.
- **Animal Manager (The Ambient Controller):** Limits the number of animals that can be startled simultaneously to prevent audio/visual spam.

### 3. Local Entity AI Manager (The Actor)
**Scope:** Individual Entity (`EntCharacter`, `EntStrayCat`).
**Responsibilities:**
- This is the actual `BTPlayer` (LimboAI Behavior Tree) node attached to the entity.
- It receives its current objective from its Faction Manager (e.g., "Flank to Tile X,Y").
- It executes the localized logic to achieve that goal: checking its own ammo, choosing to reload, playing animations, and calling the C# movement APIs.

---

## 🎮 Player Possession Mechanics (Switching Characters)

The game allows the player to seamlessly swap between the 4 squad members (`game_selectP1`, `P2`, etc.). The AI system must gracefully handle the transition between "Player Controlled" and "AI Controlled".

### How it works:
1. **Input Received:** The player presses the hotkey to switch to Character B.
2. **Orchestrator Intercepts:** The Global AI Manager catches this event.
3. **Disabling AI (New Avatar):**
   - The Orchestrator calls `DisableAI()` on Character B.
   - Character B's LimboAI `BTPlayer` is paused/disabled.
   - The camera transitions to Character B, and player input events are now routed to them.
4. **Enabling AI (Old Avatar):**
   - The Orchestrator calls `EnableAI("FollowerProfile")` on Character A (the previously controlled character).
   - Character A's LimboAI `BTPlayer` is activated, specifically loading the `Support/Follower` behavior tree.
   - Character A will now autonomously follow Character B, seek cover nearby, and return fire if engaged, relying on the Squad Manager for spacing.

---

## ⚙️ How AI Steers Characters

AI decision-making (GDScript) and physical execution (C#) are cleanly decoupled.
1. **Decision (LimboAI):** The BT reaches a leaf node called `MoveToFlankTask.gd`.
2. **Calculation:** The task asks the C# core for a path: `var path = entity.GetPathTo(target_tile)`.
3. **Execution:** The GDScript task calls `entity.MoveAlongPath(path)`.
4. **Steering (C#):** The C# script on the entity takes over. It handles the `Tween` interpolation from tile to tile, updates the isometric Z-index, triggers the "Walk" animation on the `AnimationController`, and updates the stamina/action points.
5. **Feedback:** Once the C# movement is complete, it emits an `arrived_at_destination` signal, allowing the GDScript BT to proceed to the next node (e.g., `AttackTask.gd`).

---

## 🗺️ Tactical Pathfinding & Danger Zones

To prevent entities from walking suicidally into active fire, the architecture must support "Danger Routing." Because A* weight manipulation can sometimes feel opaque or frustrating to a player (e.g., clicking to move and having the character take a bizarre, massive detour to avoid an invisible threat cone), we have two viable options for implementation:

### Option A: Dynamic A* Heatmap (Weight Manipulation)
In this approach, the `Global AI Manager` artificially spikes the pathfinding cost (weight) of grid cells currently under suppressing fire, sniper lasers, or environmental hazards.
- **How it works:** When `entity.GetPathTo()` is called, the C# algorithm naturally calculates a detour around the high-cost danger zones.
- **Pros:** Completely systemic. Entities (and player characters) automatically route around danger without needing complex AI logic.
- **Cons (Intransparency):** If a player manually clicks to move their character, the generated path might wildly zig-zag to avoid a danger zone they didn't notice, causing confusion. This requires heavy UI feedback (like turning the path line red) to explain *why* the path is weird.

### Option B: Behavior Tree-Based Danger Routing (Recommended)
Instead of manipulating the global A* grid weights, pathfinding remains "dumb and direct" (shortest path only). The responsibility of avoiding danger falls entirely on the LimboAI Behavior Tree.
- **How it works:** Before an AI commits to a move, a BT condition node (`BTCondition_IsPathSafe`) checks the proposed path against known danger zones. If the path crosses a danger zone, the BT branches to an alternative tactic (e.g., finding a different cover spot, or waiting for the danger to clear).
- **Pros:** Highly transparent. The pathfinding grid remains consistent and predictable for the player-controlled character. When an AI detours, it is because of a conscious, observable tactical decision.
- **Cons:** Requires more complex leaf nodes and raycasting/intersection checks in the BT logic before movement execution.

*Note: For civilians, Option A is often preferred so they strictly treat danger as impassable walls, while Option B is better for tactical units (player squad and enemies).* Noted, but let's make everything the same for better transparency.

---

## 👥 Extensive Entity Behaviors

### 1. Non-Controlled Player Characters (Squad Mates)
- **State:** Active only when unpossessed.
- **Behavior:** They utilize a "Tether" system. They attempt to stay within a configurable radius of the player-controlled character.
- **Combat:** If combat initiates, they will automatically seek the nearest valid full/half cover relative to the known enemy positions. They will *not* initiate attacks on unaware enemies (to preserve stealth) but will freely return fire if the squad's cover is blown.

### 2. Enemies
- **Idle / Unaware:** Governed by routine. The Enemy Manager gives them waypoints to patrol. They may pause for idle animations (smoking, chatting).
- **Suspicious:** Triggered by audio/visual cues (dogs barking, footsteps). The Enemy Manager directs the closest 1-2 guards to investigate the disturbance location while others hold position.
- **Combat:** Highly coordinated via the Enemy Manager. They utilize suppression, flanking, and grenade tossing. If their numbers dwindle, the Manager can issue a "Fall Back" order, forcing them to retreat to better cover or pull alarms.

### 3. Civilians
- **Idle:** They wander locally or stand at designated Points of Interest (POIs) like storefronts or bus stops.
- **Panic State:** Triggered globally by gunfire or dead bodies. The BT shifts to "Flee". 
- **Execution:** They ignore turn orders and move dynamically to exit zones. They act as dynamic obstacles—fleeing civilians can block Line of Sight (LoS) and movement paths for both the player and enemies. If a player shoots through a civilian, a massive monetary penalty is applied.

### 4. Animals
- **Ambient State:** Wandering (Cats hug walls, dogs roam open areas, rats stick to shadows).
- **Startled State:** If a player sprints nearby or fires a weapon, the animal's BT fires a startle event.
- **Impact:** Dogs will bark (creates a large noise sphere that alerts enemies to the *Suspicious* state). Cats will knock over trash cans (medium noise sphere) and flee. Rats will simply scatter silently. Animals are completely ignored by the pathfinding collision of larger entities, meaning you can walk "through" them, but doing so triggers their startle radius.

---

## 🎛️ Dynamic Strategy Switches (Behavior Toggles)

To make the AI feel responsive and tactical, entities and managers rely on dynamic "switches" (strategies) that can be flipped at runtime to drastically alter their behavior trees.

### 1. The Blackboard System (How to approach it)
In LimboAI, these switches are implemented as variables on the **Blackboard**. 
- The Behavior Tree (BT) uses `Condition` nodes to check these Blackboard variables at the top of a `Selector` branch. 
- If a switch is flipped, the BT naturally flows down an entirely different branch of execution on its next tick.
- The **Faction Managers** (Squad, Enemy) are responsible for flipping these switches on the individual entities' Blackboards based on the global state or player commands.

### 2. Squad Mates: Rules of Engagement (RoE)
The player can issue global or individual commands that flip switches on the non-controlled characters' Blackboards:
- **`RoE_HoldFire`:** Characters will *never* shoot, even if fired upon. They will prioritize finding absolute cover and hiding. (Useful for preserving stealth when a single guard spots you).
- **`RoE_ReturnFire` (Default):** Characters stay hidden but will engage if an enemy explicitly targets them or a squadmate.
- **`RoE_FireAtWill`:** Characters will actively pop out of cover to shoot any hostile in their Line of Sight, prioritizing aggression over stealth.
- **`Tether_Distance`:** A dynamic float switch. Can be set to `Tight` (stay within 2 tiles) for breaching, or `Loose` (stay within 8 tiles) for general exploration.

### 3. Enemy Escalation Strategies
The Enemy Manager monitors the combat situation and flips strategic switches on the enemy squad to simulate human-like panic or tactical shifts:
- **`Strategy_Cautious`:** Default combat state. Enemies prioritize half/full cover and wait for the player to push into their overwatch cones.
- **`Strategy_Aggressive`:** Triggered if the player is pinned down or reloading. Enemies abandon full cover to sprint forward and flank, attempting to close the distance.
- **`Strategy_Panicked`:** Triggered if the enemy squad suffers 50%+ casualties within a short time window (e.g., 5 seconds). Remaining enemies flip to a survival branch: they sprint toward the nearest map exit or try to reach a radio to call for backup, abandoning their offensive posts.

### 4. Implementation Workflow for a New Switch
1. **Define the Variable:** Add the variable (e.g., `bool is_flanking`) to the entity's LimboAI Blackboard setup in GDScript.
2. **Create the BT Branch:** Open the LimboAI editor and create a new `Sequence` or `Selector` branch that executes the flanking logic.
3. **Add the Condition:** Place a `BTCheckVar` node above that branch, checking if `is_flanking == true`.
4. **Trigger via C# / Orchestrator:** When the C# `EnemyManager` detects a flanking opportunity, it calls `entity.GetBlackboard().SetVar("is_flanking", true)`. The GDScript BT will automatically transition to the new strategy on the next frame.

---

## 🤖 The Relationship Between AI & State Machines

Every character in the game possesses **two distinct "Brains"** that work in tandem but serve completely different purposes. It is critical to understand the distinction between the LimboAI Behavior Tree and the local Godot State Chart on each entity.

### 1. LimboAI Behavior Tree (The Decision Maker)
- **Role:** Handles "What should I do next?" (e.g., Should I flank? Should I run away? Do I need to reload?)
- **Language:** GDScript.
- **Scope:** Abstract tactical logic.

### 2. Godot State Chart (The Physical Puppeteer)
- **Role:** Handles "What is my physical body doing right now?" (e.g., `Idle`, `Moving`, `InCover`, `Shooting`, `Dead`).
- **Language:** Configured via Nodes, driven by C#.
- **Scope:** Animation control and immediate physical state.

### How they interact:
1. The **Behavior Tree** makes a decision: "I need to move to Tile X". It tells the C# script to execute the move.
2. The C# script receives the command and fires a string event (`movement_command`) to the **Godot State Chart**.
3. The **Godot State Chart** transitions from `Idle` to `Moving`. Upon entering the `Moving` state, it triggers the walking animation and interpolation.
4. When the C# script finishes the interpolation, it fires an `arrived` event to the **Godot State Chart**, transitioning it to `InCover` (playing the crouch animation).
5. Finally, the C# script signals the **Behavior Tree** that the task is complete, allowing the AI to move to its next tactical decision (like shooting).

---

## 🐛 AI Debugging & UI Toggles

Because the AI behavior is split across multiple layers, managers, and state machines, it can be extremely difficult to track why an entity made a specific decision.

To handle this, the architecture includes a dedicated **AI Debug UI Overlay**.

### Debug UI Features
- **Behavior Tree Path:** Displays the current active branch/leaf node the entity is executing in LimboAI (e.g., `Selector -> Flank -> MoveToFlankTask`).
- **Blackboard Variables:** Real-time watch window for the entity's Blackboard (e.g., `is_flanking: true`, `has_line_of_sight: false`).
- **State Chart State:** Displays the current physical state from the Godot State Chart (e.g., `InCover (Half)`).
- **Pathfinding Paths:** Draws the A* Grid path array directly on the screen using debug lines.

### Toggling the Debug Mode
A global C# debug manager handles the visibility of this overlay.
- **In-Game Switch:** Players/Developers can toggle this on and off using a mapped hotkey (e.g., `F3` or a button in the pause menu during development). The availability of this option is being determined by a switch in the FeatureService.
- **Implementation:** When the toggle is activated, the global `GameManager` emits a `debug_ai_toggled(bool)` signal. The `EntCharacter` scripts listen for this signal and either instantiate/show their local floating debug UI canvas or hide it. This prevents the debug logic from draining performance when disabled.
