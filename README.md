# Project-Delta
Rummy-Style Card Game
Master Design & Implementation Reference
1. Game Concept Overview
Core Mechanic
Rummy-like hand management with draw pile and discard pile. Players hold 7 cards in hand, draw each turn, form patterns (runs, pairs, sets), and discard to end turn. Goals are pattern-based puzzle challenges with move limits.
Why This Design
•	Fixed hand size enables clear evaluation for leaderboards
•	Clean, mobile-friendly UI with fewer spatial rules
•	Puzzle design: sequence draws/discards to hit patterns within X moves
•	Replayability via Run Mode without synchronous PvP
Competitive Hook: Run Mode
Players play consecutive levels in a single session, collecting stats: most levels completed, highest scoring hand, most patterns in one level. Stats feed leaderboards and unlock titles, badges, card backs, and profile frames.
 
2. Core Gameplay Loop
Per-Level Flow
Start Level
•	Player sees: 7-card hand, draw pile, discard pile top card
•	Goals displayed (e.g., "2 Runs of 4+", "1 Pair")
•	Moves remaining counter visible
Turn Structure
Draw Phase: Choose draw from stock or discard pile. Hand becomes 8 cards.
Play Phase: Select cards, see real-time pattern validation, confirm to play. Playing removes cards and updates goal progress.
Discard Phase: If hand > 7, must discard 1 card. Ends turn, increments moves used.
Level End Conditions
Win: All goals reached. Show score and Run Mode stats.
Lose: Moves exhausted or deck empty without completing goals. Offer retry or end run.
 
3. Technical Specifications
Deck Composition
•	52 standard cards (4 suits × 13 ranks)
•	2 Jokers (wild cards)
•	Total: 54 cards
•	Consistent composition across all levels; difficulty scales via goals/moves
Joker Behavior
•	Acts as any card for pattern completion
•	Cannot be discarded (must be used in pattern)
•	Triggers 1.5× score multiplier when used
Pattern Scoring System
Pattern	Base Points	Requirements
Pair	10	2 same rank
Three of a Kind	30	3 same rank
Run of 3	40	3 consecutive, same suit
Run of 4	80	4 consecutive, same suit
Run of 5+	150	5+ consecutive, same suit
Flush	100	5 same suit
Full House	200	3 of kind + pair
Score Multipliers
Condition	Multiplier
Contains Joker	1.5×
All Face Cards (J, Q, K)	2.0×
Rainbow (all different suits)	1.2×
Consecutive Plays (combo)	1.1× per combo
Pattern Overlap Rules
•	Same card can be in multiple patterns within the same turn
•	Example: 7♥ 8♥ 9♥ can be both "Run of 3" AND "3-card Flush"
•	Player sees all valid patterns, chooses which to submit
•	Once submitted and scored, cards leave hand
Real-Time Validation
•	As player taps cards: green outline = valid pattern
•	Pattern label appears above selection
•	Point preview shows calculated score
•	No "submit and wait" - instant feedback
Win Conditions
•	Goals are cumulative (e.g., "3 Runs + 2 Pairs" total across turns)
•	Can play patterns in any order
•	Exceeding goals: +50 bonus per extra pattern
•	Fail: moves exhausted, deck empty, or player quits
 
4. Run Mode (Competitive)
Entry Flow
•	Player taps "Run Mode (Competitive)" from main menu
•	Choose difficulty (Normal/Hard)
•	See personal best and top 3 global preview
•	Gate: Unlock after beating Level 5 in normal mode
Run Progression
•	Start at Level 1, each level has harder goals and tighter moves
•	After success: update LevelsCompleted++, track HighestSingleLevelScore, BestHandScore
•	Prompt: "Continue to next level?"
•	On fail: Run ends, show final stats
•	Allow save/resume mid-run for short sessions
Run Summary Screen
•	Levels completed this run
•	Best hand achieved
•	Total patterns played
•	Score breakdown
•	Personal best comparison + global ranking
Leaderboard Types
1.	Most Levels in Single Run (primary)
2.	Best Single-Hand Score
3.	Most Patterns in One Level
Options: Global, Friends-only, Weekly reset boards
Achievements & Rewards
Achievement Examples:
•	"Marathoner": Complete 10+ levels in one run
•	"Perfect Hand": Score ≥ X points from single hand
•	"Combo Master": Play 3+ patterns in consecutive turns
Reward Types:
•	Titles: "Run King", "Pattern Master", "Combo Artist"
•	Card backs (special for top 1%)
•	Profile frames
•	Soft currency for boosters/cosmetics
 
