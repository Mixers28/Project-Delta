using System;

[Serializable]
public class AchievementProgress
{
    public string id;
    public int current;
    public int target;
    public bool unlocked;

    public AchievementProgress(string id, int target)
    {
        this.id = id;
        this.target = target;
        current = 0;
        unlocked = false;
    }
}

