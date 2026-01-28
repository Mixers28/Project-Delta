using System.Collections;
using UnityEngine;

public class CloudSyncRunner : MonoBehaviour
{
    private static CloudSyncRunner instance;

    public static CloudSyncRunner EnsureExists()
    {
        if (instance != null) return instance;

        var go = new GameObject("CloudSyncRunner");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<CloudSyncRunner>();
        return instance;
    }

    public static void Run(IEnumerator routine)
    {
        if (routine == null) return;
        EnsureExists().StartCoroutine(routine);
    }
}
