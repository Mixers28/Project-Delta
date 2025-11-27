using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TextAlignmentOptions = TMPro.TextAlignmentOptions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentGame { get; private set; }
    public PatternValidator PatternValidator { get; private set; }
    
    // Events for UI listeners
    public delegate void GameEndHandler(bool isWin);
    public event GameEndHandler OnGameEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EnsureUISetup();
    }

    private void Start()
    {
        PatternValidator = new PatternValidator();
        StartTestLevel();
    }

    // Test level configuration
    public void StartTestLevel()
    {
        var goals = new List<Goal>
        {
            new Goal(Goal.GoalType.Pair, 2),
            new Goal(Goal.GoalType.Run3, 1)
        };

        CurrentGame = new GameState(goals, totalMoves: 15);
        
        // Subscribe to events
        CurrentGame.OnPatternPlayed += HandlePatternPlayed;
        CurrentGame.OnGoalUpdated += HandleGoalUpdated;
        CurrentGame.OnScoreChanged += HandleScoreChanged;
        CurrentGame.OnMovesChanged += HandleMovesChanged;

        // Deal initial hand
        CurrentGame.DealInitialHand();

        Debug.Log("=== TEST LEVEL STARTED ===");
        Debug.Log($"Goals: {string.Join(", ", goals.ConvertAll(g => g.DisplayText))}");
        Debug.Log($"Moves: {CurrentGame.MovesRemaining}");
        Debug.Log($"Hand: {string.Join(", ", CurrentGame.Hand.ConvertAll(c => c.Display))}");
    }

    public void TryPlayPattern(List<int> cardIndices)
    {
        if (CurrentGame == null) return;

        var selectedCards = CurrentGame.GetSelectedCards(cardIndices);
        if (selectedCards.Count == 0) return;

        // Find best pattern
        var bestPattern = PatternValidator.GetBestPattern(selectedCards);
        if (!bestPattern.HasValue)
        {
            Debug.Log("No valid pattern detected");
            return;
        }

        // Play pattern
        bool success = CurrentGame.PlayPattern(selectedCards, bestPattern.Value.pattern);
        if (!success)
        {
            Debug.LogError("Failed to play pattern");
        }
    }

    public void DrawCard(bool fromDiscard = false)
    {
        if (CurrentGame == null) return;

        bool success = fromDiscard ? CurrentGame.DrawFromDiscard() : CurrentGame.DrawFromStock();
        
        if (!success)
        {
            Debug.Log("Cannot draw card");
        }
    }

    public void DiscardCard(int handIndex)
    {
        if (CurrentGame == null) return;
        if (handIndex < 0 || handIndex >= CurrentGame.Hand.Count) return;

        Card card = CurrentGame.Hand[handIndex];
        bool success = CurrentGame.DiscardCard(card);

        if (!success)
        {
            Debug.Log("Cannot discard card");
        }
    }

    // Event handlers
    private void HandlePatternPlayed(IPattern pattern, int score)
    {
        Debug.Log($"<color=green>Played {pattern.Name} for {score} points!</color>");
    }

    private void HandleGoalUpdated(Goal goal)
    {
        Debug.Log($"Goal updated: {goal.DisplayText}");
        
        if (goal.IsComplete)
        {
            Debug.Log($"<color=yellow>Goal complete: {goal.DisplayText}!</color>");
        }

        CheckLevelComplete();
    }

    private void HandleScoreChanged(int newScore)
    {
        Debug.Log($"Score: {newScore}");
    }

    private void HandleMovesChanged()
    {
        Debug.Log($"Moves remaining: {CurrentGame.MovesRemaining}");
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        if (CurrentGame == null)
        {
            Debug.LogError("CheckLevelComplete: CurrentGame is NULL!");
            return;
        }
        
        Debug.Log($"CheckLevelComplete: IsLevelComplete={CurrentGame.IsLevelComplete}, IsLevelFailed={CurrentGame.IsLevelFailed}");
        
        if (CurrentGame.IsLevelComplete)
        {
            Debug.Log("<color=cyan>=== LEVEL COMPLETE! ===</color>");
            Debug.Log($"Final Score: {CurrentGame.Score}");
            OnGameEnd?.Invoke(true); // Notify listeners of WIN
            Debug.Log("✓ OnGameEnd invoked for WIN");
        }
        else if (CurrentGame.IsLevelFailed)
        {
            Debug.Log("<color=red>=== LEVEL FAILED ===</color>");
            Debug.Log("Out of moves!");
            OnGameEnd?.Invoke(false); // Notify listeners of LOSS
            Debug.Log("✓ OnGameEnd invoked for LOSS");
        }
        else
        {
            Debug.Log("CheckLevelComplete: Level is still in progress");
        }
    }

    private void EnsureUISetup()
    {
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("GameManager.EnsureUISetup() - Building UI hierarchy");
        
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("  Creating new Canvas...");
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("  ✓ Canvas created");
        }
        else
        {
            Debug.Log("  ✓ Canvas found in scene");
        }
        
        // Find or create GameOverPanel GameObject
        Transform panelParent = canvas.transform.Find("GameOverPanel");
        GameOverPanel gameOverPanelComponent = null;
        if (panelParent == null)
        {
            Debug.Log("  Creating GameOverPanel GameObject...");
            GameObject panelParentObj = new GameObject("GameOverPanel");
            panelParentObj.transform.SetParent(canvas.transform, false);
            
            // IMPORTANT: Add RectTransform first before setting properties
            RectTransform panelParentRect = panelParentObj.AddComponent<RectTransform>();
            panelParentRect.anchorMin = Vector2.zero;
            panelParentRect.anchorMax = Vector2.one;
            panelParentRect.offsetMin = Vector2.zero;
            panelParentRect.offsetMax = Vector2.zero;
            
            panelParent = panelParentObj.transform;
            
            // DO NOT add GameOverPanel component yet - create children first
            Debug.Log("  ✓ GameOverPanel parent created (component will be added after children)");
        }
        else
        {
            Debug.Log("  ✓ GameOverPanel found in scene");
            gameOverPanelComponent = panelParent.GetComponent<GameOverPanel>();
        }
        
        // Find or create Panel (the visual panel)
        Transform panelTransform = panelParent.Find("Panel");
        GameObject panelObj;
        if (panelTransform == null)
        {
            Debug.Log("  Creating Panel GameObject...");
            panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(panelParent, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            // IMPORTANT: Start inactive so it doesn't show until game ends
            panelObj.SetActive(false);
            
            Debug.Log("  ✓ Panel created (inactive)");
        }
        else
        {
            panelObj = panelTransform.gameObject;
            Debug.Log("  ✓ Panel found");
            // Ensure it's inactive
            if (panelObj.activeSelf)
            {
                panelObj.SetActive(false);
                Debug.Log("  ✓ Panel set to inactive");
            }
        }
        
        // Find or create TitleText
        Transform titleTransform = panelObj.transform.Find("TitleText");
        TextMeshProUGUI titleText;
        if (titleTransform == null)
        {
            Debug.Log("  Creating TitleText...");
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 150);
            titleRect.sizeDelta = new Vector2(800, 200);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "LEVEL COMPLETE!";
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 60;
            titleText.color = Color.yellow;
            
            Debug.Log("  ✓ TitleText created");
        }
        else
        {
            titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            Debug.Log("  ✓ TitleText found");
        }
        
        // Find or create MessageText
        Transform messageTransform = panelObj.transform.Find("MessageText");
        TextMeshProUGUI messageText;
        if (messageTransform == null)
        {
            Debug.Log("  Creating MessageText...");
            GameObject messageObj = new GameObject("MessageText");
            messageObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchoredPosition = new Vector2(0, 0);
            messageRect.sizeDelta = new Vector2(800, 300);
            
            messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "Score: 0";
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.fontSize = 40;
            messageText.color = Color.white;
            
            Debug.Log("  ✓ MessageText created");
        }
        else
        {
            messageText = messageTransform.GetComponent<TextMeshProUGUI>();
            Debug.Log("  ✓ MessageText found");
        }
        
        // Find or create ContinueButton
        Transform continueTransform = panelObj.transform.Find("ContinueButton");
        Button continueButton;
        if (continueTransform == null)
        {
            Debug.Log("  Creating ContinueButton...");
            GameObject continueObj = new GameObject("ContinueButton");
            continueObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform continueRect = continueObj.AddComponent<RectTransform>();
            continueRect.anchoredPosition = new Vector2(-150, -200);
            continueRect.sizeDelta = new Vector2(250, 80);
            
            Image continueImage = continueObj.AddComponent<Image>();
            continueImage.color = new Color(0.2f, 0.6f, 0.2f);
            
            continueButton = continueObj.AddComponent<Button>();
            continueButton.targetGraphic = continueImage;
            
            GameObject continueTextObj = new GameObject("Text");
            continueTextObj.transform.SetParent(continueObj.transform, false);
            RectTransform continueTextRect = continueTextObj.AddComponent<RectTransform>();
            continueTextRect.anchorMin = Vector2.zero;
            continueTextRect.anchorMax = Vector2.one;
            continueTextRect.offsetMin = Vector2.zero;
            continueTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI continueText = continueTextObj.AddComponent<TextMeshProUGUI>();
            continueText.text = "Continue";
            continueText.alignment = TextAlignmentOptions.Center;
            continueText.fontSize = 36;
            continueText.color = Color.white;
            
            Debug.Log("  ✓ ContinueButton created");
        }
        else
        {
            continueButton = continueTransform.GetComponent<Button>();
            Debug.Log("  ✓ ContinueButton found");
        }
        
        // Find or create RetryButton
        Transform retryTransform = panelObj.transform.Find("RetryButton");
        Button retryButton;
        if (retryTransform == null)
        {
            Debug.Log("  Creating RetryButton...");
            GameObject retryObj = new GameObject("RetryButton");
            retryObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform retryRect = retryObj.AddComponent<RectTransform>();
            retryRect.anchoredPosition = new Vector2(150, -200);
            retryRect.sizeDelta = new Vector2(250, 80);
            
            Image retryImage = retryObj.AddComponent<Image>();
            retryImage.color = new Color(0.6f, 0.2f, 0.2f);
            
            retryButton = retryObj.AddComponent<Button>();
            retryButton.targetGraphic = retryImage;
            
            GameObject retryTextObj = new GameObject("Text");
            retryTextObj.transform.SetParent(retryObj.transform, false);
            RectTransform retryTextRect = retryTextObj.AddComponent<RectTransform>();
            retryTextRect.anchorMin = Vector2.zero;
            retryTextRect.anchorMax = Vector2.one;
            retryTextRect.offsetMin = Vector2.zero;
            retryTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI retryText = retryTextObj.AddComponent<TextMeshProUGUI>();
            retryText.text = "Retry";
            retryText.alignment = TextAlignmentOptions.Center;
            retryText.fontSize = 36;
            retryText.color = Color.white;
            
            Debug.Log("  ✓ RetryButton created");
        }
        else
        {
            retryButton = retryTransform.GetComponent<Button>();
            Debug.Log("  ✓ RetryButton found");
        }
        
        // Now assign all references to GameOverPanel component
        // First, ADD the component if it doesn't exist (this happens AFTER all children are created)
        if (gameOverPanelComponent == null)
        {
            gameOverPanelComponent = panelParent.gameObject.GetComponent<GameOverPanel>();
            if (gameOverPanelComponent == null)
            {
                gameOverPanelComponent = panelParent.gameObject.AddComponent<GameOverPanel>();
                Debug.Log("  ✓ Attached GameOverPanel component (after children created)");
            }
        }
        
        if (gameOverPanelComponent != null)
        {
            Debug.Log("  Assigning references to GameOverPanel component...");
            
            // Use reflection to set private SerializeField properties
            var panelField = typeof(GameOverPanel).GetField("panel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var titleTextField = typeof(GameOverPanel).GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var messageTextField = typeof(GameOverPanel).GetField("messageText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var retryButtonField = typeof(GameOverPanel).GetField("retryButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var continueButtonField = typeof(GameOverPanel).GetField("continueButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (panelField != null) panelField.SetValue(gameOverPanelComponent, panelObj);
            if (titleTextField != null) titleTextField.SetValue(gameOverPanelComponent, titleText);
            if (messageTextField != null) messageTextField.SetValue(gameOverPanelComponent, messageText);
            if (retryButtonField != null) retryButtonField.SetValue(gameOverPanelComponent, retryButton);
            if (continueButtonField != null) continueButtonField.SetValue(gameOverPanelComponent, continueButton);
            
            Debug.Log("  ✓ All references assigned");
        }
        else
        {
            Debug.LogError("  ✗ GameOverPanel component not found!");
        }
        
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("✓ GameOverPanel setup complete!");
    }
}