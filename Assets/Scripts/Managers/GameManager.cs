using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentGame { get; private set; }
    public PatternValidator PatternValidator { get; private set; }
    public bool IsRunModeActive => RunModeService.IsActive;
    public bool LastEndWasRunMode { get; private set; }

    [Header("Level Configuration")]
    [SerializeField] private LevelDefinition testLevelDefinition;
    [SerializeField] private List<LevelDefinition> levelSequence = new();
    [SerializeField] private GameOverPanel gameOverPanel;

    private bool gameEnded;
    private LevelDefinition[] loadedLevels = Array.Empty<LevelDefinition>();
    private int currentLevelIndex;
    private int progressionStep;

    public System.Collections.Generic.IReadOnlyList<PatternId> AllowedPatterns { get; private set; } = System.Array.Empty<PatternId>();
    public AchievementsService Achievements { get; private set; }
    
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

        ProgressionService.EnsureInitialized();
        Achievements = new AchievementsService(ProgressionService.Profile);
        AchievementToast.EnsureExists();
        AchievementsScreen.EnsureExists();
        MainMenuScreen.EnsureExists();
        StartupScreen.EnsureExists();
        PostTutorialScreen.EnsureExists();
        Achievements.OnUnlocked += HandleAchievementUnlocked;
        EnsureUISetup();
        EnsureLevelsLoaded();
    }

    private void Start()
    {
        StartCoroutine(BootSequence());
    }
    #endregion

    private System.Collections.IEnumerator BootSequence()
    {
        PatternValidator = new PatternValidator();

        if (AuthService.IsLoggedIn && !CloudProfileStore.BaseUrl.Contains("YOUR-RAILWAY-APP"))
        {
            bool done = false;
            PlayerProfile serverProfile = null;
            string error = null;

            yield return CloudProfileStore.FetchProfile(
                profile =>
                {
                    serverProfile = profile;
                    done = true;
                },
                err =>
                {
                    error = err;
                    done = true;
                });

            while (!done)
            {
                yield return null;
            }

            if (serverProfile != null)
            {
                var local = ProgressionService.Profile;
                if (serverProfile.lastUpdatedUtc >= local.lastUpdatedUtc)
                {
                    ProgressionService.ReplaceProfile(serverProfile, saveLocal: true, includeCloud: false);
                    ReinitializeProfileBoundServices();
                }
                else
                {
                    ProgressionService.Save(includeCloud: true);
                }
            }
            else if (string.IsNullOrWhiteSpace(error))
            {
                // No profile on server yet; push local.
                ProgressionService.Save(includeCloud: true);
            }
            else
            {
                Debug.LogWarning($"[Cloud] Profile fetch failed: {error}");
            }
        }
        else if (AuthService.IsLoggedIn)
        {
            Debug.LogWarning("[Cloud] BaseUrl not configured; skipping cloud sync.");
        }

        StartupScreen.ShowOnBoot(StartTestLevel);
    }

    // Entry point to start/restart the current level without changing the phase path
    public void StartTestLevel()
    {
        if (progressionStep < 0) progressionStep = 0;
        if (ProgressionService.IsTutorialActive)
        {
            StartTutorialLevel();
            return;
        }

        if (PostTutorialScreen.ShowIfNeeded(onStartRun: StartRunMode, onContinue: StartTestLevel))
        {
            return;
        }

        StartLevel(atIndex: currentLevelIndex);
    }

    public void StartRunMode()
    {
        if (!RunModeService.CanStart())
        {
            Debug.LogWarning("Run Mode is locked until the tutorial is complete.");
            return;
        }

        RunModeService.StartNewRun();
        progressionStep = 0;
        currentLevelIndex = 0;
        StartLevel(atIndex: currentLevelIndex);
    }

    public void StopRunMode()
    {
        RunModeService.StopRun();
    }

    public void RestartCurrentLevel()
    {
        if (ProgressionService.IsTutorialActive)
        {
            StartTutorialLevel();
            return;
        }

        // Explicit restart counts as ending the current run; start a fresh one.
        if (RunModeService.IsActive)
        {
            RunModeService.StartNewRun();
        }

        // On fail+retry, reset progression to stage 1 of the first level
        progressionStep = 0;
        currentLevelIndex = 0;
        StartLevel(atIndex: currentLevelIndex);
    }

    public void StartNextLevel()
    {
        if (ProgressionService.IsTutorialActive)
        {
            StartTutorialLevel();
            return;
        }

        if (PostTutorialScreen.ShowIfNeeded(onStartRun: StartRunMode, onContinue: StartTestLevel))
        {
            return;
        }

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

    private void StartTutorialLevel()
    {
        EnsureUISetup();

        CleanupCurrentGame();
        gameEnded = false;

        var tutorialDef = TutorialLevelFactory.Create(ProgressionService.Profile.TutorialStep);

        Debug.Log($"[Progression] tutorialStep={ProgressionService.Profile.TutorialStep} tutorialActive={ProgressionService.IsTutorialActive} nonTutorialWins={ProgressionService.NonTutorialWins} ruleTier={ProgressionService.CurrentRuleTier} (tutorial)");
        InitializeNewGame(tutorialDef);
    }

    private void StartLevel(int atIndex)
    {
        EnsureUISetup();
        EnsureLevelsLoaded();

        CleanupCurrentGame();
        gameEnded = false;

        if (ProgressionService.IsTutorialActive)
        {
            StartTutorialLevel();
            return;
        }

        if (loadedLevels.Length == 0)
        {
            loadedLevels = new[] { BuildInlineFallbackDefinition() };
        }

        currentLevelIndex = Mathf.Clamp(atIndex, 0, loadedLevels.Length - 1);
        var levelDef = BuildRuntimeVariant(loadedLevels[currentLevelIndex], progressionStep);

        Debug.Log($"[Progression] tutorialStep={ProgressionService.Profile.TutorialStep} tutorialActive={ProgressionService.IsTutorialActive} nonTutorialWins={ProgressionService.NonTutorialWins} ruleTier={ProgressionService.CurrentRuleTier} levelIndex={currentLevelIndex} progressionStep={progressionStep}");
        InitializeNewGame(levelDef);
    }

    private void InitializeNewGame(LevelDefinition levelDefinition)
    {
        if (Achievements == null)
        {
            Achievements = new AchievementsService(ProgressionService.Profile);
            Achievements.OnUnlocked += HandleAchievementUnlocked;
        }

        var ruleTier = ProgressionService.CurrentRuleTier;
        CurrentGame = new GameState(levelDefinition);
        CurrentGame.ApplyRuleTier(ruleTier);

        // Configure pattern rules per-level (Sprint 3: AllowedPatterns).
        bool hasGating = levelDefinition != null && levelDefinition.AllowedPatterns != null && levelDefinition.AllowedPatterns.Count > 0;
        AllowedPatterns = hasGating ? new System.Collections.Generic.List<PatternId>(levelDefinition.AllowedPatterns) : System.Array.Empty<PatternId>();

        // If the level doesn't explicitly gate patterns, hide suited runs until they're unlocked.
        var excludedList = new System.Collections.Generic.List<PatternId>();
        System.Collections.Generic.IEnumerable<PatternId> excluded = null;

        // Always remove suited runs of 5+ (5+ runs stay suit-agnostic).
        excludedList.Add(PatternId.SuitedRun5);

        bool useAdvancedRuns = !ProgressionService.IsTutorialActive && ProgressionService.PostTutorialLevelIndex >= 10;
        if (!useAdvancedRuns)
        {
            excludedList.AddRange(new[]
            {
                PatternId.SuitedRun3,
                PatternId.SuitedRun4,
                PatternId.ColorRun3,
                PatternId.ColorRun4
            });
        }
        else
        {
            excludedList.AddRange(new[]
            {
                PatternId.StraightRun3,
                PatternId.StraightRun4
            });
        }

        if (excludedList.Count > 0)
        {
            excluded = excludedList;
        }

        bool allowJokersInSelection = ruleTier < RuleTier.Mid;

        PatternValidator = hasGating
            ? new PatternValidator(AllowedPatterns, excludedPatterns: excluded, allowJokersInSelection: allowJokersInSelection)
            : new PatternValidator(allowedPatterns: null, excludedPatterns: excluded, allowJokersInSelection: allowJokersInSelection);

        BindGameEvents(CurrentGame);

        CurrentGame.DealInitialHand();

        // Notify listeners that a new game is ready so they can rebind UI
        OnGameStarted?.Invoke(CurrentGame);
        MainMenuScreen.EnsureExists().SetMenuButtonVisible(true);

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

        // Find best pattern, preferring patterns that advance incomplete goals.
        var bestPattern = PatternSelection.GetBestPatternForGoals(PatternValidator, selectedCards, CurrentGame.Goals);
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
            return;
        }

        Achievements?.OnPatternPlayedDetailed(bestPattern.Value.pattern, selectedCards, CurrentGame.Goals);
    }

    public void DrawCard(bool fromDiscard = false)
    {
        if (CurrentGame == null) return;

        // Draw fills the hand up to the maximum size and always costs 1 move total.
        int missing = Mathf.Max(0, GameState.MaxHandSize - CurrentGame.Hand.Count);
        if (missing == 0)
        {
            Debug.Log("Hand is already full");
            return;
        }

        bool success = CurrentGame.DrawCards(missing, fromDiscard);
        
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
        bool success = CurrentGame.DiscardCards(cards);

        if (!success)
        {
            Debug.Log("Cannot discard selected cards");
            return;
        }

        // Auto-refill: draw up to MaxHandSize without extra move cost.
        // Important: don't draw "discardCount" because if the hand was over the limit,
        // discarding 1 and drawing 1 would keep it over the limit and force discard mode forever.
        int maxHandSize = GameState.MaxHandSize;
        int missing = Mathf.Max(0, maxHandSize - CurrentGame.Hand.Count);
        for (int i = 0; i < missing; i++)
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

    private void HandleAchievementUnlocked(AchievementDefinition def)
    {
        if (def == null) return;
        AchievementToast.Show($"Achievement Unlocked: {def.name}", $"+{def.rewardCoins} coins");
    }

    public void ReinitializeProfileBoundServices()
    {
        if (Achievements != null)
        {
            Achievements.OnUnlocked -= HandleAchievementUnlocked;
        }

        Achievements = new AchievementsService(ProgressionService.Profile);
        Achievements.OnUnlocked += HandleAchievementUnlocked;
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

            LastEndWasRunMode = RunModeService.IsActive;
            if (RunModeService.IsActive)
            {
                RunModeService.RecordLevelWin(CurrentGame.Score);
            }

            Achievements?.OnLevelCompleted();
            ProgressionService.AdvanceTutorialStepIfWin(isWin: true);

            // Track basic non-tutorial progression for feature unlocks.
            if (!ProgressionService.IsTutorialActive)
            {
                var profile = ProgressionService.Profile;
                ProgressionService.RecordNonTutorialWin();
                profile.highestLevelCompleted += 1;
                if (!profile.HasFeature(RunModeService.FeatureSuitedRuns) && profile.highestLevelCompleted >= 5)
                {
                    profile.UnlockFeature(RunModeService.FeatureSuitedRuns);
                }
                ProgressionService.Save();
            }

            OnGameEnd?.Invoke(true); // Notify listeners of WIN
            Debug.Log("V OnGameEnd invoked for WIN");
            gameEnded = true;
        }
        else if (CurrentGame.IsLevelFailed)
        {
            LockPlayerInput();
            Debug.Log("<color=red>=== LEVEL FAILED ===</color>");
            Debug.Log("Out of moves!");

            LastEndWasRunMode = RunModeService.IsActive;
            if (RunModeService.IsActive)
            {
                RunModeService.RecordRunEnd();
            }
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
        variant.levelName = $"{baseDefinition.levelName} - Level {difficultyStep + 1}";
        variant.allowedPatterns = baseDefinition.allowedPatterns != null
            ? new System.Collections.Generic.List<PatternId>(baseDefinition.allowedPatterns)
            : new System.Collections.Generic.List<PatternId>();

        int moveVariance = UnityEngine.Random.Range(-1, 2) + Mathf.Min(difficultyStep, 2);
        variant.totalMoves = Mathf.Max(6, baseDefinition.totalMoves + moveVariance + difficultyStep);

        variant.goals = BuildVariantGoals(baseDefinition, difficultyStep, variant.totalMoves);

        variant.deckTweaks = CloneDeckTweaks(baseDefinition.deckTweaks, difficultyStep);
        return variant;
    }

    private List<GoalDefinition> BuildVariantGoals(LevelDefinition baseDefinition, int difficultyStep, int totalMoves)
    {
        var goals = new List<GoalDefinition>();
        bool advancedRuns = !ProgressionService.IsTutorialActive && ProgressionService.PostTutorialLevelIndex >= 10;

        var baseGoals = baseDefinition.goals != null && baseDefinition.goals.Count > 0
            ? baseDefinition.goals
            : BuildInlineFallbackDefinition().goals;

        foreach (var goal in baseGoals)
        {
            var resolvedType = ResolveGoalTypeForRunRules(goal.goalType, advancedRuns);
            // Prefer variety/constraints over steadily inflating counts.
            int bonus = UnityEngine.Random.Range(0, 1 + Mathf.Min(difficultyStep, 1));
            var required = Mathf.Max(1, goal.required + bonus);
            required = CapToMoves(required, totalMoves);

            goals.Add(new GoalDefinition
            {
                goalType = resolvedType,
                required = required
            });
        }

        // Add one extra goal occasionally (variety), but keep the goal list small.
        if (difficultyStep >= 1 && goals.Count < 3)
        {
            var existing = new HashSet<Goal.GoalType>(goals.Select(g => g.goalType));
            var extraType = GetRandomGoalType(existing, advancedRuns);
            if (extraType.HasValue)
            {
                var resolvedExtra = ResolveGoalTypeForRunRules(extraType.Value, advancedRuns);
                int required = Mathf.Max(1, 1 + UnityEngine.Random.Range(0, 1));
            required = CapToMoves(required, totalMoves);
                goals.Add(new GoalDefinition { goalType = resolvedExtra, required = required });
            }
        }

        return goals;
    }

    private Goal.GoalType? GetRandomGoalType(HashSet<Goal.GoalType> exclude, bool advancedRuns)
    {
        // Rotate through goal types except TotalScore to keep variety.
        var candidates = new List<Goal.GoalType>
        {
            Goal.GoalType.Pair,
            Goal.GoalType.ThreeOfKind,
            Goal.GoalType.Run3,
            Goal.GoalType.Run4,
            Goal.GoalType.Run5,
            Goal.GoalType.SuitSet3Plus,
            Goal.GoalType.ColorSet3Plus,
            Goal.GoalType.Flush,
            Goal.GoalType.FullHouse
        };

        if (advancedRuns)
        {
            candidates.Add(Goal.GoalType.SuitedRun3);
            candidates.Add(Goal.GoalType.SuitedRun4);
            candidates.Add(Goal.GoalType.ColorRun3);
            candidates.Add(Goal.GoalType.ColorRun4);
        }

        if (exclude != null && exclude.Count > 0)
        {
            candidates = candidates.Where(c => !exclude.Contains(c)).ToList();
        }

        if (candidates.Count == 0) return null;

        int index = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[index];
    }

    private Goal.GoalType ResolveGoalTypeForRunRules(Goal.GoalType type, bool advancedRuns)
    {
        if (!advancedRuns)
        {
            return type switch
            {
                Goal.GoalType.Run3 => Goal.GoalType.StraightRun3,
                Goal.GoalType.Run4 => Goal.GoalType.StraightRun4,
                Goal.GoalType.Run5 => Goal.GoalType.StraightRun5,
                Goal.GoalType.SuitedRun3 => Goal.GoalType.StraightRun3,
                Goal.GoalType.SuitedRun4 => Goal.GoalType.StraightRun4,
                Goal.GoalType.SuitedRun5 => Goal.GoalType.StraightRun5,
                Goal.GoalType.ColorRun3 => Goal.GoalType.StraightRun3,
                Goal.GoalType.ColorRun4 => Goal.GoalType.StraightRun4,
                _ => type
            };
        }

        return type switch
        {
            Goal.GoalType.Run3 => PickRunVariant(Goal.GoalType.SuitedRun3, Goal.GoalType.ColorRun3),
            Goal.GoalType.StraightRun3 => PickRunVariant(Goal.GoalType.SuitedRun3, Goal.GoalType.ColorRun3),
            Goal.GoalType.Run4 => PickRunVariant(Goal.GoalType.SuitedRun4, Goal.GoalType.ColorRun4),
            Goal.GoalType.StraightRun4 => PickRunVariant(Goal.GoalType.SuitedRun4, Goal.GoalType.ColorRun4),
            Goal.GoalType.Run5 => Goal.GoalType.StraightRun5,
            Goal.GoalType.StraightRun5 => Goal.GoalType.StraightRun5,
            Goal.GoalType.SuitedRun5 => Goal.GoalType.StraightRun5,
            _ => type
        };
    }

    private static Goal.GoalType PickRunVariant(Goal.GoalType suited, Goal.GoalType color)
    {
        return UnityEngine.Random.value < 0.5f ? suited : color;
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

        if (Achievements != null)
        {
            game.OnPatternPlayed += Achievements.OnPatternPlayed;
        }
    }

    private void UnbindGameEvents(GameState game)
    {
        if (game == null) return;

        game.OnPatternPlayed -= HandlePatternPlayed;
        game.OnGoalUpdated -= HandleGoalUpdated;
        game.OnScoreChanged -= HandleScoreChanged;
        game.OnMovesChanged -= HandleMovesChanged;

        if (Achievements != null)
        {
            game.OnPatternPlayed -= Achievements.OnPatternPlayed;
        }
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
