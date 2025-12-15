using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementToast : MonoBehaviour
{
    private class Toast
    {
        public string title;
        public string body;
    }

    private static AchievementToast instance;

    [SerializeField] private float durationSeconds = 2.5f;
    [SerializeField] private float fadeSeconds = 0.25f;
    [SerializeField] private float slidePixels = 20f;

    private readonly Queue<Toast> queue = new();
    private bool isShowing;

    private CanvasGroup canvasGroup;
    private RectTransform panel;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    public static AchievementToast EnsureExists()
    {
        if (instance != null) return instance;

        var go = new GameObject("AchievementToast");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AchievementToast>();
        return instance;
    }

    public static void Show(string title, string body)
    {
        EnsureExists().Enqueue(title, body);
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
    }

    private void Enqueue(string title, string body)
    {
        queue.Enqueue(new Toast { title = title, body = body });
        if (!isShowing)
        {
            StartCoroutine(ShowNext());
        }
    }

    private IEnumerator ShowNext()
    {
        isShowing = true;

        while (queue.Count > 0)
        {
            var toast = queue.Dequeue();
            titleText.text = toast.title ?? string.Empty;
            bodyText.text = toast.body ?? string.Empty;

            yield return Animate(show: true);
            yield return new WaitForSecondsRealtime(durationSeconds);
            yield return Animate(show: false);
        }

        isShowing = false;
    }

    private IEnumerator Animate(bool show)
    {
        float elapsed = 0f;

        float fromAlpha = show ? 0f : 1f;
        float toAlpha = show ? 1f : 0f;

        Vector2 fromPos = show ? hiddenPosition : shownPosition;
        Vector2 toPos = show ? shownPosition : hiddenPosition;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            panel.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
        panel.anchoredPosition = toPos;
    }

    private void BuildUI()
    {
        if (panel != null) return;

        var canvasGo = new GameObject("ToastCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("ToastPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        panel = panelGo.AddComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 1f);
        panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.sizeDelta = new Vector2(680f, 120f);

        // Move the toast down to avoid notches / top HUD overlaps.
        shownPosition = new Vector2(0f, -120f);
        hiddenPosition = new Vector2(0f, -120f + slidePixels);
        panel.anchoredPosition = hiddenPosition;

        var bg = panelGo.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        bg.raycastTarget = false;

        canvasGroup = panelGo.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        titleText = CreateText(panel, "Title", fontSize: 34, bold: true);
        titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        titleText.rectTransform.pivot = new Vector2(0.5f, 1f);
        titleText.rectTransform.anchoredPosition = new Vector2(0f, -14f);
        titleText.rectTransform.sizeDelta = new Vector2(-40f, 44f);

        bodyText = CreateText(panel, "Body", fontSize: 28, bold: false);
        bodyText.rectTransform.anchorMin = new Vector2(0f, 1f);
        bodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        bodyText.rectTransform.pivot = new Vector2(0.5f, 1f);
        bodyText.rectTransform.anchoredPosition = new Vector2(0f, -62f);
        bodyText.rectTransform.sizeDelta = new Vector2(-40f, 44f);
    }

    private static TextMeshProUGUI CreateText(RectTransform parent, string name, float fontSize, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.raycastTarget = false;
        text.fontSize = fontSize;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;

        var rt = text.rectTransform;
        rt.offsetMin = new Vector2(20f, 0f);
        rt.offsetMax = new Vector2(-20f, 0f);

        return text;
    }
}
