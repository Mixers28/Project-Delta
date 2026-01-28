using UnityEngine;
using TMPro;
using System;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI goalsText;
    [SerializeField] private TextMeshProUGUI deckCountText;

    private GameState cachedGameState;

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("GameHUD: Missing required UI references!");
            return;
        }

        EnsureSafeArea();
        StartCoroutine(InitializeHUD());
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += HandleGameStarted;
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (scoreText == null)
        {
            Debug.LogError("GameHUD: scoreText is not assigned!");
            isValid = false;
        }
        if (movesText == null)
        {
            Debug.LogError("GameHUD: movesText is not assigned!");
            isValid = false;
        }
        if (goalsText == null)
        {
            Debug.LogError("GameHUD: goalsText is not assigned!");
            isValid = false;
        }
        if (deckCountText == null)
        {
            Debug.LogError("GameHUD: deckCountText is not assigned!");
            isValid = false;
        }

        return isValid;
    }

    private System.Collections.IEnumerator InitializeHUD()
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
            cachedGameState.OnScoreChanged -= UpdateScore;
            cachedGameState.OnGoalUpdated -= UpdateGoals;
            cachedGameState.OnMovesChanged -= UpdateMoves;
            cachedGameState.OnCardDrawn -= UpdateDeckCount;
        }
    }

    private void RefreshAllDisplays()
    {
        if (cachedGameState == null) return;

        UpdateScore(cachedGameState.Score);
        UpdateMoves();
        UpdateGoals(null);
        UpdateDeckCount(default);
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateMoves()
    {
        if (cachedGameState == null || movesText == null) return;

        movesText.text = $"Moves: {cachedGameState.MovesRemaining}";
    }

    private void UpdateGoals(Goal goal)
    {
        if (cachedGameState == null || goalsText == null) return;

        if (cachedGameState.Goals == null || cachedGameState.Goals.Count == 0)
        {
            goalsText.text = "Goals:\nNone";
            return;
        }

        // Show only level info (if present) and goals; hide deck labels to declutter overlay
        string stageLabel = cachedGameState.LevelName;
        int levelIndex = stageLabel.LastIndexOf("Level", StringComparison.OrdinalIgnoreCase);
        if (levelIndex >= 0)
        {
            stageLabel = stageLabel.Substring(levelIndex).Trim();
        }

        string goalsDisplay = string.IsNullOrWhiteSpace(stageLabel)
            ? "Goals:\n"
            : $"{stageLabel}\nGoals:\n";

        foreach (var g in cachedGameState.Goals)
        {
            string status = g.IsComplete ? " [done]" : "";
            goalsDisplay += $"{g.DisplayText}{status}\n";
        }

        goalsText.text = goalsDisplay;
    }

    private void UpdateDeckCount(Card card)
    {
        if (cachedGameState == null || deckCountText == null) return;

        deckCountText.text = $"Deck: {cachedGameState.Deck.DrawPileCount}\nDiscard: {cachedGameState.Deck.DiscardPileCount}";
    }

    private void HandleGameStarted(GameState game)
    {
        if (cachedGameState != null)
        {
            cachedGameState.OnScoreChanged -= UpdateScore;
            cachedGameState.OnGoalUpdated -= UpdateGoals;
            cachedGameState.OnMovesChanged -= UpdateMoves;
            cachedGameState.OnCardDrawn -= UpdateDeckCount;
        }

        cachedGameState = game;

        Debug.Log("HUD rebinding to new GameState");
        Debug.Log($"HUD Level: {cachedGameState.LevelName} | Deck: {cachedGameState.DeckDescription}");

        cachedGameState.OnScoreChanged += UpdateScore;
        cachedGameState.OnGoalUpdated += UpdateGoals;
        cachedGameState.OnMovesChanged += UpdateMoves;
        cachedGameState.OnCardDrawn += UpdateDeckCount;

        RefreshAllDisplays();
    }

    private void EnsureSafeArea()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        if (canvas.GetComponent<SafeAreaFitter>() != null) return;
        canvas.gameObject.AddComponent<SafeAreaFitter>();
    }
}
