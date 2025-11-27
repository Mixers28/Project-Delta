Assets/
├── Scripts/
│   ├── GameScripts.asmdef
│   ├── Models/
│   │   ├── Card.cs (54 cards, jokers, symbols)
│   │   ├── Deck.cs (shuffle, draw, discard)
│   │   └── Goal.cs (7 goal types, progress tracking)
│   ├── Patterns/
│   │   ├── IPattern.cs (interface + multipliers)
│   │   ├── PairPattern.cs
│   │   ├── ThreeOfKindPattern.cs
│   │   ├── RunPattern.cs (3/4/5+)
│   │   ├── FlushPattern.cs
│   │   ├── FullHousePattern.cs
│   │   └── PatternValidator.cs (detects all patterns)
│   ├── Managers/
│   │   ├── GameState.cs (hand, score, goals, events)
│   │   └── GameManager.cs (level config, orchestration)
│   └── UI/
│       ├── CardDisplay.cs (individual card rendering)
│       ├── HandDisplay.cs (manages 7 cards, selection)
│       ├── GameHUD.cs (score/moves/goals/deck display)
│       └── ActionButtons.cs (draw/play/discard controls)
└── Tests/
    ├── PlayModeTests.asmdef
    └── PatternTests.cs (20+ tests)