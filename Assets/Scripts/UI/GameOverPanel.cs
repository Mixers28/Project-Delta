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
        if (panel == null || titleText == null || messageText == null || retryButton == null || continueButton == null)
        {
            Debug.LogError("GameOverPanel: References not set in inspector. Please assign panel, titleText, messageText, retryButton, and continueButton.");
            return;
        }

        panel.SetActive(false);
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
        if (GameManager.Instance != null && GameManager.Instance.CurrentGame != null)
        {
            cachedGameState = GameManager.Instance.CurrentGame;
        }

        Debug.Log($"GameOverPanel.HandleGameEnd isWin={isWin}");

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
        if (panel == null || titleText == null || messageText == null || retryButton == null || continueButton == null || cachedGameState == null)
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
            // Make sure this overlay is on top of everything under the Canvas
            transform.SetAsLastSibling();

            panel.SetActive(true);
            panel.transform.SetAsLastSibling();

            // Ensure the root and panel stretch full screen
            var rootRt = GetComponent<RectTransform>();
            if (rootRt != null)
            {
                rootRt.anchorMin = Vector2.zero;
                rootRt.anchorMax = Vector2.one;
                rootRt.offsetMin = Vector2.zero;
                rootRt.offsetMax = Vector2.zero;
                rootRt.localScale = Vector3.one;
            }

            var rt = panel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
            }

            // Ensure any CanvasGroup is visible and blocks raycasts
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }

            // If the panel background is fully transparent, give it a default overlay
            var img = panel.GetComponent<Image>();
            if (img != null && img.color.a <= 0.01f)
            {
                img.color = new Color(0f, 0f, 0f, 0.7f);
            }

            // Make sure text is visible (no zero alpha)
            if (titleText != null)
            {
                var c = titleText.color;
                titleText.color = new Color(c.r, c.g, c.b, 1f);
            }
            if (messageText != null)
            {
                var c = messageText.color;
                messageText.color = new Color(c.r, c.g, c.b, 1f);
            }
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
