using UnityEngine;

public static class ProgressionService
{
    public const int TutorialMaxStep = 7;

    private static PlayerProfile cachedProfile;
    private static bool initialized;

    public static PlayerProfile Profile
    {
        get
        {
            EnsureInitialized();
            return cachedProfile;
        }
    }

    // Test hook: allow unit tests to provide an isolated profile instance.
    public static void SetProfileForTests(PlayerProfile profile)
    {
        cachedProfile = profile ?? new PlayerProfile();
        initialized = true;
    }

    public static bool IsTutorialActive => Profile.TutorialStep <= TutorialMaxStep;

    public static void EnsureInitialized()
    {
        if (initialized) return;
        cachedProfile = PlayerProfileStore.LoadOrCreate();
        initialized = true;
    }

    public static void Save()
    {
        EnsureInitialized();
        PlayerProfileStore.Save(cachedProfile);
    }

    public static void SetTutorialStep(int step)
    {
        EnsureInitialized();
        cachedProfile.TutorialStep = step;
        Save();
    }

    public static void AdvanceTutorialStepIfWin(bool isWin)
    {
        if (!isWin) return;
        EnsureInitialized();

        if (cachedProfile.TutorialStep <= TutorialMaxStep)
        {
            cachedProfile.TutorialStep += 1;
            if (cachedProfile.TutorialStep == TutorialMaxStep + 1)
            {
                cachedProfile.UnlockFeature(RunModeService.FeatureRunMode);
            }
            Save();
        }
    }
}
