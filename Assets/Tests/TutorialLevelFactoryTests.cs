using NUnit.Framework;

public class TutorialLevelFactoryTests
{
    [Test]
    public void Step1_IsPairsOnly()
    {
        var level = TutorialLevelFactory.Create(1);
        Assert.AreEqual("Level 1", level.levelName);
        Assert.AreEqual(1, level.allowedPatterns.Count);
        Assert.AreEqual(PatternId.Pair, level.allowedPatterns[0]);
        Assert.AreEqual(1, level.goals.Count);
        Assert.AreEqual(Goal.GoalType.Pair, level.goals[0].goalType);
        Assert.AreEqual(2, level.goals[0].required);
    }

    [Test]
    public void Step4_IntroducesSuitSet()
    {
        var level = TutorialLevelFactory.Create(4);
        Assert.Contains(PatternId.SuitSet3Plus, level.allowedPatterns);
        Assert.IsTrue(level.goals.Exists(g => g.goalType == Goal.GoalType.SuitSet3Plus));
    }
}

