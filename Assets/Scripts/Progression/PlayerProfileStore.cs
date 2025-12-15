using UnityEngine;

public static class PlayerProfileStore
{
    private const string PROFILE_KEY = "ProjectDelta.PlayerProfile.v1";

    public static PlayerProfile LoadOrCreate()
    {
        try
        {
            if (PlayerPrefs.HasKey(PROFILE_KEY))
            {
                var json = PlayerPrefs.GetString(PROFILE_KEY, string.Empty);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var loaded = JsonUtility.FromJson<PlayerProfile>(json);
                    if (loaded != null)
                    {
                        loaded.TutorialStep = loaded.TutorialStep;
                        return loaded;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"PlayerProfileStore.LoadOrCreate failed, creating new profile. {ex.GetType().Name}: {ex.Message}");
        }

        return new PlayerProfile();
    }

    public static void Save(PlayerProfile profile)
    {
        if (profile == null) return;
        profile.TutorialStep = profile.TutorialStep;

        var json = JsonUtility.ToJson(profile);
        PlayerPrefs.SetString(PROFILE_KEY, json);
        PlayerPrefs.Save();
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(PROFILE_KEY);
        PlayerPrefs.Save();
    }
}

