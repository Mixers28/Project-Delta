using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        Debug.Log("GameOverPanel Awake");
        
        if (panel != null)
        {
            panel.SetActive(false);
            Debug.Log("Panel set to inactive");
        }
        else
        {
            Debug.LogError("Panel reference is NULL!");
        }
        
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetry);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
        }
    }

    private void Start()
    {
        Debug.Log("GameOverPanel Start");
        // Use polling for now to debug
        InvokeRepeating(nameof(CheckWinLose), 0.5f, 0.5f);
    }

    private void Update()
    {
        // Manual triggers for testing
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Manual WIN trigger");
            ShowWin();
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Manual LOSE trigger");
            ShowLose();
        }
    }

    private void CheckWinLose()
    {
        var game = GameManager.Instance?.CurrentGame;
        if (game == null) return;
        
        if (panel != null && panel.activeSelf) return;

        bool isComplete = game.IsLevelComplete;
        bool isFailed = game.IsLevelFailed;

        if (isComplete)
        {
            Debug.Log("!!! WIN CONDITION MET !!!");
            ShowWin();
        }
        else if (isFailed)
        {
            Debug.Log("!!! LOSE CONDITION MET !!!");
            ShowLose();
        }
    }

    private void ShowWin()
    {
        Debug.Log("ShowWin called");
        
        if (panel == null)
        {
            Debug.LogError("Cannot show win - panel is NULL!");
            return;
        }
        
        panel.SetActive(true);
        Debug.Log("Panel activated");
        
        titleText.text = "🎉 LEVEL COMPLETE! 🎉";
        titleText.color = Color.yellow;
        
        var game = GameManager.Instance.CurrentGame;
        string goalsSummary = "";
        foreach (var goal in game.Goals)
        {
            goalsSummary += $"✓ {goal.DisplayText}\n";
        }
        
        messageText.text = $"<size=48>Score: {game.Score}</size>\n\n{goalsSummary}\nMoves Remaining: {game.MovesRemaining}";
        
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        
        StartCoroutine(CelebrationAnimation());
    }

    private void ShowLose()
    {
        Debug.Log("ShowLose called");
        
        if (panel == null)
        {
            Debug.LogError("Cannot show lose - panel is NULL!");
            return;
        }
        
        panel.SetActive(true);
        Debug.Log("Panel activated");
        
        titleText.text = "LEVEL FAILED";
        titleText.color = Color.red;
        
        var game = GameManager.Instance.CurrentGame;
        string incompleteGoals = "";
        foreach (var goal in game.Goals)
        {
            if (!goal.IsComplete)
            {
                incompleteGoals += $"✗ {goal.DisplayText}\n";
            }
        }
        
        messageText.text = $"Out of moves!\n\nIncomplete Goals:\n{incompleteGoals}\nFinal Score: {game.Score}";
        
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(true);
    }

    private System.Collections.IEnumerator CelebrationAnimation()
    {
        if (titleText == null) yield break;
        
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = titleText.transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / duration * Mathf.PI) * 0.3f;
            titleText.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        titleText.transform.localScale = originalScale;
    }

    private void OnRetry()
    {
        Debug.Log("Retry clicked");
        if (panel != null) panel.SetActive(false);
        GameManager.Instance.StartTestLevel();
    }

    private void OnContinue()
    {
        Debug.Log("Continue clicked");
        if (panel != null) panel.SetActive(false);
        GameManager.Instance.StartTestLevel();
    }
}