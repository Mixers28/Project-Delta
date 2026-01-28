# Sprint Plan (Tutorial Sessions + Progression)

This breaks the agreed tutorial/progression work into small, shippable iterations.

## Status

- Sprint 0: implemented (profile persistence scaffold + progression service + debug logging/overlay).
- Sprint 1: implemented (PatternId + StraightRun support + goal mapping updates + tests).
- Sprint 2: implemented (Suit/Color set patterns + goal types + tests).
- Sprint 3: implemented (per-level AllowedPatterns gating wired through GameManager/UI + tests).
- Sprint 4: implemented (tutorial levels 1–7 via factory + tutorial step advancement + Continue/Retry routing).
- Sprint 5: implemented (seeded shuffle + optional preset draw pile; tutorial uses deterministic shuffle).
- Sprint 6: implemented (basic achievements + coin rewards + win-screen summary).
- Sprint 7: implemented (Run Mode start/stop + run stats persistence + suited-runs unlock gate).
- Sprint 8: implemented (post-tutorial run gating + ColorRun goals + cloud persistence backend/auth + WebGL/mobile input + safe area).

## Product targets (locked)

- Tutorial progression is **7 steps**, advancing **per level win** (a loss does not advance).
- Target level length: **~2–3 minutes**, starting easy and ramping up.
- Early game uses **Straight Runs** (suit-agnostic); **Suited Runs** unlock later as an advanced family.
  - Post-tutorial gating decision: Straight runs through post-tutorial level 9; from level 10+ require suited/color runs for length 3/4; StraightRun5+ stays agnostic.

## Notes (read before starting)

- **Stability first:** avoid matching goals to patterns by `pattern.Name` strings; introduce a stable `PatternId` (enum/string constant) early to prevent rework.
- **Level context:** pattern gating (`AllowedPatterns`) needs access to the current level/session context; today `PatternValidator` is instantiated as a fixed list, so plan for a “validator configured for this level” or an injected filter.
- **Tutorial exit behavior (decision):** after tutorial step 7, either (A) remove gating entirely, or (B) switch to “early real game” rules (still allow StraightRuns) for N levels.
- **Persistence (decision):** start with the smallest thing that works (e.g., `PlayerPrefs` for `TutorialStep`), then upgrade to a JSON profile when you actually need structured data.

## Sprint 0 — Baseline + scaffolding (1–2 days)

**Goal:** Make the system easy to iterate on without changing gameplay yet.

- Add a lightweight `PlayerProfile` (or equivalent) with local persistence:
  - `TutorialStep` (1..7), `HighestLevelCompleted`, `UnlockedFeatures`, `Coins` (placeholder).
- Add a single integration point that exposes progression state to gameplay:
  - `ProgressionService` (or similar) queried by `GameManager` during `StartLevel`.
- Add a debug overlay / log banner showing: tutorial step, enabled patterns, and current level.

**Notes / constraints:**
- Keep persistence minimal in Sprint 0: only store what Sprint 1–4 actually reads.
- Add a simple “tutorial active?” boolean derived from `TutorialStep <= 7`.

**Done when:**
- Restarting the app retains tutorial step.
- Starting a level logs the resolved progression state (tutorial vs normal).

**Implementation notes (done):**
- Added `PlayerProfile` + `PlayerProfileStore` persisted via `PlayerPrefs` (JSON).
- Added `ProgressionService` as the single integration point and initialized it in `GameManager`.
- Added a log banner on level start: `[Progression] tutorialStep=... tutorialActive=...`.
- Added optional `ProgressionDebugOverlay` (OnGUI) for quick runtime visibility (attach to a scene object if desired).

## Sprint 1 — Pattern split: Straight vs Suited runs (2–3 days)

**Goal:** Support the teaching arc without breaking existing patterns.

- Implement `StraightRunPattern` (3/4/5+ variants):
  - Consecutive ranks, suit-agnostic, supports jokers.
- Keep existing run behavior as `SuitedRunPattern` (rename or wrap the current `RunPattern` logic).
- Introduce a stable `PatternId` (enum or string constants) and have each pattern expose it.
- Update `GoalType` / goal mapping so goals can target Straight vs Suited runs distinctly.
- Add/adjust unit tests for run validation (jokers, gaps, duplicates).

**Notes / constraints:**
- Do not rename patterns solely for UI; keep user-facing names independent from internal ids.

**Done when:**
- You can enable only Straight runs in a level and complete it.
- Existing suited-run behavior still works when enabled.

