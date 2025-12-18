using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
    [SerializeField] private float selectScale = 1.05f; // small lift on select
    [SerializeField] private float dragScale = 1.08f; // small lift while dragging
    [SerializeField] private float dragRotationZ = -4f;
    [SerializeField] private float animationSpeed = 10f;

    private Button button;
    private bool buttonWasInteractable = true;
    private bool didDragSinceLastClick;
    private Vector3 normalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private float dragScaleMultiplier = 1f;
    private Quaternion baseRotation;
    private Quaternion targetRotation;
    private UnityEngine.UI.Shadow shadow;
    private Vector2 baseShadowDistance;
    private Color baseShadowColor;
    private CardSpriteLibrary spriteLibrary;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }
        normalScale = transform.localScale;
        baseRotation = transform.localRotation;
        targetRotation = baseRotation;

        // Subtle shadow for depth
        shadow = GetComponent<UnityEngine.UI.Shadow>();
        if (shadow == null)
        {
            shadow = gameObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }
        baseShadowDistance = shadow.effectDistance;
        baseShadowColor = shadow.effectColor;
    }

    private void Update()
    {
        // Smooth scale animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale * dragScaleMultiplier, Time.deltaTime * animationSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
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

    public void SetDraggingVisual(bool dragging)
    {
        dragScaleMultiplier = dragging ? dragScale : 1f;

        if (dragging)
        {
            targetRotation = Quaternion.Euler(0f, 0f, dragRotationZ);
            if (shadow != null)
            {
                shadow.effectDistance = baseShadowDistance * 2.2f;
                shadow.effectColor = new Color(baseShadowColor.r, baseShadowColor.g, baseShadowColor.b, Mathf.Clamp01(baseShadowColor.a + 0.25f));
            }
        }
        else
        {
            targetRotation = baseRotation;
            if (shadow != null)
            {
                shadow.effectDistance = baseShadowDistance;
                shadow.effectColor = baseShadowColor;
            }
        }
    }

    private void OnClicked()
    {
        if (didDragSinceLastClick)
        {
            didDragSinceLastClick = false;
            return;
        }

        if (HandDisplay.Instance != null)
        {
            HandDisplay.Instance.OnCardClicked(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        didDragSinceLastClick = true;
        SetDraggingVisual(true);
        if (button != null)
        {
            buttonWasInteractable = button.interactable;
            button.interactable = false;
        }

        HandDisplay.Instance?.OnCardBeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandDisplay.Instance?.OnCardDrag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HandDisplay.Instance?.OnCardEndDrag(this, eventData);

        SetDraggingVisual(false);
        if (button != null)
        {
            button.interactable = buttonWasInteractable;
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
