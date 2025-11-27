using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        Debug.Log("GameOverPanel Awake");
        
        // If panel is not assigned, try to find it
        if (panel == null)
        {
            Debug.LogWarning("Panel reference not assigned in inspector. Searching for panel...");
            // Try to find a child panel
            Transform panelTransform = transform.Find("Panel");
            if (panelTransform != null)
            {
                panel = panelTransform.gameObject;
                Debug.Log("✓ Found panel in children!");
            }
            else
            {
                Debug.LogError("✗ Panel not found! Make sure to assign it in the inspector or add a child GameObject named 'Panel'");
            }
        }
        
        Debug.Log($"Panel reference: {(panel != null ? "SET" : "NULL")}");
        Debug.Log($"titleText reference: {(titleText != null ? "SET" : "NULL")}");
        Debug.Log($"messageText reference: {(messageText != null ? "SET" : "NULL")}");
        
        if (panel != null)
        {
            // IMPORTANT: Make sure panel starts INACTIVE so it doesn't show on startup
            panel.SetActive(false);
            Debug.Log("Panel set to INACTIVE (will activate on game end)");
        }
        else
        {
            Debug.LogWarning("Panel reference is NULL - GameOverPanel won't display!");
        }
    }

    private void Start()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("GameOverPanel.Start() CALLED - subscribing to GameManager.OnGameEnd");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("✗ CRITICAL: GameManager.Instance is NULL in Start()!");
            Debug.LogError("  This means GameOverPanel cannot subscribe to events.");
            Debug.LogError("  Check: Is GameManager spawning before GameOverPanel?");
            return;
        }
        
        Debug.Log("✓ GameManager.Instance is NOT null");
        
        // Subscribe to the OnGameEnd event
        GameManager.Instance.OnGameEnd += HandleGameEnd;
        
        Debug.Log("✓ Successfully subscribed to GameManager.OnGameEnd");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnd -= HandleGameEnd;
        }
    }

    private void HandleGameEnd(bool isWin)
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"⚡ GameOverPanel.HandleGameEnd TRIGGERED ⚡");
        Debug.Log($"  isWin: {isWin}");
        Debug.Log($"  panel active state: {(panel != null ? panel.activeSelf : "UNKNOWN")}");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        if (isWin)
        {
            ShowWin();
        }
        else
        {
            ShowLose();
        }
    }

    private void ShowWin()
    {
        Debug.Log("ShowWin called");
        
        if (panel == null)
        {
            Debug.LogError("Cannot show win - panel is NULL!");
            return;
        }
        
        if (titleText == null || messageText == null)
        {
            Debug.LogError("titleText or messageText is NULL!");
            return;
        }
        
        var game = GameManager.Instance?.CurrentGame;
        if (game == null)
        {
            Debug.LogError("CurrentGame is NULL in ShowWin!");
            return;
        }
        
        // Set content
        titleText.text = "🎉 LEVEL COMPLETE! 🎉";
        titleText.color = Color.yellow;
        
        string goalsSummary = "";
        foreach (var goal in game.Goals)
        {
            goalsSummary += $"✓ {goal.DisplayText}\n";
        }
        
        messageText.text = $"<size=48>Score: {game.Score}</size>\n\n{goalsSummary}\nMoves Remaining: {game.MovesRemaining}";
        
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        
        // Ensure panel is active
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            Debug.Log("Win panel activated");
        }
        else
        {
            Debug.Log("Win panel already active, content updated");
        }
        
        StartCoroutine(CelebrationAnimation());
    }

    private void ShowLose()
    {
        Debug.Log("ShowLose called");
        
        if (panel == null)
        {
            Debug.LogError("Cannot show lose - panel is NULL!");
            return;
        }
        
        if (titleText == null || messageText == null)
        {
            Debug.LogError("titleText or messageText is NULL!");
            return;
        }
        
        var game = GameManager.Instance?.CurrentGame;
        if (game == null)
        {
            Debug.LogError("CurrentGame is NULL in ShowLose!");
            return;
        }
        
        // Set content
        titleText.text = "LEVEL FAILED";
        titleText.color = Color.red;
        
        string incompleteGoals = "";
        foreach (var goal in game.Goals)
        {
            if (!goal.IsComplete)
            {
                incompleteGoals += $"✗ {goal.DisplayText}\n";
            }
        }
        
        messageText.text = $"Out of moves!\n\nIncomplete Goals:\n{incompleteGoals}\nFinal Score: {game.Score}";
        
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(true);
        
        // Ensure panel is active
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            Debug.Log("Lose panel activated");
        }
        else
        {
            Debug.Log("Lose panel already active, content updated");
        }
    }

    private System.Collections.IEnumerator CelebrationAnimation()
    {
        if (titleText == null) yield break;
        
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = titleText.transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / duration * Mathf.PI) * 0.3f;
            titleText.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        titleText.transform.localScale = originalScale;
    }

    private void OnRetry()
    {
        Debug.Log("Retry clicked");
        if (panel != null) panel.SetActive(false);
        GameManager.Instance.StartTestLevel();
    }

    private void OnContinue()
    {
        Debug.Log("Continue clicked");
        if (panel != null) panel.SetActive(false);
        GameManager.Instance.StartTestLevel();
    }

    // TEST: Press W key to manually test win screen
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("TEST: Manual WIN trigger (W pressed)");
            HandleGameEnd(true);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("TEST: Manual LOSE trigger (L pressed)");
            HandleGameEnd(false);
        }
    }
}