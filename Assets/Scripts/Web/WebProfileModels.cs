using System;
using System.Collections.Generic;

[Serializable]
public class WebAuthResponse
{
    public string token;
    public string userId;
    public bool isNewUser;
}

[Serializable]
public class WebProfileResponse
{
    public string userId;
    public int tutorialStep;
    public WebUnlocks unlocksJson;
    public WebRunStats runStatsJson;
    public WebAchievements achievementsJson;
    public int coins;
}

[Serializable]
public class WebProfileUpdateRequest
{
    public int tutorialStep;
    public WebUnlocks unlocksJson;
    public WebRunStats runStatsJson;
    public WebAchievements achievementsJson;
    public int coins;
}

[Serializable]
public class WebUnlocks
{
    public List<string> features = new();
    public bool hasSeenIntro;
    public bool hasSeenPostTutorialIntro;
    public int highestLevelCompleted;
}

[Serializable]
public class WebRunStats
{
    public bool runModeActive;
    public int currentRunLength;
    public int currentRunScore;
    public int bestRunLength;
    public int bestRunScore;
    public int nonTutorialWins;
}

[Serializable]
public class WebAchievements
{
    public List<AchievementProgress> achievements = new();
}
