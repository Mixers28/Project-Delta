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
│       ├── CardDisplay.cs (individual card rendering with sprite support)
│       ├── CardSpriteLibrary.cs (ScriptableObject for card sprites)
│       ├── HandDisplay.cs (manages 7 cards, selection)
│       ├── GameHUD.cs (score/moves/goals/deck display)
│       ├── GameOverPanel.cs (win/lose screen)
│       └── ActionButtons.cs (draw/play/discard controls)
├── Data/
│   └── CardSpriteLibrary.asset (sprite mappings)
└── Tests/
    ├── PlayModeTests.asmdef
    └── PatternTests.cs (20+ tests)