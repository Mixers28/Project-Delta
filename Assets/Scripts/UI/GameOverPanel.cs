using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    private const float CELEBRATION_DURATION = 0.5f;
    private const float CELEBRATION_SCALE = 0.3f;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;

    private GameState cachedGameState;

    private void Awake()
    {
        // Make sure panel reference is resolved even if named differently in prefab
        EnsureReferences();

        if (panel == null)
        {
            Debug.LogError("GameOverPanel: Panel not found and could not be created!");
            return;
        }

        panel.SetActive(false);
    }

    private bool TryFindPanel()
    {
        if (panel != null) return true;

        Transform panelTransform = transform.Find("Panel") ?? transform.Find("PanelBackground");
        if (panelTransform != null)
        {
            panel = panelTransform.gameObject;
            return true;
        }

        return false;
    }

    private void Start()
    {
        StartCoroutine(InitializePanel());
    }

    private System.Collections.IEnumerator InitializePanel()
    {
        while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
        {
            yield return null;
        }

        cachedGameState = GameManager.Instance.CurrentGame;
        GameManager.Instance.OnGameEnd += HandleGameEnd;

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetry);
        }
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
        }
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
        EnsureReferences();
        if (isWin)
        {
            ShowWin();
        }
        else
        {
            ShowLose();
        }
    }

    private void EnsureReferences()
    {
        if (panel == null)
        {
            var p = transform.Find("Panel") ?? transform.Find("PanelBackground");
            if (p != null) panel = p.gameObject;
        }

        if (panel != null)
        {
            if (titleText == null)
            {
                var t = panel.transform.Find("TitleText");
                if (t != null) titleText = t.GetComponent<TextMeshProUGUI>();
            }

            if (messageText == null)
            {
                var t = panel.transform.Find("MessageText");
                if (t != null) messageText = t.GetComponent<TextMeshProUGUI>();
            }

            if (retryButton == null)
            {
                var t = panel.transform.Find("RetryButton");
                if (t != null) retryButton = t.GetComponent<Button>();
            }

            if (continueButton == null)
            {
                var t = panel.transform.Find("ContinueButton");
                if (t != null) continueButton = t.GetComponent<Button>();
            }
        }
    }

    private void ShowWin()
    {
        if (!ValidateReferences()) return;

        titleText.text = "🎉 LEVEL COMPLETE! 🎉";
        titleText.color = Color.yellow;

        messageText.text = BuildWinMessage();

        SetButtonVisibility(showContinue: true, showRetry: false);
        ActivatePanel();
        StartCoroutine(CelebrationAnimation());
    }

    private string BuildWinMessage()
    {
        string goalsSummary = "";
        foreach (var goal in cachedGameState.Goals)
        {
            goalsSummary += $"✓ {goal.DisplayText}\n";
        }

        return $"<size=48>Score: {cachedGameState.Score}</size>\n\n{goalsSummary}\nMoves Remaining: {cachedGameState.MovesRemaining}";
    }

    private void ShowLose()
    {
        if (!ValidateReferences()) return;

        titleText.text = "LEVEL FAILED";
        titleText.color = Color.red;

        messageText.text = BuildLoseMessage();

        SetButtonVisibility(showContinue: false, showRetry: true);
        ActivatePanel();
    }

    private string BuildLoseMessage()
    {
        string incompleteGoals = "";
        foreach (var goal in cachedGameState.Goals)
        {
            if (!goal.IsComplete)
            {
                incompleteGoals += $"✗ {goal.DisplayText}\n";
            }
        }

        return $"Out of moves!\n\nIncomplete Goals:\n{incompleteGoals}\nFinal Score: {cachedGameState.Score}";
    }

    private bool ValidateReferences()
    {
        EnsureReferences();

        if (panel == null || titleText == null || messageText == null || cachedGameState == null)
        {
            Debug.LogError("GameOverPanel: Missing required references!");
            return false;
        }
        return true;
    }

    private void SetButtonVisibility(bool showContinue, bool showRetry)
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(showContinue);
        }
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(showRetry);
        }
    }

    private void ActivatePanel()
    {
        if (panel != null && !panel.activeSelf)
        {
            panel.SetActive(true);
        }
    }

    private System.Collections.IEnumerator CelebrationAnimation()
    {
        if (titleText == null) yield break;

        float elapsed = 0f;
        Vector3 originalScale = titleText.transform.localScale;

        while (elapsed < CELEBRATION_DURATION)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / CELEBRATION_DURATION * Mathf.PI) * CELEBRATION_SCALE;
            titleText.transform.localScale = originalScale * scale;
            yield return null;
        }

        titleText.transform.localScale = originalScale;
    }

    private void OnRetry()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        GameManager.Instance?.StartTestLevel();
    }

    private void OnContinue()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        GameManager.Instance?.StartTestLevel();
    }
}
