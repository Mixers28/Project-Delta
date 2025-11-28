using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    private const float DEFAULT_CARD_WIDTH = 150f;
    private const float DEFAULT_CARD_HEIGHT = 210f;
    private const float DEFAULT_TEXT_SIZE = 48f;
    private const float IMAGE_SCALE = 1f; // do not overscale sprites

    public Card CardData { get; private set; }
    public bool IsSelected { get; private set; }

    [Header("References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Animation")]
    [SerializeField] private float selectScale = 1f; // disable bump on select
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

        EnsureImageLayout();
        // Fill the rect instead of letterboxing (sprites already sized like cards)
        cardImage.preserveAspect = false;

        if (!TrySetSpriteFromLibrary(card))
        {
            UseFallbackDisplay(card);
        }

        SetCardSize();
    }

    private void EnsureImageLayout()
    {
        // Force the card image to stretch to the parent so sprites are visible
        var rt = cardImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = new Vector3(IMAGE_SCALE, IMAGE_SCALE, 1f);
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
        if (sprite == null)
        {
            // For face cards and jokers, prefer asset placeholders over text
            if (card.IsJoker && spriteLibrary.jokerSprite != null)
            {
                sprite = spriteLibrary.jokerSprite;
            }
            else if ((card.IsFaceCard || card.IsJoker) && spriteLibrary.cardBack != null)
            {
                sprite = spriteLibrary.cardBack;
            }
        }

        if (sprite != null)
        {
            cardImage.sprite = sprite;
            cardImage.enabled = true;
            DisableFallbackText();
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
        // Create text as fallback if no sprites are available
        cardImage.sprite = null;
        cardImage.enabled = false;
        
        var text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text == null)
        {
            GameObject textObj = new GameObject("CardText");
            textObj.transform.SetParent(transform);
            text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            
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

    private void DisableFallbackText()
    {
        var text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.enabled = false;
        }
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

        // If deselected, snap back in case it was left enlarged
        if (!selected)
        {
            transform.localScale = targetScale;
        }
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
