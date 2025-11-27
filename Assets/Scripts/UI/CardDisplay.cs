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
    
    [Header("Animation")]
    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;

    private Button button;
    private Vector3 normalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
        normalScale = transform.localScale;
    }

    private void Update()
    {
        // Smooth scale animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
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
        
        // Force size
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 200);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        background.color = selected ? selectedColor : normalColor;
        targetScale = selected ? normalScale * selectScale : normalScale;
    }

    private void OnClicked()
    {
        HandDisplay.Instance?.OnCardClicked(this);
    }
}