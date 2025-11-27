using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardSpriteLibrary", menuName = "Card Game/Sprite Library")]
public class CardSpriteLibrary : ScriptableObject
{
    [System.Serializable]
    public class CardSprite
    {
        public Card.Suit suit;
        public Card.Rank rank;
        public Sprite sprite;
    }

    public List<CardSprite> cardSprites = new();
    public Sprite cardBack;
    public Sprite jokerSprite;

    private Dictionary<(Card.Suit, Card.Rank), Sprite> spriteLookup;

    private void OnEnable()
    {
        BuildSpriteLookup();
    }

    private void BuildSpriteLookup()
    {
        spriteLookup = new Dictionary<(Card.Suit, Card.Rank), Sprite>();

        foreach (var cardSprite in cardSprites)
        {
            if (cardSprite.sprite != null)
            {
                spriteLookup[(cardSprite.suit, cardSprite.rank)] = cardSprite.sprite;
            }
        }
    }

    public Sprite GetSprite(Card card)
    {
        if (card.IsJoker)
        {
            return jokerSprite;
        }

        if (spriteLookup == null)
        {
            BuildSpriteLookup();
        }

        if (spriteLookup.TryGetValue((card.suit, card.rank), out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"No sprite found for {card.suit} {card.rank}");
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildSpriteLookup();
    }
#endif
}