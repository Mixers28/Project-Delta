using System.Collections.Generic;
using System.Linq;

public static class PatternSelection
{
    public static (IPattern pattern, int score)? GetBestPatternForGoals(
        PatternValidator validator,
        List<Card> cards,
        List<Goal> goals)
    {
        if (validator == null || cards == null || cards.Count == 0) return null;

        var validPatterns = validator.DetectPatterns(cards);
        if (validPatterns.Count == 0) return null;

        var scored = validPatterns
            .Select(p => (pattern: p, score: p.CalculateScore(cards)))
            .ToList();

        // Prefer patterns that satisfy any incomplete goal (so multi-matching hands advance the intended goal).
        if (goals != null && goals.Count > 0)
        {
            var goalRelevant = scored
                .Where(s => goals.Any(g => g != null && !g.IsComplete && g.MatchesPattern(s.pattern)))
                .ToList();

            if (goalRelevant.Count > 0)
            {
                return goalRelevant.OrderByDescending(s => s.score).First();
            }
        }

        // Fallback: highest score wins.
        return scored.OrderByDescending(s => s.score).First();
    }
}

