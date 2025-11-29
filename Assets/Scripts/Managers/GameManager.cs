using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentGame { get; private set; }
    public PatternValidator PatternValidator { get; private set; }

    [Header("Level Configuration")]
    [SerializeField] private LevelDefinition testLevelDefinition;
    [SerializeField] private List<LevelDefinition> levelSequence = new();
    [SerializeField] private GameOverPanel gameOverPanel;

    private bool gameEnded;
    private LevelDefinition[] loadedLevels = Array.Empty<LevelDefinition>();
    private int currentLevelIndex;
    private int progressionStep;
    
    // Events for UI listeners
    public delegate void GameEndHandler(bool isWin);
    public event GameEndHandler OnGameEnd;
    public event System.Action<GameState> OnGameStarted;

    #region Unity lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureUISetup();
        EnsureLevelsLoaded();
    }

    private void Start()
    {
        PatternValidator = new PatternValidator();
        StartTestLevel();
    }
    #endregion

    // Entry point to start/restart the current level without changing the phase path
    public void StartTestLevel()
    {
        if (progressionStep < 0) progressionStep = 0;
        StartLevel(atIndex: currentLevelIndex);
    }

    public void RestartCurrentLevel()
    {
        // On fail+retry, reset progression to stage 1 of the first level
        progressionStep = 0;
        currentLevelIndex = 0;
        StartLevel(atIndex: currentLevelIndex);
    }

    public void StartNextLevel()
    {
        EnsureLevelsLoaded();
        if (loadedLevels.Length == 0)
        {
            StartLevel(atIndex: 0);
            return;
        }

        progressionStep++;
        currentLevelIndex = (currentLevelIndex + 1) % loadedLevels.Length;
        StartLevel(atIndex: currentLevelIndex);
    }

    private void StartLevel(int atIndex)
    {
        EnsureUISetup();
        EnsureLevelsLoaded();

        CleanupCurrentGame();
        gameEnded = false;

        if (loadedLevels.Length == 0)
        {
            loadedLevels = new[] { BuildInlineFallbackDefinition() };
        }

        currentLevelIndex = Mathf.Clamp(atIndex, 0, loadedLevels.Length - 1);
        var levelDef = BuildRuntimeVariant(loadedLevels[currentLevelIndex], progressionStep);

        InitializeNewGame(levelDef);
    }

    private void InitializeNewGame(LevelDefinition levelDefinition)
    {
        CurrentGame = new GameState(levelDefinition);
        BindGameEvents(CurrentGame);

        CurrentGame.DealInitialHand();

        // Notify listeners that a new game is ready so they can rebind UI
        OnGameStarted?.Invoke(CurrentGame);

        Debug.Log($"=== LEVEL STARTED: {CurrentGame.LevelName} ===");
        Debug.Log($"Goals: {string.Join(", ", CurrentGame.Goals.ConvertAll(g => g.DisplayText))}");
        Debug.Log($"Moves: {CurrentGame.MovesRemaining}");
        Debug.Log($"Deck tweaks: {CurrentGame.DeckDescription}");
        Debug.Log($"Hand: {string.Join(", ", CurrentGame.Hand.ConvertAll(c => c.Display))}");
    }

    public void TryPlayPattern(List<int> cardIndices)
    {
        if (CurrentGame == null) return;

        var selectedCards = CurrentGame.GetSelectedCards(cardIndices);
        if (selectedCards.Count == 0) return;

        // Find best pattern
        var bestPattern = PatternValidator.GetBestPattern(selectedCards);
        if (!bestPattern.HasValue)
        {
            Debug.Log("No valid pattern detected");
            return;
        }

        // Play pattern
        bool success = CurrentGame.PlayPattern(selectedCards, bestPattern.Value.pattern);
        if (!success)
        {
            Debug.LogError("Failed to play pattern");
        }
    }

    public void DrawCard(bool fromDiscard = false)
    {
        if (CurrentGame == null) return;

        bool success = fromDiscard ? CurrentGame.DrawFromDiscard() : CurrentGame.DrawFromStock();
        
        if (!success)
        {
            Debug.Log("Cannot draw card");
        }
    }

    public void DiscardCards(List<int> handIndices)
    {
        if (CurrentGame == null) return;
        if (handIndices == null || handIndices.Count == 0) return;

        // Normalize indices to cards to avoid shifting issues
        var cards = CurrentGame.GetSelectedCards(handIndices);
        int discardCount = cards.Count;
        bool success = CurrentGame.DiscardCards(cards);

        if (!success)
        {
            Debug.Log("Cannot discard selected cards");
            return;
        }

        // Auto-refill: draw the same number of cards without extra move cost.
        for (int i = 0; i < discardCount; i++)
        {
            if (!CurrentGame.DrawFromStock(consumeMove: false))
            {
                break; // stop if deck is empty or blocked
            }
        }
    }

    // Batch draw: draw N cards in one move
    public void DrawCards(int count, bool fromDiscard = false)
    {
        if (CurrentGame == null) return;
        if (count <= 0) return;

        bool success = CurrentGame.DrawCards(count, fromDiscard);
        if (!success)
        {
            Debug.Log("Cannot draw requested cards");
        }
    }

    // Event handlers
    private void HandlePatternPlayed(IPattern pattern, int score)
    {
        Debug.Log($"<color=green>Played {pattern.Name} for {score} points!</color>");
    }

    private void HandleGoalUpdated(Goal goal)
    {
        Debug.Log($"Goal updated: {goal.DisplayText}");
        
        if (goal.IsComplete)
        {
            Debug.Log($"<color=yellow>Goal complete: {goal.DisplayText}!</color>");
        }

        CheckLevelComplete();
    }

    private void HandleScoreChanged(int newScore)
    {
        Debug.Log($"Score: {newScore}");
    }

    private void HandleMovesChanged()
    {
        Debug.Log($"Moves remaining: {CurrentGame.MovesRemaining}");
        CheckLevelComplete();
    }

    private void LockPlayerInput()
    {
        var actionButtons = FindObjectOfType<ActionButtons>();
        actionButtons?.SetInputEnabled(false);

        if (HandDisplay.Instance != null)
        {
            HandDisplay.Instance.SetInputEnabled(false);
        }
    }

    private void CheckLevelComplete()
    {
        if (CurrentGame == null)
        {
            Debug.LogError("CheckLevelComplete: CurrentGame is NULL!");
            return;
        }

        if (gameEnded)
        {
            Debug.Log("CheckLevelComplete: Game already ended; skipping repeat handling.");
            return;
        }

        Debug.Log($"CheckLevelComplete: IsLevelComplete={CurrentGame.IsLevelComplete}, IsLevelFailed={CurrentGame.IsLevelFailed}");
        
        if (CurrentGame.IsLevelComplete)
        {
            LockPlayerInput();
            Debug.Log("<color=cyan>=== LEVEL COMPLETE! ===</color>");
            Debug.Log($"Final Score: {CurrentGame.Score}");
            OnGameEnd?.Invoke(true); // Notify listeners of WIN
            Debug.Log("V OnGameEnd invoked for WIN");
            gameEnded = true;
        }
        else if (CurrentGame.IsLevelFailed)
        {
            LockPlayerInput();
            Debug.Log("<color=red>=== LEVEL FAILED ===</color>");
            Debug.Log("Out of moves!");
            OnGameEnd?.Invoke(false); // Notify listeners of LOSS
            Debug.Log("V OnGameEnd invoked for LOSS");
            gameEnded = true;
        }
        else
        {
            Debug.Log("CheckLevelComplete: Level is still in progress");
        }
    }

    private void EnsureUISetup()
    {
        if (gameOverPanel == null)
        {
            throw new MissingReferenceException("GameManager requires a GameOverPanel reference set in the inspector.");
        }

        gameOverPanel.Configure();
    }

    private void CleanupCurrentGame()
    {
        if (CurrentGame == null)
        {
            return;
        }

        UnbindGameEvents(CurrentGame);
        CurrentGame = null;
    }

    private void EnsureLevelsLoaded()
    {
        if (loadedLevels.Length > 0)
        {
            return;
        }

        if (levelSequence != null && levelSequence.Count > 0)
        {
            loadedLevels = levelSequence.FindAll(l => l != null).ToArray();
        }

        if (loadedLevels.Length == 0)
        {
            var resourceLevels = Resources.LoadAll<LevelDefinition>("Levels");
            if (resourceLevels != null && resourceLevels.Length > 0)
            {
                loadedLevels = resourceLevels;
            }
        }

        if (loadedLevels.Length == 0 && testLevelDefinition != null)
        {
            loadedLevels = new[] { testLevelDefinition };
        }
    }

    private LevelDefinition BuildRuntimeVariant(LevelDefinition baseDefinition, int difficultyStep)
    {
        if (baseDefinition == null)
        {
            return BuildInlineFallbackDefinition();
        }

        var variant = ScriptableObject.CreateInstance<LevelDefinition>();
        variant.levelName = $"{baseDefinition.levelName} - Stage {difficultyStep + 1}";

        int moveVariance = UnityEngine.Random.Range(-1, 2) + Mathf.Min(difficultyStep, 2);
        variant.totalMoves = Mathf.Max(6, baseDefinition.totalMoves + moveVariance + difficultyStep);

        variant.goals = BuildVariantGoals(baseDefinition, difficultyStep, variant.totalMoves);

        variant.deckTweaks = CloneDeckTweaks(baseDefinition.deckTweaks, difficultyStep);
        return variant;
    }

    private List<GoalDefinition> BuildVariantGoals(LevelDefinition baseDefinition, int difficultyStep, int totalMoves)
    {
        var goals = new List<GoalDefinition>();

        var baseGoals = baseDefinition.goals != null && baseDefinition.goals.Count > 0
            ? baseDefinition.goals
            : BuildInlineFallbackDefinition().goals;

        foreach (var goal in baseGoals)
        {
            int bonus = UnityEngine.Random.Range(0, 2 + Mathf.Min(difficultyStep, 2));
            var required = Mathf.Max(1, goal.required + bonus);
            required = CapToMoves(required, totalMoves);

            goals.Add(new GoalDefinition
            {
                goalType = goal.goalType,
                required = required
            });
        }

        // Add an extra challenge goal as difficulty increases
        if (difficultyStep >= 1)
        {
            var extraType = GetRandomGoalType();
            int required = Mathf.Max(1, 1 + difficultyStep / 2 + UnityEngine.Random.Range(0, 2));
            required = CapToMoves(required, totalMoves);
            goals.Add(new GoalDefinition { goalType = extraType, required = required });
        }

        return goals;
    }

    private Goal.GoalType GetRandomGoalType()
    {
        // Rotate through goal types except TotalScore to keep variety
        var candidates = new List<Goal.GoalType>
        {
            Goal.GoalType.Pair,
            Goal.GoalType.ThreeOfKind,
            Goal.GoalType.Run3,
            Goal.GoalType.Run4,
            Goal.GoalType.Run5,
            Goal.GoalType.Flush,
            Goal.GoalType.FullHouse
        };

        int index = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[index];
    }

    private int CapToMoves(int required, int totalMoves)
    {
        // Ensure the required count stays achievable given move budget
        int maxReasonable = Mathf.Max(1, totalMoves / 2);
        return Mathf.Min(required, maxReasonable);
    }

    #region Event binding helpers
    private void BindGameEvents(GameState game)
    {
        if (game == null) return;

        game.OnPatternPlayed += HandlePatternPlayed;
        game.OnGoalUpdated += HandleGoalUpdated;
        game.OnScoreChanged += HandleScoreChanged;
        game.OnMovesChanged += HandleMovesChanged;
    }

    private void UnbindGameEvents(GameState game)
    {
        if (game == null) return;

        game.OnPatternPlayed -= HandlePatternPlayed;
        game.OnGoalUpdated -= HandleGoalUpdated;
        game.OnScoreChanged -= HandleScoreChanged;
        game.OnMovesChanged -= HandleMovesChanged;
    }
    #endregion

    private DeckTweakSettings CloneDeckTweaks(DeckTweakSettings source, int difficultyStep)
    {
        var clone = new DeckTweakSettings();
        if (source != null)
        {
            clone.extraJokers = Mathf.Max(0, source.extraJokers + UnityEngine.Random.Range(0, 1 + Mathf.Min(difficultyStep, 1)));
            clone.shuffleAfterTweaks = source.shuffleAfterTweaks;
            if (source.additionalCards != null)
            {
                foreach (var extra in source.additionalCards)
                {
                    clone.additionalCards.Add(new ExtraCardDefinition
                    {
                        suit = extra.suit,
                        rank = extra.rank,
                        count = Mathf.Max(0, extra.count + UnityEngine.Random.Range(0, Mathf.Min(3 + difficultyStep, 5)))
                    });
                }
            }
        }
        else
        {
            clone.extraJokers = UnityEngine.Random.Range(0, 2 + Mathf.Min(difficultyStep, 1));
        }

        return clone;
    }

    private LevelDefinition BuildInlineFallbackDefinition()
    {
        var fallback = ScriptableObject.CreateInstance<LevelDefinition>();
        fallback.levelName = "Inline Test Level";
        fallback.totalMoves = 15;
        fallback.goals = new List<GoalDefinition>
        {
            new GoalDefinition { goalType = Goal.GoalType.Pair, required = 2 },
            new GoalDefinition { goalType = Goal.GoalType.Run3, required = 1 }
        };
        return fallback;
    }
}
