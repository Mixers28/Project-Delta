using System.Collections.Generic;
using System.Linq;

public class Deck
{
    private List<Card> drawPile = new List<Card>();
    private List<Card> discardPile = new List<Card>();

    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public Card? TopDiscard => discardPile.Count > 0 ? discardPile[^1] : null;

    public Deck()
    {
        Reset();
    }

    public void Reset()
    {
        drawPile.Clear();
        discardPile.Clear();

        // Create standard 52 cards
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            if (suit == Card.Suit.Joker) continue;

            foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
            {
                if (rank == Card.Rank.Joker) continue;
                drawPile.Add(new Card(suit, rank));
            }
        }

        // Add 2 jokers
        drawPile.Add(new Card(Card.Suit.Joker, Card.Rank.Joker));
        drawPile.Add(new Card(Card.Suit.Joker, Card.Rank.Joker));

        Shuffle();
    }

    public void ApplyTweaks(DeckTweakSettings tweaks)
    {
        if (tweaks == null)
        {
            return;
        }

        if (tweaks.extraJokers > 0)
        {
            for (int i = 0; i < tweaks.extraJokers; i++)
            {
                drawPile.Add(new Card(Card.Suit.Joker, Card.Rank.Joker));
            }
        }

        if (tweaks.additionalCards != null)
        {
            foreach (var extra in tweaks.additionalCards)
            {
                if (extra.count <= 0) continue;

                for (int i = 0; i < extra.count; i++)
                {
                    drawPile.Add(new Card(extra.suit, extra.rank));
                }
            }
        }

        if (tweaks.shuffleAfterTweaks)
        {
            Shuffle();
        }
    }

    public void Shuffle()
    {
        // Fisher-Yates shuffle
        System.Random rng = new System.Random();
        int n = drawPile.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (drawPile[k], drawPile[n]) = (drawPile[n], drawPile[k]);
        }
    }

    public Card? DrawFromStock()
    {
        if (drawPile.Count == 0) return null;
        
        Card card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }

    public Card? DrawFromDiscard()
    {
        if (discardPile.Count == 0) return null;
        
        // Pull a random card from the discard pile (not just the top)
        System.Random rng = new System.Random();
        int index = rng.Next(discardPile.Count);
        Card card = discardPile[index];
        discardPile.RemoveAt(index);
        return card;
    }

    public void AddToDiscard(Card card)
    {
        discardPile.Add(card);
    }
}
