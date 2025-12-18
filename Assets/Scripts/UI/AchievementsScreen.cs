using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsScreen : MonoBehaviour
{
    private static AchievementsScreen instance;

    private CanvasGroup overlayGroup;
    private RectTransform listContent;
    private ScrollRect scrollRect;
    private Button closeButton;

    public static AchievementsScreen EnsureExists()
    {
        if (instance != null) return instance;

        var go = new GameObject("AchievementsScreen");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AchievementsScreen>();
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
        Hide();
    }

    private void BuildUI()
    {
        var canvasGo = new GameObject("AchievementsCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Fullscreen overlay
        var overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(canvasGo.transform, false);

        var overlayRt = overlayGo.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        var overlayImage = overlayGo.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.75f);

        overlayGroup = overlayGo.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;

        // Panel
        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(overlayGo.transform, false);
        var panelRt = panelGo.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(900f, 1400f);
        panelRt.anchoredPosition = Vector2.zero;

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.95f);

        // Title
        var title = CreateText(panelRt, "Title", "Achievements", 52, bold: true);
        var titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -26f);
        titleRt.sizeDelta = new Vector2(-80f, 80f);
        title.alignment = TextAlignmentOptions.Left;

        // Close button
        closeButton = CreateButton(panelRt, "CloseButton", "Close");
        var closeRt = closeButton.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1f, 1f);
        closeRt.anchorMax = new Vector2(1f, 1f);
        closeRt.pivot = new Vector2(1f, 1f);
        closeRt.anchoredPosition = new Vector2(-24f, -24f);
        closeRt.sizeDelta = new Vector2(180f, 60f);
        closeButton.onClick.AddListener(Hide);

        // Scroll view
        var scrollGo = new GameObject("ScrollView");
        scrollGo.transform.SetParent(panelRt, false);

        var scrollRt = scrollGo.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(24f, 24f);
        scrollRt.offsetMax = new Vector2(-24f, -130f);

        var scrollImage = scrollGo.AddComponent<Image>();
        scrollImage.color = new Color(0f, 0f, 0f, 0.15f);

        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scrollRect = scroll;

        var viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollRt, false);
        var viewportRt = viewportGo.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;

        viewportGo.AddComponent<RectMask2D>();
        var viewportImage = viewportGo.AddComponent<Image>();
        viewportImage.raycastTarget = false;
        viewportImage.color = new Color(0f, 0f, 0f, 0f);

        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportRt, false);
        listContent = contentGo.AddComponent<RectTransform>();
        listContent.anchorMin = new Vector2(0f, 1f);
        listContent.anchorMax = new Vector2(1f, 1f);
        listContent.pivot = new Vector2(0.5f, 1f);
        listContent.anchoredPosition = Vector2.zero;
        listContent.sizeDelta = new Vector2(0f, 0f);

        var layout = contentGo.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 12f;
        layout.padding = new RectOffset(12, 12, 12, 12);

        contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRt;
        scroll.content = listContent;
    }

    public void Show()
    {
        Refresh();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
        overlayGroup.alpha = 1f;
        overlayGroup.interactable = true;
        overlayGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;
    }

    private void Refresh()
    {
        if (listContent == null) return;

        foreach (Transform child in listContent)
        {
            Destroy(child.gameObject);
        }

        var profile = ProgressionService.Profile;
        // Ensure entries exist
        _ = new AchievementsService(profile);

        var progressById = (profile.achievements ?? new List<AchievementProgress>())
            .ToDictionary(a => a.id, a => a);

        foreach (var def in AchievementsCatalog.All)
        {
            progressById.TryGetValue(def.id, out var progress);
            bool unlocked = progress != null && progress.unlocked;
            int current = progress != null ? progress.current : 0;
            int target = def.targetValue;

            CreateRow(def, current, target, unlocked);
        }

        if (scrollRect != null)
        {
            scrollRect.content = listContent;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(listContent);
        if (listContent.parent is RectTransform parentRt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
        }
        listContent.anchoredPosition = Vector2.zero;

#if UNITY_EDITOR
        if (listContent.childCount > 0 && listContent.rect.height <= 1f)
        {
            Debug.LogWarning($"[AchievementsScreen] List content collapsed (children={listContent.childCount}, height={listContent.rect.height}).");
        }
#endif
    }

    private void CreateRow(AchievementDefinition def, int current, int target, bool unlocked)
    {
        var rowGo = new GameObject(def.id);
        rowGo.transform.SetParent(listContent, false);

        var rowRt = rowGo.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0f, 1f);
        rowRt.anchorMax = new Vector2(1f, 1f);
        rowRt.pivot = new Vector2(0.5f, 1f);
        rowRt.sizeDelta = new Vector2(0f, 140f);

        var rowLayout = rowGo.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 140f;

        var bg = rowGo.AddComponent<Image>();
        bg.color = unlocked ? new Color(0.12f, 0.18f, 0.12f, 0.9f) : new Color(0.12f, 0.12f, 0.14f, 0.9f);

        var v = rowGo.AddComponent<VerticalLayoutGroup>();
        v.childAlignment = TextAnchor.UpperLeft;
        v.childControlHeight = true;
        v.childControlWidth = true;
        v.childForceExpandHeight = false;
        v.childForceExpandWidth = true;
        v.spacing = 4f;
        v.padding = new RectOffset(14, 14, 10, 10);

        string title = unlocked ? $"✓ {def.name}" : def.name;
        var titleText = CreateText(rowRt, "Name", title, 34, bold: true);
        titleText.alignment = TextAlignmentOptions.Left;

        var descText = CreateText(rowRt, "Desc", def.description, 26, bold: false);
        descText.alignment = TextAlignmentOptions.Left;
        descText.alpha = 0.9f;

        string progress = unlocked ? "Unlocked" : $"{current}/{Mathf.Max(1, target)}";
        string reward = def.rewardCoins > 0 ? $"+{def.rewardCoins} coins" : "";
        var metaText = CreateText(rowRt, "Meta", $"{progress}  {reward}".Trim(), 24, bold: false);
        metaText.alignment = TextAlignmentOptions.Left;
        metaText.alpha = 0.85f;

        // Progress bar
        float ratio = target <= 0 ? 0f : Mathf.Clamp01((float)current / target);
        var barGo = new GameObject("ProgressBar");
        barGo.transform.SetParent(rowRt, false);
        var barRt = barGo.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0f, 1f);
        barRt.anchorMax = new Vector2(1f, 1f);
        barRt.pivot = new Vector2(0.5f, 1f);
        barRt.sizeDelta = new Vector2(0f, 14f);

        var barLayout = barGo.AddComponent<LayoutElement>();
        barLayout.preferredHeight = 14f;
        barLayout.minHeight = 14f;

        var barBg = barGo.AddComponent<Image>();
        barBg.color = new Color(0f, 0f, 0f, 0.25f);

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(barRt, false);
        var fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(ratio, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = unlocked ? new Color(0.35f, 0.85f, 0.45f, 0.9f) : new Color(0.35f, 0.65f, 1f, 0.75f);
    }

    private static Button CreateButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 60f);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.2f, 0.28f, 0.95f);

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = img.color;
        colors.highlightedColor = new Color(0.22f, 0.3f, 0.4f, 1f);
        colors.pressedColor = new Color(0.12f, 0.16f, 0.22f, 1f);
        colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
        button.colors = colors;

        var text = CreateText(rt, "Label", label, 28, bold: true);
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
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, fontSize + 16f);

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = fontSize + 16f;
        layout.minHeight = fontSize + 12f;
        return text;
    }
}
