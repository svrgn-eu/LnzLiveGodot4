# Animal Reactions

## 📖 Overview
Introduces an `AnimalType` enum to categorize animals and defines a system of behavioral reactions (e.g., avoid, chase) between different entities in the game, such as dogs chasing cats or cats avoiding people.

## 🎯 Goals & Use Cases
- **Goal 1:** Make the game world feel more alive by introducing dynamic and natural interactions between animals and characters.
- **Use Case A:** A stray dog spots a stray cat and starts chasing it.
- **Use Case B:** A stray cat notices a dog or a person approaching and actively avoids them by running away.

## 👥 Target Audience / User Roles
- Players exploring the game world, enhancing immersion through ambient AI behaviors.

## ⚙️ Functional Requirements
- [x] **Define `AnimalType` Enum:** Add an enum with values like `None`, `Cat`, `Dog`, `Bird`, `Rodent`, etc.
- [x] **Define `ReactionType` Enum:** Add an enum with values like `Ignore`, `Avoid`, `Chase`.
- [x] **Reaction Matrix:** Establish a set of rules determining how one entity reacts to another (e.g., `Cat` reacts to `Dog` with `Avoid`, `Dog` reacts to `Cat` with `Chase`, `Cat` reacts to `Person/Character` with `Avoid`).
- [x] **Data Refactoring:** Refactor specific animal scripts (like `StrayCatData.cs` and controllers) into generalized `AnimalData` and `AnimalController` base classes. All specific animals will inherit from these bases to utilize the universal `AnimalType` and reaction logic.
- [x] **AI Integration:** Update the behavior trees (BT) or AI logic so that animals scan for nearby entities, check the reaction matrix, and trigger appropriate movement states (like fleeing or pursuing).

## ✅ Implementation Notes (Completed)
- `AnimalType.cs`, `ReactionType.cs`, and `ReactionMatrix.cs` were created to define the core data and logic.
- `StrayCatData.cs` was refactored to inherit from the new generic `AnimalData.cs` resource.
- The `IsoStrayCat.cs` controller was entirely replaced by a new, generalized `IsoAnimal.cs` controller.
- A new `BTReactToEntities.gd` behavior tree task was created and integrated into the `StrayCatAI.gd` using a `Selector` pattern to seamlessly transition between reacting to entities or aimlessly wandering.


## 🎨 UI/UX Design & Interactions
- **Visuals:** This is primarily an AI feature. Interactions will be represented by in-game movement behaviors (running towards a target for a chase, or running in the opposite direction to avoid).
- **Indicators (Optional):** Consider adding small visual cues (like a startled exclamation mark `!` above a cat's head when it spots a dog) for better player feedback.

## 💻 Technical Details
- **C# Models:** Create `AnimalType.cs` and `ReactionType.cs` enums.
- **Architecture Updates:** Create a new `AnimalData.cs` base class containing `public AnimalType Type { get; set; }`. Have `StrayCatData.cs` inherit from this. Refactor logic to use a general `AnimalController`.
- **Behavior Tree:** Implement new behavior tree nodes (e.g., `BTCheckNearbyAnimals`, `BTReactToEntity`) that utilize existing movement scripts like `BTMoveToDestination.gd` to handle the actual logic of pursuing or evading.

## 🧪 Testing Plan
- **Scenario 1:** Place a Dog and a Cat in the same scene. Verify that the Dog detects the Cat and changes its state to `Chase`, and the Cat changes to `Avoid`.
- **Scenario 2:** Place a Cat near a Character/Person. Verify that the Cat flees upon detection.
- **Scenario 3:** Place two entities that should ignore each other (e.g., two Cats, depending on design) and ensure they do not trigger aggressive or evasive behaviors.

## 🔗 Related Resources
- Existing AI logic in `BTMoveToDestination.gd`
- Current specific scripts like `StrayCatData.cs` (to be refactored into generic `AnimalData` and `AnimalController`).
- `CharacterData.cs` for entity references.
