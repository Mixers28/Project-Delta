using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    [Min(1)]
    public int tutorialStep = 1;

    public bool hasSeenIntro = false;

    public bool hasSeenPostTutorialIntro = false;

    public int highestLevelCompleted = 0;

    // Count of level wins after the tutorial is complete.
    // Used for difficulty tiering (early → mid/late).
    [Min(0)]
    public int nonTutorialWins = 0;

    public int coins = 0;

    public long lastUpdatedUtc = 0;

    public long sessionUpdatedUtc = 0;

    public SavedGameSession activeSession = null;

    public List<string> unlockedFeatures = new();

    public List<AchievementProgress> achievements = new();

    // Run mode stats
    public bool runModeActive = false;
    public int currentRunLength = 0;
    public int currentRunScore = 0;
    public int bestRunLength = 0;
    public int bestRunScore = 0;

    public int TutorialStep
    {
        get => Mathf.Max(1, tutorialStep);
        set => tutorialStep = Mathf.Max(1, value);
    }

    public int NonTutorialWins
    {
        get => Mathf.Max(0, nonTutorialWins);
        set => nonTutorialWins = Mathf.Max(0, value);
    }

    public bool HasFeature(string featureFlag)
    {
        if (string.IsNullOrWhiteSpace(featureFlag)) return false;
        return unlockedFeatures != null && unlockedFeatures.Contains(featureFlag);
    }

    public void UnlockFeature(string featureFlag)
    {
        if (string.IsNullOrWhiteSpace(featureFlag)) return;
        unlockedFeatures ??= new List<string>();
        if (!unlockedFeatures.Contains(featureFlag))
        {
            unlockedFeatures.Add(featureFlag);
        }
    }
}
