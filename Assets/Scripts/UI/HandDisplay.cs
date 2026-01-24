using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandDisplay : MonoBehaviour
{
    public static HandDisplay Instance { get; private set; }

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;
    [SerializeField] private TextMeshProUGUI patternPreviewText;
    
    [Header("Sprites")]
    [SerializeField] private CardSpriteLibrary spriteLibrary;

    private readonly List<CardDisplay> cardDisplays = new();
    private ActionButtons cachedActionButtons;
    private bool inputLocked;
    private GameState cachedGameState;

    private CardDisplay draggingCard;
    private GameObject draggingPlaceholder;
    private CanvasGroup draggingCanvasGroup;
    private LayoutElement draggingLayoutElement;
    private float lastDragEndTime;
    private GridLayoutGroup handGrid;
    private Canvas rootCanvas;
    private float dragBaselineY;
    [Header("Drag Reorder")]
    [SerializeField] private float dragFollowTime = 0.12f;
    [SerializeField] private float maxDragSpeed = 4500f;
    [SerializeField] private float dragLiftOffset = 16f;
    [SerializeField] private float dragYBand = 28f;
    [SerializeField] private float dropAnimDuration = 0.12f;
    [SerializeField] private float placeholderUpdateInterval = 0.04f;
    [SerializeField] private float placeholderDeadzone = 14f;
    [SerializeField] private bool dimOtherCardsWhileDragging = true;

    private Vector2 dragVelocity;
    private float lastPlaceholderUpdateTime;

    private void Awake()
    {
        Instance = this;
        rootCanvas = GetComponentInParent<Canvas>();
        if (handContainer != null)
        {
            handGrid = handContainer.GetComponent<GridLayoutGroup>();
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForGameManager());
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += HandleGameStarted;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= HandleGameStarted;
        }

        if (cachedGameState != null)
        {
            cachedGameState.OnHandChanged -= RefreshHand;
        }
    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        Debug.Log("Waiting for GameManager...");
        
        while (GameManager.Instance == null || GameManager.Instance.CurrentGame == null)
        {
            yield return null;
        }

        Debug.Log("GameManager ready!");

        // Subscribe once the GameManager exists (covers the case OnEnable ran before Awake)
        GameManager.Instance.OnGameStarted -= HandleGameStarted;
        GameManager.Instance.OnGameStarted += HandleGameStarted;

        HandleGameStarted(GameManager.Instance.CurrentGame);
    }

    public void RefreshHand()
    {
        Debug.Log("=== RefreshHand() called ===");
        
        // Clear existing displays
        foreach (var display in cardDisplays)
        {
            Destroy(display.gameObject);
        }
        cardDisplays.Clear();
        
        if (cachedGameState == null)
        {
            Debug.LogError("GameManager or CurrentGame is NULL!");
            return;
        }

        var hand = cachedGameState.Hand;
        
        if (cardPrefab == null)
        {
            Debug.LogError("cardPrefab is NULL!");
            return;
        }
        
        if (handContainer == null)
        {
            Debug.LogError("handContainer is NULL!");
            return;
        }

        // Create new card displays
        for (int i = 0; i < hand.Count; i++)
        {
            Card card = hand[i];
            
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display == null)
            {
                Debug.LogError("CardDisplay component not found!");
                continue;
            }

            display.SetCard(card, spriteLibrary);
            cardDisplays.Add(display);
        }

        UpdatePatternPreview();
    }

    public void OnCardClicked(CardDisplay clickedCard)
    {
        if (inputLocked) return;
        if (draggingCard != null) return;
        if (Time.unscaledTime - lastDragEndTime < 0.15f) return;

        clickedCard.SetSelected(!clickedCard.IsSelected);
        UpdatePatternPreview();
        UpdateActionButtons();
    }

    public void OnCardBeginDrag(CardDisplay card, PointerEventData eventData)
    {
        if (inputLocked) return;
        if (cachedGameState == null) return;
        if (handContainer == null) return;
        if (card == null) return;
        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }
        if (handGrid == null)
        {
            handGrid = handContainer.GetComponent<GridLayoutGroup>();
        }

        draggingCard = card;
        dragVelocity = Vector2.zero;
        lastPlaceholderUpdateTime = 0f;
        var cardRt = card.GetComponent<RectTransform>();
        if (cardRt != null)
        {
            dragBaselineY = cardRt.anchoredPosition.y + dragLiftOffset;
        }
        if (dimOtherCardsWhileDragging)
        {
            SetCardsDimmed(true, exclude: draggingCard);
        }

        // Placeholder keeps spacing in the layout while we drag the real card.
        draggingPlaceholder = new GameObject("DragPlaceholder", typeof(RectTransform));
        draggingPlaceholder.transform.SetParent(handContainer, false);
        draggingPlaceholder.transform.SetSiblingIndex(card.transform.GetSiblingIndex());

        var placeholderLayout = draggingPlaceholder.AddComponent<LayoutElement>();
        if (cardRt != null)
        {
            placeholderLayout.preferredWidth = cardRt.rect.width;
            placeholderLayout.preferredHeight = cardRt.rect.height;
        }
        placeholderLayout.flexibleWidth = 0f;
        placeholderLayout.flexibleHeight = 0f;

        // Visual highlight for insertion point.
        var placeholderImage = draggingPlaceholder.AddComponent<Image>();
        placeholderImage.raycastTarget = false;
        placeholderImage.color = new Color(1f, 1f, 1f, 0.10f);
        var outline = draggingPlaceholder.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Keep the card under the hand container (stable scaling/layout), but ignore layout while dragging.
        card.transform.SetParent(handContainer, worldPositionStays: true);
        card.transform.SetAsLastSibling();

        draggingLayoutElement = card.GetComponent<LayoutElement>();
        if (draggingLayoutElement == null)
        {
            draggingLayoutElement = card.gameObject.AddComponent<LayoutElement>();
        }
        draggingLayoutElement.ignoreLayout = true;

        draggingCanvasGroup = card.GetComponent<CanvasGroup>();
        if (draggingCanvasGroup == null)
        {
            draggingCanvasGroup = card.gameObject.AddComponent<CanvasGroup>();
        }
        draggingCanvasGroup.blocksRaycasts = false;
        draggingCanvasGroup.alpha = 0.9f;

        UpdateDraggedCardPosition(eventData);
        UpdatePlaceholderIndex(eventData);
    }

    public void OnCardDrag(CardDisplay card, PointerEventData eventData)
    {
        if (inputLocked) return;
        if (draggingCard == null) return;
        if (card != draggingCard) return;

        UpdateDraggedCardPosition(eventData);
        UpdatePlaceholderIndex(eventData);
    }

    public void OnCardEndDrag(CardDisplay card, PointerEventData eventData)
    {
        if (draggingCard == null) return;
        if (card != draggingCard) return;

        var handRt = handContainer as RectTransform;
        var cardRt = draggingCard.transform as RectTransform;
        var placeholderRt = draggingPlaceholder != null ? draggingPlaceholder.transform as RectTransform : null;

        int targetIndex = draggingPlaceholder != null ? draggingPlaceholder.transform.GetSiblingIndex() : 0;

        // Ensure the placeholder's position is up-to-date before we animate.
        if (handRt != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(handRt);
        }

        Vector2 targetPos = placeholderRt != null ? placeholderRt.anchoredPosition : Vector2.zero;

        // Animate to the placeholder, then swap into layout-controlled position.
        if (cardRt != null)
        {
            StartCoroutine(AnimateDropToTarget(targetIndex, cardRt, targetPos));
        }
        else
        {
            FinalizeDrop(targetIndex);
        }
    }

    private System.Collections.IEnumerator AnimateDropToTarget(int targetIndex, RectTransform cardRt, Vector2 targetPos)
    {
        float t = 0f;
        Vector2 startPos = cardRt.anchoredPosition;

        // Keep layout ignored during animation.
        if (draggingLayoutElement != null)
        {
            draggingLayoutElement.ignoreLayout = true;
        }

        while (t < dropAnimDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, dropAnimDuration));
            float eased = 1f - Mathf.Pow(1f - u, 3f);
            cardRt.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, eased);
            yield return null;
        }

        FinalizeDrop(targetIndex);
    }

    private void FinalizeDrop(int targetIndex)
    {
        if (draggingCard == null) return;

        draggingCard.transform.SetSiblingIndex(targetIndex);

        if (draggingLayoutElement != null)
        {
            draggingLayoutElement.ignoreLayout = false;
        }

        if (draggingCanvasGroup != null)
        {
            draggingCanvasGroup.blocksRaycasts = true;
            draggingCanvasGroup.alpha = 1f;
        }

        if (draggingPlaceholder != null)
        {
            Destroy(draggingPlaceholder);
        }

        draggingCard = null;
        draggingPlaceholder = null;
        draggingCanvasGroup = null;
        draggingLayoutElement = null;
        lastDragEndTime = Time.unscaledTime;
        if (dimOtherCardsWhileDragging)
        {
            SetCardsDimmed(false, exclude: null);
        }

        SyncHandOrderFromVisuals();
        UpdatePatternPreview();
        UpdateActionButtons();
    }

    private void UpdateDraggedCardPosition(PointerEventData eventData)
    {
        if (draggingCard == null) return;
        if (handContainer == null) return;
        var containerRt = handContainer as RectTransform;
        if (containerRt == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRt,
            eventData.position,
            GetEventCamera(eventData),
            out var localPoint))
        {
            var rt = draggingCard.transform as RectTransform;
            if (rt != null)
            {
                float baselineY = dragBaselineY;

                var desired = localPoint;
                desired.y = Mathf.Clamp(desired.y, baselineY - dragYBand, baselineY + dragYBand);

                desired = ClampAnchoredPositionToContainer(containerRt, rt, desired);
                rt.anchoredPosition = Vector2.SmoothDamp(
                    rt.anchoredPosition,
                    desired,
                    ref dragVelocity,
                    Mathf.Max(0.01f, dragFollowTime),
                    maxDragSpeed,
                    Time.unscaledDeltaTime);
            }
        }
    }

    private static Vector2 ClampAnchoredPositionToContainer(RectTransform containerRt, RectTransform cardRt, Vector2 desiredAnchoredPos)
    {
        // Both positions are in container local-space (anchoredPosition uses the parent rect as reference).
        var containerRect = containerRt.rect;
        var cardRect = cardRt.rect;

        // Approximate half-extents in parent space (include current local scale).
        float halfW = Mathf.Abs(cardRect.width * cardRt.localScale.x) * 0.5f;
        float halfH = Mathf.Abs(cardRect.height * cardRt.localScale.y) * 0.5f;

        float minX = containerRect.xMin + halfW;
        float maxX = containerRect.xMax - halfW;
        float minY = containerRect.yMin + halfH;
        float maxY = containerRect.yMax - halfH;

        // If the card is larger than the container, fall back to centering on that axis.
        float x = (minX > maxX) ? 0f : Mathf.Clamp(desiredAnchoredPos.x, minX, maxX);
        float y = (minY > maxY) ? 0f : Mathf.Clamp(desiredAnchoredPos.y, minY, maxY);

        return new Vector2(x, y);
    }

    private void SetCardsDimmed(bool dim, CardDisplay exclude)
    {
        float target = dim ? 0.82f : 1f;

        foreach (var display in cardDisplays)
        {
            if (display == null) continue;
            if (exclude != null && display == exclude) continue;

            var group = display.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = display.gameObject.AddComponent<CanvasGroup>();
            }
            group.alpha = target;
        }
    }

    private void UpdatePlaceholderIndex(PointerEventData eventData)
    {
        if (draggingPlaceholder == null) return;
        if (handContainer == null) return;
        if (placeholderUpdateInterval > 0f && Time.unscaledTime - lastPlaceholderUpdateTime < placeholderUpdateInterval) return;
        lastPlaceholderUpdateTime = Time.unscaledTime;

        var containerRt = handContainer as RectTransform;
        if (containerRt == null) return;
        if (eventData == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRt,
            eventData.position,
            GetEventCamera(eventData),
            out var localPoint))
        {
            return;
        }

        int layoutChildCount = GetLayoutChildCount();
        if (layoutChildCount <= 0) return;

        if (handGrid != null)
        {
            int gridIndex = GetGridSiblingIndex(containerRt, localPoint, layoutChildCount);
            if (draggingPlaceholder.transform.GetSiblingIndex() != gridIndex)
            {
                draggingPlaceholder.transform.SetSiblingIndex(gridIndex);
            }
            return;
        }

        float draggedX = localPoint.x;
        int currentIndex = draggingPlaceholder.transform.GetSiblingIndex();
        int newIndex = layoutChildCount;

        for (int i = 0; i < handContainer.childCount; i++)
        {
            var child = handContainer.GetChild(i);
            if (child == draggingPlaceholder.transform) continue;
            if (draggingCard != null && child == draggingCard.transform) continue;

            var childRt = child as RectTransform;
            if (childRt == null) continue;

            // Work in the handContainer's local space for stability across resolutions/cameras.
            float childX = childRt.anchoredPosition.x;

            // Add hysteresis so the placeholder doesn't flicker when hovering near a boundary.
            float deadzone = Mathf.Max(0f, placeholderDeadzone);
            float threshold = i < currentIndex ? (childX - deadzone) : (childX + deadzone);

            if (draggedX < threshold)
            {
                newIndex = i;
                break;
            }
        }

        if (draggingPlaceholder.transform.GetSiblingIndex() != newIndex)
        {
            // Clamp to valid range (SetSiblingIndex expects 0..childCount-1).
            int clamped = Mathf.Clamp(newIndex, 0, Mathf.Max(0, layoutChildCount - 1));
            draggingPlaceholder.transform.SetSiblingIndex(clamped);
        }
    }

    private Camera GetEventCamera(PointerEventData eventData)
    {
        if (eventData != null && eventData.pressEventCamera != null)
        {
            return eventData.pressEventCamera;
        }

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            return rootCanvas.worldCamera;
        }

        return null;
    }

    private int GetLayoutChildCount()
    {
        if (handContainer == null) return 0;
        int count = 0;
        for (int i = 0; i < handContainer.childCount; i++)
        {
            var child = handContainer.GetChild(i);
            if (draggingCard != null && child == draggingCard.transform) continue;
            var layout = child.GetComponent<LayoutElement>();
            if (layout != null && layout.ignoreLayout) continue;
            count++;
        }
        return count;
    }

    private int GetGridSiblingIndex(RectTransform containerRt, Vector2 localPoint, int layoutChildCount)
    {
        var padding = handGrid.padding;
        float cellW = handGrid.cellSize.x;
        float cellH = handGrid.cellSize.y;
        float spacingX = handGrid.spacing.x;
        float spacingY = handGrid.spacing.y;

        int columns = 1;
        if (handGrid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            columns = Mathf.Max(1, handGrid.constraintCount);
        }
        else if (handGrid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            int rows = Mathf.Max(1, handGrid.constraintCount);
            columns = Mathf.Max(1, Mathf.CeilToInt(layoutChildCount / (float)rows));
        }
        else
        {
            float availableWidth = containerRt.rect.width - padding.left - padding.right;
            float denom = Mathf.Max(1f, cellW + spacingX);
            columns = Mathf.Max(1, Mathf.FloorToInt((availableWidth + spacingX) / denom));
        }

        int rowsEstimate = Mathf.Max(1, Mathf.CeilToInt(layoutChildCount / (float)columns));

        float startX = containerRt.rect.xMin + padding.left;
        float startY = containerRt.rect.yMax - padding.top;
        float relX = localPoint.x - startX;
        float relY = startY - localPoint.y;

        int col = Mathf.FloorToInt(relX / Mathf.Max(1f, cellW + spacingX));
        int row = Mathf.FloorToInt(relY / Mathf.Max(1f, cellH + spacingY));

        col = Mathf.Clamp(col, 0, columns - 1);
        row = Mathf.Clamp(row, 0, rowsEstimate - 1);

        bool flipX = handGrid.startCorner == GridLayoutGroup.Corner.UpperRight ||
                     handGrid.startCorner == GridLayoutGroup.Corner.LowerRight;
        bool flipY = handGrid.startCorner == GridLayoutGroup.Corner.LowerLeft ||
                     handGrid.startCorner == GridLayoutGroup.Corner.LowerRight;

        if (flipX)
        {
            col = columns - 1 - col;
        }
        if (flipY)
        {
            row = rowsEstimate - 1 - row;
        }

        int index = handGrid.startAxis == GridLayoutGroup.Axis.Horizontal
            ? (row * columns + col)
            : (col * rowsEstimate + row);

        int maxIndex = Mathf.Max(0, layoutChildCount - 1);
        return Mathf.Clamp(index, 0, maxIndex);
    }

    private void SyncHandOrderFromVisuals()
    {
        if (cachedGameState == null) return;
        if (handContainer == null) return;
        if (cardDisplays.Count == 0) return;

        // Rebuild the display list to match current sibling order.
        cardDisplays.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        // Update the underlying hand order to match the new visual order.
        if (cachedGameState.Hand != null && cachedGameState.Hand.Count == cardDisplays.Count)
        {
            var ordered = cardDisplays.Select(cd => cd.CardData).ToList();
            cachedGameState.Hand.Clear();
            cachedGameState.Hand.AddRange(ordered);
        }
    }

    private void UpdateActionButtons()
    {
        if (cachedActionButtons == null)
        {
            cachedActionButtons = FindObjectOfType<ActionButtons>();
        }

        if (cachedActionButtons != null)
        {
            cachedActionButtons.UpdateButtonStates();
        }
    }

    private void UpdatePatternPreview()
    {
        if (patternPreviewText == null)
        {
            return;
        }

        var game = cachedGameState ?? GameManager.Instance?.CurrentGame;

        bool mustDiscard = game != null && game.MustDiscard;
        patternPreviewText.color = mustDiscard ? Color.red : Color.yellow;
        
        var selectedCards = cardDisplays
            .Where(cd => cd.IsSelected)
            .Select(cd => cd.CardData)
            .ToList();

        if (selectedCards.Count == 0)
        {
            string basePrompt = mustDiscard
                ? "Hand over limit: play a pattern or select cards and press Discard"
                : "Select cards to see patterns";

            var allowed = GameManager.Instance != null ? GameManager.Instance.AllowedPatterns : null;
            if (allowed != null && allowed.Count > 0)
            {
                basePrompt += $"\nAllowed: {string.Join(", ", allowed.Select(p => p.ToDisplayName()))}";
            }

            patternPreviewText.text = basePrompt;
            return;
        }

        if (mustDiscard && selectedCards.Count == 1)
        {
            patternPreviewText.text = "Press Discard (or select more to play a pattern)";
            return;
        }

        var validator = GameManager.Instance != null ? GameManager.Instance.PatternValidator : null;
        var patterns = validator != null ? validator.DetectPatterns(selectedCards) : new List<IPattern>();

        if (patterns.Count == 0)
        {
            patternPreviewText.text = $"Selected: {selectedCards.Count} cards - No valid pattern";
        }
        else
        {
            var best = PatternSelection.GetBestPatternForGoals(validator, selectedCards, cachedGameState != null ? cachedGameState.Goals : null);
            if (best.HasValue)
            {
                patternPreviewText.text = $"{best.Value.pattern.Name} - {best.Value.score} points";
            }
        }
    }

    public List<int> GetSelectedIndices()
    {
        var indices = new List<int>();
        for (int i = 0; i < cardDisplays.Count; i++)
        {
            if (cardDisplays[i].IsSelected)
            {
                indices.Add(i);
            }
        }
        return indices;
    }

    public void ClearSelection()
    {
        foreach (var display in cardDisplays)
        {
            display.SetSelected(false);
        }
        UpdatePatternPreview();
        UpdateActionButtons();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputLocked = !enabled;
        if (inputLocked)
        {
            ClearSelection();
        }
    }

    private void HandleGameStarted(GameState game)
    {
        if (game == null) return;

        if (cachedGameState != null)
        {
            cachedGameState.OnHandChanged -= RefreshHand;
        }

        cachedGameState = game;
        cachedGameState.OnHandChanged += RefreshHand;
        inputLocked = false;
        ClearSelection();
        RefreshHand();
    }
}
