using System;
using UnityEngine;

public static class ProgressionService
{
    public const int TutorialMaxStep = 7;
    public const int MidTierStartsAtNonTutorialWins = 7;

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

    public static int NonTutorialWins => Profile.NonTutorialWins;

    // Post-tutorial level index (1-based). Returns 0 while tutorial is active.
    public static int PostTutorialLevelIndex
    {
        get
        {
            EnsureInitialized();
            if (IsTutorialActive) return 0;
            return Mathf.Max(1, cachedProfile.NonTutorialWins + 1);
        }
    }

    public static RuleTier CurrentRuleTier
    {
        get
        {
            EnsureInitialized();

            if (IsTutorialActive)
            {
                return RuleTier.Tutorial;
            }

            if (cachedProfile.NonTutorialWins >= MidTierStartsAtNonTutorialWins)
            {
                return RuleTier.Mid;
            }

            return RuleTier.Early;
        }
    }

    public static void EnsureInitialized()
    {
        if (initialized) return;
        cachedProfile = PlayerProfileStore.LoadOrCreate();
        initialized = true;
    }

    public static void Save()
    {
        EnsureInitialized();
        Save(includeCloud: true, updateTimestamp: true);
    }

    public static void Save(bool includeCloud)
    {
        Save(includeCloud, updateTimestamp: true);
    }

    public static void Save(bool includeCloud, bool updateTimestamp)
    {
        EnsureInitialized();
        if (updateTimestamp)
        {
            cachedProfile.lastUpdatedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        PlayerProfileStore.Save(cachedProfile);
        if (includeCloud && AuthService.IsLoggedIn)
        {
            CloudProfileStore.SaveProfile(cachedProfile);
        }
    }

    public static void ResetProfile()
    {
        PlayerProfileStore.Reset();
        cachedProfile = new PlayerProfile();
        initialized = true;
        Save(includeCloud: true);
    }

    public static void ReplaceProfile(PlayerProfile profile, bool saveLocal = true, bool includeCloud = false)
    {
        EnsureInitialized();
        cachedProfile = profile ?? new PlayerProfile();
        if (saveLocal)
        {
            Save(includeCloud, updateTimestamp: false);
        }
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

    public static void RecordNonTutorialWin()
    {
        EnsureInitialized();

        if (IsTutorialActive)
        {
            return;
        }

        cachedProfile.NonTutorialWins += 1;
    }
}
