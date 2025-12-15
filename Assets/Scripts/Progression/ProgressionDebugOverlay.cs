using UnityEngine;

// Optional: add this component in a scene to visualize progression values at runtime.
public class ProgressionDebugOverlay : MonoBehaviour
{
    [SerializeField] private bool show = true;

    private void OnGUI()
    {
        if (!show) return;

        ProgressionService.EnsureInitialized();
        var profile = ProgressionService.Profile;

        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 420, 120), GUI.skin.box);
        GUILayout.Label("Progression Debug");
        GUILayout.Label($"TutorialStep: {profile.TutorialStep} / {ProgressionService.TutorialMaxStep} (active={ProgressionService.IsTutorialActive})");
        GUILayout.Label($"HighestLevelCompleted: {profile.highestLevelCompleted}");
        GUILayout.Label($"Coins: {profile.coins}");
        GUILayout.EndArea();
    }
}

