using System.Collections.Generic;
using System.Linq;

public class FlushPattern : IPattern
{
    public string Name => "Flush";
    public int BasePoints => 100;

    public bool Validate(List<Card> cards)
    {
        // Must be exactly 5 cards
        if (cards.Count != 5) return false;

        int jokerCount = cards.Count(c => c.IsJoker);
        var nonJokers = cards.Where(c => !c.IsJoker).ToList();

        // Need at least one non-joker to determine suit
        if (nonJokers.Count == 0) return false;

        // All non-jokers must be same suit
        Card.Suit suit = nonJokers[0].suit;
        return nonJokers.All(c => c.suit == suit);
    }

    public int CalculateScore(List<Card> cards)
    {
        return this.CalculateScoreWithMultipliers(cards);
    }
}