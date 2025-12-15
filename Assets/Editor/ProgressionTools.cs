using UnityEditor;
using UnityEngine;

public static class ProgressionTools
{
    [MenuItem("Tools/Progression/Reset Tutorial Progress")]
    public static void ResetTutorialProgress()
    {
        PlayerProfileStore.Reset();
        Debug.Log("Tutorial progress reset.");
    }

    [MenuItem("Tools/Progression/Start Run Mode")]
    public static void StartRunMode()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found. Enter Play Mode first.");
            return;
        }

        GameManager.Instance.StartRunMode();
        Debug.Log("Run Mode started.");
    }

    [MenuItem("Tools/Progression/Stop Run Mode")]
    public static void StopRunMode()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found. Enter Play Mode first.");
            return;
        }

        GameManager.Instance.StopRunMode();
        Debug.Log("Run Mode stopped.");
    }
}
