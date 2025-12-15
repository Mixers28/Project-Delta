using System.Collections.Generic;

public static class AchievementsCatalog
{
    public static readonly IReadOnlyList<AchievementDefinition> All = new List<AchievementDefinition>
    {
        new(AchievementIds.FirstLevel, "First Steps", "Complete your first level.", targetValue: 1, rewardCoins: 50),
        new(AchievementIds.FirstPair, "Pair Apprentice", "Play your first valid pair.", targetValue: 1, rewardCoins: 25),
        new(AchievementIds.FirstStraightRun3, "Run Rookie", "Play your first straight run of 3.", targetValue: 1, rewardCoins: 25),
    };
}

