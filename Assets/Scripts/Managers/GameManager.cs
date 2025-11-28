using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentGame { get; private set; }
    public PatternValidator PatternValidator { get; private set; }
    
    // Events for UI listeners
    public delegate void GameEndHandler(bool isWin);
    public event GameEndHandler OnGameEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // GameOverPanel is expected to be in the scene/prefab, no runtime construction here.
    }

    private void Start()
    {
        PatternValidator = new PatternValidator();
        StartTestLevel();
    }

    // Test level configuration
    public void StartTestLevel()
    {
        var goals = new List<Goal>
        {
            new Goal(Goal.GoalType.Pair, 2),
            new Goal(Goal.GoalType.Run3, 1)
        };

        CurrentGame = new GameState(goals, totalMoves: 15);
        
        // Subscribe to events
        CurrentGame.OnPatternPlayed += HandlePatternPlayed;
        CurrentGame.OnGoalUpdated += HandleGoalUpdated;
        CurrentGame.OnScoreChanged += HandleScoreChanged;
        CurrentGame.OnMovesChanged += HandleMovesChanged;

        // Deal initial hand
        CurrentGame.DealInitialHand();

        Debug.Log("=== TEST LEVEL STARTED ===");
        Debug.Log($"Goals: {string.Join(", ", goals.ConvertAll(g => g.DisplayText))}");
        Debug.Log($"Moves: {CurrentGame.MovesRemaining}");
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
        
        Debug.Log($"CheckLevelComplete: IsLevelComplete={CurrentGame.IsLevelComplete}, IsLevelFailed={CurrentGame.IsLevelFailed}");
        
        if (CurrentGame.IsLevelComplete)
        {
            LockPlayerInput();
            Debug.Log("<color=cyan>=== LEVEL COMPLETE! ===</color>");
            Debug.Log($"Final Score: {CurrentGame.Score}");
            OnGameEnd?.Invoke(true); // Notify listeners of WIN
            Debug.Log("V OnGameEnd invoked for WIN");
        }
        else if (CurrentGame.IsLevelFailed)
        {
            LockPlayerInput();
            Debug.Log("<color=red>=== LEVEL FAILED ===</color>");
            Debug.Log("Out of moves!");
            OnGameEnd?.Invoke(false); // Notify listeners of LOSS
            Debug.Log("V OnGameEnd invoked for LOSS");
        }
        else
        {
            Debug.Log("CheckLevelComplete: Level is still in progress");
        }
    }
}
