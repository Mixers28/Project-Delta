using System.Collections.Generic;
using System.Linq;

public class ColorSetPattern : IPattern
{
    private readonly int minimumLength;

    public PatternId Id => PatternId.ColorSet3Plus;
    public string Name => "Color Set";
    public int BasePoints => 30;

    public ColorSetPattern(int minLength = 3)
    {
        minimumLength = minLength < 2 ? 2 : minLength;
    }

    public bool Validate(List<Card> cards)
    {
        if (cards == null) return false;
        if (cards.Count < minimumLength) return false;

        var nonJokers = cards.Where(c => !c.IsJoker).ToList();
        if (nonJokers.Count == 0) return false;

        bool isRed = IsRed(nonJokers[0].suit);
        return nonJokers.All(c => IsRed(c.suit) == isRed);
    }

    public int CalculateScore(List<Card> cards)
    {
        if (!Validate(cards)) return 0;
        int extra = cards.Count - minimumLength;
        int basePoints = BasePoints + (extra > 0 ? extra * 10 : 0);
        return PatternExtensions.ApplyMultipliers(basePoints, cards);
    }

    private static bool IsRed(Card.Suit suit)
    {
        return suit == Card.Suit.Hearts || suit == Card.Suit.Diamonds;
    }
}

