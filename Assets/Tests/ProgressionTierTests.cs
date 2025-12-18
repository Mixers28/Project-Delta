using NUnit.Framework;

public class ProgressionTierTests
{
    [Test]
    public void CurrentRuleTier_IsTutorial_WhenTutorialActive()
    {
        var profile = new PlayerProfile
        {
            tutorialStep = 1,
            nonTutorialWins = 999
        };
        ProgressionService.SetProfileForTests(profile);

        Assert.IsTrue(ProgressionService.IsTutorialActive);
        Assert.AreEqual(RuleTier.Tutorial, ProgressionService.CurrentRuleTier);
    }

    [Test]
    public void CurrentRuleTier_IsEarly_UntilSevenNonTutorialWins()
    {
        var profile = new PlayerProfile
        {
            tutorialStep = ProgressionService.TutorialMaxStep + 1,
            nonTutorialWins = 6
        };
        ProgressionService.SetProfileForTests(profile);

        Assert.IsFalse(ProgressionService.IsTutorialActive);
        Assert.AreEqual(RuleTier.Early, ProgressionService.CurrentRuleTier);
    }

    [Test]
    public void CurrentRuleTier_IsMid_AtSevenNonTutorialWins()
    {
        var profile = new PlayerProfile
        {
            tutorialStep = ProgressionService.TutorialMaxStep + 1,
            nonTutorialWins = 7
        };
        ProgressionService.SetProfileForTests(profile);

        Assert.IsFalse(ProgressionService.IsTutorialActive);
        Assert.AreEqual(RuleTier.Mid, ProgressionService.CurrentRuleTier);
    }

    [Test]
    public void RecordNonTutorialWin_DoesNotIncrementDuringTutorial()
    {
        var profile = new PlayerProfile
        {
            tutorialStep = 1,
            nonTutorialWins = 0
        };
        ProgressionService.SetProfileForTests(profile);

        ProgressionService.RecordNonTutorialWin();
        Assert.AreEqual(0, ProgressionService.NonTutorialWins);
    }

    [Test]
    public void ApplyRuleTier_Mid_RemovesJokersFromDeck()
    {
        var game = new GameState(goals: new System.Collections.Generic.List<Goal>(), totalMoves: 1);
        Assert.AreEqual(54, game.Deck.DrawPileCount);

        game.ApplyRuleTier(RuleTier.Mid);
        Assert.AreEqual(52, game.Deck.DrawPileCount);
    }
}
