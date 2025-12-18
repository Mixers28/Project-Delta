using System.Collections.Generic;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using System.Linq;

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
    public void DrawFromDiscardReturnsTopCard()
    {
        var deck = new Deck();

        var bottom = new Card(Card.Suit.Hearts, Card.Rank.Two);
        var top = new Card(Card.Suit.Spades, Card.Rank.Ace);

        deck.AddToDiscard(bottom);
        deck.AddToDiscard(top);

        var drawn = deck.DrawFromDiscard();
        Assert.IsTrue(drawn.HasValue);
        Assert.AreEqual(top, drawn.Value);
        Assert.AreEqual(1, deck.DiscardPileCount);
        Assert.AreEqual(bottom, deck.TopDiscard.Value);
    }

    [Test]
    public void SeededShuffleIsDeterministic()
    {
        var tweaks = new DeckTweakSettings
        {
            useDeterministicShuffle = true,
            shuffleSeed = 12345,
            shuffleAfterTweaks = true
        };

        var deckA = new Deck();
        deckA.ApplyTweaks(tweaks);
        var deckB = new Deck();
        deckB.ApplyTweaks(tweaks);

        var drawnA = new List<Card>();
        var drawnB = new List<Card>();
        for (int i = 0; i < 10; i++)
        {
            drawnA.Add(deckA.DrawFromStock().Value);
            drawnB.Add(deckB.DrawFromStock().Value);
        }

        CollectionAssert.AreEqual(drawnA, drawnB);
    }

    [Test]
    public void PresetDrawPileIsUsed()
    {
        var tweaks = new DeckTweakSettings
        {
            shuffleAfterTweaks = false
        };
        tweaks.presetDrawPile.Add(new Card(Card.Suit.Hearts, Card.Rank.Ace));
        tweaks.presetDrawPile.Add(new Card(Card.Suit.Spades, Card.Rank.King));

        var deck = new Deck();
        deck.ApplyTweaks(tweaks);

        var first = deck.DrawFromStock();
        var second = deck.DrawFromStock();
        Assert.AreEqual(new Card(Card.Suit.Hearts, Card.Rank.Ace), first.Value);
        Assert.AreEqual(new Card(Card.Suit.Spades, Card.Rank.King), second.Value);
    }

    [Test]
    public void PatternValidatorHonorsAllowedPatterns()
    {
        var onlyPairs = new PatternValidator(new[] { PatternId.Pair });

        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Spades, Card.Rank.Six),
            new Card(Card.Suit.Clubs, Card.Rank.Seven)
        };

        // This selection forms a straight run (mixed suits), but runs are not allowed.
        var patterns = onlyPairs.DetectPatterns(cards);
        Assert.AreEqual(0, patterns.Count);
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
    public void StraightRunAllowsMixedSuits()
    {
        var run = new StraightRunPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Spades, Card.Rank.Six),
            new Card(Card.Suit.Clubs, Card.Rank.Seven)
        };

        Assert.IsTrue(run.Validate(cards));
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
    public void StraightRunSupportsJokerGapFill()
    {
        var run = new StraightRunPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Clubs, Card.Rank.Five),
            new Card(Card.Suit.Joker, Card.Rank.Joker),
            new Card(Card.Suit.Hearts, Card.Rank.Seven)
        };

        Assert.IsTrue(run.Validate(cards));
    }

    [Test]
    public void LegacyRunGoalCountsStraightRuns()
    {
        var goal = new Goal(Goal.GoalType.Run3, 1);
        var straight = new StraightRunPattern(3);

        Assert.IsTrue(goal.MatchesPattern(straight));
    }

    [Test]
    public void ValidSuitSetIsDetected()
    {
        var suitSet = new SuitSetPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Two),
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.King)
        };

        Assert.IsTrue(suitSet.Validate(cards));
    }

    [Test]
    public void MixedSuitSetIsRejected()
    {
        var suitSet = new SuitSetPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Two),
            new Card(Card.Suit.Spades, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.King)
        };

        Assert.IsFalse(suitSet.Validate(cards));
    }

    [Test]
    public void SuitSetWithJokerIsValid()
    {
        var suitSet = new SuitSetPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Two),
            new Card(Card.Suit.Joker, Card.Rank.Joker),
            new Card(Card.Suit.Hearts, Card.Rank.King)
        };

        Assert.IsTrue(suitSet.Validate(cards));
    }

    [Test]
    public void ValidColorSetIsDetected()
    {
        var colorSet = new ColorSetPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Two),
            new Card(Card.Suit.Diamonds, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.King)
        };

        Assert.IsTrue(colorSet.Validate(cards));
    }

    [Test]
    public void MixedColorSetIsRejected()
    {
        var colorSet = new ColorSetPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Two),
            new Card(Card.Suit.Clubs, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.King)
        };

        Assert.IsFalse(colorSet.Validate(cards));
    }

    [Test]
    public void ColorSetWithJokerIsValid()
    {
        var colorSet = new ColorSetPattern(3);
        var cards = new List<Card>
        {
            new Card(Card.Suit.Clubs, Card.Rank.Two),
            new Card(Card.Suit.Joker, Card.Rank.Joker),
            new Card(Card.Suit.Spades, Card.Rank.King)
        };

        Assert.IsTrue(colorSet.Validate(cards));
    }

    [Test]
    public void GoalRelevantPatternIsPreferredOverHigherBaseScore()
    {
        var validator = new PatternValidator();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.Six),
            new Card(Card.Suit.Hearts, Card.Rank.Queen)
        };

        // This selection is both a Suit Set and a Color Set. If the goal is ColorSet,
        // we should choose Color Set even though Suit Set has a higher base score.
        var goals = new List<Goal> { new Goal(Goal.GoalType.ColorSet3Plus, 1) };

        var best = PatternSelection.GetBestPatternForGoals(validator, cards, goals);
        Assert.IsTrue(best.HasValue);
        Assert.AreEqual(PatternId.ColorSet3Plus, best.Value.pattern.Id);
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
    public void DiscardAlwaysCostsOneMove()
    {
        var goals = new List<Goal> { new Goal(Goal.GoalType.Pair, 1) };
        var gameState = new GameState(goals, 10);

        var card1 = new Card(Card.Suit.Hearts, Card.Rank.Two);
        var card2 = new Card(Card.Suit.Spades, Card.Rank.Three);
        var card3 = new Card(Card.Suit.Clubs, Card.Rank.Four);

        gameState.Hand.Add(card1);
        gameState.Hand.Add(card2);
        gameState.Hand.Add(card3);

        bool ok = gameState.DiscardCards(new List<Card> { card1, card2, card3 });
        Assert.IsTrue(ok);
        Assert.AreEqual(9, gameState.MovesRemaining);
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

    [Test]
    public void FourOfKindIsDetected()
    {
        var four = new FourOfKindPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven),
            new Card(Card.Suit.Clubs, Card.Rank.Seven),
            new Card(Card.Suit.Diamonds, Card.Rank.Seven)
        };

        Assert.IsTrue(four.Validate(cards));
    }

    [Test]
    public void RoyalFlushIsDetected()
    {
        var rf = new RoyalFlushPattern();
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Ten),
            new Card(Card.Suit.Hearts, Card.Rank.Jack),
            new Card(Card.Suit.Hearts, Card.Rank.Queen),
            new Card(Card.Suit.Hearts, Card.Rank.King),
            new Card(Card.Suit.Hearts, Card.Rank.Ace)
        };

        Assert.IsTrue(rf.Validate(cards));
    }

    [Test]
    public void ThreeOfKindGoalCountsFourOfKind()
    {
        var goal = new Goal(Goal.GoalType.ThreeOfKind, 1);
        var four = new FourOfKindPattern();
        Assert.IsTrue(goal.MatchesPattern(four));
    }

    [Test]
    public void FlushGoalCountsRoyalFlush()
    {
        var goal = new Goal(Goal.GoalType.Flush, 1);
        var rf = new RoyalFlushPattern();
        Assert.IsTrue(goal.MatchesPattern(rf));
    }
}
