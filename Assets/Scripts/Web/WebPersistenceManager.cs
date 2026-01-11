using System;
using System.Collections;
using UnityEngine;

public static class WebPersistenceManager
{
    private const string TokenKey = "ProjectDelta.Web.Token";
    private const string UserIdKey = "ProjectDelta.Web.UserId";

    private static bool initialized;
    private static bool initializing;
    private static string token;
    private static string userId;
    private static WebConfigData config;
    private static MonoBehaviour coroutineHost;
    private static event Action<string> OnStatus;

    public static bool IsReady => initialized && !string.IsNullOrWhiteSpace(token);

    public static void Initialize(MonoBehaviour host, Action<string> statusListener = null)
    {
#if UNITY_WEBGL
        if (statusListener != null)
        {
            OnStatus += statusListener;
        }

        if (initialized || initializing)
        {
            NotifyStatus("Web persistence already initialized.");
            return;
        }

        if (coroutineHost == null || !coroutineHost.isActiveAndEnabled)
        {
            coroutineHost = host;
        }
        initializing = true;
        coroutineHost.StartCoroutine(InitializeRoutine());
#else
        statusListener?.Invoke("Web persistence skipped (non-WebGL platform).");
#endif
    }

    public static void HandleGameEnd(bool isWin)
    {
#if UNITY_WEBGL
        if (!IsReady || coroutineHost == null)
        {
            return;
        }

        coroutineHost.StartCoroutine(PushProfileRoutine());
#endif
    }

    private static IEnumerator InitializeRoutine()
    {
        NotifyStatus("Loading web config...");
        config = WebConfigLoader.Load();
        WebStorageBridge.PersistApiUrl(config.apiUrl);

        token = PlayerPrefs.GetString(TokenKey, string.Empty);
        userId = PlayerPrefs.GetString(UserIdKey, string.Empty);

        if (string.IsNullOrWhiteSpace(token))
        {
            NotifyStatus("Registering device...");
            yield return WebAuthManager.Register(
                config,
                onSuccess: response =>
                {
                    token = response.token;
                    userId = response.userId;
                    PlayerPrefs.SetString(TokenKey, token);
                    PlayerPrefs.SetString(UserIdKey, userId);
                    PlayerPrefs.Save();
                    NotifyStatus("Registration complete.");
                },
                onError: err => NotifyStatus(err ?? "Registration failed"));
        }
        else
        {
            NotifyStatus("Using cached token.");
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            NotifyStatus("Syncing profile...");
            yield return WebProfileSyncService.PullProfile(
                config.apiUrl,
                token,
                onSuccess: remote =>
                {
                    WebProfileSyncService.ApplyRemoteProfile(remote, ProgressionService.Profile);
                    ProgressionService.Save();
                    NotifyStatus("Profile synced.");
                },
                onError: err => NotifyStatus(err ?? "Profile sync failed"));
        }

        initializing = false;
        initialized = true;
        NotifyStatus("Web persistence ready.");
    }

    private static IEnumerator PushProfileRoutine()
    {
        NotifyStatus("Pushing profile...");
        yield return WebProfileSyncService.PushProfile(
            config.apiUrl,
            token,
            ProgressionService.Profile,
            onSuccess: _ => NotifyStatus("Profile push complete."),
            onError: err => NotifyStatus(err ?? "Profile push failed"));
    }

    private static void NotifyStatus(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        Debug.Log($"[Web] {message}");
        OnStatus?.Invoke(message);
    }
}
