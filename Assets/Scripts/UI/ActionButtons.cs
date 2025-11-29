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
    [SerializeField] private Button drawDiscardButton;
    [SerializeField] private Button playPatternButton;
    [SerializeField] private TextMeshProUGUI discardTopCardText;

    private readonly PatternValidator patternValidator = new();
    private GameState cachedGameState;
    private bool inputLocked;

    private void Start()
    {
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
        drawDiscardButton.onClick.AddListener(OnDrawDiscard);
        playPatternButton.onClick.AddListener(OnPlayPattern);
    }

    private void ApplyHoverEffects()
    {
        StyleButton(drawStockButton);
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
        var selectedIndices = HandDisplay.Instance?.GetSelectedIndices();

        // If cards are selected, treat Draw Stock as a discard-then-refill action
        if (selectedIndices != null && selectedIndices.Count > 0)
        {
            GameManager.Instance.DiscardCards(selectedIndices);
            HandDisplay.Instance.ClearSelection();
        }
        else
        {
            GameManager.Instance.DrawCard(fromDiscard: false);
        }

        UpdateButtonStates();
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
        bool hasSelection = selectedIndices.Count > 0;

        if (inputLocked)
        {
            drawStockButton.interactable = false;
            drawDiscardButton.interactable = false;
            playPatternButton.interactable = false;
            return;
        }

        bool mustDiscard = cachedGameState.MustDiscard;

        UpdateDrawButtons(mustDiscard, hasSelection);
        UpdateDiscardPileDisplay();
        UpdatePlayPatternButton(selectedIndices);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputLocked = !enabled;
        UpdateButtonStates();
    }

    private void UpdateDrawButtons(bool mustDiscard, bool hasSelection)
    {
        bool canDraw = cachedGameState.CanDraw();
        bool stockAvailable = cachedGameState.Deck.DrawPileCount > 0;
        bool discardAvailable = cachedGameState.Deck.DiscardPileCount > 0;

        // Allow draw button when must-discard if there is a selection to discard first.
        drawStockButton.interactable =
            ((!mustDiscard && canDraw && stockAvailable) ||
             (mustDiscard && hasSelection));

        drawDiscardButton.interactable = (!mustDiscard && canDraw && discardAvailable);
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
        var hasPattern = patternValidator.DetectPatterns(selectedCards).Count > 0;
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
}
