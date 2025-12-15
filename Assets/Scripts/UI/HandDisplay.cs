using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;

public class HandDisplay : MonoBehaviour
{
    public static HandDisplay Instance { get; private set; }

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private TextMeshProUGUI patternPreviewText;
    
    [Header("Sprites")]
    [SerializeField] private CardSpriteLibrary spriteLibrary;

    private readonly List<CardDisplay> cardDisplays = new();
    private ActionButtons cachedActionButtons;
    private bool inputLocked;
    private GameState cachedGameState;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaitForGameManager());
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += HandleGameStarted;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= HandleGameStarted;
        }

        if (cachedGameState != null)
        {
            cachedGameState.OnHandChanged -= RefreshHand;
        }
    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        Debug.Log("Waiting for GameManager...");
        
        while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
        {
            yield return null;
        }

        Debug.Log("GameManager ready!");

        // Subscribe once the GameManager exists (covers the case OnEnable ran before Awake)
        GameManager.Instance.OnGameStarted -= HandleGameStarted;
        GameManager.Instance.OnGameStarted += HandleGameStarted;

        HandleGameStarted(GameManager.Instance.CurrentGame);
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
        
        if (cachedGameState == null)
        {
            Debug.LogError("GameManager or CurrentGame is NULL!");
            return;
        }

        var hand = cachedGameState.Hand;
        
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
        if (patternPreviewText == null)
        {
            return;
        }

        var game = cachedGameState ?? GameManager.Instance?.CurrentGame;

        bool mustDiscard = game != null && game.MustDiscard;
        patternPreviewText.color = mustDiscard ? Color.red : Color.yellow;
        
        var selectedCards = cardDisplays
            .Where(cd => cd.IsSelected)
            .Select(cd => cd.CardData)
            .ToList();

        if (selectedCards.Count == 0)
        {
            string basePrompt = mustDiscard
                ? "Hand over limit: play a pattern or select cards and press Discard"
                : "Select cards to see patterns";

            var allowed = GameManager.Instance != null ? GameManager.Instance.AllowedPatterns : null;
            if (allowed != null && allowed.Count > 0)
            {
                basePrompt += $"\nAllowed: {string.Join(", ", allowed.Select(p => p.ToDisplayName()))}";
            }

            patternPreviewText.text = basePrompt;
            return;
        }

        if (mustDiscard && selectedCards.Count == 1)
        {
            patternPreviewText.text = "Press Discard (or select more to play a pattern)";
            return;
        }

        var validator = GameManager.Instance != null ? GameManager.Instance.PatternValidator : null;
        var patterns = validator != null ? validator.DetectPatterns(selectedCards) : new List<IPattern>();

        if (patterns.Count == 0)
        {
            patternPreviewText.text = $"Selected: {selectedCards.Count} cards - No valid pattern";
        }
        else
        {
            var best = PatternSelection.GetBestPatternForGoals(validator, selectedCards, cachedGameState != null ? cachedGameState.Goals : null);
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

    private void HandleGameStarted(GameState game)
    {
        if (game == null) return;

        if (cachedGameState != null)
        {
            cachedGameState.OnHandChanged -= RefreshHand;
        }

        cachedGameState = game;
        cachedGameState.OnHandChanged += RefreshHand;
        inputLocked = false;
        ClearSelection();
        RefreshHand();
    }
}
