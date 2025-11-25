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
        if (GameManager.Instance?.CurrentGame != null)
        {
            var game = GameManager.Instance.CurrentGame;
            game.OnScoreChanged += UpdateScore;
            game.OnGoalUpdated += UpdateGoals;
            game.OnHandChanged += UpdateMoves;
            game.OnCardDrawn += UpdateDeckCount;

            // Initial update
            UpdateScore(game.Score);
            UpdateMoves();
            UpdateGoals(null);
            UpdateDeckCount(default);
        }
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
        var game = GameManager.Instance.CurrentGame;
        string goalsDisplay = "Goals:\n" + string.Join("\n", 
            game.Goals.Select(g => g.DisplayText + (g.IsComplete ? " ✓" : "")));
        goalsText.text = goalsDisplay;
    }

    private void UpdateDeckCount(Card card)
    {
        var game = GameManager.Instance.CurrentGame;
        deckCountText.text = $"Deck: {game.Deck.DrawPileCount}\nDiscard: {game.Deck.DiscardPileCount}";
    }
}