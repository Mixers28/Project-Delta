using System;

[Serializable]
public class Goal
{
    public enum GoalType
    {
        Pair,
        ThreeOfKind,
        Run3,
        Run4,
        Run5,
        Flush,
        FullHouse,
        TotalScore,

        // New run families (tutorial/early vs advanced)
        StraightRun3,
        StraightRun4,
        StraightRun5,
        SuitedRun3,
        SuitedRun4,
        SuitedRun5,

        SuitSet3Plus,
        ColorSet3Plus
    }

    public GoalType type;
    public int required;
    public int current;

    public Goal(GoalType goalType, int requiredCount)
    {
        type = goalType;
        required = requiredCount;
        current = 0;
    }

    public bool IsComplete => current >= required;

    public float Progress => (float)current / required;

    public string DisplayText
    {
        get
        {
            string typeName = type switch
            {
                GoalType.Pair => "Pairs",
                GoalType.ThreeOfKind => "Three of a Kind",
                GoalType.Run3 => "Runs of 3",
                GoalType.Run4 => "Runs of 4",
                GoalType.Run5 => "Runs of 5+",
                GoalType.StraightRun3 => "Straight Runs of 3",
                GoalType.StraightRun4 => "Straight Runs of 4",
                GoalType.StraightRun5 => "Straight Runs of 5+",
                GoalType.SuitedRun3 => "Suited Runs of 3",
                GoalType.SuitedRun4 => "Suited Runs of 4",
                GoalType.SuitedRun5 => "Suited Runs of 5+",
                GoalType.SuitSet3Plus => "Suit Sets (3+)",
                GoalType.ColorSet3Plus => "Color Sets (3+)",
                GoalType.Flush => "Flushes",
                GoalType.FullHouse => "Full Houses",
                GoalType.TotalScore => "Score",
                _ => "Unknown"
            };

            return $"{typeName}: {current}/{required}";
        }
    }

    // Check if a pattern satisfies this goal
    public bool MatchesPattern(IPattern pattern)
    {
        switch (type)
        {
            case GoalType.Pair:
                return pattern.Id == PatternId.Pair;

            case GoalType.ThreeOfKind:
                return pattern.Id == PatternId.ThreeOfKind || pattern.Id == PatternId.FourOfKind;

            // Legacy suited-run goals (kept to preserve existing behavior).
            case GoalType.Run3:
                // "Run*" goals mean "any run" for now: straight (suit-agnostic) or suited.
                return pattern.Id == PatternId.SuitedRun3 ||
                       pattern.Id == PatternId.SuitedRun4 ||
                       pattern.Id == PatternId.SuitedRun5 ||
                       pattern.Id == PatternId.StraightRun3 ||
                       pattern.Id == PatternId.StraightRun4 ||
                       pattern.Id == PatternId.StraightRun5;

            case GoalType.Run4:
                return pattern.Id == PatternId.SuitedRun4 ||
                       pattern.Id == PatternId.SuitedRun5 ||
                       pattern.Id == PatternId.StraightRun4 ||
                       pattern.Id == PatternId.StraightRun5;

            case GoalType.Run5:
                return pattern.Id == PatternId.SuitedRun5 ||
                       pattern.Id == PatternId.StraightRun5;

            // New run families for tutorial/advanced gating.
            case GoalType.StraightRun3:
                return pattern.Id == PatternId.StraightRun3 ||
                       pattern.Id == PatternId.StraightRun4 ||
                       pattern.Id == PatternId.StraightRun5;

            case GoalType.StraightRun4:
                return pattern.Id == PatternId.StraightRun4 ||
                       pattern.Id == PatternId.StraightRun5;

            case GoalType.StraightRun5:
                return pattern.Id == PatternId.StraightRun5;

            case GoalType.SuitedRun3:
                return pattern.Id == PatternId.SuitedRun3 ||
                       pattern.Id == PatternId.SuitedRun4 ||
                       pattern.Id == PatternId.SuitedRun5;

            case GoalType.SuitedRun4:
                return pattern.Id == PatternId.SuitedRun4 ||
                       pattern.Id == PatternId.SuitedRun5;

            case GoalType.SuitedRun5:
                return pattern.Id == PatternId.SuitedRun5;

            case GoalType.Flush:
                return pattern.Id == PatternId.Flush5 || pattern.Id == PatternId.RoyalFlush5;

            case GoalType.FullHouse:
                return pattern.Id == PatternId.FullHouse5;

            case GoalType.SuitSet3Plus:
                return pattern.Id == PatternId.SuitSet3Plus;

            case GoalType.ColorSet3Plus:
                return pattern.Id == PatternId.ColorSet3Plus;

            case GoalType.TotalScore:
                return false; // Score goals handled separately

            default:
                return false;
        }
    }

    public void Increment(int amount = 1)
    {
        current += amount;
    }

    public void Reset()
    {
        current = 0;
    }
}
