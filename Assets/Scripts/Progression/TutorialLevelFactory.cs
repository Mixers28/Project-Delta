using System.Collections.Generic;
using UnityEngine;

public static class TutorialLevelFactory
{
    public static LevelDefinition Create(int tutorialStep)
    {
        int step = Mathf.Max(1, tutorialStep);

        var level = ScriptableObject.CreateInstance<LevelDefinition>();
        level.levelName = $"Level {step}";
        level.deckTweaks = new DeckTweakSettings();
        level.deckTweaks.useDeterministicShuffle = true;
        level.deckTweaks.shuffleSeed = 1000 + step;
        level.allowedPatterns = new List<PatternId>();
        level.goals = new List<GoalDefinition>();

        switch (step)
        {
            case 1:
                level.totalMoves = 13; // 12–14
                level.allowedPatterns.Add(PatternId.Pair);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.Pair, required = 2 });
                break;

            case 2:
                level.totalMoves = 13; // 12–14
                level.allowedPatterns.Add(PatternId.ThreeOfKind);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.ThreeOfKind, required = 1 });
                break;

            case 3:
                level.totalMoves = 15; // 14–16
                level.allowedPatterns.Add(PatternId.FourOfKind);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.FourOfKind, required = 1 });
                break;

            case 4:
                level.totalMoves = 17; // 16–18
                level.allowedPatterns.Add(PatternId.SuitedRun3);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.SuitedRun3, required = 1 });
                break;

            case 5:
                level.totalMoves = 17; // 16–18
                level.allowedPatterns.Add(PatternId.StraightRun3);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.StraightRun3, required = 1 });
                break;

            case 6:
                level.totalMoves = 17; // 16–18
                level.allowedPatterns.Add(PatternId.ColorRun3);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.ColorRun3, required = 1 });
                break;

            case 7:
                level.totalMoves = 20; // 18–22
                level.allowedPatterns.Add(PatternId.Flush5);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.Flush, required = 1 });
                break;

            default:
                level.totalMoves = 20; // 18–22
                level.allowedPatterns.Add(PatternId.Pair);
                level.allowedPatterns.Add(PatternId.StraightRun3);
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.Pair, required = 1 });
                level.goals.Add(new GoalDefinition { goalType = Goal.GoalType.StraightRun3, required = 1 });
                break;
        }

        return level;
    }
}
