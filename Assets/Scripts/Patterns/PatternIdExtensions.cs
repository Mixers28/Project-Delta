using System;

public static class PatternIdExtensions
{
    public static string ToDisplayName(this PatternId id)
    {
        return id switch
        {
            PatternId.Pair => "Pair",
            PatternId.ThreeOfKind => "Three of a Kind",

            PatternId.SuitedRun3 => "Suited Run (3+)",
            PatternId.SuitedRun4 => "Suited Run (4+)",
            PatternId.SuitedRun5 => "Suited Run (5+)",

            PatternId.StraightRun3 => "Straight Run (3+)",
            PatternId.StraightRun4 => "Straight Run (4+)",
            PatternId.StraightRun5 => "Straight Run (5+)",

            PatternId.SuitSet3Plus => "Suit Set (3+)",
            PatternId.ColorSet3Plus => "Color Set (3+)",

            PatternId.Flush5 => "Flush (5)",
            PatternId.FullHouse5 => "Full House (5)",

            _ => id.ToString()
        };
    }
}

