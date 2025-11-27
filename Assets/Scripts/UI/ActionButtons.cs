using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionButtons : MonoBehaviour
{
    [SerializeField] private Button drawStockButton;
    [SerializeField] private Button drawDiscardButton;
    [SerializeField] private Button playPatternButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private TextMeshProUGUI discardTopCardText;

    private void Start()
    {
        drawStockButton.onClick.AddListener(OnDrawStock);
        drawDiscardButton.onClick.AddListener(OnDrawDiscard);
        playPatternButton.onClick.AddListener(OnPlayPattern);
        discardButton.onClick.AddListener(OnDiscard);

        // Add hover effects
        AddHoverEffect(drawStockButton);
        AddHoverEffect(drawDiscardButton);
        AddHoverEffect(playPatternButton);
        AddHoverEffect(discardButton);

        if (GameManager.Instance?.CurrentGame != null)
        {
            GameManager.Instance.CurrentGame.OnHandChanged += UpdateButtonStates;
            UpdateButtonStates();
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
        var game = GameManager.Instance?.CurrentGame;
        if (game == null) return;
        
        bool mustDiscard = game.MustDiscard;
        
        // Draw buttons - disabled if must discard
        drawStockButton.interactable = !mustDiscard && game.CanDraw() && game.Deck.DrawPileCount > 0;
        drawDiscardButton.interactable = !mustDiscard && game.CanDraw() && game.Deck.DiscardPileCount > 0;

        // Update discard pile display
        if (game.Deck.TopDiscard.HasValue)
        {
            discardTopCardText.text = game.Deck.TopDiscard.Value.Display;
        }
        else
        {
            discardTopCardText.text = "Empty";
        }

        // Play pattern button - can play patterns even if must discard
        var selectedIndices = HandDisplay.Instance?.GetSelectedIndices();
        if (selectedIndices == null || selectedIndices.Count == 0)
        {
            playPatternButton.interactable = false;
        }
        else
        {
            var selectedCards = game.GetSelectedCards(selectedIndices);
            var validator = new PatternValidator();
            var hasPattern = validator.DetectPatterns(selectedCards).Count > 0;
            playPatternButton.interactable = hasPattern;
        }

        // Discard button - enabled when at least one card selected
        bool hasOneSelected = selectedIndices != null && selectedIndices.Count == 1;
        discardButton.interactable = hasOneSelected && game.Hand.Count >= 1;
        
        // Highlight discard button if must discard
        if (mustDiscard)
        {
            var colors = discardButton.colors;
            colors.normalColor = new Color(1f, 0.5f, 0.5f); // Reddish to indicate required
            discardButton.colors = colors;
        }
        else
        {
            var colors = discardButton.colors;
            colors.normalColor = Color.white;
            discardButton.colors = colors;
        }
    }

    private void AddHoverEffect(Button button)
    {
        var colors = button.colors;
        colors.highlightedColor = new Color(0.8f, 0.8f, 1f); // Light blue
        colors.pressedColor = new Color(0.6f, 0.6f, 0.8f); // Darker blue
        colors.selectedColor = new Color(0.7f, 0.7f, 0.9f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Grayed out
        button.colors = colors;
    }
}