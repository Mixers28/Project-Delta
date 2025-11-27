using UnityEngine;
using TMPro;

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

        StartCoroutine(InitializeHUD());
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

        cachedGameState = GameManager.Instance.CurrentGame;

        cachedGameState.OnScoreChanged += UpdateScore;
        cachedGameState.OnGoalUpdated += UpdateGoals;
        cachedGameState.OnHandChanged += UpdateMoves;
        cachedGameState.OnCardDrawn += UpdateDeckCount;

        RefreshAllDisplays();
    }

    private void OnDestroy()
    {
        if (cachedGameState != null)
        {
            cachedGameState.OnScoreChanged -= UpdateScore;
            cachedGameState.OnGoalUpdated -= UpdateGoals;
            cachedGameState.OnHandChanged -= UpdateMoves;
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

        string goalsDisplay = "Goals:\n";

        foreach (var g in cachedGameState.Goals)
        {
            string checkmark = g.IsComplete ? " ✓" : "";
            goalsDisplay += $"{g.DisplayText}{checkmark}\n";
        }

        goalsText.text = goalsDisplay;
    }

    private void UpdateDeckCount(Card card)
    {
        if (cachedGameState == null || deckCountText == null) return;

        deckCountText.text = $"Deck: {cachedGameState.Deck.DrawPileCount}\nDiscard: {cachedGameState.Deck.DiscardPileCount}";
    }
}