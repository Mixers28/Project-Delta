using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState
{
    // Core game data
    public List<Card> Hand { get; private set; }
    public Deck Deck { get; private set; }
    public int Score { get; private set; }
    public int MovesRemaining { get; private set; }
    public List<Goal> Goals { get; private set; }
    
    // Constants
    public const int MaxHandSize = 7;
    
    // NEW: Check if player must discard before drawing
    public bool MustDiscard => Hand.Count > MaxHandSize;
    
    // Events (for UI updates)
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardDiscarded;
    public event Action<IPattern, int> OnPatternPlayed;
    public event Action<int> OnScoreChanged;
    public event Action<Goal> OnGoalUpdated;
    public event Action OnHandChanged;

    public bool IsLevelComplete => Goals.All(g => g.IsComplete);
    public bool IsLevelFailed => MovesRemaining <= 0 && !IsLevelComplete;

    public GameState(List<Goal> goals, int totalMoves)
    {
        Hand = new List<Card>();
        Deck = new Deck();
        Goals = goals;
        MovesRemaining = totalMoves;
        Score = 0;
    }

    public void DealInitialHand()
    {
        for (int i = 0; i < MaxHandSize; i++)
        {
            DrawFromStock();
        }
    }

    public bool DrawFromStock()
    {
        if (MustDiscard) return false; // Can't draw if you need to discard first
        
        var card = Deck.DrawFromStock();
        if (!card.HasValue) return false;

        Hand.Add(card.Value);
        OnCardDrawn?.Invoke(card.Value);
        OnHandChanged?.Invoke();
        return true;
    }

    public bool DrawFromDiscard()
    {
        if (MustDiscard) return false; // Can't draw if you need to discard first
        
        var card = Deck.DrawFromDiscard();
        if (!card.HasValue) return false;

        Hand.Add(card.Value);
        OnCardDrawn?.Invoke(card.Value);
        OnHandChanged?.Invoke();
        return true;
    }

    public bool DiscardCard(Card card)
    {
        if (!Hand.Contains(card)) return false;
        
        Hand.Remove(card);
        Deck.AddToDiscard(card);
        
        // Only decrement moves if we're completing a turn (discarding after drawing)
        if (Hand.Count == MaxHandSize)
        {
            MovesRemaining--;
        }
        
        OnCardDiscarded?.Invoke(card);
        OnHandChanged?.Invoke();
        return true;
    }

    public bool PlayPattern(List<Card> cards, IPattern pattern)
    {
        // Validate pattern
        if (!pattern.Validate(cards)) return false;

        // Validate all cards are in hand
        if (!cards.All(c => Hand.Contains(c))) return false;

        // Remove cards from hand
        foreach (var card in cards)
        {
            Hand.Remove(card);
        }

        // Calculate score
        int points = pattern.CalculateScore(cards);
        Score += points;

        // Update goals
        UpdateGoals(pattern, points);

        // Trigger events
        OnPatternPlayed?.Invoke(pattern, points);
        OnScoreChanged?.Invoke(Score);
        OnHandChanged?.Invoke();

        return true;
    }

    private void UpdateGoals(IPattern pattern, int points)
    {
        foreach (var goal in Goals)
        {
            if (goal.IsComplete) continue;

            // Check pattern goals
            if (goal.MatchesPattern(pattern))
            {
                goal.Increment();
                OnGoalUpdated?.Invoke(goal);
            }

            // Check score goals
            if (goal.type == Goal.GoalType.TotalScore)
            {
                goal.current = Score;
                OnGoalUpdated?.Invoke(goal);
            }
        }
    }

    public List<Card> GetSelectedCards(List<int> indices)
    {
        return indices.Where(i => i >= 0 && i < Hand.Count)
                      .Select(i => Hand[i])
                      .ToList();
    }

    public bool CanDiscard()
    {
        return Hand.Count >= 1;
    }

    public bool CanDraw()
    {
        return MovesRemaining > 0 && !MustDiscard && (Deck.DrawPileCount > 0 || Deck.DiscardPileCount > 0);
    }
}