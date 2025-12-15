using System;

[Serializable]
public class AchievementDefinition
{
    public string id;
    public string name;
    public string description;
    public int targetValue;
    public int rewardCoins;

    public AchievementDefinition(string id, string name, string description, int targetValue, int rewardCoins)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.targetValue = targetValue;
        this.rewardCoins = rewardCoins;
    }
}

