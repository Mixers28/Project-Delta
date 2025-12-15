using UnityEngine;

public static class RunModeService
{
    public const string FeatureRunMode = "RunMode";
    public const string FeatureSuitedRuns = "SuitedRuns";

    public static bool IsActive => ProgressionService.Profile.runModeActive;

    public static bool CanStart()
    {
        return ProgressionService.Profile.TutorialStep > ProgressionService.TutorialMaxStep;
    }

    public static void StartNewRun()
    {
        if (!CanStart())
        {
            Debug.LogWarning("RunModeService: cannot start run until tutorial is complete.");
            return;
        }

        var profile = ProgressionService.Profile;
        profile.runModeActive = true;
        profile.currentRunLength = 0;
        profile.currentRunScore = 0;
        ProgressionService.Save();
    }

    public static void StopRun()
    {
        var profile = ProgressionService.Profile;
        profile.runModeActive = false;
        ProgressionService.Save();
    }

    public static void RecordLevelWin(int levelScore)
    {
        var profile = ProgressionService.Profile;
        if (!profile.runModeActive) return;

        profile.currentRunLength += 1;
        profile.currentRunScore += Mathf.Max(0, levelScore);

        if (profile.currentRunLength > profile.bestRunLength)
        {
            profile.bestRunLength = profile.currentRunLength;
        }

        if (profile.currentRunScore > profile.bestRunScore)
        {
            profile.bestRunScore = profile.currentRunScore;
        }

        ProgressionService.Save();
    }

    public static void RecordRunEnd()
    {
        var profile = ProgressionService.Profile;
        if (!profile.runModeActive) return;
        profile.runModeActive = false;
        ProgressionService.Save();
    }
}

