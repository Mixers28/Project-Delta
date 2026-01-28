using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLPromptInput : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ShowPrompt(string gameObjectName, string callbackMethod, string message, string defaultValue);
#endif

    private Action<string> pendingCallback;

    public void RequestPrompt(string message, string defaultValue, Action<string> onComplete)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        pendingCallback = onComplete;
        ShowPrompt(gameObject.name, nameof(OnPromptResult), message, defaultValue ?? string.Empty);
#else
        onComplete?.Invoke(defaultValue ?? string.Empty);
#endif
    }

    public void OnPromptResult(string value)
    {
        pendingCallback?.Invoke(value);
        pendingCallback = null;
    }
}
