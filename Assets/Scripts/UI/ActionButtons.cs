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

        if (GameManager.Instance?.CurrentGame != null)
        {
            GameManager.Instance.CurrentGame.OnHandChanged += UpdateButtons;
            UpdateButtons();
        }
    }

    public void OnDrawStock()
    {
        GameManager.Instance.DrawCard(fromDiscard: false);
        UpdateButtons();
    }

    public void OnDrawDiscard()
    {
        GameManager.Instance.DrawCard(fromDiscard: true);
        UpdateButtons();
    }

    public void OnPlayPattern()
    {
        var selectedIndices = HandDisplay.Instance.GetSelectedIndices();
        GameManager.Instance.TryPlayPattern(selectedIndices);
        HandDisplay.Instance.ClearSelection();
        UpdateButtons();
        
    }

    public void OnDiscard()
    {
        var selectedIndices = HandDisplay.Instance.GetSelectedIndices();
        if (selectedIndices.Count == 1)
        {
            GameManager.Instance.DiscardCard(selectedIndices[0]);
            HandDisplay.Instance.ClearSelection();
            UpdateButtons();
        }
    }

    public void UpdateButtons()
    {
        var game = GameManager.Instance.CurrentGame;
        
        // Draw buttons
        drawStockButton.interactable = game.CanDraw() && game.Deck.DrawPileCount > 0;
        drawDiscardButton.interactable = game.CanDraw() && game.Deck.DiscardPileCount > 0;

        // Update discard pile display
        if (game.Deck.TopDiscard.HasValue)
        {
            discardTopCardText.text = game.Deck.TopDiscard.Value.Display;
        }
        else
        {
            discardTopCardText.text = "Empty";
        }

        // Play pattern button
        var selectedIndices = HandDisplay.Instance.GetSelectedIndices();
        var selectedCards = game.GetSelectedCards(selectedIndices);
        var validator = new PatternValidator();
        var hasPattern = validator.DetectPatterns(selectedCards).Count > 0;
        playPatternButton.interactable = hasPattern;

        // Discard button
        discardButton.interactable = game.CanDiscard() && selectedIndices.Count == 1;
    }
}