**Implementation notes (done):**
- Added `PatternId` and extended `IPattern` with `Id`.
- Added `StraightRunPattern` (3/4/5+) and kept existing `RunPattern` as the suited-run implementation.
- Updated `Goal.GoalType` to include `StraightRun*` and `SuitedRun*` (appended to preserve existing serialized values) and switched `Goal.MatchesPattern(...)` to use `pattern.Id`.
- Adjusted legacy `Run3/Run4/Run5` goals to count both straight and suited runs (so existing level assets work during the tutorial transition).
- Added NUnit coverage for straight runs (mixed suits + joker gap-fill).

## Sprint 2 — New set patterns + goal alignment (2–3 days)

**Goal:** Match the tutorial doc’s Suit/Color set goals (3+ cards).

- Add `SuitSetPattern(min=3)` and `ColorSetPattern(min=3)` (3+ cards).
- Refactor `Goal.MatchesPattern(...)` to use `PatternId` (or equivalent) instead of `pattern.Name`.
- Ensure scoring rules are consistent (base points + multipliers).

**Notes / constraints:**
- Keep “Flush (exactly 5)” and “SuitSet (3+)” as separate patterns if you want both; don’t overload one rule.

**Done when:**
- Level goals can require suit sets and color sets (3+ cards).
- Pattern detection is deterministic and goal matching isn’t string-fragile.

**Implementation notes (done):**
- Added `SuitSetPattern(3)` and `ColorSetPattern(3)` (3+ cards) and registered them in `PatternValidator`.
- Added `PatternId.SuitSet3Plus` / `PatternId.ColorSet3Plus` plus `GoalType.SuitSet3Plus` / `GoalType.ColorSet3Plus`.
- Added NUnit coverage for suit/color sets (valid, mixed reject, joker support).

## Sprint 3 — Level gating: Allowed patterns per level (2–4 days)

**Goal:** Make tutorial steps enforce “Pairs only”, “Straight runs only”, etc.

- Extend `LevelDefinition` (or runtime variant) to define `AllowedPatterns`.
- Update `PatternValidator` (or a wrapper) to respect allowed patterns for the current level/session.
- Update UI messaging to reflect what’s allowed (minimal: tooltip / text on HUD).

**Notes / constraints:**
- Prefer “configure validator with allowed ids” over hardcoding checks inside each pattern.

**Done when:**
- Session 1 can truly be “Pairs Only” (runs rejected even if selected).
- Session 2 can be “StraightRun3 only”.

**Implementation notes (done):**
- Added `LevelDefinition.allowedPatterns` (empty = allow all).
- Added `PatternValidator(IEnumerable<PatternId>)` that filters patterns by `Id`.
- `GameManager` now configures `PatternValidator` per level and exposes `AllowedPatterns` for UI.
- `HandDisplay` / `ActionButtons` now use `GameManager.PatternValidator` so gating is enforced consistently.
- Added a minimal “Allowed: …” hint in `HandDisplay` when gating is active.
- Added test coverage for allowed-pattern filtering.

## Sprint 4 — Tutorial step content: Session 1–7 level set (3–5 days)

**Goal:** Ship the first 7 tutorial steps with the agreed ramp.

- Create/curate `LevelDefinition` assets or runtime-generated “tutorial levels”:
  - S1: Pair x2 (12–14 moves)
  - S2: StraightRun3 x1 (12–14 moves)
  - S3: Pair x2 + StraightRun3 x1 (14–16 moves)
  - S4: SuitSet3+ x1 + Pair x1 (16–18 moves)
  - S5: ColorSet3+ x1 + Pair x1 (16–18 moves)
  - S6: AnyPatterns x3 (16–18 moves)
  - S7: StraightRun4+ x1 + AnyPattern x2 (18–22 moves)
- Implement tutorial step advancement:
  - On `OnGameEnd(isWin: true)` increment tutorial step (cap at 7).
  - On loss, keep the same step (optionally offer “retry” messaging).

**Notes / constraints:**
- Keep step advancement separate from “level index”; tutorial step should drive which template/definition to load.

**Done when:**
- Playing 7 wins advances through all steps and then exits tutorial mode.
- Each step enforces its allowed patterns and goals.

**Implementation notes (done):**
- Added `TutorialLevelFactory` to generate Levels 1–7 with goals and `allowedPatterns`.
- `GameManager` now:
  - starts tutorial levels when `ProgressionService.IsTutorialActive` is true
  - advances tutorial step on win
  - routes Continue/Retry to the current tutorial step until the tutorial completes
- Added basic tests to validate tutorial templates (`Assets/Tests/TutorialLevelFactoryTests.cs`).

## Sprint 5 — Deck shaping for tutorial reliability (3–6 days)

**Goal:** Ensure tutorial levels are consistently solvable and hit 2–3 min pacing.

- Extend deck tweaking to support at least one of:
  - deterministic seed per level, and/or
  - filtered ranks/suits subset, and/or
  - a small “preset draw pile” list for early tutorial steps.
- Add internal validation helpers (editor-time or runtime) to catch impossible goals.

