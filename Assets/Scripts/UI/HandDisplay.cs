using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class HandDisplay : MonoBehaviour
{
    public static HandDisplay Instance { get; private set; }

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private TextMeshProUGUI patternPreviewText;
    
    [Header("Sprites")]
    [SerializeField] private CardSpriteLibrary spriteLibrary;

    private readonly List<CardDisplay> cardDisplays = new();
    private readonly PatternValidator patternValidator = new();
    private ActionButtons cachedActionButtons;
    private bool inputLocked;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaitForGameManager());
    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        Debug.Log("Waiting for GameManager...");
        
        while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
        {
            yield return null;
        }
        
        Debug.Log("GameManager ready!");
        
        GameManager.Instance.CurrentGame.OnHandChanged += RefreshHand;
        RefreshHand();
    }

    public void RefreshHand()
    {
        Debug.Log("=== RefreshHand() called ===");
        
        // Clear existing displays
        foreach (var display in cardDisplays)
        {
            Destroy(display.gameObject);
        }
        cardDisplays.Clear();
        
        if (GameManager.Instance?.CurrentGame == null)
        {
            Debug.LogError("GameManager or CurrentGame is NULL!");
            return;
        }

        var hand = GameManager.Instance.CurrentGame.Hand;
        
        if (cardPrefab == null)
        {
            Debug.LogError("cardPrefab is NULL!");
            return;
        }
        
        if (handContainer == null)
        {
            Debug.LogError("handContainer is NULL!");
            return;
        }

        // Create new card displays
        for (int i = 0; i < hand.Count; i++)
        {
            Card card = hand[i];
            
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display == null)
            {
                Debug.LogError("CardDisplay component not found!");
                continue;
            }

            display.SetCard(card, spriteLibrary);
            cardDisplays.Add(display);
        }

        UpdatePatternPreview();
    }

    public void OnCardClicked(CardDisplay clickedCard)
    {
        if (inputLocked) return;

        clickedCard.SetSelected(!clickedCard.IsSelected);
        UpdatePatternPreview();
        UpdateActionButtons();
    }

    private void UpdateActionButtons()
    {
        if (cachedActionButtons == null)
        {
            cachedActionButtons = FindObjectOfType<ActionButtons>();
        }

        if (cachedActionButtons != null)
        {
            cachedActionButtons.UpdateButtonStates();
        }
    }

    private void UpdatePatternPreview()
    {
        var game = GameManager.Instance?.CurrentGame;
        
        if (game != null && game.MustDiscard)
        {
            patternPreviewText.text = "⚠️ SELECT 1 CARD TO DISCARD ⚠️";
            patternPreviewText.color = Color.red;
            return;
        }
        
        patternPreviewText.color = Color.yellow;
        
        var selectedCards = cardDisplays
            .Where(cd => cd.IsSelected)
            .Select(cd => cd.CardData)
            .ToList();

        if (selectedCards.Count == 0)
        {
            patternPreviewText.text = "Select cards to see patterns";
            return;
        }

        var patterns = patternValidator.DetectPatterns(selectedCards);

        if (patterns.Count == 0)
        {
            patternPreviewText.text = $"Selected: {selectedCards.Count} cards - No valid pattern";
        }
        else
        {
            var best = patternValidator.GetBestPattern(selectedCards);
            if (best.HasValue)
            {
                patternPreviewText.text = $"{best.Value.pattern.Name} - {best.Value.score} points";
            }
        }
    }

    public List<int> GetSelectedIndices()
    {
        var indices = new List<int>();
        for (int i = 0; i < cardDisplays.Count; i++)
        {
            if (cardDisplays[i].IsSelected)
            {
                indices.Add(i);
            }
        }
        return indices;
    }

    public void ClearSelection()
    {
        foreach (var display in cardDisplays)
        {
            display.SetSelected(false);
        }
        UpdatePatternPreview();
        UpdateActionButtons();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputLocked = !enabled;
        if (inputLocked)
        {
            ClearSelection();
        }
    }
}
