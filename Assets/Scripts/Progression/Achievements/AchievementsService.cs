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

        if (pattern.Id == PatternId.SuitedRun3 || pattern.Id == PatternId.SuitedRun4 || pattern.Id == PatternId.SuitedRun5)
        {
            Increment(AchievementIds.FirstSuitedRun3, 1);
        }

        if (pattern.Id == PatternId.SuitSet3Plus)
        {
            Increment(AchievementIds.FirstSuitSet, 1);
        }

        if (pattern.Id == PatternId.ColorSet3Plus)
        {
            Increment(AchievementIds.FirstColorSet, 1);
        }

        if (pattern.Id == PatternId.FourOfKind)
        {
            Increment(AchievementIds.FirstFourOfKind, 1);
        }

        if (pattern.Id == PatternId.RoyalFlush5)
        {
            Increment(AchievementIds.FirstRoyalFlush, 1);
        }
    }

    public void OnPatternPlayed(IPattern pattern, int score)
    {
        OnPatternPlayed(pattern);
    }

    public void OnPatternPlayedDetailed(IPattern pattern, List<Card> cards, List<Goal> goals)
    {
        if (pattern == null || cards == null || goals == null || goals.Count == 0) return;

        if (DidOverachieveAnActiveGoal(pattern, cards, goals))
        {
            Increment(AchievementIds.Overachiever, 1);
        }
    }

    private static bool DidOverachieveAnActiveGoal(IPattern pattern, List<Card> cards, List<Goal> goals)
    {
        foreach (var goal in goals)
        {
            if (goal == null) continue;
            if (goal.type == Goal.GoalType.TotalScore) continue;
            if (!goal.MatchesPattern(pattern)) continue;

            // Goal matching has already occurred in GameState.UpdateGoals by this point; infer "was incomplete"
            // by checking whether current was below required before the +1 increment.
            int currentBefore = goal.current - 1;
            bool wasIncompleteBefore = currentBefore < goal.required;
            if (!wasIncompleteBefore) continue;

            if (PatternExceedsGoalMinimum(pattern, cards, goal.type))
            {
                return true;
            }
        }

        return false;
    }

    private static bool PatternExceedsGoalMinimum(IPattern pattern, List<Card> cards, Goal.GoalType goalType)
    {
        int playedCount = cards.Count;

        switch (goalType)
        {
            case Goal.GoalType.ThreeOfKind:
                return pattern.Id == PatternId.FourOfKind;

            case Goal.GoalType.Flush:
                return pattern.Id == PatternId.RoyalFlush5;

            case Goal.GoalType.Run3:
            case Goal.GoalType.StraightRun3:
            case Goal.GoalType.SuitedRun3:
                return playedCount > 3;

            case Goal.GoalType.Run4:
            case Goal.GoalType.StraightRun4:
            case Goal.GoalType.SuitedRun4:
                return playedCount > 4;

            case Goal.GoalType.SuitSet3Plus:
            case Goal.GoalType.ColorSet3Plus:
                return playedCount > 3;

            default:
                return false;
        }
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
