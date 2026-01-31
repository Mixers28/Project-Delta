# Session Notes – Session Memory (SM)

> Rolling log of what happened in each focused work session.<br>
> Append-only. Do not delete past sessions.

---

## Example Entry

### 2025-12-01

**Participants:** User, VS Code Agent, Chatgpt
**Branch:** main

### What we worked on
- Set up local MCP-style context system.
- Added session helper scripts and VS Code tasks.
- Defined PROJECT_CONTEXT / NOW / SESSION_NOTES workflow.

### Files touched
- docs/PROJECT_CONTEXT.md
- docs/NOW.md
- docs/SESSION_NOTES.md
- docs/AGENT_SESSION_PROTOCOL.md
- docs/MCP_LOCAL_DESIGN.md
- scripts/session-helper.ps1
- scripts/commit-session.ps1
- .vscode/tasks.json

### Outcomes / Decisions
- Established start/end session ritual.
- Agents will maintain summaries and NOW.md.
- This repo will be used as a public template.

---

## Session Template (Copy/Paste for each new session)

### [DATE – e.g. 2025-12-02]

**Participants:** [You, VS Code Agent, other agents]
**Branch:** [main / dev / feature-x]

### What we worked on
- (fill in)

### Files touched
- (fill in)

### Outcomes / Decisions
-

## Recent Sessions (last 3-5)

### 2026-01-31

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Removed Suit/Color Set patterns and achievements; updated goals/tests to match.
- Rebuilt the tutorial into a 7-rule sequence (Pair, Three-of-a-Kind, Four-of-a-Kind, Suited Run 3, Straight Run 3, Color Run 3, Flush).
- Added a Four-of-a-Kind goal type for tutorial clarity; pair goals now count higher kinds.
- Fixed pattern gating so explicit `allowedPatterns` overrides exclusions; prevented multiple run goals in dynamic goal selection.
- UI polish: removed the startup “Tip” copy (kept intro), increased Continue button text size in the Swivel scene.

