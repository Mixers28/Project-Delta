# Copilot Instructions for Project Delta

## Repository Summary

- **Type:** Unity C# project implementing a card-based puzzle game.
- **Entry points:** `Assets/Scenes/GameScene.unity` and the `GameManager` singleton (`Assets/Scripts/Managers/GameManager.cs`).

## Big Picture Architecture

### Game Manager
`GameManager` (in `Assets/Scripts/Managers/GameManager.cs`) is a singleton that:
- Constructs and holds the `GameState` instance and `PatternValidator`
- Orchestrates core gameplay flows via `StartTestLevel()` (configures goals, deals initial hand)
- Exposes public methods for player actions: `TryPlayPattern`, `DrawCard`, `DiscardCard`
- Subscribes to `GameState` events and logs results via `Debug.Log`

### Core Model Layer
Located in `Assets/Scripts/Models`:
- **`Card.cs`**: A struct with `suit` (Hearts, Diamonds, Clubs, Spades, Joker) and `rank` (Ace–King, Joker). The `IsJoker` and `IsFaceCard` properties are used in scoring and validation. The `Display` property formats cards for logs/UI (e.g., "5♥", "★JKR").
- **`Deck.cs`**: Manages a draw pile and discard pile. `Reset()` builds a standard 52-card deck + 2 jokers and shuffles. `DrawFromStock()` and `DrawFromDiscard()` return `Card?` (nullable).
- **`Goal.cs`**: Represents a level objective (e.g., "Play 2 Pairs"). Has a `GoalType`, required count, and current count. The `MatchesPattern()` method ties goals to pattern types.

### Game State & Events
`GameState` (in `Assets/Scripts/Managers/GameState.cs`) holds:
- Gameplay data: `Hand` (List<Card>), `Deck`, `Score`, `MovesRemaining`, `Goals`
- Public events (`OnCardDrawn`, `OnCardDiscarded`, `OnPatternPlayed`, `OnScoreChanged`, `OnGoalUpdated`, `OnHandChanged`) that UI components subscribe to
- Public methods: `DrawFromStock`, `DrawFromDiscard`, `DiscardCard`, `PlayPattern`, `GetSelectedCards`, and helper checks (`CanDiscard`, `CanDraw`, `IsLevelComplete`, `IsLevelFailed`)

### Patterns & Rules
All pattern classes implement `IPattern` (defined in `Assets/Scripts/Patterns/AllPatterns.cs`):
- **Pattern interface:** `Name`, `BasePoints`, `Validate(cards)`, `CalculateScore(cards)`
- **Available patterns:**
  - `PairPattern` (2 cards, same rank or jokers): 10 base points
  - `ThreeOfKindPattern` (3 cards): 30 base points
  - `RunPattern` (3, 4, or 5+ cards, same suit in sequence): 40/80/150 base points
  - `FlushPattern` (exactly 5 cards, same suit): 100 base points
  - `FullHousePattern` (exactly 5 cards, 3-of-a-kind + pair): 200 base points
- **Scoring multipliers** (in `PatternExtensions.CalculateScoreWithMultipliers`):
  - Joker in hand: ×1.5
  - All non-jokers are face cards: ×2
  - All cards different suits: ×1.2 (unique suits must equal card count)
- **PatternValidator** collects all pattern instances and provides `GetBestPattern(cards)` to pick the highest-scoring valid match.

### UI & Scenes
- UI components live in `Assets/Scripts/UI`
- Scenes under `Assets/Scenes` (main gameplay in `GameScene.unity`)
- Typical flow: UI listens to `GameState` events and updates display accordingly

## Data Flow Example: Playing a Pattern

1. Player selects cards and clicks "Play"
2. UI calls `GameManager.TryPlayPattern(cardIndices)`
3. `GameManager` builds `selectedCards` via `GameState.GetSelectedCards()`
4. `PatternValidator.GetBestPattern(selectedCards)` validates and scores
5. `GameState.PlayPattern(cards, pattern)` updates `Hand`, `Score`, calls `UpdateGoals`, fires events
6. `OnPatternPlayed`, `OnScoreChanged`, `OnGoalUpdated`, `OnHandChanged` events trigger UI refresh

## Project-Specific Conventions

