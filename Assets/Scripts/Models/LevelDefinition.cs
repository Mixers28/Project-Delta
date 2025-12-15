using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Levels/Level Definition", order = 0)]
public class LevelDefinition : ScriptableObject
{
    [Header("Level Details")]
    public string levelName = "Test Level";
    public int totalMoves = 15;

    [Header("Goals")]
    public List<GoalDefinition> goals = new();

    [Header("Rules")]
    [Tooltip("If empty, all patterns are allowed. Otherwise only these PatternIds can be played.")]
    public List<PatternId> allowedPatterns = new();

    [Header("Deck Tweaks")]
    public DeckTweakSettings deckTweaks = new();

    public List<Goal> BuildGoals()
    {
        var builtGoals = new List<Goal>();

        if (goals == null || goals.Count == 0)
        {
            builtGoals.Add(new Goal(Goal.GoalType.Pair, 2));
            builtGoals.Add(new Goal(Goal.GoalType.Run3, 1));
            return builtGoals;
        }

        foreach (var definition in goals)
        {
            builtGoals.Add(new Goal(definition.goalType, Mathf.Max(1, definition.required)));
        }

        return builtGoals;
    }

    public IReadOnlyList<PatternId> AllowedPatterns => allowedPatterns;
}

[Serializable]
public class GoalDefinition
{
    public Goal.GoalType goalType = Goal.GoalType.Pair;
    public int required = 1;
}

[Serializable]
public class DeckTweakSettings
{
    [Header("Shuffle")]
    [Tooltip("Add extra jokers on top of the default two")]
    public int extraJokers = 0;

    public bool shuffleAfterTweaks = true;

    [Tooltip("Use a deterministic shuffle seed (helps tutorial reliability).")]
    public bool useDeterministicShuffle = false;

    [Tooltip("Seed used when deterministic shuffle is enabled.")]
    public int shuffleSeed = 0;

    [Tooltip("Optional extra cards to add before shuffling")]
    public List<ExtraCardDefinition> additionalCards = new();

    [Header("Preset Pile")]
    [Tooltip("If provided, replaces the standard deck's draw pile before applying other tweaks (optional).")]
    public List<Card> presetDrawPile = new();

    public string Describe()
    {
        var parts = new List<string>();

        if (extraJokers > 0)
        {
            parts.Add($"+{extraJokers} jokers");
        }

        if (additionalCards != null)
        {
            foreach (var card in additionalCards)
            {
                if (card.count <= 0) continue;
                parts.Add($"+{card.count}x {card.rank} of {card.suit}");
            }
        }

        if (parts.Count == 0)
        {
            parts.Add("Standard deck (52 + 2 jokers)");
        }

        if (!shuffleAfterTweaks)
        {
            parts.Add("No shuffle after tweaks");
        }

        if (useDeterministicShuffle)
        {
            parts.Add($"Seeded shuffle ({shuffleSeed})");
        }

        if (presetDrawPile != null && presetDrawPile.Count > 0)
        {
            parts.Add($"Preset draw pile ({presetDrawPile.Count})");
        }

        return string.Join(", ", parts);
    }
}

[Serializable]
public class ExtraCardDefinition
{
    public Card.Suit suit = Card.Suit.Hearts;
    public Card.Rank rank = Card.Rank.Ace;
    public int count = 1;
}
