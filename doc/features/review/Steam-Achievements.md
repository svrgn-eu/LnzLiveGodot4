# Feature: Steam Achievements Integration

## 📖 Overview
Integrating Steam Achievements to reward player milestones, encourage exploration, and increase player retention. This feature outlines the technical approach to communicating with the Steamworks API within the Godot C# environment.

## 🎯 Goals & Use Cases
- **Goal 1:** Connect the game to the Steam client to unlock achievements for the player.
- **Goal 2:** Provide in-game notifications when an achievement is unlocked.
- **Use Case A:** A player completes the first mission, and the "First Blood" achievement unlocks on Steam.
- **Use Case B:** A player accumulates 100 headshots over multiple play sessions, triggering a cumulative achievement.

## 👥 Target Audience / User Roles
- **Players:** Engage with achievements for completionist goals and bragging rights.

## ⚙️ Functional Requirements
- [ ] Requirement 1: The game must initialize the Steam API on startup.
- [ ] Requirement 2: The game must safely ignore Steam calls if the Steam client is not running (e.g., DRM-free builds).
- [ ] Requirement 3: An `IAchievementService` should abstract the Steamworks calls, allowing for easy testing or integration with other platforms (Epic, GOG) later.
- [ ] Requirement 4: Define signals or events that trigger achievement checks throughout the game loop.

## 🎨 UI/UX Design & Interactions
- Steam natively provides an overlay notification when an achievement is unlocked.
- Optional: Add an in-game toast notification or an "Achievements" screen in the Main Menu to show progress.

## 💻 Technical Details
- **Packages/Plugins:** 
  - For Godot C#, [Facepunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks) is highly recommended as it is robust and easy to integrate in C# projects.
  - Alternatively, GodotSteam (C# bindings) can be used.
- **Architecture:**
  - Create an `IAchievementService` (e.g., `SteamAchievementService`).
  - Use Event/Signal-based triggers. For example, `IGameStateService.OnMissionComplete` triggers `IAchievementService.CheckMissionAchievements()`.
- **Examples for Tactical RPG Genre:**
  - **"First Blood"**: Complete the first combat encounter.
  - **"Ghost"**: Complete a mission without being detected.
  - **"Surgical Precision"**: Complete a mission using only headshots.
  - **"Demolition Expert"**: Destroy 50 pieces of cover.
  - **"No Man Left Behind"**: Revive a downed squadmate 10 times.

## 🧪 Testing Plan
- Test initialization with the Steam Spacewar AppID (480) during development.
- Use Steamworks developer tools to lock/unlock achievements and verify the game updates correctly.
- Test playing without Steam running to ensure the game doesn't crash.
