using System;

[Serializable]
public struct Card
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades,
        Joker
    }

    public enum Rank
    {
        Joker = 0,
        Ace = 1,
        Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King
    }

    public Suit suit;
    public Rank rank;

    public Card(Suit s, Rank r)
    {
        suit = s;
        rank = r;
    }

    public bool IsJoker => rank == Rank.Joker;
    public bool IsFaceCard => rank >= Rank.Jack && rank <= Rank.King;

    public string Symbol
    {
        get
        {
            // Use ASCII-friendly markers to keep text fallbacks readable
            return suit switch
            {
                Suit.Hearts => "H",
                Suit.Diamonds => "D",
                Suit.Clubs => "C",
                Suit.Spades => "S",
                Suit.Joker => "?",
                _ => string.Empty
            };
        }
    }

    public string Display
    {
        get
        {
            if (IsJoker) return "?JKR";
            
            string rankStr = rank switch
            {
                Rank.Ace => "A",
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                _ => ((int)rank).ToString()
            };
            
            return $"{rankStr}{Symbol}";
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is Card other)
            return suit == other.suit && rank == other.rank;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(suit, rank);
    }
}
