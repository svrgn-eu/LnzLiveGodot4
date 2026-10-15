# Smooth Pathfinding Shortcuts

## 📖 Overview
Currently, character movement strictly follows the centers of the isometric diamond-shaped tiles returned by the A* pathfinding algorithm. This results in rigid, "zig-zag" movement patterns, especially when traveling diagonally across open spaces. This feature aims to implement path smoothing (often called "String Pulling"), allowing characters to take straight-line shortcuts across multiple tiles if the direct path is unobstructed. 

## 🎯 Goals & Use Cases
- **Goal 1:** Improve visual fluidity of character movement.
- **Goal 2:** Optimize travel distance by allowing straight lines across open areas.
- **Use Case A:** A player clicks a destination across an open warehouse floor. Instead of zig-zagging strictly along the grid, the character walks in a direct straight line to the destination.
- **Use Case B:** A player routes a character around a corner. The character walks in a straight line to the corner, turns, and then walks in a straight line to the destination, minimizing unnecessary waypoints.

## ⚙️ Functional Requirements
- [x] The system must generate a standard A* grid path first to guarantee valid routing around obstacles.
- [x] A post-processing "path smoothing" algorithm must run on the generated path to strip out unnecessary intermediate waypoints.
- [x] The smoothing algorithm must verify line-of-sight (walkability) between non-adjacent waypoints before taking a shortcut.
- [x] The character's `MovementService` or `IsoCharacter` script must be able to follow vector-based waypoints rather than strict grid coordinates.
- [ ] The smoothed path must only consist of segments that perfectly align with the 8 isometric directions (N, NE, E, SE, S, SW, W, NW) to prevent the character from sliding off-axis relative to their movement animations.

## 💻 Technical Details
- **Path Smoothing Algorithm (String Pulling / Raycasting):** 
  Iterate through the `AStarGrid2D` path array. For any waypoint `A`, test if there is an unobstructed line of sight directly to waypoint `C` (skipping `B`). If the straight line is walkable, remove `B` from the path. Repeat until the path is minimal.
- **Line of Sight Check:** 
  Godot does not natively support Raycasting on `AStarGrid2D` cells. We can simulate this using Bresenham's line algorithm mapped to grid coordinates, or rely on Physics2D Raycasts if our tiles have physical collision shapes (which they do, thanks to the recent `AStarGridManager` collision updates).
- **Core Components to Modify:**
  - `TacticalController.cs` or `AStarGridManager.cs`: Where `GetIdPath()` is called. The smoothing logic should be applied immediately after fetching the path.
  - `IsoCharacter.cs`: The movement interpolation may need slight adjustments to ensure it smoothly rotates and moves towards arbitrary Vector2 coordinates rather than strictly snapping to isometric cell centers.

## 🧪 Testing Plan
- **Open Space Test:** Click far across an empty room; verify the path is a single straight line (2 waypoints: start and end).
- **Obstruction Test:** Click around a wall; verify the path hugs the corner without clipping through the wall.
- **Diagonal Check:** Ensure that moving "diagonally" (which is straight left/right/up/down in screen space for isometric grids) correctly takes a straight visual line.

## 🔗 Related Resources
- [Bresenham's Line Algorithm](https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm)
- Current pathfinding implementation: `AStarGridManager.cs`
