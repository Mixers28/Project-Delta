using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    private const float DEFAULT_CARD_WIDTH = 150f;
    private const float DEFAULT_CARD_HEIGHT = 210f;
    private const float DEFAULT_TEXT_SIZE = 48f;

    public Card CardData { get; private set; }
    public bool IsSelected { get; private set; }

    [Header("References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Animation")]
    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;

    private Button button;
    private Vector3 normalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private CardSpriteLibrary spriteLibrary;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }
        normalScale = transform.localScale;
    }

    private void Update()
    {
        // Smooth scale animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void SetCard(Card card, CardSpriteLibrary library = null)
    {
        CardData = card;

        if (library != null)
        {
            spriteLibrary = library;
        }

        if (!TryFindCardImage())
        {
            return;
        }

        cardImage.preserveAspect = true;

        if (TrySetSpriteFromLibrary(card))
        {
            cardImage.enabled = true;
        }
        else
        {
            UseFallbackDisplay(card);
        }

        SetCardSize();
    }

    private bool TryFindCardImage()
    {
        if (cardImage != null)
        {
            return true;
        }

        Debug.LogWarning("cardImage is NULL, searching for Image component...");

        Transform child = transform.Find("CardImage");
        if (child != null)
        {
            cardImage = child.GetComponent<Image>();
        }

        if (cardImage == null)
        {
            cardImage = GetComponentInChildren<Image>();
        }

        if (cardImage == null)
        {
            Debug.LogError("Cannot find cardImage! Make sure CardPrefab has a child with Image component.");
            return false;
        }

        return true;
    }

    private bool TrySetSpriteFromLibrary(Card card)
    {
        if (spriteLibrary == null)
        {
            Debug.LogWarning("No sprite library assigned, using text fallback");
            return false;
        }

        Sprite sprite = spriteLibrary.GetSprite(card);
        if (sprite != null)
        {
            cardImage.sprite = sprite;
            return true;
        }

        Debug.LogWarning($"No sprite found for {card.Display}, using fallback");
        return false;
    }

    private void SetCardSize()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(DEFAULT_CARD_WIDTH, DEFAULT_CARD_HEIGHT);
        }
    }

    private void UseFallbackDisplay(Card card)
    {
        // Create text as fallback if no sprites
        // This uses the old text display method
        cardImage.sprite = null;
        cardImage.enabled = false;
        
        // Try to find or create TextMeshPro component
        var text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text == null)
        {
            // Create text child if it doesn't exist
            GameObject textObj = new GameObject("CardText");
            textObj.transform.SetParent(transform);
            text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            
            // Configure text
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.fontSize = DEFAULT_TEXT_SIZE;
            text.color = Color.black;
        }
        
        text.text = card.Display;
        text.enabled = true;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        
        // Update background color
        if (background != null)
        {
            background.color = selected ? selectedColor : normalColor;
        }
        
        // Update scale target for animation
        targetScale = selected ? normalScale * selectScale : normalScale;
    }

    private void OnClicked()
    {
        if (HandDisplay.Instance != null)
        {
            HandDisplay.Instance.OnCardClicked(this);
        }
    }

    // Called when card is destroyed/returned to pool
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
        }
    }
}