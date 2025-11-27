using UnityEngine;
using TMPro;
using System.Linq;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI goalsText;
    [SerializeField] private TextMeshProUGUI deckCountText;

    private void Start()
{
    Debug.Log("=== GameHUD.Start() ===");
    
    // Check all references
    if (scoreText == null) Debug.LogError("scoreText is NULL!");
    if (movesText == null) Debug.LogError("movesText is NULL!");
    if (goalsText == null) Debug.LogError("goalsText is NULL!");
    if (deckCountText == null) Debug.LogError("deckCountText is NULL!");
    
    // Wait for GameManager
    StartCoroutine(InitializeHUD());
}

private System.Collections.IEnumerator InitializeHUD()
{
    // Wait for GameManager to be ready
    while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
    {
        yield return null;
    }
    
    Debug.Log("GameManager ready, subscribing to events");
    
    var game = GameManager.Instance.CurrentGame;
    game.OnScoreChanged += UpdateScore;
    game.OnGoalUpdated += UpdateGoals;
    game.OnHandChanged += UpdateMoves;
    game.OnCardDrawn += UpdateDeckCount;

    // Force initial update
    Debug.Log("Forcing initial updates...");
    UpdateScore(game.Score);
    UpdateMoves();
    UpdateGoals(null);
    UpdateDeckCount(default);
}

    private void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    private void UpdateMoves()
    {
        var game = GameManager.Instance.CurrentGame;
        movesText.text = $"Moves: {game.MovesRemaining}";
    }

  private void UpdateGoals(Goal goal)
{
    var game = GameManager.Instance?.CurrentGame;
    if (game == null)
    {
        Debug.LogError("CurrentGame is null in UpdateGoals");
        return;
    }
    
    if (game.Goals == null || game.Goals.Count == 0)
    {
        goalsText.text = "Goals:\nNone";
        return;
    }
    
    string goalsDisplay = "Goals:\n";
    
    foreach (var g in game.Goals)
    {
        string checkmark = g.IsComplete ? " ✓" : "";
        goalsDisplay += $"{g.DisplayText}{checkmark}\n";
    }
    
    goalsText.text = goalsDisplay;
    
    Debug.Log($"Goals updated: {goalsDisplay}");
}

    private void UpdateDeckCount(Card card)
    {
        var game = GameManager.Instance.CurrentGame;
        deckCountText.text = $"Deck: {game.Deck.DrawPileCount}\nDiscard: {game.Deck.DiscardPileCount}";
    }
}