using System;
using System.Collections.Generic;

[Serializable]
public class SavedGameSession
{
    public int version = 1;
    public int levelIndex = 0;
    public int progressionStep = 0;
    public int totalMoves = 0;
    public int movesRemaining = 0;
    public int score = 0;
    public string levelName = string.Empty;
    public string deckDescription = string.Empty;
    public RuleTier ruleTier = RuleTier.Early;
    public bool advancedRuns = false;

    public List<PatternId> allowedPatterns = new();
    public List<Card> drawPile = new();
    public List<Card> discardPile = new();
    public List<Card> hand = new();
    public List<GoalSnapshot> goals = new();

    public bool IsValid()
    {
        return totalMoves > 0 && movesRemaining >= 0 && goals != null && drawPile != null && discardPile != null && hand != null;
    }
}

[Serializable]
public class GoalSnapshot
{
    public Goal.GoalType type;
    public int required;
    public int current;

    public GoalSnapshot()
    {
    }

    public GoalSnapshot(Goal goal)
    {
        if (goal == null) return;
        type = goal.type;
        required = goal.required;
        current = goal.current;
    }
}
