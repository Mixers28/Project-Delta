using System.Collections.Generic;
using System.Linq;

public class FullHousePattern : IPattern
{
    public string Name => "Full House";
    public int BasePoints => 200;

    public bool Validate(List<Card> cards)
    {
        // Must be exactly 5 cards
        if (cards.Count != 5) return false;

        int jokerCount = cards.Count(c => c.IsJoker);
        var nonJokers = cards.Where(c => !c.IsJoker).ToList();

        // Group by rank
        var rankGroups = nonJokers.GroupBy(c => c.rank)
                                   .OrderByDescending(g => g.Count())
                                   .ToList();

        // Full House = 3 of a kind + pair (can use jokers to complete)
        return CanFormFullHouse(rankGroups, jokerCount);
    }

    private bool CanFormFullHouse(List<IGrouping<Card.Rank, Card>> rankGroups, int jokersAvailable)
    {
        if (rankGroups.Count == 0)
        {
            // All jokers - valid if we have 5
            return jokersAvailable == 5;
        }

        if (rankGroups.Count == 1)
        {
            // All same rank + jokers
            int mainGroupSize = rankGroups[0].Count();
            int total = mainGroupSize + jokersAvailable;
            
            // Need exactly 5 cards, can form 3+2 if we have 5 total
            return total == 5 && mainGroupSize >= 2;
        }

        if (rankGroups.Count == 2)
        {
            // Two different ranks
            int firstSize = rankGroups[0].Count();
            int secondSize = rankGroups[1].Count();

            // Perfect full house (3+2)
            if ((firstSize == 3 && secondSize == 2) || (firstSize == 2 && secondSize == 3))
            {
                return jokersAvailable == 0;
            }

            // Use jokers to complete
            int totalCards = firstSize + secondSize + jokersAvailable;
            if (totalCards != 5) return false;

            // Can we form 3 of one rank and 2 of another?
            // Need at least 2 of one kind and 1 of another (jokers fill rest)
            return (firstSize >= 2 && secondSize >= 1) || (firstSize >= 1 && secondSize >= 2);
        }

        // 3+ different ranks - can't form full house
        return false;
    }

    public int CalculateScore(List<Card> cards)
    {
        return this.CalculateScoreWithMultipliers(cards);
    }
}