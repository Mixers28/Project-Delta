using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentGame { get; private set; }
    public PatternValidator PatternValidator { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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

    public void DiscardCard(int handIndex)
    {
        if (CurrentGame == null) return;
        if (handIndex < 0 || handIndex >= CurrentGame.Hand.Count) return;

        Card card = CurrentGame.Hand[handIndex];
        bool success = CurrentGame.DiscardCard(card);

        if (!success)
        {
            Debug.Log("Cannot discard card");
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

    private void CheckLevelComplete()
    {
        if (CurrentGame.IsLevelComplete)
        {
            Debug.Log("<color=cyan>=== LEVEL COMPLETE! ===</color>");
            Debug.Log($"Final Score: {CurrentGame.Score}");
        }
        else if (CurrentGame.IsLevelFailed)
        {
            Debug.Log("<color=red>=== LEVEL FAILED ===</color>");
            Debug.Log("Out of moves!");
        }
    }
}