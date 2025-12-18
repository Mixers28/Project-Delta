using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GameOverPanel : MonoBehaviour
{
    private const float CELEBRATION_DURATION = 0.5f;
    private const float CELEBRATION_SCALE = 0.3f;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;

    private GameState cachedGameState;
    private bool isConfigured;
    private bool lastIsWin;

    public void Configure()
    {
        if (isConfigured)
        {
            return;
        }

        ValidateSerializedField(panel, nameof(panel));
        ValidateSerializedField(titleText, nameof(titleText));
        ValidateSerializedField(messageText, nameof(messageText));
        ValidateSerializedField(retryButton, nameof(retryButton));
        ValidateSerializedField(continueButton, nameof(continueButton));

        EnsureStatsTextExists();

        retryButton.onClick.AddListener(OnRetry);
        continueButton.onClick.AddListener(OnContinue);

        panel.SetActive(false);

        isConfigured = true;
    }

    private void Awake()
    {
        try
        {
            Configure();
        }
        catch (MissingReferenceException ex)
        {
            Debug.LogError($"GameOverPanel: {ex.Message}");
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += HandleGameStarted;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= HandleGameStarted;
        }
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
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetry);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinue);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnd -= HandleGameEnd;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= HandleGameStarted;
        }
    }

    private void HandleGameEnd(bool isWin)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGame != null)
        {
            cachedGameState = GameManager.Instance.CurrentGame;
        }

        Debug.Log($"GameOverPanel.HandleGameEnd isWin={isWin}");
        lastIsWin = isWin;

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

        MainMenuScreen.EnsureExists().SetMenuButtonVisible(false);

        EnsureStatsTextExists();
        ApplyStatsTextLayout();

        titleText.text = "LEVEL COMPLETE!";
        titleText.color = Color.yellow;

        messageText.text = BuildGoalsSummary();
        if (statsText != null)
        {
            statsText.text = BuildWinStatsSummary();
        }

        // Ensure Continue label is restored if Run Mode changed it.
        if (continueButton != null)
        {
            var label = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = "Continue";
        }

        SetButtonVisibility(showContinue: true, showRetry: false);
        ActivatePanel();
        StartCoroutine(CelebrationAnimation());
    }

    private string BuildGoalsSummary()
    {
        string goalsSummary = "";
        foreach (var goal in cachedGameState.Goals)
        {
            goalsSummary += $"- {goal.DisplayText}\n";
        }

        return goalsSummary;
    }

    private string BuildWinStatsSummary()
    {
        string achievementsSummary = "";
        var gm = GameManager.Instance;
        if (gm != null && gm.Achievements != null)
        {
            var unlocked = gm.Achievements.TakeRecentlyUnlocked();
            if (unlocked != null && unlocked.Count > 0)
            {
                int coins = unlocked.Sum(a => a.rewardCoins);
                achievementsSummary = "\nAchievements Unlocked:\n" +
                                      string.Join("\n", unlocked.Select(a => $"- {a.name} (+{a.rewardCoins})")) +
                                      $"\nCoins Earned: +{coins}";
            }
        }

        return $"Score: {cachedGameState.Score}\nMoves: {cachedGameState.MovesRemaining}{achievementsSummary}";
    }

    private void ShowLose()
    {
        if (!ValidateReferences()) return;

        MainMenuScreen.EnsureExists().SetMenuButtonVisible(false);

        EnsureStatsTextExists();
        ApplyStatsTextLayout();

        bool isRunMode = GameManager.Instance != null && GameManager.Instance.LastEndWasRunMode;

        titleText.text = isRunMode ? "RUN ENDED" : "LEVEL FAILED";
        titleText.color = Color.red;

        messageText.text = BuildLoseMessage();
        if (statsText != null)
        {
            statsText.text = "";
        }

        // In run mode, "Continue" becomes "New Run".
        SetButtonVisibility(showContinue: isRunMode, showRetry: !isRunMode);
        if (continueButton != null && isRunMode)
        {
            var label = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = "New Run";
        }
        ActivatePanel();
    }

    private string BuildLoseMessage()
    {
        string incompleteGoals = "";
        foreach (var goal in cachedGameState.Goals)
        {
            if (!goal.IsComplete)
            {
                incompleteGoals += $"- {goal.DisplayText}\n";
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
        if (panel != null)
        {
            // Make sure this overlay is on top of everything under the Canvas
            transform.SetAsLastSibling();

            SetCanvasGroup(panel, true);
            SetCanvasGroup(gameObject, true);
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
            if (statsText != null)
            {
                var c = statsText.color;
                statsText.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
    }

    private void EnsureStatsTextExists()
    {
        if (statsText != null) return;
        if (panel == null || messageText == null) return;

        var go = new GameObject("StatsText");
        go.transform.SetParent(panel.transform, false);

        statsText = go.AddComponent<TextMeshProUGUI>();
        statsText.raycastTarget = false;
        statsText.font = messageText.font;
        statsText.fontSize = Mathf.Max(18, messageText.fontSize - 2);
        statsText.color = messageText.color;
        statsText.enableWordWrapping = true;
        statsText.alignment = TextAlignmentOptions.BottomLeft;

        ApplyStatsTextLayout();
    }

    private void ApplyStatsTextLayout()
    {
        if (statsText == null) return;

        statsText.alignment = TextAlignmentOptions.BottomLeft;

        var rt = statsText.rectTransform;
        // Place above the Continue button, roughly aligned with the goals list height.
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(24f, 220f);
        rt.sizeDelta = new Vector2(520f, 260f);
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
        StartCoroutine(RetryRoutine());
    }

    private void OnContinue()
    {
        StartCoroutine(ContinueRoutine());
    }

    private void HandleGameStarted(GameState newGame)
    {
        cachedGameState = newGame;
        MainMenuScreen.EnsureExists().SetMenuButtonVisible(true);
        HidePanel();
    }

    private System.Collections.IEnumerator RetryRoutine()
    {
        HidePanel();
        cachedGameState = null; // ensure we don't reuse the ended game's state

        var gm = GameManager.Instance;
        var previousGame = gm != null ? gm.CurrentGame : null;

        gm?.RestartCurrentLevel();

        // Wait a few frames for GameManager to spin up a new game instance
        yield return WaitForNewGame(previousGame);
        HidePanel();
    }

    private System.Collections.IEnumerator ContinueRoutine()
    {
        HidePanel();
        cachedGameState = null; // ensure we don't reuse the ended game's state

        var gm = GameManager.Instance;
        var previousGame = gm != null ? gm.CurrentGame : null;

        if (gm != null && gm.LastEndWasRunMode && !lastIsWin)
        {
            gm.StartRunMode();
        }
        else
        {
            gm?.StartNextLevel();
        }

        // Wait a few frames for GameManager to spin up a new game instance
        yield return WaitForNewGame(previousGame);
        HidePanel();
    }

    private System.Collections.IEnumerator WaitForNewGame(GameState previousGame)
    {
        var gm = GameManager.Instance;

        // Wait up to a handful of frames for a new GameState reference
        const int maxFrames = 10;
        for (int i = 0; i < maxFrames; i++)
        {
            if (gm != null && gm.CurrentGame != null && gm.CurrentGame != previousGame)
            {
                cachedGameState = gm.CurrentGame;
                yield break;
            }
            yield return null;
        }

        // Fallback: if we still don't have a new game, attempt a direct restart
        if (gm != null)
        {
            gm.StartTestLevel();

            for (int i = 0; i < maxFrames; i++)
            {
                if (gm.CurrentGame != null && gm.CurrentGame != previousGame)
                {
                    cachedGameState = gm.CurrentGame;
                    yield break;
                }
                yield return null;
            }
        }
    }

    private void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            SetCanvasGroup(panel, false);
        }
        SetCanvasGroup(gameObject, false);
    }

    private void SetCanvasGroup(GameObject go, bool visible)
    {
        if (go == null) return;

        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = go.AddComponent<CanvasGroup>();
        }

        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    private static void ValidateSerializedField(Object field, string fieldName)
    {
        if (field == null)
        {
            throw new MissingReferenceException($"GameOverPanel requires '{fieldName}' to be assigned in the inspector.");
        }
    }
}
