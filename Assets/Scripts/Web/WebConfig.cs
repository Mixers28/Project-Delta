using System;
using UnityEngine;

[Serializable]
public class WebConfigData
{
    public string apiUrl = "http://localhost:3000";
    public string environment = "development";
}

public static class WebConfigLoader
{
    private const string ResourcePath = "Config/WebConfig";

    public static WebConfigData Load()
    {
        var textAsset = Resources.Load<TextAsset>(ResourcePath);
        if (textAsset == null || string.IsNullOrWhiteSpace(textAsset.text))
        {
            Debug.LogWarning($"WebConfigLoader: Missing WebConfig at Resources/{ResourcePath}. Using defaults.");
            return new WebConfigData();
        }

        try
        {
            var config = JsonUtility.FromJson<WebConfigData>(textAsset.text);
            if (config == null)
            {
                throw new Exception("JsonUtility returned null");
            }

            return config;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"WebConfigLoader: Failed to parse WebConfig. {ex.GetType().Name}: {ex.Message}");
            return new WebConfigData();
        }
    }
}
