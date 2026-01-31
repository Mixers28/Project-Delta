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
    public void Step2_IsThreeOfKindOnly()
    {
        var level = TutorialLevelFactory.Create(2);
        Assert.AreEqual(1, level.allowedPatterns.Count);
        Assert.AreEqual(PatternId.ThreeOfKind, level.allowedPatterns[0]);
        Assert.AreEqual(1, level.goals.Count);
        Assert.AreEqual(Goal.GoalType.ThreeOfKind, level.goals[0].goalType);
    }

    [Test]
    public void Step3_IsFourOfKindOnly()
    {
        var level = TutorialLevelFactory.Create(3);
        Assert.AreEqual(1, level.allowedPatterns.Count);
        Assert.AreEqual(PatternId.FourOfKind, level.allowedPatterns[0]);
        Assert.AreEqual(1, level.goals.Count);
        Assert.AreEqual(Goal.GoalType.FourOfKind, level.goals[0].goalType);
    }

    [Test]
    public void Step4_UsesSuitedRunAndNoDeprecatedSets()
    {
        var level = TutorialLevelFactory.Create(4);
        Assert.IsTrue(level.goals.Exists(g => g.goalType == Goal.GoalType.SuitedRun3));
        Assert.IsFalse(level.allowedPatterns.Contains(PatternId.SuitSet3Plus));
        Assert.IsFalse(level.allowedPatterns.Contains(PatternId.ColorSet3Plus));
        Assert.IsFalse(level.goals.Exists(g => g.goalType == Goal.GoalType.SuitSet3Plus));
        Assert.IsFalse(level.goals.Exists(g => g.goalType == Goal.GoalType.ColorSet3Plus));
    }

    [Test]
    public void Step5_IncludesStraightRunGoal()
    {
        var level = TutorialLevelFactory.Create(5);
        Assert.IsTrue(level.goals.Exists(g => g.goalType == Goal.GoalType.StraightRun3));
    }

    [Test]
    public void Step6_IncludesColorRunGoal()
    {
        var level = TutorialLevelFactory.Create(6);
        Assert.IsTrue(level.goals.Exists(g => g.goalType == Goal.GoalType.ColorRun3));
    }

    [Test]
    public void Step7_IncludesFlushGoal()
    {
        var level = TutorialLevelFactory.Create(7);
        Assert.IsTrue(level.goals.Exists(g => g.goalType == Goal.GoalType.Flush));
    }
}
