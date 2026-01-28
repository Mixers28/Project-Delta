using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class CloudProfileStore
{
    private const string BaseUrlKey = "ProjectDelta.CloudBaseUrl.v1";
    private const string DefaultBaseUrl = "https://project-delta-production.up.railway.app/";

    [Serializable]
    private class AuthRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class AuthResponse
    {
        public string token;
    }

    [Serializable]
    private class ProfileResponse
    {
        public PlayerProfile profile;
        public long serverUpdatedUtc;
    }

    [Serializable]
    private class ProfileRequest
    {
        public PlayerProfile profile;
    }

    public static string BaseUrl
    {
        get => PlayerPrefs.GetString(BaseUrlKey, DefaultBaseUrl);
        set
        {
            PlayerPrefs.SetString(BaseUrlKey, value ?? string.Empty);
            PlayerPrefs.Save();
        }
    }

    public static void SaveProfile(PlayerProfile profile)
    {
        if (!AuthService.IsLoggedIn) return;
        CloudSyncRunner.Run(SaveProfileRoutine(profile, null, null));
    }

    public static System.Collections.IEnumerator Signup(string email, string password, Action<string> onSuccess, Action<string> onError)
    {
        return AuthRoutine("/auth/signup", email, password, onSuccess, onError);
    }

    public static System.Collections.IEnumerator Login(string email, string password, Action<string> onSuccess, Action<string> onError)
    {
        return AuthRoutine("/auth/login", email, password, onSuccess, onError);
    }

    public static System.Collections.IEnumerator FetchProfile(Action<PlayerProfile> onSuccess, Action<string> onError)
    {
        if (!AuthService.IsLoggedIn)
        {
            onError?.Invoke("Not logged in.");
            yield break;
        }

        var request = UnityWebRequest.Get(BaseUrl.TrimEnd('/') + "/profile");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + AuthService.Token);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var response = JsonUtility.FromJson<ProfileResponse>(request.downloadHandler.text);
        onSuccess?.Invoke(response != null ? response.profile : null);
    }

    public static System.Collections.IEnumerator SaveProfileRoutine(PlayerProfile profile, Action onSuccess, Action<string> onError)
    {
        if (!AuthService.IsLoggedIn)
        {
            onError?.Invoke("Not logged in.");
            yield break;
        }

        var body = new ProfileRequest { profile = profile };
        var json = JsonUtility.ToJson(body);
        var request = new UnityWebRequest(BaseUrl.TrimEnd('/') + "/profile", "PUT");
        var bytes = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + AuthService.Token);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        onSuccess?.Invoke();
    }

    private static System.Collections.IEnumerator AuthRoutine(string path, string email, string password, Action<string> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            onError?.Invoke("Email and password are required.");
            yield break;
        }

        var body = new AuthRequest { email = email, password = password };
        var json = JsonUtility.ToJson(body);

        var request = new UnityWebRequest(BaseUrl.TrimEnd('/') + path, "POST");
        var bytes = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
        if (response == null || string.IsNullOrWhiteSpace(response.token))
        {
            onError?.Invoke("Invalid auth response.");
            yield break;
        }

        onSuccess?.Invoke(response.token);
    }
}
