using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ActionButtons : MonoBehaviour
{
    private static readonly Color HIGHLIGHT_COLOR = new(0.8f, 0.8f, 1f);
    private static readonly Color PRESSED_COLOR = new(0.6f, 0.6f, 0.8f);
    private static readonly Color SELECTED_COLOR = new(0.7f, 0.7f, 0.9f);
    private static readonly Color DISABLED_COLOR = new(0.5f, 0.5f, 0.5f, 0.5f);
    private static readonly Color DISCARD_WARNING_COLOR = new(1f, 0.5f, 0.5f);
    private static readonly Color NORMAL_COLOR = new(0.22f, 0.27f, 0.34f);
    private static readonly Color TEXT_COLOR = Color.white;

    [SerializeField] private Button drawStockButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button drawDiscardButton;
    [SerializeField] private Button playPatternButton;
    [SerializeField] private TextMeshProUGUI discardTopCardText;

    private GameState cachedGameState;
    private bool inputLocked;

    private void Start()
    {
        EnsureDiscardButtonExists();
        SetupButtonListeners();
        ApplyHoverEffects();
        StartCoroutine(InitializeButtons());
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += HandleGameStarted;
        }
    }

    private void SetupButtonListeners()
    {
        drawStockButton.onClick.AddListener(OnDrawStock);
        if (discardButton != null)
        {
            discardButton.onClick.AddListener(OnDiscardSelected);
        }
        drawDiscardButton.onClick.AddListener(OnDrawDiscard);
        playPatternButton.onClick.AddListener(OnPlayPattern);
    }

    private void ApplyHoverEffects()
    {
        StyleButton(drawStockButton);
        StyleButton(discardButton);
        StyleButton(drawDiscardButton);
        StyleButton(playPatternButton);
    }

    private System.Collections.IEnumerator InitializeButtons()
    {
        while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
        {
            yield return null;
        }

        // Subscribe once the GameManager exists (covers the case OnEnable ran before Awake)
        GameManager.Instance.OnGameStarted -= HandleGameStarted;
        GameManager.Instance.OnGameStarted += HandleGameStarted;

        HandleGameStarted(GameManager.Instance.CurrentGame);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= HandleGameStarted;
        }

        if (cachedGameState != null)
        {
            cachedGameState.OnHandChanged -= UpdateButtonStates;
        }
    }

    private void OnDrawStock()
    {
        GameManager.Instance.DrawCard(fromDiscard: false);
        UpdateButtonStates();
    }

    private void OnDiscardSelected()
    {
        var selectedIndices = HandDisplay.Instance?.GetSelectedIndices();
        if (selectedIndices == null || selectedIndices.Count == 0) return;

        GameManager.Instance.DiscardCards(selectedIndices);
        HandDisplay.Instance.ClearSelection();
        UpdateButtonStates();
    }

    // Back-compat for old scene button wiring.
    private void OnDiscard()
    {
        OnDiscardSelected();
    }

    private void OnDrawDiscard()
    {
        GameManager.Instance.DrawCard(fromDiscard: true);
        UpdateButtonStates();
    }

    private void OnPlayPattern()
    {
        var selectedIndices = HandDisplay.Instance.GetSelectedIndices();
        GameManager.Instance.TryPlayPattern(selectedIndices);
        HandDisplay.Instance.ClearSelection();
        UpdateButtonStates();
    }

    public void UpdateButtonStates()
    {
        if (cachedGameState == null) return;

        var selectedIndices = HandDisplay.Instance?.GetSelectedIndices() ?? new System.Collections.Generic.List<int>();
        int selectedCount = selectedIndices.Count;
        bool hasSelection = selectedCount > 0;

        if (inputLocked)
        {
            drawStockButton.interactable = false;
            if (discardButton != null) discardButton.interactable = false;
            drawDiscardButton.interactable = false;
            playPatternButton.interactable = false;
            return;
        }

        bool mustDiscard = cachedGameState.MustDiscard;

        UpdateDrawButtons(mustDiscard);
        UpdateDiscardButton(hasSelection);
        UpdateDiscardPileDisplay();
        UpdatePlayPatternButton(selectedIndices);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputLocked = !enabled;
        UpdateButtonStates();
    }

    private void UpdateDrawButtons(bool mustDiscard)
    {
        bool canDraw = cachedGameState.CanDraw();
        bool stockAvailable = cachedGameState.Deck.DrawPileCount > 0
            || cachedGameState.Deck.DiscardPileCount > 0;
        bool discardAvailable = cachedGameState.Deck.DiscardPileCount > 0;

        drawStockButton.interactable = (!mustDiscard && canDraw && stockAvailable);
        drawDiscardButton.interactable = (!mustDiscard && canDraw && discardAvailable);
    }

    private void UpdateDiscardButton(bool hasSelection)
    {
        if (discardButton == null) return;
        discardButton.interactable = hasSelection && cachedGameState.CanDiscard();
    }

    private void UpdateDiscardPileDisplay()
    {
        if (discardTopCardText == null) return;

        discardTopCardText.text = cachedGameState.Deck.TopDiscard.HasValue
            ? cachedGameState.Deck.TopDiscard.Value.Display
            : "Empty";
    }

    private void UpdatePlayPatternButton(System.Collections.Generic.List<int> selectedIndices)
    {
        if (selectedIndices == null || selectedIndices.Count == 0)
        {
            playPatternButton.interactable = false;
            return;
        }

        var selectedCards = cachedGameState.GetSelectedCards(selectedIndices);
        var validator = GameManager.Instance != null ? GameManager.Instance.PatternValidator : null;
        var hasPattern = validator != null && validator.DetectPatterns(selectedCards).Count > 0;
        playPatternButton.interactable = hasPattern && cachedGameState.MovesRemaining > 0;
    }

    private void AddHoverEffect(Button button)
    {
        var colors = button.colors;
        colors.normalColor = NORMAL_COLOR;
        colors.highlightedColor = HIGHLIGHT_COLOR;
        colors.pressedColor = PRESSED_COLOR;
        colors.selectedColor = SELECTED_COLOR;
        colors.disabledColor = DISABLED_COLOR;
        colors.colorMultiplier = 1f;
        button.colors = colors;
    }

    private void StyleButton(Button button)
    {
        if (button == null) return;

        AddHoverEffect(button);

        var text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = TEXT_COLOR;
        }

        var image = button.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = NORMAL_COLOR;
        }
    }

    private void HandleGameStarted(GameState newGame)
    {
        if (cachedGameState != null)
        {
            cachedGameState.OnHandChanged -= UpdateButtonStates;
        }

        cachedGameState = newGame;
        cachedGameState.OnHandChanged += UpdateButtonStates;
        inputLocked = false;
        UpdateButtonStates();
    }

    private void EnsureDiscardButtonExists()
    {
        if (discardButton != null) return;
        if (drawStockButton == null || drawDiscardButton == null) return;

        var parent = drawStockButton.transform.parent;
        if (parent == null) return;

        var clone = Instantiate(drawStockButton.gameObject, parent);
        clone.name = "DiscardButton";

        var rt = clone.GetComponent<RectTransform>();
        var stockRt = drawStockButton.GetComponent<RectTransform>();
        var discardRt = drawDiscardButton.GetComponent<RectTransform>();
        if (rt != null && stockRt != null && discardRt != null)
        {
            rt.anchorMin = stockRt.anchorMin;
            rt.anchorMax = stockRt.anchorMax;
            rt.pivot = stockRt.pivot;
            rt.sizeDelta = stockRt.sizeDelta;
            rt.localScale = stockRt.localScale;

            float midX = (stockRt.anchoredPosition.x + discardRt.anchoredPosition.x) / 2f;
            rt.anchoredPosition = new Vector2(midX, stockRt.anchoredPosition.y);
        }

        discardButton = clone.GetComponent<Button>();
        if (discardButton != null)
        {
            discardButton.onClick = new Button.ButtonClickedEvent();
        }

        var label = clone.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = "Discard";
        }
    }
}
