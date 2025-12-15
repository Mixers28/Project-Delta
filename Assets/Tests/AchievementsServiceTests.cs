using NUnit.Framework;

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
        var achievements = new AchievementsService(profile);
        achievements.OnLevelCompleted();

        var first = achievements.TakeRecentlyUnlocked();
        var second = achievements.TakeRecentlyUnlocked();

        Assert.IsTrue(first.Count > 0);
        Assert.AreEqual(0, second.Count);
    }
}

