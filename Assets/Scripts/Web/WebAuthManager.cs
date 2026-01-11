using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class WebAuthManager
{
    private class RegisterRequest
    {
        public string deviceFingerprint;
    }

    public static IEnumerator Register(WebConfigData config, Action<WebAuthResponse> onSuccess, Action<string> onError)
    {
        if (config == null || string.IsNullOrWhiteSpace(config.apiUrl))
        {
            onError?.Invoke("Missing API URL");
            yield break;
        }

        var url = $"{config.apiUrl.TrimEnd('/')}/auth/register";
        var payload = new RegisterRequest { deviceFingerprint = DeviceFingerprint.GetOrCreate() };
        var json = JsonUtility.ToJson(payload);
        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Register failed: {request.error}");
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<WebAuthResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrWhiteSpace(response.token))
                {
                    onError?.Invoke("Register failed: empty response");
                    yield break;
                }
                onSuccess?.Invoke(response);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Register parse failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
