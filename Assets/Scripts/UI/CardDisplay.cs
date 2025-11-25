using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Card CardData { get; private set; }
    public bool IsSelected { get; private set; }

    [SerializeField] private TextMeshProUGUI cardText;
    [SerializeField] private Image background;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    public void SetCard(Card card)
{
    CardData = card;
    
    if (cardText == null)
    {
        Debug.LogError("cardText is NULL on " + gameObject.name);
        cardText = GetComponentInChildren<TextMeshProUGUI>();
        if (cardText == null)
        {
            Debug.LogError("Still can't find TextMeshProUGUI!");
            return;
        }
    }
    
    cardText.text = card.Display;
    cardText.fontSize = 48;
    
    Debug.Log($"Set card text to: {card.Display} on {gameObject.name}");
    
    // Force size
    RectTransform rt = GetComponent<RectTransform>();
    rt.sizeDelta = new Vector2(150, 200);
}

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        background.color = selected ? selectedColor : normalColor;
    }

    private void OnClicked()
    {
        HandDisplay.Instance?.OnCardClicked(this);
    }
}