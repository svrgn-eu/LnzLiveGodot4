### **Feature Request: Generic `MarbleBag<T>` Implementation in C#**

#### **1. Objective & Background**
Pure random number generation (RNG) in games is stateless, which can lead to frustrating unlucky streaks (for instance, a 25% critical hit rate has a 31.6% chance of failing 4 times in a row and a 10% chance of failing 8 times in a row). Because humans subconsciously expect random independent events to self-correct over short stretches, stateless RNG often feels broken or unfair to players.

To solve this, implement a stateful **Marble Bag** (also known as a shuffle bag or deck) pattern in C#. Instead of rolling pure probabilities, items (marbles) are placed in a bag and drawn **without replacement**. When the bag is empty, it automatically refills and reshuffles. This gives randomness a memory and guarantees a fair distribution over short stretches.

---

#### **2. Architectural & Naming Requirements**
- **Class Name**: Must be named `MarbleBag<T>` (avoid using `Deck` to prevent namespace collisions in game codebases).
- **Type Safety**: Implement as a strongly typed, generic C# class (`MarbleBag<T>`).
- **Framework Independence**: The implementation should be purely standard C# compatible and not strictly require Godot-specific assemblies. It should be fully portable to other C# environments or standalone backends.
- **RNG Dependency**: The constructor must accept an optional RNG instance to support deterministic, seeded runs. (To maintain framework independence, this could be a standard `System.Random` or a custom interface like `IRandomProvider` that wraps Godot's `RandomNumberGenerator`).
- **State Management**:
  - Maintain an **Initial Template Pool** (the full set of items and their base frequencies).
  - Maintain an **Active Drawing Pool** (the remaining marbles currently available in the bag).
- **Refill Logic**: Automatically refill the active pool from the template pool and reshuffle when the bag runs out.

---

#### **3. Key Functional Specifications**

1. **Initialization & Population**:
   - Provide constructors to initialize the bag from a collection or dictionary of item counts, and optionally accept an external RNG instance.
   - Provide helper methods like `Add(T item, int count)` to populate distributions. **Note on State Mutation:** If `Add()` is called after `Draw()` has already started, the new items should only update the Initial Template Pool (taking effect on the next Refill) or the bag should throw an exception to prevent mid-cycle corruption.
   - *Example*: Adding 2 "Crit" items and 6 "Normal" items creates a bag with a 25% crit probability.

2. **Core Operations**:
   - `T Draw()`: Draws and removes one marble from the active pool in **O(1) time complexity** (e.g., via random index swap-and-pop with the last list element). If the active pool is empty, auto-refill and reshuffle first.
   - `void Reset()` / `Refill()`: Manually resets the active pool back to the initial template state and reshuffles.
   - `int RemainingCount { get; }`: Returns how many marbles remain in the active drawing pool.
   - `int TotalCapacity { get; }`: Returns the total size of the bag when fully refilled.

3. **Support for Nested Marble Bags**:
   - The design must support nesting (e.g., `MarbleBag<MarbleBag<T>>` or drawing an item that triggers a sub-bag draw). *Note: With standard C# generics, drawing from an outer bag returns the inner bag itself, meaning the developer must write logic to resolve nested bags manually.*
   - *Example Scenario*: In a word game, an outer bag determines whether to spawn a Consonant or Vowel. The Consonant bag can contain a marble that leads to a nested sub-bag of rare letters (`J`, `K`, `Q`, `X`, `Z`). This ensures rare items are distributed predictably and spaced apart.

---

#### **4. Example Usage Target**

```csharp
// Example 1: Simple Crit Rate (25% crit = 2 Crit Marbles, 6 Non-Crit Marbles)
var critBag = new MarbleBag<bool>();
critBag.Add(true, 2);   // 2 Crit marbles
critBag.Add(false, 6);  // 6 Non-Crit marbles

bool isCrit = critBag.Draw(); // Draws without replacement; auto-refills when empty

// Example 2: Nested Sub-Bag Setup
var rareLetterBag = new MarbleBag<char>();
rareLetterBag.Add('J', 1);
rareLetterBag.Add('K', 1);
rareLetterBag.Add('Q', 1);
rareLetterBag.Add('X', 1);
rareLetterBag.Add('Z', 1);
```

---

#### **5. Acceptance Criteria**
1. **Guaranteed Bounded Distribution**: Drawing 8 times from a bag initialized with 2 red and 6 blue marbles will always yield exactly 2 red and 6 blue draws.
2. **Auto-Refill**: Calling `Draw()` on an empty bag automatically refills and reshuffles without throwing errors.
3. **Generics Support**: Works seamlessly with primitives, custom classes, and nested `MarbleBag` objects.
4. **Unit Tests**: Include unit tests verifying state persistence across draws, auto-refill behavior, seeded RNG determinism, and nested bag handling.
5. **Serialization:** The active pool state must be exposed or designed to be easily serialized to prevent players from reloading saves to bypass bad RNG (save scumming).

#### **6. Unit Testing Setup**
Because `MarbleBag<T>` is built for framework independence, its unit tests should run via standard .NET tools rather than inside Godot.

- **Project Location**: The test project should be created outside of the Godot `src/` directory (for example, in a `tests/IsoTactics.Tests/` folder at the root of the repository). This prevents Godot from unnecessarily importing test files.
- **Initialization**: Create the project using MSTest (e.g., `dotnet new mstest -n IsoTactics.Tests`).
- **Dependencies**: The test project must reference the main game assembly (`dotnet add reference ../src/<YourProjectName>.csproj`). Ensure that the class does not invoke Godot APIs (like `GD.Print`) to guarantee the tests can run purely via the .NET CLI.
- **Execution**: Tests should be executed using the standard `dotnet test` command or a C# IDE test explorer.
- **CI/CD Pipeline Integration**: These unit tests must be integrated into the deployment pipeline. A successful run of `dotnet test` is required to act as a deployment gate; if any `MarbleBag` test fails, the pipeline must block the deployment.

### Sources
https://www.youtube.com/shorts/2anY9ICA-Pg
https://store.godotengine.org/asset/aravix/marblebag/
