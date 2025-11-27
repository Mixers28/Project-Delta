using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionButtons : MonoBehaviour
{
    private static readonly Color HIGHLIGHT_COLOR = new(0.8f, 0.8f, 1f);
    private static readonly Color PRESSED_COLOR = new(0.6f, 0.6f, 0.8f);
    private static readonly Color SELECTED_COLOR = new(0.7f, 0.7f, 0.9f);
    private static readonly Color DISABLED_COLOR = new(0.5f, 0.5f, 0.5f, 0.5f);
    private static readonly Color DISCARD_WARNING_COLOR = new(1f, 0.5f, 0.5f);

    [SerializeField] private Button drawStockButton;
    [SerializeField] private Button drawDiscardButton;
    [SerializeField] private Button playPatternButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private TextMeshProUGUI discardTopCardText;

    private readonly PatternValidator patternValidator = new();
    private GameState cachedGameState;

    private void Start()
    {
        SetupButtonListeners();
        ApplyHoverEffects();
        StartCoroutine(InitializeButtons());
    }

    private void SetupButtonListeners()
    {
        drawStockButton.onClick.AddListener(OnDrawStock);
        drawDiscardButton.onClick.AddListener(OnDrawDiscard);
        playPatternButton.onClick.AddListener(OnPlayPattern);
        discardButton.onClick.AddListener(OnDiscard);
    }

    private void ApplyHoverEffects()
    {
        AddHoverEffect(drawStockButton);
        AddHoverEffect(drawDiscardButton);
        AddHoverEffect(playPatternButton);
        AddHoverEffect(discardButton);
    }

    private System.Collections.IEnumerator InitializeButtons()
    {
        while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
        {
            yield return null;
        }

        cachedGameState = GameManager.Instance.CurrentGame;
        cachedGameState.OnHandChanged += UpdateButtonStates;
        UpdateButtonStates();
    }

    private void OnDestroy()
    {
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

    private void OnDiscard()
    {
        var selectedIndices = HandDisplay.Instance.GetSelectedIndices();
        if (selectedIndices.Count == 1)
        {
            GameManager.Instance.DiscardCard(selectedIndices[0]);
            HandDisplay.Instance.ClearSelection();
            UpdateButtonStates();
        }
    }

    public void UpdateButtonStates()
    {
        if (cachedGameState == null) return;

        bool mustDiscard = cachedGameState.MustDiscard;

        UpdateDrawButtons(mustDiscard);
        UpdateDiscardPileDisplay();
        UpdatePlayPatternButton();
        UpdateDiscardButton(mustDiscard);
    }

    private void UpdateDrawButtons(bool mustDiscard)
    {
        drawStockButton.interactable = !mustDiscard && cachedGameState.CanDraw() && cachedGameState.Deck.DrawPileCount > 0;
        drawDiscardButton.interactable = !mustDiscard && cachedGameState.CanDraw() && cachedGameState.Deck.DiscardPileCount > 0;
    }

    private void UpdateDiscardPileDisplay()
    {
        if (discardTopCardText == null) return;

        discardTopCardText.text = cachedGameState.Deck.TopDiscard.HasValue
            ? cachedGameState.Deck.TopDiscard.Value.Display
            : "Empty";
    }

    private void UpdatePlayPatternButton()
    {
        var selectedIndices = HandDisplay.Instance?.GetSelectedIndices();
        if (selectedIndices == null || selectedIndices.Count == 0)
        {
            playPatternButton.interactable = false;
            return;
        }

        var selectedCards = cachedGameState.GetSelectedCards(selectedIndices);
        var hasPattern = patternValidator.DetectPatterns(selectedCards).Count > 0;
        playPatternButton.interactable = hasPattern;
    }

    private void UpdateDiscardButton(bool mustDiscard)
    {
        var selectedIndices = HandDisplay.Instance?.GetSelectedIndices();
        bool hasOneSelected = selectedIndices != null && selectedIndices.Count == 1;
        discardButton.interactable = hasOneSelected && cachedGameState.Hand.Count >= 1;

        var colors = discardButton.colors;
        colors.normalColor = mustDiscard ? DISCARD_WARNING_COLOR : Color.white;
        discardButton.colors = colors;
    }

    private void AddHoverEffect(Button button)
    {
        var colors = button.colors;
        colors.highlightedColor = HIGHLIGHT_COLOR;
        colors.pressedColor = PRESSED_COLOR;
        colors.selectedColor = SELECTED_COLOR;
        colors.disabledColor = DISABLED_COLOR;
        button.colors = colors;
    }
}