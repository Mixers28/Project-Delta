Session notes (Codex)
---------------------

- Level progression now uses a `progressionStep`; each Continue advances level difficulty (goals/moves/tweaks), while Retry resets difficulty to 1 (base goals).
- UI (HUD, ActionButtons, HandDisplay, GameOverPanel) rebinds to the new `GameState` on Continue/Retry and hides the overlay properly.
- Playing a pattern no longer consumes a move (makes goals achievable within move limits).
- Level data loads from inspector sequence or `Assets/Resources/Levels/` (Rookie/StreetSmart/HighRoller/GambitGauntlet assets), and runtime variants add difficulty/extra goals per stage.
- HUD overlay now shows only Level and goals (deck text removed for clarity).
- Action panel reduced to three buttons: Draw Stock, Draw Discard, Play Pattern.
- Draw Stock discards selected cards first (so you can clear hand when at max) and then refills from stock.
- UI polish: themed action buttons, subtle card shadows, and a slight lift on selected cards.
- Progression scaffolding (Sprint 0): `PlayerProfile` persisted locally + `ProgressionService` provides `TutorialStep`/tutorial-active and logs progression state at level start.
- Pattern groundwork (Sprint 1): added `PatternId`, added suit-agnostic `StraightRunPattern`, and updated goal matching to use pattern ids (legacy `Run*` goals now count both straight+suited runs; new goal types for straight vs suited runs are available for gating).
- Pattern sets (Sprint 2): added `SuitSetPattern(3+)` and `ColorSetPattern(3+)` plus new goal types `SuitSet3Plus` / `ColorSet3Plus` (with tests).
- Rules update: Discard always costs 1 move regardless of cards discarded; Draw fills hand up to `MaxHandSize` and costs 1 move total.
- Pattern gating (Sprint 3): `LevelDefinition.allowedPatterns` restricts playable patterns; `GameManager` configures `PatternValidator` per level and UI uses that validator.
- Tutorial content (Sprint 4): tutorial Levels 1–7 are generated via `TutorialLevelFactory`; `GameManager` advances `TutorialStep` on wins and routes Continue/Retry through tutorial mode until complete.
- Tutorial reliability (Sprint 5): added deterministic shuffle + optional preset draw pile via `DeckTweakSettings`; tutorial levels now use seeded shuffles per step.
- Achievements (Sprint 6): basic achievements + coin rewards persisted in `PlayerProfile`; unlocks surface on the win screen and are logged.
- Achievements toast: unlocked achievements now also show an on-screen toast (no scene wiring needed).
- Run Mode (Sprint 7): can start/stop via editor menu; run ends on loss and tracks best run length/score; suited runs are locked until unlocked (after 5 non-tutorial wins).
- UI: added a dedicated Discard button (Draw Stock now only draws/fills); achievement toast is now Canvas-based for consistent scaling.
- UI: added an Achievements screen with a top-right `Achievements` button (lists progress + rewards, no scene wiring).

Tutorial progression (confirmed plan)
------------------------------------

- Progression model: first 7 tutorial steps advance per *level completion* (not real days); a loss does not advance the tutorial step.
- Target level length: ~2–3 minutes; start easy and ramp up each session.
- Run rules: tutorial + early real game uses suit-agnostic runs ("straight runs"); suited runs are introduced later as an advanced pattern family.

Tutorial session arc (1–7)
--------------------------

1) Pairs Only
   - Enabled patterns: Pair
   - Goals: Pairs x2
   - Moves: 12–14

2) Straight Runs Intro
   - Enabled patterns: StraightRun3 (suit-agnostic)
   - Goals: StraightRun3 x1
   - Moves: 12–14

3) Mix Basics
   - Enabled patterns: Pair, StraightRun3
   - Goals: Pairs x2 + StraightRun3 x1
   - Moves: 14–16

4) Suit Sets (not suited runs)
   - Enabled patterns: Pair, StraightRun3, SuitSet3+
   - Goals: SuitSet3+ x1 + Pairs x1
   - Moves: 16–18

5) Color Sets
   - Enabled patterns: Pair, StraightRun3, SuitSet3+, ColorSet3+
   - Goals: ColorSet3+ x1 + Pairs x1
   - Moves: 16–18

6) Hand Management / Discard Matters
   - Enabled patterns: all from Session 5
   - Goals: AnyPatterns x3 (or 3 specific patterns; TBD)
   - Moves: 16–18

7) Capstone + Longer Straight Runs
   - Enabled patterns: add StraightRun4+
   - Goals: StraightRun4+ x1 + AnyPattern x2 (+ optional efficiency bonus: win with ≥3 moves left)
   - Moves: 18–22

Post-tutorial rollout
---------------------

- Early real levels: keep StraightRun enabled alongside pairs/sets.
- Later chapter: introduce SuitedRun3/4+ as an explicit "advanced runs" unlock and level theme.

Sprint plan
-----------

- See `SPRINTS.md` for the iteration plan to implement tutorial sessions, pattern gating, deck shaping, and progression persistence.