- **Singleton access:** Use `GameManager.Instance` to reference the global manager.
- **Event naming:** All observable events start with `On` (e.g., `OnCardDrawn`, `OnPatternPlayed`). Subscribe/unsubscribe carefully to avoid memory leaks.
- **Add new patterns:** Implement `IPattern` in `Assets/Scripts/Patterns/YourPattern.cs`, then add `new YourPattern()` to the `patterns` list in `PatternValidator()` constructor (`AllPatterns.cs`). Add a unit test in `Assets/Tests/PatternTests.cs`.
- **Hand size:** Reference `GameState.MaxHandSize` (currently 7) instead of hardcoding.
- **Scoring:** Always use `pattern.CalculateScore(cards)` to respect multipliers; avoid recalculating manually.
- **Joker mechanics:** Jokers are wildcards in runs and groups. Always check `card.IsJoker` when validating patterns.

## Developer Workflows

### Running the Game
1. **Unity Editor:** Open the project folder in Unity Editor, open `Assets/Scenes/GameScene.unity`, press Play
2. **Debugging:** Open `Project-Delta.sln` in Visual Studio, attach to the Unity Editor process, set breakpoints
3. **Logs:** Check the Console in Unity Editor for `Debug.Log` output from `GameManager`, `GameState`, and pattern validators

### Running Tests
- **Play Mode tests:** Use Unity Test Runner (Window → General → Test Runner), then "Play Mode"
- **Edit Mode tests:** Same menu, then "Edit Mode"
- Test assemblies exist (`PlayModeTests.csproj`, `PlayModeTesters.csproj`) but prefer the Unity Test Runner UI

### Debugging Tips
- Add `Debug.Log($"Hand: {string.Join(", ", CurrentGame.Hand.ConvertAll(c => c.Display))}")` to see current hand in readable form
- Pattern validators log `No valid pattern detected` if no match is found; add more specific logging to `Validate` methods
- Event flow can be traced by logging in `GameState` event handlers (e.g., `OnGoalUpdated`)

## Common Extensions & Changes

### Add a New Pattern
1. Create `Assets/Scripts/Patterns/MyPattern.cs`:
   ```csharp
   public class MyPattern : IPattern
   {
       public string Name => "My Pattern";
       public int BasePoints => 50;
       
       public bool Validate(List<Card> cards)
       {
           // Your validation logic
       }
       
       public int CalculateScore(List<Card> cards)
       {
           return this.CalculateScoreWithMultipliers(cards);
       }
   }
   ```
2. Register in `AllPatterns.cs` constructor:
   ```csharp
   patterns.Add(new MyPattern());
   ```
3. Add a unit test in `Assets/Tests/PatternTests.cs`.

### Add a New Goal Type
1. Update `Goal.GoalType` enum in `Assets/Scripts/Models/Goal.cs`
2. Implement the mapping in `Goal.MatchesPattern()` switch statement
3. Optionally update `Goal.DisplayText` for human-readable text
4. Use in `GameManager.StartTestLevel()` when composing level goals

### Adjust Deck Composition
- Edit `Deck.Reset()` in `Assets/Scripts/Models/Deck.cs`
- Currently: 52 standard cards + 2 jokers, then Fisher-Yates shuffle
- To add/remove cards or change deck size, modify the loop and `drawPile.Add(...)` calls

## Key Files Quick Reference

| Purpose | File |
|---------|------|
| Gameplay orchestration | `Assets/Scripts/Managers/GameManager.cs` |
| Game state & rules | `Assets/Scripts/Managers/GameState.cs` |
| Pattern definitions | `Assets/Scripts/Patterns/AllPatterns.cs` |
| Individual patterns | `Assets/Scripts/Patterns/{FlushPattern,FullHousePattern}.cs` |
| Card & deck models | `Assets/Scripts/Models/{Card,Deck,Goal}.cs` |
| Unit tests | `Assets/Tests/PatternTests.cs` |
| Main scene | `Assets/Scenes/GameScene.unity` |

## Pull Request Checklist

- Run tests via Unity Test Runner (Play Mode + Edit Mode) before opening a PR
- When adding patterns or models, include a focused unit test demonstrating the rule
- Use `Card.Display` for logs to ensure readability
- Only modify `GameScripts.asmdef` if adding new assemblies; otherwise, leave unchanged
- Prefer extending `OnXxx` event handlers over adding new public methods when possible

---

**For clarification:** If you need expanded examples (e.g., a full pattern implementation with tests), detailed diagrams, or deeper dives into specific subsystems, let me know which section to expand.
