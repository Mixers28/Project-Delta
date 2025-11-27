using UnityEngine;

/// <summary>
/// Minimal scene manager that ensures GameOverPanelSetup runs before other UI components.
/// This is placed in the scene as a GameObject to trigger auto-UI setup.
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("SceneBootstrap.Awake() - Initializing scene");
        
        // Ensure GameOverPanelSetup exists and runs
        GameOverPanelSetup setup = FindObjectOfType<GameOverPanelSetup>();
        if (setup == null)
        {
            Debug.LogWarning("GameOverPanelSetup not found, creating one...");
            GameObject setupObj = new GameObject("GameOverPanelSetup");
            setup = setupObj.AddComponent<GameOverPanelSetup>();
        }
    }
}
