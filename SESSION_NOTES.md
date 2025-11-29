Session notes (Codex)
---------------------

- Level progression now uses a `progressionStep`; each Continue advances stage difficulty (goals/moves/tweaks), while Retry resets to Stage 1 (base goals).
- UI (HUD, ActionButtons, HandDisplay, GameOverPanel) rebinds to the new `GameState` on Continue/Retry and hides the overlay properly.
- Playing a pattern no longer consumes a move (makes goals achievable within move limits).
- Level data loads from inspector sequence or `Assets/Resources/Levels/` (Rookie/StreetSmart/HighRoller/GambitGauntlet assets), and runtime variants add difficulty/extra goals per stage.
- HUD overlay now shows only Stage and goals (level/deck text removed for clarity).
- Action panel reduced to three buttons: Draw Stock, Draw Discard, Play Pattern.
- Draw Stock discards selected cards first (so you can clear hand when at max) and then refills from stock.
- UI polish: themed action buttons, subtle card shadows, and a slight lift on selected cards.
