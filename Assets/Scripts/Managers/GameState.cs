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
    public int TotalMoves { get; private set; }
    public List<Goal> Goals { get; private set; }
    public string LevelName { get; private set; }
    public string DeckDescription { get; private set; }
    public RuleTier CurrentRuleTier { get; private set; } = RuleTier.Early;
    
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
    public event Action OnMovesChanged;

    public bool IsLevelComplete => Goals.All(g => g.IsComplete);
    public bool IsLevelFailed => MovesRemaining <= 0 && !IsLevelComplete;

    public GameState(List<Goal> goals, int totalMoves)
    {
        Hand = new List<Card>();
        Deck = new Deck();
        Goals = goals;
        TotalMoves = totalMoves;
        MovesRemaining = totalMoves;
        Score = 0;
        LevelName = "Test Level";
        DeckDescription = "Standard deck (52 + 2 jokers)";
    }

    public GameState(LevelDefinition levelDefinition)
    {
        Hand = new List<Card>();
        Deck = new Deck();
        Goals = levelDefinition.BuildGoals();
        TotalMoves = levelDefinition.totalMoves;
        MovesRemaining = levelDefinition.totalMoves;
        Score = 0;
        LevelName = string.IsNullOrWhiteSpace(levelDefinition.levelName) ? "Test Level" : levelDefinition.levelName;
        DeckDescription = levelDefinition.deckTweaks != null ? levelDefinition.deckTweaks.Describe() : "Standard deck (52 + 2 jokers)";

        Deck.ApplyTweaks(levelDefinition.deckTweaks);
    }

    public static GameState FromSession(SavedGameSession session)
    {
        if (session == null) return null;

        var goals = new List<Goal>();
        if (session.goals != null)
        {
            foreach (var snapshot in session.goals)
            {
                var goal = new Goal(snapshot.type, snapshot.required);
                goal.current = snapshot.current;
                goals.Add(goal);
            }
        }

        var state = new GameState(goals, Mathf.Max(1, session.totalMoves));
        state.MovesRemaining = Mathf.Max(0, session.movesRemaining);
        state.Score = Mathf.Max(0, session.score);
        state.LevelName = string.IsNullOrWhiteSpace(session.levelName) ? "Level" : session.levelName;
        state.DeckDescription = session.deckDescription ?? string.Empty;
        state.CurrentRuleTier = session.ruleTier;

        state.Hand = session.hand != null ? new List<Card>(session.hand) : new List<Card>();
        state.Deck.SetState(session.drawPile, session.discardPile);
        return state;
    }

    public void ApplyRuleTier(RuleTier tier)
    {
        CurrentRuleTier = tier;

        if (tier >= RuleTier.Mid)
        {
            Deck.RemoveJokers();

            if (!string.IsNullOrWhiteSpace(DeckDescription))
            {
                DeckDescription = $"{DeckDescription} (no jokers)";
            }
            else
            {
                DeckDescription = "Standard deck (no jokers)";
            }
        }
    }

    public void DealInitialHand()
    {
        for (int i = 0; i < MaxHandSize; i++)
        {
            DrawFromStock(consumeMove: false);
        }
    }

    public bool DrawFromStock(bool consumeMove = true)
    {
        if (MustDiscard) return false; // Can't draw if you need to discard first
        if (consumeMove && MovesRemaining <= 0) return false;
        
        var card = Deck.DrawFromStock();
        if (!card.HasValue) return false;

        Hand.Add(card.Value);
        OnCardDrawn?.Invoke(card.Value);
        OnHandChanged?.Invoke();
        return consumeMove ? SpendMoves(1) : true;
    }

    public bool DrawFromDiscard(bool consumeMove = true)
    {
        if (MustDiscard) return false; // Can't draw if you need to discard first
        if (consumeMove && MovesRemaining <= 0) return false;
        
        var card = Deck.DrawFromDiscard();
        if (!card.HasValue) return false;

        Hand.Add(card.Value);
        OnCardDrawn?.Invoke(card.Value);
        OnHandChanged?.Invoke();
        return consumeMove ? SpendMoves(1) : true;
    }

    // Draw multiple cards in a single action for one move cost
    public bool DrawCards(int count, bool fromDiscard = false)
    {
        if (count <= 0) return false;
        if (MovesRemaining <= 0) return false;
        if (MustDiscard) return false;

        int drawn = 0;
        for (int i = 0; i < count; i++)
        {
            if (MustDiscard) break;
            bool ok = fromDiscard
                ? DrawFromDiscard(consumeMove: false)
                : DrawFromStock(consumeMove: false);
            if (!ok) break;
            drawn++;
        }

        if (drawn == 0) return false;
        return SpendMoves(1);
    }

    public bool DiscardCard(Card card)
    {
        if (!Hand.Contains(card)) return false;
        return DiscardCards(new List<Card> { card });
    }

    public bool DiscardCards(List<Card> cards)
    {
        if (cards == null || cards.Count == 0) return false;

        if (!cards.All(c => Hand.Contains(c))) return false;

        // Discard always costs exactly 1 move regardless of how many cards are discarded.
        if (!SpendMoves(1))
        {
            Debug.LogWarning("Not enough moves to discard.");
            return false;
        }

        foreach (var card in cards)
        {
            Hand.Remove(card);
            Deck.AddToDiscard(card);
            OnCardDiscarded?.Invoke(card);
        }

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

        // Playing a pattern no longer costs a move
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
        return Hand.Count >= 1 && MovesRemaining > 0;
    }

    public bool CanDraw()
    {
        return MovesRemaining > 0 && !MustDiscard && (Deck.DrawPileCount > 0 || Deck.DiscardPileCount > 0);
    }

    private bool SpendMoves(int amount)
    {
        if (amount <= 0) return true;
        if (MovesRemaining < amount) return false;

        MovesRemaining -= amount;
        OnMovesChanged?.Invoke();
        return true;
    }
}
