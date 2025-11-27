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
        TotalScore
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
            return pattern.Name == "Pair";
            
        case GoalType.ThreeOfKind:
            return pattern.Name == "Three of a Kind";
            
        case GoalType.Run3:
            // Any run of 3 or more counts
            return pattern.Name == "Run of 3" || 
                   pattern.Name == "Run of 4" || 
                   pattern.Name == "Run of 5";
            
        case GoalType.Run4:
            // Any run of 4 or more counts
            return pattern.Name == "Run of 4" || 
                   pattern.Name == "Run of 5";
            
        case GoalType.Run5:
            // Only run of 5+ counts
            return pattern.Name == "Run of 5";
            
        case GoalType.Flush:
            return pattern.Name == "Flush";
            
        case GoalType.FullHouse:
            return pattern.Name == "Full House";
            
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