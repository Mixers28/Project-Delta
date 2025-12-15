using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptsTools
{
    [MenuItem("Tools/Diagnostics/Report Missing Scripts (Open Scenes)")]
    public static void ReportMissingScriptsOpenScenes()
    {
        int count = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            count += ReportMissingScriptsInScene(scene);
        }

        Debug.Log($"MissingScriptsTools: found {count} missing script reference(s) in open scenes.");
    }

    [MenuItem("Tools/Diagnostics/Remove Missing Scripts (Open Scenes)")]
    public static void RemoveMissingScriptsOpenScenes()
    {
        int removed = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            removed += RemoveMissingScriptsInScene(scene);
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        Debug.Log($"MissingScriptsTools: removed {removed} missing script reference(s) from open scenes.");
    }

    [MenuItem("Tools/Diagnostics/Report Missing Scripts (Selection)")]
    public static void ReportMissingScriptsSelection()
    {
        int total = 0;
        foreach (var go in Selection.gameObjects)
        {
            total += ReportMissingScriptsOnGameObject(go);
        }

        Debug.Log($"MissingScriptsTools: found {total} missing script reference(s) in selection.");
    }

    [MenuItem("Tools/Diagnostics/Remove Missing Scripts (Selection)")]
    public static void RemoveMissingScriptsSelection()
    {
        int removed = 0;
        foreach (var go in Selection.gameObjects)
        {
            removed += RemoveMissingScriptsOnGameObject(go);
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        Debug.Log($"MissingScriptsTools: removed {removed} missing script reference(s) from selection.");
    }

    private static int ReportMissingScriptsInScene(Scene scene)
    {
        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var go in EnumerateHierarchy(root))
            {
                count += ReportMissingScriptsOnGameObject(go, scene);
            }
        }

        return count;
    }

    private static int RemoveMissingScriptsInScene(Scene scene)
    {
        int removed = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var go in EnumerateHierarchy(root))
            {
                removed += RemoveMissingScriptsOnGameObject(go);
            }
        }

        return removed;
    }

    private static int ReportMissingScriptsOnGameObject(GameObject gameObject, Scene? scene = null)
    {
        if (gameObject == null) return 0;

        var components = gameObject.GetComponents<Component>();
        int missing = 0;

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null) continue;
            missing++;
        }

        if (missing > 0)
        {
            string sceneName = scene.HasValue ? scene.Value.name : gameObject.scene.name;
            Debug.LogWarning($"Missing script x{missing} on `{GetPath(gameObject)}` (scene: `{sceneName}`)", gameObject);
        }

        return missing;
    }

    private static int RemoveMissingScriptsOnGameObject(GameObject gameObject)
    {
        if (gameObject == null) return 0;
        int before = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
        if (before <= 0) return 0;

        Undo.RegisterCompleteObjectUndo(gameObject, "Remove Missing Scripts");
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
        int after = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
        return before - after;
    }

    private static IEnumerable<GameObject> EnumerateHierarchy(GameObject root)
    {
        if (root == null) yield break;

        var stack = new Stack<Transform>();
        stack.Push(root.transform);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current.gameObject;

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }
    }

    private static string GetPath(GameObject gameObject)
    {
        var parts = new List<string>();
        var t = gameObject.transform;
        while (t != null)
        {
            parts.Add(t.name);
            t = t.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }
}

