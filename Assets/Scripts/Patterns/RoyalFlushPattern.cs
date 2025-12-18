using System.Collections.Generic;
using System.Linq;

public class RoyalFlushPattern : IPattern
{
    public PatternId Id => PatternId.RoyalFlush5;
    public string Name => "Royal Flush";
    public int BasePoints => 250;

    public bool Validate(List<Card> cards)
    {
        if (cards == null || cards.Count != 5) return false;
        if (cards.Any(c => c.IsJoker)) return false;

        var suit = cards[0].suit;
        if (!cards.All(c => c.suit == suit)) return false;

        var ranks = cards.Select(c => c.rank).ToHashSet();
        if (ranks.Count != 5) return false;

        return ranks.SetEquals(new[]
        {
            Card.Rank.Ten,
            Card.Rank.Jack,
            Card.Rank.Queen,
            Card.Rank.King,
            Card.Rank.Ace
        });
    }

    public int CalculateScore(List<Card> cards)
    {
        return this.CalculateScoreWithMultipliers(cards);
    }
}
