using UnityEngine;

public static class AuthService
{
    private const string TokenKey = "ProjectDelta.AuthToken.v1";
    private static bool initialized;
    private static string token;

    public static bool IsLoggedIn
    {
        get
        {
            EnsureLoaded();
            return !string.IsNullOrWhiteSpace(token);
        }
    }

    public static string Token
    {
        get
        {
            EnsureLoaded();
            return token;
        }
    }

    public static void SetToken(string newToken)
    {
        token = newToken;
        initialized = true;
        if (string.IsNullOrWhiteSpace(newToken))
        {
            PlayerPrefs.DeleteKey(TokenKey);
        }
        else
        {
            PlayerPrefs.SetString(TokenKey, newToken);
        }
        PlayerPrefs.Save();
    }

    public static void ClearToken()
    {
        SetToken(string.Empty);
    }

    private static void EnsureLoaded()
    {
        if (initialized) return;
        token = PlayerPrefs.GetString(TokenKey, string.Empty);
        initialized = true;
    }
}