5. Development Phases
Phase 1: Core Mechanics (2-3 weeks)
Status: COMPLETE
Deliverables:
•	Hand display (7 cards, tap to select)
•	Draw pile + discard pile interactions
•	Pattern recognition system with all patterns
•	Scoring with multipliers
•	Joker wildcards working
•	Full unit test coverage
•	Win/fail state for test level
Success Metric: Play one level start-to-finish with basic goals
Phase 2: Level Wrapper + Progression (1-2 weeks)
Status: NEXT
Deliverables:
•	Moves counter and fail state when exhausted
•	Level config system (JSON-driven goals/deck/moves)
•	5-10 handcrafted levels with increasing difficulty
•	Level select screen
•	Retry/Continue flow
•	Save/load progress
Success Metric: Progress through multiple levels with varied goals
Phase 3: Run Mode Foundation (2 weeks)
Deliverables:
•	Run Mode entry point (separate from level select)
•	Stats tracking: levels completed, best hand, patterns played
•	Run summary screen with personal bests
•	Save/resume mid-run state
•	Fail = run ends, show stats
Success Metric: Complete multi-level runs with improving personal stats
Phase 4: Competitive Layer (2-3 weeks)
Deliverables:
•	Backend integration (submit stats, retrieve leaderboards)
•	2-3 leaderboard types: Best Run, Best Single Hand
•	Basic anti-cheat validation
•	Leaderboard UI (global top 100 + player position)
Success Metric: Players see where they rank globally
Phase 5: Retention Hooks (2 weeks)
Deliverables:
•	Achievement system (5-10 initial achievements)
•	Cosmetic rewards (3 card backs, 3 titles)
•	Equip/unequip cosmetics
•	"New Personal Best" celebration animations
•	Soft currency + shop basics
Success Metric: Players have reasons to return beyond leaderboards
 
6. Technical Architecture
Pattern Validation Architecture
Rules engine pattern: each pattern type implements IPattern interface. Easy to add new patterns without touching core code.
State Management
Command pattern for moves (enables undo/replay). Serializable game state for save/resume.
Backend Choice
Recommendation: Firebase (low-latency leaderboard reads). Alternative: Custom backend with Redis for leaderboards. Consider Game Center for iOS-only launch.
Project Structure (Unity)
Assets/Scripts/ contains: Models (Card.cs, Deck.cs), Patterns (IPattern interface + implementations), Managers (GameManager.cs), UI components, and Tests folder with unit tests.
7. UX Considerations
Player Feedback & Dopamine
•	Personal best beaten: Big, obvious feedback with celebration
•	Leaderboard placement: Show rank movement (e.g., "#120 → #87")
•	Achievement unlock: Toast + badge popup + "Equip Now" button
•	Pattern completion: Satisfying visual and audio feedback
Onboarding
•	Don't show leaderboards at Level 1
•	Gate Run Mode until Level 5 complete
•	Tutorial level for Joker usage
•	Progressive goal complexity
Anti-Abuse & Fairness
•	Stats only from valid, standard modes (exclude practice/custom)
•	Backend sanity checks for impossible scores
•	Tiebreakers: earlier achievement time wins, or secondary metric
8. Identified Risks & Mitigations
Risk	Mitigation
Pattern overlap UI confusion	Show all detected patterns as buttons; player taps to select
Joker behavior ambiguity	Tutorial level: "Use the Joker to complete this Run"
Validation performance lag	Optimize validator (max 5 cards = small search space); profile early
Pattern selection clunky	Fallback: simplify to single best pattern auto-select
Goals unclear to player	Add visual indicators (icons for each goal type)
 
Document consolidated from project conversations
Last Updated: November 2025
