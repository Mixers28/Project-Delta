using System.Collections.Generic;
using System.Linq;

public class FourOfKindPattern : IPattern
{
    public PatternId Id => PatternId.FourOfKind;
    public string Name => "Four of a Kind";
    public int BasePoints => 120;

    public bool Validate(List<Card> cards)
    {
        if (cards == null || cards.Count != 4) return false;

        int jokerCount = cards.Count(c => c.IsJoker);
        var nonJokers = cards.Where(c => !c.IsJoker).ToList();

        // All jokers is allowed (even if not normally possible with the default deck).
        if (nonJokers.Count == 0) return jokerCount == 4;

        // If there are multiple non-joker ranks, they can't form a four-of-kind.
        if (nonJokers.Select(c => c.rank).Distinct().Count() != 1) return false;

        // With wild jokers, any number of jokers can fill the missing copies.
        return true;
    }

    public int CalculateScore(List<Card> cards)
    {
        return this.CalculateScoreWithMultipliers(cards);
    }
}
