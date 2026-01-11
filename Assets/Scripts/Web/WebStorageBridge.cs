using System.Runtime.InteropServices;
using UnityEngine;

public static class WebStorageBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void SetApiUrl(string url);
#endif

    public static void PersistApiUrl(string apiUrl)
    {
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            SetApiUrl(apiUrl);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"WebStorageBridge: failed to persist api_url via JS. {ex.GetType().Name}: {ex.Message}");
        }
#else
        PlayerPrefs.SetString("api_url", apiUrl);
        PlayerPrefs.Save();
#endif
    }
}
