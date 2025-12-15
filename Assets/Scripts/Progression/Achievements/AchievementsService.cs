using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementsService
{
    private readonly PlayerProfile profile;
    private readonly Dictionary<string, AchievementDefinition> definitionsById;
    private readonly List<AchievementDefinition> recentlyUnlocked = new();

    public event System.Action<AchievementDefinition> OnUnlocked;

    public AchievementsService(PlayerProfile profile)
    {
        this.profile = profile;
        definitionsById = AchievementsCatalog.All.ToDictionary(d => d.id, d => d);
        EnsureProgressEntries();
    }

    public IReadOnlyList<AchievementDefinition> TakeRecentlyUnlocked()
    {
        if (recentlyUnlocked.Count == 0) return System.Array.Empty<AchievementDefinition>();
        var copy = recentlyUnlocked.ToArray();
        recentlyUnlocked.Clear();
        return copy;
    }

    public void OnLevelCompleted()
    {
        Increment(AchievementIds.FirstLevel, 1);
    }

    public void OnPatternPlayed(IPattern pattern)
    {
        if (pattern == null) return;

        if (pattern.Id == PatternId.Pair)
        {
            Increment(AchievementIds.FirstPair, 1);
        }

        if (pattern.Id == PatternId.StraightRun3 || pattern.Id == PatternId.StraightRun4 || pattern.Id == PatternId.StraightRun5)
        {
            Increment(AchievementIds.FirstStraightRun3, 1);
        }
    }

    public void OnPatternPlayed(IPattern pattern, int score)
    {
        OnPatternPlayed(pattern);
    }

    private void EnsureProgressEntries()
    {
        profile.achievements ??= new List<AchievementProgress>();

        foreach (var def in AchievementsCatalog.All)
        {
            if (profile.achievements.All(a => a.id != def.id))
            {
                profile.achievements.Add(new AchievementProgress(def.id, def.targetValue));
            }
        }
    }

    private void Increment(string achievementId, int amount)
    {
        if (!definitionsById.TryGetValue(achievementId, out var def)) return;

        var progress = profile.achievements.FirstOrDefault(a => a.id == achievementId);
        if (progress == null)
        {
            progress = new AchievementProgress(def.id, def.targetValue);
            profile.achievements.Add(progress);
        }

        if (progress.unlocked) return;

        progress.target = def.targetValue;
        progress.current = Mathf.Clamp(progress.current + amount, 0, def.targetValue);

        if (progress.current >= def.targetValue)
        {
            progress.unlocked = true;
            profile.coins += Mathf.Max(0, def.rewardCoins);
            recentlyUnlocked.Add(def);
            OnUnlocked?.Invoke(def);
            Debug.Log($"[Achievement] Unlocked {def.id} (+{def.rewardCoins} coins)");
            ProgressionService.Save();
        }
        else
        {
            ProgressionService.Save();
        }
    }
}
