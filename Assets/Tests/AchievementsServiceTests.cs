using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class AchievementsServiceTests
{
    [Test]
    public void UnlockingFirstPairAddsCoins()
    {
        var profile = new PlayerProfile();
        profile.coins = 0;

        var achievements = new AchievementsService(profile);
        achievements.OnPatternPlayed(new PairPattern());

        Assert.GreaterOrEqual(profile.coins, 25);
    }

    [Test]
    public void RecentlyUnlockedIsClearedAfterTake()
    {
        var profile = new PlayerProfile();
        ProgressionService.SetProfileForTests(profile);
        var achievements = new AchievementsService(profile);
        achievements.OnLevelCompleted();

        var first = achievements.TakeRecentlyUnlocked();
        var second = achievements.TakeRecentlyUnlocked();

        Assert.IsTrue(first.Count > 0);
        Assert.AreEqual(0, second.Count);
    }

    [Test]
    public void OverachieverUnlocksWhenExceedingActiveGoal()
    {
        var profile = new PlayerProfile();
        ProgressionService.SetProfileForTests(profile);

        var achievements = new AchievementsService(profile);
        var goals = new List<Goal>
        {
            // Simulate state after pattern play: goal already incremented from 0 -> 1.
            new Goal(Goal.GoalType.ThreeOfKind, 1) { current = 1 }
        };

        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven),
            new Card(Card.Suit.Clubs, Card.Rank.Seven),
            new Card(Card.Suit.Diamonds, Card.Rank.Seven)
        };

        achievements.OnPatternPlayedDetailed(new FourOfKindPattern(), cards, goals);

        var progress = profile.achievements.FirstOrDefault(a => a.id == AchievementIds.Overachiever);
        Assert.IsNotNull(progress);
        Assert.IsTrue(progress.unlocked);
    }
}
