using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class WebProfileSyncService
{
    private const int RequestTimeoutSeconds = 20;

    public static IEnumerator PullProfile(string apiUrl, string token, Action<WebProfileResponse> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(token))
        {
            onError?.Invoke("Missing apiUrl or token");
            yield break;
        }

        var url = $"{apiUrl.TrimEnd('/')}/api/profile";
        using (var request = UnityWebRequest.Get(url))
        {
            request.timeout = RequestTimeoutSeconds;
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Profile fetch failed: {request.error}");
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<WebProfileResponse>(request.downloadHandler.text);
                if (response == null || response.tutorialStep <= 0)
                {
                    onError?.Invoke("Profile fetch returned empty response");
                    yield break;
                }

                onSuccess?.Invoke(response);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Profile parse failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    public static IEnumerator PushProfile(string apiUrl, string token, PlayerProfile profile, Action<WebProfileResponse> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(token))
        {
            onError?.Invoke("Missing apiUrl or token");
            yield break;
        }

        if (profile == null)
        {
            onError?.Invoke("Profile is null");
            yield break;
        }

        var url = $"{apiUrl.TrimEnd('/')}/api/profile";
        var payload = BuildUpdatePayload(profile);
        var json = JsonUtility.ToJson(payload);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.timeout = RequestTimeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Profile push failed: {request.error}");
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<WebProfileResponse>(request.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Profile push parse failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static WebProfileUpdateRequest BuildUpdatePayload(PlayerProfile profile)
    {
        return new WebProfileUpdateRequest
        {
            tutorialStep = profile.TutorialStep,
            coins = profile.coins,
            unlocksJson = new WebUnlocks
            {
                features = new List<string>(profile.unlockedFeatures ?? new List<string>()),
                hasSeenIntro = profile.hasSeenIntro,
                hasSeenPostTutorialIntro = profile.hasSeenPostTutorialIntro,
                highestLevelCompleted = profile.highestLevelCompleted
            },
            runStatsJson = new WebRunStats
            {
                runModeActive = profile.runModeActive,
                currentRunLength = profile.currentRunLength,
                currentRunScore = profile.currentRunScore,
                bestRunLength = profile.bestRunLength,
                bestRunScore = profile.bestRunScore,
                nonTutorialWins = profile.NonTutorialWins
            },
            achievementsJson = new WebAchievements
            {
                achievements = new List<AchievementProgress>(profile.achievements ?? new List<AchievementProgress>())
            }
        };
    }

    public static void ApplyRemoteProfile(WebProfileResponse remote, PlayerProfile local)
    {
        if (remote == null || local == null) return;

        local.TutorialStep = Mathf.Max(local.TutorialStep, remote.tutorialStep);
        local.coins = Math.Max(local.coins, remote.coins);

        if (remote.unlocksJson != null)
        {
            local.hasSeenIntro = local.hasSeenIntro || remote.unlocksJson.hasSeenIntro;
            local.hasSeenPostTutorialIntro = local.hasSeenPostTutorialIntro || remote.unlocksJson.hasSeenPostTutorialIntro;
            local.highestLevelCompleted = Math.Max(local.highestLevelCompleted, remote.unlocksJson.highestLevelCompleted);

            if (remote.unlocksJson.features != null && remote.unlocksJson.features.Count > 0)
            {
                foreach (var feature in remote.unlocksJson.features)
                {
                    local.UnlockFeature(feature);
                }
            }
        }

        if (remote.runStatsJson != null)
        {
            local.runModeActive = remote.runStatsJson.runModeActive;
            local.currentRunLength = Math.Max(local.currentRunLength, remote.runStatsJson.currentRunLength);
            local.currentRunScore = Math.Max(local.currentRunScore, remote.runStatsJson.currentRunScore);
            local.bestRunLength = Math.Max(local.bestRunLength, remote.runStatsJson.bestRunLength);
            local.bestRunScore = Math.Max(local.bestRunScore, remote.runStatsJson.bestRunScore);
            local.NonTutorialWins = Math.Max(local.NonTutorialWins, remote.runStatsJson.nonTutorialWins);
        }

        if (remote.achievementsJson != null && remote.achievementsJson.achievements != null)
        {
            local.achievements = new List<AchievementProgress>(remote.achievementsJson.achievements);
        }
    }
}
