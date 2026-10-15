# Feature Design: Player Spawn Points via Tiled

**Status:** Concept / Proposed
**Component:** Level Design / Tiled Pipeline

## Goal
To allow level designers to define exact starting positions (spawn points) for player characters, enemies, and NPCs directly within the Tiled Map Editor, rather than manually dragging nodes in the Godot Editor.

## Proposed Workflow in Tiled

The cleanest and most robust way to define spawn points in Tiled—which translates perfectly to Godot via the YATI importer—is to use **Object Layers** and **Point Objects**.

### 1. Create a Spawn Object Layer
1. In Tiled, add a new **Object Layer** (Layer -> New -> Object Layer).
2. Name it something clear, like `SpawnPoints`

### 2. Place Spawn Points
1. Select the **Insert Point** tool (or press `I`) in the Tiled toolbar.
2. Click on the map where you want a character to spawn. This creates a tiny, invisible coordinate point.
3. Select the point, and in the **Properties** panel on the left:
   - **Name:** Set this to the character's callsign or role (e.g., `Spawn_Test01` or `Spawn_Player`).
   - **Class:** (Optional) Set this to `SpawnPoint` to help categorize it later.

### 3. Add Custom Properties
You can pass data directly from Tiled to the Godot spawn scripts by adding Custom Properties to the point object:
1. At the bottom of the Properties panel, click the `+` button to add a custom property.
2. Add a `String` property named `character_id` or character class like "NPC" or "Enemy_WithGun" to enforce a random entity within that category to be created. For Player Teams, the character_id is still relevant.
3. Set the value to match your character resource (e.g., `Test01` or `StrayCat`).
4. (Optional) Add a `Float` property named `facing_angle` if you want them to spawn looking a certain direction.

## How it Imports into Godot (YATI)

When YATI imports a Tiled map containing an Object Layer with Point Objects:
1. It creates a `Node2D` container for the Object Layer (e.g., named `Spawns`).
2. Inside that container, it generates a **`Marker2D`** node for every Point Object you placed.
3. The `Marker2D` will be placed at the exact global coordinates defined in Tiled.
4. Crucially, any **Custom Properties** you added in Tiled (like `character_id = Test01`) will be attached to that `Marker2D` node as **Metadata** (`meta`).

## Implementation Plan (For Later)

When we are ready to implement this in code, the workflow will look like this:

1. **Level Bootstrapper:** During `SysMainGameWindow` or `SceneManagementService` load, we run a routine to find the `Spawns` node.
2. **Scan Markers:** Iterate through all `Marker2D` children of the `Spawns` node.
3. **Read Metadata:** For each marker, read `GetMeta("character_id")`. This string could be a specific ID (e.g., `oksana`) or a category class (e.g., `NPC`, `Enemy_WithGun`).
4. **Resolution & Instantiation:**
   - **Specific ID:** If the ID matches a known unique character, load that exact `EntCharacter.tscn` and its specific data resource.
   - **Category (Randomization):** If the string is a category, the spawn routine queries a central `EntityRegistry` (or similar data structure) for all character resources matching that category. It then randomly selects one valid candidate and instantiates it. This makes crowd generation and enemy encounters highly dynamic.
5. **Position & Facing:** Set the newly spawned character's `GlobalPosition` to match the `Marker2D`'s `GlobalPosition`. If a `facing_angle` meta exists, apply it to the character's initial orientation.
6. **Cleanup:** Delete the `Marker2D` node since it is no longer needed.

### Feasibility of Random Category Spawning
This enhancement is completely feasible and highly recommended for isometric tactical games.
- **Godot/YATI:** YATI seamlessly maps Tiled custom string properties into Godot node metadata. Reading it is a single `GetMeta()` call.
- **Architecture:** The logic merely requires an `EntityRegistry` or `SpawnService` that groups scene/resource paths by category tags (e.g., a dictionary `{"NPC": ["res://civilian1.tscn", "res://worker.tscn"]}`). Random selection is then a simple array pick.

### Benefits
- **Zero Godot Editor Work:** Designers can build the level and place all entities entirely in Tiled.
- **Data-Driven:** Custom properties allow spawning dynamic enemy types, setting initial patrol paths, or defining team factions right from the map file.
- **Grid Perfect:** Spawns will automatically respect Tiled's isometric grid coordinates.
