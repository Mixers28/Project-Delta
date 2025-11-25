using System.Collections.Generic;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

public class PatternTests
{
    [Test]
    public void DeckHas54Cards()
    {
        var deck = new Deck();
        Assert.AreEqual(54, deck.DrawPileCount);
    }

    [Test]
    public void CanDrawFromDeck()
    {
        var deck = new Deck();
        var card = deck.DrawFromStock();
        
        Assert.IsNotNull(card);
        Assert.AreEqual(53, deck.DrawPileCount);
    }

    [Test]
    public void ValidPairIsDetected()
    {
        var pair = new PairPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven)
        };
        
        Assert.IsTrue(pair.Validate(cards));
    }

    [Test]
    public void InvalidPairIsRejected()
    {
        var pair = new PairPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Eight)
        };
        
        Assert.IsFalse(pair.Validate(cards));
    }

    [Test]
    public void PairWithJokerIsValid()
    {
        var pair = new PairPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Ace),
            new Card(Card.Suit.Joker, Card.Rank.Joker)
        };
        
        Assert.IsTrue(pair.Validate(cards));
    }

    [Test]
    public void ValidRunIsDetected()
    {
        var run = new RunPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.Six),
            new Card(Card.Suit.Hearts, Card.Rank.Seven)
        };
        
        Assert.IsTrue(run.Validate(cards));
    }

    [Test]
    public void MixedSuitRunIsRejected()
    {
        var run = new RunPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Spades, Card.Rank.Six),
            new Card(Card.Suit.Hearts, Card.Rank.Seven)
        };
        
        Assert.IsFalse(run.Validate(cards));
    }

    [Test]
    public void RunWithJokerGapIsValid()
    {
        var run = new RunPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Clubs, Card.Rank.Five),
            new Card(Card.Suit.Joker, Card.Rank.Joker),
            new Card(Card.Suit.Clubs, Card.Rank.Seven)
        };
        
        Assert.IsTrue(run.Validate(cards));
    }

    [Test]
    public void PairBasePointsAreCorrect()
    {
        var pair = new PairPattern();
        Assert.AreEqual(10, pair.BasePoints);
    }

    [Test]
    public void PairWithRainbowBonus()
    {
        var pair = new PairPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven)
        };
        
        // Different suits = Rainbow multiplier: 10 × 1.2 = 12
        Assert.AreEqual(12, pair.CalculateScore(cards));
    }

    [Test]
    public void PairWithJokerMultiplier()
    {
        var pair = new PairPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Joker, Card.Rank.Joker)
        };
        
        // Joker multiplier: 10 × 1.5 = 15
        Assert.AreEqual(15, pair.CalculateScore(cards));
    }

    [Test]
    public void PatternValidatorDetectsPair()
    {
        var validator = new PatternValidator();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven)
        };
        
        var patterns = validator.DetectPatterns(cards);
        
        Assert.AreEqual(1, patterns.Count);
        Assert.AreEqual("Pair", patterns[0].Name);
    }

    // NEW TESTS FOR DAY 2

    [Test]
    public void ValidFlushIsDetected()
    {
        var flush = new FlushPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Two),
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Hearts, Card.Rank.Nine),
            new Card(Card.Suit.Hearts, Card.Rank.King)
        };
        
        Assert.IsTrue(flush.Validate(cards));
    }

    [Test]
    public void MixedSuitFlushIsRejected()
    {
        var flush = new FlushPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Two),
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Spades, Card.Rank.Seven),
            new Card(Card.Suit.Hearts, Card.Rank.Nine),
            new Card(Card.Suit.Hearts, Card.Rank.King)
        };
        
        Assert.IsFalse(flush.Validate(cards));
    }

    [Test]
    public void ValidFullHouseIsDetected()
    {
        var fullHouse = new FullHousePattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven),
            new Card(Card.Suit.Clubs, Card.Rank.Seven),
            new Card(Card.Suit.Hearts, Card.Rank.Ace),
            new Card(Card.Suit.Spades, Card.Rank.Ace)
        };
        
        Assert.IsTrue(fullHouse.Validate(cards));
    }

    [Test]
    public void FullHouseWithJokerIsValid()
    {
        var fullHouse = new FullHousePattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven),
            new Card(Card.Suit.Joker, Card.Rank.Joker),
            new Card(Card.Suit.Hearts, Card.Rank.Ace),
            new Card(Card.Suit.Spades, Card.Rank.Ace)
        };
        
        Assert.IsTrue(fullHouse.Validate(cards));
    }

    [Test]
    public void GameStateInitializes()
    {
        var goals = new List<Goal> { new Goal(Goal.GoalType.Pair, 2) };
        var gameState = new GameState(goals, 10);
        
        Assert.AreEqual(0, gameState.Score);
        Assert.AreEqual(10, gameState.MovesRemaining);
        Assert.AreEqual(0, gameState.Hand.Count);
    }

    [Test]
    public void GameStateDealsSevenCards()
    {
        var goals = new List<Goal> { new Goal(Goal.GoalType.Pair, 2) };
        var gameState = new GameState(goals, 10);
        
        gameState.DealInitialHand();
        
        Assert.AreEqual(7, gameState.Hand.Count);
    }

    [Test]
    public void PlayingPatternIncreasesScore()
    {
        var goals = new List<Goal> { new Goal(Goal.GoalType.Pair, 1) };
        var gameState = new GameState(goals, 10);
        
        // Manually add cards to hand
        var card1 = new Card(Card.Suit.Hearts, Card.Rank.Seven);
        var card2 = new Card(Card.Suit.Spades, Card.Rank.Seven);
        gameState.Hand.Add(card1);
        gameState.Hand.Add(card2);
        
        var pattern = new PairPattern();
        var cards = new List<Card> { card1, card2 };
        
        gameState.PlayPattern(cards, pattern);
        
        Assert.Greater(gameState.Score, 0);
    }

    [Test]
    public void GoalUpdatesWhenPatternPlayed()
    {
        var goals = new List<Goal> { new Goal(Goal.GoalType.Pair, 2) };
        var gameState = new GameState(goals, 10);
        
        var card1 = new Card(Card.Suit.Hearts, Card.Rank.Seven);
        var card2 = new Card(Card.Suit.Spades, Card.Rank.Seven);
        gameState.Hand.Add(card1);
        gameState.Hand.Add(card2);
        
        var pattern = new PairPattern();
        var cards = new List<Card> { card1, card2 };
        
        gameState.PlayPattern(cards, pattern);
        
        Assert.AreEqual(1, goals[0].current);
    }
}