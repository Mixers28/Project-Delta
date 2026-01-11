using System;
using UnityEngine;

public static class DeviceFingerprint
{
    private const string FingerprintKey = "ProjectDelta.DeviceFingerprint.v1";

    public static string GetOrCreate()
    {
        var stored = PlayerPrefs.GetString(FingerprintKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(stored))
        {
            return stored;
        }

        var source = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrWhiteSpace(source))
        {
            source = $"{SystemInfo.operatingSystem}-{SystemInfo.deviceModel}-{SystemInfo.graphicsDeviceName}";
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            source = Guid.NewGuid().ToString("N");
        }

        PlayerPrefs.SetString(FingerprintKey, source);
        PlayerPrefs.Save();
        return source;
    }
}
