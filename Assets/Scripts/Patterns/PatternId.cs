public enum PatternId
{
    Pair,
    ThreeOfKind,

    FourOfKind,

    SuitedRun3,
    SuitedRun4,
    SuitedRun5,

    StraightRun3,
    StraightRun4,
    StraightRun5,

    // Deprecated (Suit/Color sets removed from gameplay; kept for serialization compatibility).
    SuitSet3Plus,
    ColorSet3Plus,

    Flush5,
    FullHouse5,

    RoyalFlush5,

    // Color-based runs (same color, consecutive ranks).
    ColorRun3,
    ColorRun4
}
