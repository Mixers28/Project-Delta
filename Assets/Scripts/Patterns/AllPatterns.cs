using System.Collections.Generic;
using System.Linq;

// Interface
public interface IPattern
{
    string Name { get; }
    int BasePoints { get; }
    bool Validate(List<Card> cards);
    int CalculateScore(List<Card> cards);
}

// Extension methods
public static class PatternExtensions
{
    public static int CalculateScoreWithMultipliers(this IPattern pattern, List<Card> cards)
    {
        if (!pattern.Validate(cards)) return 0;
        int score = pattern.BasePoints;

        if (cards.Any(c => c.IsJoker))
            score = (int)(score * 1.5);

        if (cards.Where(c => !c.IsJoker).All(c => c.IsFaceCard))
            score *= 2;

        var nonJokers = cards.Where(c => !c.IsJoker).ToList();
        var uniqueSuits = nonJokers.Select(c => c.suit).Distinct().Count();
        if (uniqueSuits == nonJokers.Count && nonJokers.Count > 1)
            score = (int)(score * 1.2);

        return score;
    }
}

// Pair Pattern
public class PairPattern : IPattern
{
    public string Name => "Pair";
    public int BasePoints => 10;

    public bool Validate(List<Card> cards)
    {
        if (cards.Count != 2) return false;
        if (cards.All(c => c.IsJoker)) return true;
        if (cards.Count(c => c.IsJoker) == 1) return true;
        return cards[0].rank == cards[1].rank;
    }

    public int CalculateScore(List<Card> cards)
    {
        return this.CalculateScoreWithMultipliers(cards);
    }
}

// Three of a Kind Pattern
public class ThreeOfKindPattern : IPattern
{
    public string Name => "Three of a Kind";
    public int BasePoints => 30;

    public bool Validate(List<Card> cards)
    {
        if (cards.Count != 3) return false;
        int jokerCount = cards.Count(c => c.IsJoker);
        var nonJokers = cards.Where(c => !c.IsJoker).ToList();

        if (jokerCount == 3) return true;
        if (jokerCount == 2) return true;
        if (jokerCount == 1)
            return nonJokers.Count == 2 && nonJokers[0].rank == nonJokers[1].rank;

        return nonJokers.All(c => c.rank == nonJokers[0].rank);
    }

    public int CalculateScore(List<Card> cards)
    {
        return this.CalculateScoreWithMultipliers(cards);
    }
}

// Run Pattern
public class RunPattern : IPattern
{
    private int minimumLength;

    public string Name => $"Run of {minimumLength}";
    public int BasePoints => minimumLength switch
    {
        3 => 40,
        4 => 80,
        >= 5 => 150,
        _ => 0
    };

    public RunPattern(int length = 3)
    {
        minimumLength = length;
    }

    public bool Validate(List<Card> cards)
    {
        if (cards.Count < minimumLength) return false;
        int jokerCount = cards.Count(c => c.IsJoker);
        var nonJokers = cards.Where(c => !c.IsJoker).ToList();

        if (nonJokers.Count == 0) return false;

        Card.Suit suit = nonJokers[0].suit;
        if (!nonJokers.All(c => c.suit == suit)) return false;

        nonJokers = nonJokers.OrderBy(c => c.rank).ToList();
        return CanFormRun(nonJokers, jokerCount);
    }

    private bool CanFormRun(List<Card> nonJokers, int jokersAvailable)
    {
        if (nonJokers.Count == 0)
            return jokersAvailable >= minimumLength;

        int jokersUsed = 0;
        int expectedRank = (int)nonJokers[0].rank;

        foreach (var card in nonJokers)
        {
            int gap = (int)card.rank - expectedRank;
            if (gap < 0) return false;
            jokersUsed += gap;
            if (jokersUsed > jokersAvailable) return false;
            expectedRank = (int)card.rank + 1;
        }

        int totalCards = nonJokers.Count + jokersAvailable;
        return totalCards >= minimumLength;
    }

    public int CalculateScore(List<Card> cards)
    {
        return this.CalculateScoreWithMultipliers(cards);
    }
}

public class NewBaseType
{
    private List<IPattern> patterns;
}

// Pattern Validator
public class PatternValidator
{
    private List<IPattern> patterns;

    public PatternValidator()
    {
        patterns = new List<IPattern>
        {
            new PairPattern(),
            new ThreeOfKindPattern(),
            new RunPattern(3),
            new RunPattern(4),
            new RunPattern(5),
            new FlushPattern(),           // NEW
            new FullHousePattern()        // NEW
        };
    }

    public List<IPattern> DetectPatterns(List<Card> cards)
    {
        return patterns.Where(p => p.Validate(cards)).ToList();
    }

    public (IPattern pattern, int score)? GetBestPattern(List<Card> cards)
    {
        var validPatterns = DetectPatterns(cards);
        
        if (validPatterns.Count == 0) return null;

        var scored = validPatterns
            .Select(p => (pattern: p, score: p.CalculateScore(cards)))
            .OrderByDescending(x => x.score)
            .FirstOrDefault();

        return scored;
    }
}




    
