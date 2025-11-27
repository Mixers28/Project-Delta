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

    private List<CardDisplay> cardDisplays = new List<CardDisplay>();
    private PatternValidator patternValidator;

    private void Awake()
    {
        Instance = this;
        patternValidator = new PatternValidator();
    }

    private void Start()
    {
        StartCoroutine(WaitForGameManager());
    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        Debug.Log("Waiting for GameManager...");
        
        // Wait until GameManager and CurrentGame exist
        while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
        {
            yield return null;
        }
        
        Debug.Log("GameManager ready!");
        Debug.Log($"Hand has {GameManager.Instance.CurrentGame.Hand.Count} cards");
        
        // Subscribe to game state events
        GameManager.Instance.CurrentGame.OnHandChanged += RefreshHand;
        
        // Initial display
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
            Debug.LogError("GameManager or CurrentGame is NULL in RefreshHand!");
            return;
        }

        var hand = GameManager.Instance.CurrentGame.Hand;
        Debug.Log($"Hand has {hand.Count} cards");
        
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
        
        Debug.Log($"About to create {hand.Count} card displays");

        // Create new card displays
        for (int i = 0; i < hand.Count; i++)
        {
            Card card = hand[i];
            Debug.Log($"Creating card {i}: {card.Display}");
            
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            Debug.Log($"Instantiated GameObject: {cardObj.name}");
            
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display == null)
            {
                Debug.LogError("CardDisplay component not found on instantiated object!");
                continue;
            }
            
            display.SetCard(card);
            cardDisplays.Add(display);
            Debug.Log($"Card {i} added successfully");
        }
        
        Debug.Log($"Total cards created: {cardDisplays.Count}");

        UpdatePatternPreview();
    }

    public void OnCardClicked(CardDisplay clickedCard)
    {
        // Toggle selection
        clickedCard.SetSelected(!clickedCard.IsSelected);
        UpdatePatternPreview();
        
        // Update button states
        ActionButtons actionButtons = FindObjectOfType<ActionButtons>();
        if (actionButtons != null)
        {
            actionButtons.UpdateButtonStates();
        }
    }

    private void UpdatePatternPreview()
{
    var game = GameManager.Instance?.CurrentGame;
    
    // Check if must discard
    if (game != null && game.MustDiscard)
    {
        patternPreviewText.text = "⚠️ SELECT 1 CARD TO DISCARD ⚠️";
        patternPreviewText.color = Color.red;
        return;
    }
    
    patternPreviewText.color = Color.yellow; // Reset to normal color
    
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
        
        // Update button states
        ActionButtons actionButtons = FindObjectOfType<ActionButtons>();
        if (actionButtons != null)
        {
            actionButtons.UpdateButtonStates();
        }
    }
}