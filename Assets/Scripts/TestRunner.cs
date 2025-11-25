using UnityEngine;
using System.Collections.Generic;

public class TestRunner : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== RUNNING TESTS ===");
        
        TestPairValidation();
        TestPairWithJoker();
        TestRunValidation();
        TestRunWithJoker();
        TestScoreMultiplier();
        TestDeckInitialization();
        
        Debug.Log("=== TESTS COMPLETE ===");
    }

    void TestPairValidation()
    {
        var pair = new PairPattern();
        
        var cards1 = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven)
        };
        Assert(pair.Validate(cards1), "Valid pair should pass");
        
        var cards2 = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Eight)
        };
        Assert(!pair.Validate(cards2), "Invalid pair should fail");
        
        Debug.Log("✓ TestPairValidation passed");
    }

    void TestPairWithJoker()
    {
        var pair = new PairPattern();
        
        var cards = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Ace),
            new Card(Card.Suit.Joker, Card.Rank.Joker)
        };
        Assert(pair.Validate(cards), "Pair with joker should be valid");
        
        Debug.Log("✓ TestPairWithJoker passed");
    }

    void TestRunValidation()
    {
        var run = new RunPattern(3);
        
        var cards1 = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Hearts, Card.Rank.Six),
            new Card(Card.Suit.Hearts, Card.Rank.Seven)
        };
        Assert(run.Validate(cards1), "Valid run should pass");
        
        var cards2 = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Five),
            new Card(Card.Suit.Spades, Card.Rank.Six),
            new Card(Card.Suit.Hearts, Card.Rank.Seven)
        };
        Assert(!run.Validate(cards2), "Mixed suit run should fail");
        
        Debug.Log("✓ TestRunValidation passed");
    }

    void TestRunWithJoker()
    {
        var run = new RunPattern(3);
        
        var cards = new List<Card>
        {
            new Card(Card.Suit.Clubs, Card.Rank.Five),
            new Card(Card.Suit.Joker, Card.Rank.Joker),
            new Card(Card.Suit.Clubs, Card.Rank.Seven)
        };
        Assert(run.Validate(cards), "Run with joker gap should be valid");
        
        Debug.Log("✓ TestRunWithJoker passed");
    }

    void TestScoreMultiplier()
    {
        var pair = new PairPattern();
        
        var cards1 = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Spades, Card.Rank.Seven)
        };
        AssertEqual(10, pair.CalculateScore(cards1), "Base pair score");
        
        var cards2 = new List<Card>
        {
            new Card(Card.Suit.Hearts, Card.Rank.Seven),
            new Card(Card.Suit.Joker, Card.Rank.Joker)
        };
        AssertEqual(15, pair.CalculateScore(cards2), "Pair with joker score");
        
        Debug.Log("✓ TestScoreMultiplier passed");
    }

    void TestDeckInitialization()
    {
        var deck = new Deck();
        AssertEqual(54, deck.DrawPileCount, "Deck should have 54 cards");
        
        Debug.Log("✓ TestDeckInitialization passed");
    }

    void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError($"ASSERTION FAILED: {message}");
        }
    }

    void AssertEqual(int expected, int actual, string message)
    {
        if (expected != actual)
        {
            Debug.LogError($"ASSERTION FAILED: {message} (expected {expected}, got {actual})");
        }
    }
}