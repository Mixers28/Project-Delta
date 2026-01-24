using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartupScreen : MonoBehaviour
{
    private static StartupScreen instance;

    [SerializeField] private float splashSeconds = 2.2f;
    [SerializeField] private float fadeSeconds = 0.25f;
    [SerializeField] private float popSeconds = 0.2f;
    [Header("Polish")]
    [SerializeField] private string backgroundSpriteResource = "SplashBackGround";

    private CanvasGroup overlayGroup;
    private Canvas overlayCanvas;
    private RectTransform splashPanel;
    private RectTransform introPanel;
    private CanvasGroup splashGroup;
    private CanvasGroup introGroup;
    private Button startButton;

    private Action onStart;
    private float previousTimeScale = 1f;
    private Coroutine flowRoutine;
    private Coroutine transitionRoutine;

    private static Texture2D gradientTexture;

    public static StartupScreen EnsureExists()
    {
        if (instance != null) return instance;

        var go = new GameObject("StartupScreen");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<StartupScreen>();
        return instance;
    }

    public static void ShowOnBoot(Action onStart)
    {
        EnsureExists().Show(onStart);
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

    private void Show(Action onStartAction)
    {
        onStart = onStartAction;
        Freeze();

        if (overlayCanvas != null)
        {
            overlayCanvas.enabled = true;
        }

        overlayGroup.alpha = 0f;
        overlayGroup.interactable = true;
        overlayGroup.blocksRaycasts = true;

        splashPanel.gameObject.SetActive(true);
        introPanel.gameObject.SetActive(false);
        splashGroup.alpha = 1f;
        introGroup.alpha = 0f;
        splashPanel.localScale = Vector3.one * 0.98f;
        introPanel.localScale = Vector3.one * 0.98f;

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(FadeOverlay(show: true));

        if (flowRoutine != null) StopCoroutine(flowRoutine);
        flowRoutine = StartCoroutine(SplashThenIntro());
    }

    private IEnumerator SplashThenIntro()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, splashSeconds));

        if (ProgressionService.Profile.hasSeenIntro)
        {
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(FadeOutThenStart());
            yield break;
        }

        TransitionToIntro();
    }

    private void TransitionToIntro()
    {
        if (flowRoutine != null)
        {
            StopCoroutine(flowRoutine);
            flowRoutine = null;
        }

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionPanels(splashPanel, splashGroup, introPanel, introGroup));
    }

    private void Hide(bool immediate)
    {
        if (flowRoutine != null)
        {
            StopCoroutine(flowRoutine);
            flowRoutine = null;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;

        if (immediate)
        {
            splashPanel.gameObject.SetActive(false);
            introPanel.gameObject.SetActive(false);
        }

        if (immediate && overlayCanvas != null)
        {
            overlayCanvas.enabled = false;
        }
    }

    private void OnStartClicked()
    {
        if (startButton != null) startButton.interactable = false;

        var profile = ProgressionService.Profile;
        profile.hasSeenIntro = true;
        ProgressionService.Save();

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(FadeOutThenStart());
    }

    private IEnumerator FadeOutThenStart()
    {
        yield return FadeOverlay(show: false);
        Hide(immediate: true);
        Unfreeze();

        onStart?.Invoke();
        onStart = null;

        if (startButton != null) startButton.interactable = true;
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

    private void BuildUI()
    {
        if (overlayGroup != null) return;

        var canvasGo = new GameObject("StartupCanvas");
        canvasGo.transform.SetParent(transform, false);

        overlayCanvas = canvasGo.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 850;
        overlayCanvas.enabled = false;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(canvasGo.transform, false);
        var overlayRt = overlayGo.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        var overlayImage = overlayGo.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.85f);

        CreateBackground(
            overlayRt,
            spriteResource: backgroundSpriteResource,
            fallbackTop: new Color(0.07f, 0.42f, 0.46f, 1f),
            fallbackBottom: new Color(0.03f, 0.18f, 0.2f, 1f));

        overlayGroup = overlayGo.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;

        splashPanel = BuildSplash(overlayRt);
        introPanel = BuildIntro(overlayRt);
    }

    private RectTransform BuildSplash(RectTransform parent)
    {
        var panelGo = new GameObject("Splash");
        panelGo.transform.SetParent(parent, false);
        var rt = panelGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 900f);
        rt.anchoredPosition = Vector2.zero;

        var skipButton = CreateButton(rt, "SkipButton", "Skip");
        var skipRt = skipButton.GetComponent<RectTransform>();
        skipRt.anchorMin = new Vector2(0.5f, 0.1f);
        skipRt.anchorMax = new Vector2(0.5f, 0.1f);
        skipRt.pivot = new Vector2(0.5f, 0.5f);
        skipRt.anchoredPosition = Vector2.zero;
        skipRt.sizeDelta = new Vector2(240f, 70f);
        skipButton.onClick.AddListener(TransitionToIntro);

        splashGroup = panelGo.AddComponent<CanvasGroup>();
        return rt;
    }

    private RectTransform BuildIntro(RectTransform parent)
    {
        var panelGo = new GameObject("Intro");
        panelGo.transform.SetParent(parent, false);
        var rt = panelGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(960f, 1500f);
        rt.anchoredPosition = Vector2.zero;

        var bg = panelGo.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.1f, 0.12f, 0.95f);

        var title = CreateText(rt, "Title", "How to Play", 58, bold: true);
        title.alignment = TextAlignmentOptions.Center;
        var titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -40f);
        titleRt.sizeDelta = new Vector2(900f, 90f);

        string body =
            "Select cards to form patterns, then press Play Pattern.\n\n" +
            "Moves:\n" +
            "• Draw fills your hand to max for 1 move.\n" +
            "• Discard costs 1 move, no matter how many cards you discard.\n\n" +
            "Goals:\n" +
            "• Complete the listed goals before you run out of moves.\n\n" +
            "Tip:\n" +
            "• Early sessions are suit-agnostic — focus on learning runs and pairs.";

        var bodyText = CreateText(rt, "Body", body, 32, bold: false);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.enableWordWrapping = true;
        var bodyRt = bodyText.rectTransform;
        bodyRt.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRt.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRt.pivot = new Vector2(0.5f, 0.5f);
        bodyRt.anchoredPosition = new Vector2(0f, -40f);
        bodyRt.sizeDelta = new Vector2(860f, 980f);

        startButton = CreateButton(rt, "StartButton", "Start");
        var startRt = startButton.GetComponent<RectTransform>();
        startRt.anchorMin = new Vector2(0.5f, 0f);
        startRt.anchorMax = new Vector2(0.5f, 0f);
        startRt.pivot = new Vector2(0.5f, 0f);
        startRt.anchoredPosition = new Vector2(0f, 64f);
        startRt.sizeDelta = new Vector2(420f, 92f);
        startButton.onClick.AddListener(OnStartClicked);

        introGroup = panelGo.AddComponent<CanvasGroup>();
        return rt;
    }

    private IEnumerator TransitionPanels(RectTransform fromPanel, CanvasGroup fromGroup, RectTransform toPanel, CanvasGroup toGroup)
    {
        if (fromPanel != null) fromPanel.gameObject.SetActive(true);
        if (toPanel != null) toPanel.gameObject.SetActive(true);

        float elapsed = 0f;
        float seconds = Mathf.Max(0.01f, fadeSeconds);

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            if (fromGroup != null) fromGroup.alpha = 1f - t;
            if (toGroup != null) toGroup.alpha = t;

            float popT = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, popSeconds));
            if (fromPanel != null) fromPanel.localScale = Vector3.one * Mathf.Lerp(1f, 0.98f, popT);
            if (toPanel != null) toPanel.localScale = Vector3.one * Mathf.Lerp(0.98f, 1f, popT);

            yield return null;
        }

        if (fromGroup != null) fromGroup.alpha = 0f;
        if (toGroup != null) toGroup.alpha = 1f;
        if (fromPanel != null) fromPanel.gameObject.SetActive(false);
        if (toPanel != null) toPanel.gameObject.SetActive(true);
    }

    private IEnumerator FadeOverlay(bool show)
    {
        float from = show ? 0f : 1f;
        float to = show ? 1f : 0f;
        float elapsed = 0f;
        float seconds = Mathf.Max(0.01f, fadeSeconds);

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            overlayGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        overlayGroup.alpha = to;
    }

    private static void CreateBackground(RectTransform overlayRoot, string spriteResource, Color fallbackTop, Color fallbackBottom)
    {
        var backgroundGo = new GameObject("Background");
        backgroundGo.transform.SetParent(overlayRoot, false);
        var bgRt = backgroundGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgRt.SetSiblingIndex(0);

        var sprite = !string.IsNullOrWhiteSpace(spriteResource) ? Resources.Load<Sprite>(spriteResource) : null;
        if (sprite != null)
        {
            // Two-layer approach:
            // - Backdrop fills the screen (may crop) so there are no gaps.
            // - Main keeps full image visible (FitInParent) so nothing important is cropped.
            float aspect = sprite.rect.width / Mathf.Max(1f, sprite.rect.height);

            var backdropGo = new GameObject("Backdrop");
            backdropGo.transform.SetParent(bgRt, false);
            var backdropRt = backdropGo.AddComponent<RectTransform>();
            backdropRt.anchorMin = Vector2.zero;
            backdropRt.anchorMax = Vector2.one;
            backdropRt.offsetMin = Vector2.zero;
            backdropRt.offsetMax = Vector2.zero;

            var backdrop = backdropGo.AddComponent<Image>();
            backdrop.raycastTarget = false;
            backdrop.sprite = sprite;
            backdrop.preserveAspect = true;
            backdrop.color = new Color(1f, 1f, 1f, 0.65f);

            var backdropFitter = backdropGo.AddComponent<AspectRatioFitter>();
            backdropFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            backdropFitter.aspectRatio = aspect;

            var mainGo = new GameObject("Main");
            mainGo.transform.SetParent(bgRt, false);
            var mainRt = mainGo.AddComponent<RectTransform>();
            mainRt.anchorMin = Vector2.zero;
            mainRt.anchorMax = Vector2.one;
            mainRt.offsetMin = Vector2.zero;
            mainRt.offsetMax = Vector2.zero;

            var main = mainGo.AddComponent<Image>();
            main.raycastTarget = false;
            main.sprite = sprite;
            main.preserveAspect = true;
            main.color = Color.white;

            var mainFitter = mainGo.AddComponent<AspectRatioFitter>();
            mainFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            mainFitter.aspectRatio = aspect;

            var dimmerGo = new GameObject("Dimmer");
            dimmerGo.transform.SetParent(bgRt, false);
            var dimmerRt = dimmerGo.AddComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;

            var dimmer = dimmerGo.AddComponent<Image>();
            dimmer.raycastTarget = false;
            dimmer.color = new Color(0f, 0f, 0f, 0.25f);
            return;
        }

        var raw = backgroundGo.AddComponent<RawImage>();
        raw.raycastTarget = false;
        raw.texture = GetGradientTexture(fallbackTop, fallbackBottom);
        raw.color = new Color(1f, 1f, 1f, 0.95f);

        var fallbackDimmerGo = new GameObject("Dimmer");
        fallbackDimmerGo.transform.SetParent(bgRt, false);
        var fallbackDimmerRt = fallbackDimmerGo.AddComponent<RectTransform>();
        fallbackDimmerRt.anchorMin = Vector2.zero;
        fallbackDimmerRt.anchorMax = Vector2.one;
        fallbackDimmerRt.offsetMin = Vector2.zero;
        fallbackDimmerRt.offsetMax = Vector2.zero;

        var fallbackDimmer = fallbackDimmerGo.AddComponent<Image>();
        fallbackDimmer.raycastTarget = false;
        fallbackDimmer.color = new Color(0f, 0f, 0f, 0.25f);
    }

    private static Texture2D GetGradientTexture(Color top, Color bottom)
    {
        if (gradientTexture != null) return gradientTexture;

        gradientTexture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        gradientTexture.filterMode = FilterMode.Bilinear;

        gradientTexture.SetPixel(0, 1, top);
        gradientTexture.SetPixel(1, 1, top);
        gradientTexture.SetPixel(0, 0, bottom);
        gradientTexture.SetPixel(1, 0, bottom);
        gradientTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return gradientTexture;
    }

    private static Button CreateButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220f, 70f);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.2f, 0.28f, 0.95f);

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = img.color;
        colors.highlightedColor = new Color(0.22f, 0.3f, 0.4f, 1f);
        colors.pressedColor = new Color(0.12f, 0.16f, 0.22f, 1f);
        colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
        button.colors = colors;

        var text = CreateText(rt, "Label", label, 30, bold: true);
        text.alignment = TextAlignmentOptions.Center;
        var textRt = text.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return button;
    }

    private static TextMeshProUGUI CreateText(RectTransform parent, string name, string textValue, float fontSize, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.raycastTarget = false;
        text.text = textValue ?? string.Empty;
        text.fontSize = fontSize;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.color = Color.white;
        text.enableWordWrapping = true;

        var rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, fontSize + 20f);

        return text;
    }
}