**Notes / constraints:**
- This is the sprint that makes Sessions 1–3 feel “designed” instead of RNG-driven; expect iteration here.

**Done when:**
- Session 1–3 feel reliable (no “unlucky” dead-ends) across multiple replays.

**Implementation notes (done):**
- Added deterministic shuffle support to `DeckTweakSettings` (`useDeterministicShuffle`, `shuffleSeed`).
- Added optional `presetDrawPile` to `DeckTweakSettings` for fully controlled tutorial setups.
- `Deck.ApplyTweaks(...)` now rebuilds from a deterministic base when seeded/preset is used, then shuffles with the seed.
- Tutorial levels now set a deterministic shuffle seed per step (`TutorialLevelFactory`).
- Added tests for seeded shuffle determinism and preset draw pile behavior.

## Sprint 6 — Achievements + unlock surfacing (optional next, 3–6 days)

**Goal:** Add the “feel rewarded” loop without blocking core tutorial.

- Add a basic `AchievementDefinition` + progress tracking using existing events.
- Add “highlighted achievements this step” config (by tutorial step).
- Rewards: coins + placeholder cosmetics inventory.

**Done when:**
- Completing tutorial steps triggers 1–2 achievements with a simple summary UI.

**Implementation notes (done):**
- Added `AchievementsService` with a small in-code catalog (First Level / First Pair / First Straight Run 3).
- Persisted achievement progress and coin rewards on `PlayerProfile`.
- Wired achievements into `GameManager` (pattern played + level completed) and surfaced unlocks on the win screen (`GameOverPanel`).
- Added basic unit tests for coin reward and recent-unlock queue behavior.

## Sprint 7 — Run Mode + advanced unlocks (later, 4–8 days)

**Goal:** Introduce replayable meta mode after tutorial.

- Implement Run Mode (streak until fail), store personal bests.
- Gate `SuitedRun` introduction as a post-tutorial unlock with dedicated levels.

**Done when:**
- Players can choose Normal vs Run Mode; best run length persists.

**Implementation notes (done):**
- Added `RunModeService` with persisted run stats on `PlayerProfile` (current/best run length + score).
- Added editor tools to Start/Stop Run Mode (`Tools → Progression → Start/Stop Run Mode`).
- Run mode ends on loss (Game Over shows "Run Ended" and offers "New Run").
- Suited runs are filtered out of non-gated levels until `SuitedRuns` is unlocked (auto-unlocks after 5 non-tutorial wins).

## Sprint 8 — Post-tutorial run gating + Cloud persistence (done)

**Goal:** Align post-tutorial run rules and add cloud save/auth for WebGL/mobile sessions.

- Add a post-tutorial level index (non-tutorial wins + 1) for gating.
- Keep runs suit-agnostic through post-tutorial level 9.
- From post-tutorial level 10+, require suited or color runs for length 3/4; keep StraightRun5+ always agnostic.
- Add `ColorRunPattern` (consecutive ranks, same color, jokers allowed) and `ColorRun3/4` goal types.
- Implement cloud persistence:
  - Backend: Node/Express + Postgres (Railway), JWT auth, profile CRUD.
  - Unity: `AuthService`, `CloudProfileStore`, and sync runner; account UI in main menu.
  - WebGL/mobile: prompt-based text input fallback, safe area fitting, menu button persists across levels.

**Done when:**
- Post-tutorial run goals follow the level 9/10 gating rules.
- Login/signup works against the Railway backend and profiles persist across sessions.

**Implementation notes (done):**
- Added `ProgressionService.PostTutorialLevelIndex` and gated run goals in `GameManager.InitializeNewGame`.
- Added `ColorRunPattern`, `PatternId.ColorRun3/4`, and `GoalType.ColorRun3/4` (+ tests).
- Added backend service under `backend/` with auth + profile endpoints and SQL schema.
- Added Unity cloud sync + WebGL/mobile input and HUD safe-area fixes.

## Tracking / cadence

- End each sprint with a playable build and a short checklist:
  - tutorial step correctness, pattern gating, goals achievable, pacing ~2–3 min.

## Resolved decisions

- Tutorial “session” increments on **level win only** (not on “Continue” button flow).
- Persistence store is **`PlayerPrefs` JSON** for this project (upgrade to file-based JSON only if/when you need export/debug/mod support).
- Post-tutorial transition is an **early real game chapter** before introducing suited runs:
  - Decision: use an early real game ramp of **N = 5 wins post-tutorial**.
  - During early real game: keep Straight runs + sets + pairs; keep suited runs locked.
  - After N wins: unlock `SuitedRuns` and begin mixing suited-run goals into the level rotation.

## Open decisions (future)

- When to migrate from `PlayerPrefs` to file-based JSON (only if needed).
