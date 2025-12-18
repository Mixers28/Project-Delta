using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : MonoBehaviour
{
    private static MainMenuScreen instance;

    private CanvasGroup overlayGroup;
    private CanvasGroup quitConfirmGroup;
    private Button openButton;
    private Button resumeButton;
    private Button achievementsButton;
    private Button restartRunButton;
    private Button restartGameButton;
    private Button quitButton;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI quitConfirmBodyText;
    private Button quitConfirmQuitButton;
    private Button quitConfirmCancelButton;

    private float previousTimeScale = 1f;

    public static MainMenuScreen EnsureExists()
    {
        if (instance != null) return instance;

        var go = new GameObject("MainMenuScreen");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MainMenuScreen>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        Hide(immediate: true);
    }

    private void BuildUI()
    {
        if (overlayGroup != null) return;

        var canvasGo = new GameObject("MainMenuCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Keep below boot overlays (Startup/PostTutorial) but above gameplay HUD.
        canvas.sortingOrder = 840;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        openButton = CreateButton(canvasGo.transform, "OpenMainMenuButton", "Menu");
        var openRt = openButton.GetComponent<RectTransform>();
        openRt.anchorMin = new Vector2(1f, 1f);
        openRt.anchorMax = new Vector2(1f, 1f);
        openRt.pivot = new Vector2(1f, 1f);
        openRt.anchoredPosition = new Vector2(-18f, -18f);
        openRt.sizeDelta = new Vector2(200f, 70f);
        openButton.onClick.AddListener(Show);

        var overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(canvasGo.transform, false);
        var overlayRt = overlayGo.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        var overlayImage = overlayGo.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.78f);

        overlayGroup = overlayGo.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;

        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(overlayRt, false);
        var panelRt = panelGo.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(900f, 980f);
        panelRt.anchoredPosition = Vector2.zero;

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.96f);

        var title = CreateText(panelRt, "Title", "Main Menu", 64, bold: true);
        title.alignment = TextAlignmentOptions.Center;
        var titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -40f);
        titleRt.sizeDelta = new Vector2(860f, 90f);

        statusText = CreateText(panelRt, "Status", "", 30, bold: false);
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.alpha = 0.9f;
        var statusRt = statusText.rectTransform;
        statusRt.anchorMin = new Vector2(0.5f, 1f);
        statusRt.anchorMax = new Vector2(0.5f, 1f);
        statusRt.pivot = new Vector2(0.5f, 1f);
        statusRt.anchoredPosition = new Vector2(0f, -150f);
        statusRt.sizeDelta = new Vector2(860f, 160f);

        resumeButton = CreateButton(panelRt, "ResumeButton", "Resume");
        achievementsButton = CreateButton(panelRt, "AchievementsButton", "Achievements");
        restartRunButton = CreateButton(panelRt, "RestartRunButton", "Restart Run");
        restartGameButton = CreateButton(panelRt, "RestartGameButton", "Restart Game (Tutorial)");
        quitButton = CreateButton(panelRt, "QuitButton", "Quit (Save)");

        PositionMenuButton(resumeButton, y: -340f);
        PositionMenuButton(achievementsButton, y: -430f);
        PositionMenuButton(restartRunButton, y: -520f);
        PositionMenuButton(restartGameButton, y: -610f);
        PositionMenuButton(quitButton, y: -720f);

        resumeButton.onClick.AddListener(Hide);
        achievementsButton.onClick.AddListener(OnAchievements);
        restartRunButton.onClick.AddListener(OnRestartRun);
        restartGameButton.onClick.AddListener(OnRestartGame);
        quitButton.onClick.AddListener(OnQuitRequested);

        BuildQuitConfirmDialog(overlayRt);
    }

    private void BuildQuitConfirmDialog(RectTransform overlayRoot)
    {
        var confirmRootGo = new GameObject("QuitConfirm");
        confirmRootGo.transform.SetParent(overlayRoot, false);

        var confirmRootRt = confirmRootGo.AddComponent<RectTransform>();
        confirmRootRt.anchorMin = Vector2.zero;
        confirmRootRt.anchorMax = Vector2.one;
        confirmRootRt.offsetMin = Vector2.zero;
        confirmRootRt.offsetMax = Vector2.zero;

        var dim = confirmRootGo.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.35f);

        quitConfirmGroup = confirmRootGo.AddComponent<CanvasGroup>();
        quitConfirmGroup.alpha = 0f;
        quitConfirmGroup.interactable = false;
        quitConfirmGroup.blocksRaycasts = false;

        var dialogGo = new GameObject("Dialog");
        dialogGo.transform.SetParent(confirmRootRt, false);

        var dialogRt = dialogGo.AddComponent<RectTransform>();
        dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRt.pivot = new Vector2(0.5f, 0.5f);
        dialogRt.sizeDelta = new Vector2(860f, 520f);
        dialogRt.anchoredPosition = Vector2.zero;

        var dialogImage = dialogGo.AddComponent<Image>();
        dialogImage.color = new Color(0.1f, 0.12f, 0.15f, 0.98f);

        var title = CreateText(dialogRt, "Title", "Quit Game?", 54, bold: true);
        title.alignment = TextAlignmentOptions.Center;
        var titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -32f);
        titleRt.sizeDelta = new Vector2(820f, 90f);

        quitConfirmBodyText = CreateText(dialogRt, "Body", "", 32, bold: false);
        quitConfirmBodyText.alignment = TextAlignmentOptions.Center;
        quitConfirmBodyText.enableWordWrapping = true;
        var bodyRt = quitConfirmBodyText.rectTransform;
        bodyRt.anchorMin = new Vector2(0.5f, 1f);
        bodyRt.anchorMax = new Vector2(0.5f, 1f);
        bodyRt.pivot = new Vector2(0.5f, 1f);
        bodyRt.anchoredPosition = new Vector2(0f, -150f);
        bodyRt.sizeDelta = new Vector2(820f, 230f);

        quitConfirmCancelButton = CreateButton(dialogRt, "CancelButton", "Cancel");
        quitConfirmQuitButton = CreateButton(dialogRt, "QuitButton", "Quit");

        var cancelRt = quitConfirmCancelButton.GetComponent<RectTransform>();
        cancelRt.anchorMin = new Vector2(0.5f, 0f);
        cancelRt.anchorMax = new Vector2(0.5f, 0f);
        cancelRt.pivot = new Vector2(0.5f, 0f);
        cancelRt.anchoredPosition = new Vector2(-190f, 36f);
        cancelRt.sizeDelta = new Vector2(320f, 78f);

        var quitRt = quitConfirmQuitButton.GetComponent<RectTransform>();
        quitRt.anchorMin = new Vector2(0.5f, 0f);
        quitRt.anchorMax = new Vector2(0.5f, 0f);
        quitRt.pivot = new Vector2(0.5f, 0f);
        quitRt.anchoredPosition = new Vector2(190f, 36f);
        quitRt.sizeDelta = new Vector2(320f, 78f);

        quitConfirmCancelButton.onClick.AddListener(HideQuitConfirm);
        quitConfirmQuitButton.onClick.AddListener(OnQuitConfirmed);
    }

    private static void PositionMenuButton(Button button, float y)
    {
        if (button == null) return;
        var rt = button.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(640f, 74f);
    }

    public void Show()
    {
        Freeze();
        RefreshStatus();
        HideQuitConfirm();
        overlayGroup.alpha = 1f;
        overlayGroup.interactable = true;
        overlayGroup.blocksRaycasts = true;
    }

    public void SetMenuButtonVisible(bool visible)
    {
        if (openButton != null)
        {
            openButton.gameObject.SetActive(visible);
        }
    }

    public void Hide()
    {
        Hide(immediate: false);
    }

    private void Hide(bool immediate)
    {
        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;
        HideQuitConfirm();
        Unfreeze();

        if (immediate)
        {
            // no-op; kept for parity with other overlays
        }
    }

    private void RefreshStatus()
    {
        if (statusText == null) return;

        var profile = ProgressionService.Profile;
        bool tutorialActive = ProgressionService.IsTutorialActive;
        bool runActive = RunModeService.IsActive;

        string runLine = runActive
            ? $"Run: {profile.currentRunLength} wins | Score: {profile.currentRunScore}"
            : "Run: not active";

        statusText.text = tutorialActive
            ? $"Tutorial Step: {profile.TutorialStep}/{ProgressionService.TutorialMaxStep}\n{runLine}"
            : $"Non-tutorial wins: {profile.NonTutorialWins} | Tier: {ProgressionService.CurrentRuleTier}\n{runLine}";

        if (restartRunButton != null)
        {
            restartRunButton.interactable = !tutorialActive && RunModeService.CanStart();
        }
    }

    private void OnAchievements()
    {
        var screen = AchievementsScreen.EnsureExists();
        screen.Show();
        Hide();
    }

    private void OnRestartRun()
    {
        Hide();
        GameManager.Instance?.StartRunMode();
    }

    private void OnRestartGame()
    {
        Hide();
        StartCoroutine(RestartGameRoutine());
    }

    private IEnumerator RestartGameRoutine()
    {
        // Give UI a frame to close before reinitializing the world.
        yield return null;

        ProgressionService.ResetProfile();
        GameManager.Instance?.ReinitializeProfileBoundServices();

        // Show the intro flow again since we're resetting to "fresh install".
        StartupScreen.ShowOnBoot(() => GameManager.Instance?.StartTestLevel());
    }

    private void OnQuitRequested()
    {
        ShowQuitConfirm();
    }

    private void ShowQuitConfirm()
    {
        if (quitConfirmGroup == null || quitConfirmBodyText == null) return;

        bool runActive = RunModeService.IsActive;
        quitConfirmBodyText.text = runActive
            ? "Progress will be saved.\nYour current run will resume next time you launch the game."
            : "Progress will be saved.\nYou can continue next time you launch the game.";

        SetMenuButtonsInteractable(false);
        quitConfirmGroup.alpha = 1f;
        quitConfirmGroup.interactable = true;
        quitConfirmGroup.blocksRaycasts = true;
    }

    private void HideQuitConfirm()
    {
        if (quitConfirmGroup == null) return;
        quitConfirmGroup.alpha = 0f;
        quitConfirmGroup.interactable = false;
        quitConfirmGroup.blocksRaycasts = false;
        SetMenuButtonsInteractable(true);
    }

    private void SetMenuButtonsInteractable(bool interactable)
    {
        if (resumeButton != null) resumeButton.interactable = interactable;
        if (achievementsButton != null) achievementsButton.interactable = interactable;
        if (restartRunButton != null) restartRunButton.interactable = interactable;
        if (restartGameButton != null) restartGameButton.interactable = interactable;
        if (quitButton != null) quitButton.interactable = interactable;
    }

    private void OnQuitConfirmed()
    {
        ProgressionService.Save();

#if UNITY_EDITOR
        Debug.Log("[MainMenu] Quit requested (Unity Editor).");
#else
        Application.Quit();
#endif
    }

    private void Freeze()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private void Unfreeze()
    {
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
    }

    private static Button CreateButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320f, 80f);

        var image = go.AddComponent<Image>();
        image.color = new Color(0.22f, 0.27f, 0.34f, 1f);

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.32f, 0.37f, 0.44f, 1f);
        colors.pressedColor = new Color(0.18f, 0.22f, 0.28f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.22f, 0.27f, 0.34f, 0.45f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);

        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 34;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        var textRt = text.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return button;
    }

    private static TextMeshProUGUI CreateText(RectTransform parent, string name, string textValue, int size, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = size;
        text.color = Color.white;
        if (bold)
        {
            text.fontStyle = FontStyles.Bold;
        }

        var rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800f, 80f);
        rt.anchoredPosition = Vector2.zero;

        return text;
    }
}