### Files touched
- `Assets/Scripts/Patterns/AllPatterns.cs`
- `Assets/Scripts/Models/Goal.cs`
- `Assets/Scripts/Progression/TutorialLevelFactory.cs`
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/Progression/Achievements/*`
- `Assets/Tests/PatternTests.cs`
- `Assets/Tests/TutorialLevelFactoryTests.cs`
- `Assets/Scripts/UI/StartupScreen.cs`
- `Assets/Swivel.unity`
- `README.md`
- `docs/PROJECT_CONTEXT.md`
- `docs/NOW.md`

### Outcomes / Decisions
- Suit/Color Set patterns are no longer part of gameplay.
- Tutorial steps now explicitly introduce 7 patterns, one per step.
- Explicit per-level pattern gating is authoritative over exclusion rules.

### 2026-01-28

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Implemented post-tutorial run gating (straight runs through level 9; suited/color runs for 3/4 from level 10+) and added Color Run goals.
- Added cloud persistence (Node/Express + Postgres on Railway) and Unity auth/sync client.
- WebGL/mobile polish: prompt-based text input, safe-area fitting, and menu button persistence.
- Debugged Railway deployment (branch mismatch, correct backend domain, and env vars).

### Files touched
- `Assets/Scripts/Progression/*`
- `Assets/Scripts/UI/*`
- `Assets/Plugins/WebGL/*`
- `backend/*`
- `Assets/Swivel.unity`, `Assets/Scenes/GameScene.unity`
- `docs/NOW.md`
- `docs/SESSION_NOTES.md`

### Outcomes / Decisions
- Backend service must deploy from `main` with root `backend` and use the backend domain for Cloud Base URL.
- Signup/login validated via `/auth/signup`; `/health` returns `{"ok":true}` from the correct service.

### 2026-01-24 (Session 2)

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Adjusted Win/Lose overlay button anchors/size for mobile and increased Continue label size.
- Hid Startup overlay canvas until shown and set splash backdrop to FitInParent.
- Restored Draw Stock/Discard button scale after accidental shrink.

### Files touched
- `Assets/Swivel.unity`
- `Assets/Scripts/UI/StartupScreen.cs`
- `docs/NOW.md`
- `docs/SESSION_NOTES.md`

### Outcomes / Decisions
- Win/Lose Continue/Retry buttons should sit on-screen with readable labels.
- Startup backdrop now fits parent without overflow; overlay stays hidden until shown.

### 2026-01-24

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Investigated mobile hand drag jitter; found HandContainer uses GridLayoutGroup (no ContentSizeFitter).
- Implemented grid-aware placeholder indexing with raw pointer input and stable drag baseline to reduce jitter.

### Files touched
- `Assets/Scripts/UI/HandDisplay.cs`
- `docs/NOW.md`
- `docs/SESSION_NOTES.md`

### Outcomes / Decisions
- Treat hand reorder as grid-based; avoid smoothed-position placeholder logic on mobile.

### 2025-12-18 (Session 2)

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Repo hygiene baseline for Unity (ignore/untrack generated folders; keep `ProjectSettings/` + `Packages/` tracked).
- Progression rules: added `nonTutorialWins` tiering (Mid tier at 7+ wins), with Mid+ removing jokers and requiring suited runs (no suit-agnostic straight runs).
- Gameplay/UI reliability: fixed run mode end-state so runs persist across wins/continues and only reset on loss or explicit restart.
- UX upgrades: stable drag-to-reorder hand (smoothing, bounds clamp, placeholder highlight), expanded achievements (incl. “Overachiever”), and added a Main Menu that hosts Achievements + safe actions.
- Main Menu polish: confirm-quit dialog with “run will resume” note when run mode is active; “Restart Run” and “Restart Game (Tutorial)” support.

### Files touched
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/UI/GameOverPanel.cs`
- `Assets/Scripts/UI/MainMenuScreen.cs`
- `Assets/Scripts/UI/AchievementsScreen.cs`
- `Assets/Scripts/UI/HandDisplay.cs`
- `Assets/Scripts/UI/CardDisplay.cs`
- `Assets/Scripts/Progression/ProgressionService.cs`
- `Assets/Scripts/Progression/PlayerProfile.cs`
- `Assets/Scripts/Progression/RunModeService.cs`
- `Assets/Scripts/Progression/Achievements/*`
- `Assets/Scripts/Patterns/*`
- `.gitignore`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/*`

### Outcomes / Decisions
- Prefer “stable over snappy” for hand drag/reorder; no “shuffle hand” feature.
- Mid game begins at 7 non-tutorial wins; Mid+ removes jokers and requires suited runs.
- Run mode persists through wins/continues and across quit/resume; reset only on run failure or explicit restart/new run.
- Achievements are accessed from Main Menu; Quit requires confirmation and explains saved state + run resume behavior.

### 2025-12-18

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Repurposed the local MCP memory docs to reflect Project Delta (Unity card-puzzle repo) instead of the local-mcp-context-kit template.
- Updated `docs/Repo_Structure.md` and clarified the naming split between `SESSION_NOTES.md` (game notes) and `docs/SESSION_NOTES.md` (agent session log).

### Notes / Decisions
- Canonical gameplay/progression plan lives in `SPRINTS.md`, `SESSION_NOTES.md`, and `levelprogressioninto.md`.
- Immediate objective includes repo hygiene (Unity-generated folders) + validating tutorial progression and deterministic deck behavior.

### 2025-12-01 (Session 2)

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Re-read PROJECT_CONTEXT, NOW, and SESSION_NOTES to prep session handoff.
- Tightened the summaries in PROJECT_CONTEXT.md and NOW.md to mirror the current project definition.
- Reconfirmed the immediate tasks: polish docs, add an example project, and test on a real repo.

### Files touched
- docs/PROJECT_CONTEXT.md
- docs/NOW.md
- docs/SESSION_NOTES.md

### Outcomes / Decisions
- Locked the near-term plan around doc polish, example walkthrough, and single-repo validation.
- Still waiting on any additional stakeholder inputs before expanding scope.

### 2025-12-01

**Participants:** User, Codex Agent
**Branch:** main

### What we worked on
- Reviewed the memory docs to confirm expectations for PROJECT_CONTEXT, NOW, and SESSION_NOTES.
- Updated NOW.md and PROJECT_CONTEXT.md summaries to reflect that real project data is still pending.
- Highlighted the need for stakeholder inputs before populating concrete tasks or deliverables.

### Files touched
- docs/PROJECT_CONTEXT.md
- docs/NOW.md
- docs/SESSION_NOTES.md

### Outcomes / Decisions
- Documented that the repo currently serves as a template awaiting real project data.
- Set the short-term focus on collecting actual objectives and backlog details.

## Archive (do not load by default)
...
