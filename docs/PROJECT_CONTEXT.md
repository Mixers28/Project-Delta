# Project Context – Long-Term Memory (LTM)

> High-level design, tech decisions, constraints for this project.<br>
> This is the **source of truth** for agents and humans.

<!-- SUMMARY_START -->
**Summary (auto-maintained by Agent):**
- Unity 2022.3.62f3 card-based puzzle game (“Project Delta”) focused on pattern-play, goals, and move-limited levels.
- Core architecture: GameManager orchestration → GameState rules/events → PatternValidator + IPattern set → progression services (ProgressionService, tutorial, achievements, run mode).
- Progression model: 7-step tutorial advances on wins only; post-tutorial unlocks include Run Mode and later Suited Runs (locked behind early real-game wins), with rules tiering driven by `nonTutorialWins` (Mid tier at 7+ wins).
- Levels: loaded from inspector sequence or Assets/Resources/Levels/, with runtime difficulty variants via progressionStep.
- Persistence: PlayerPrefs-backed PlayerProfile (tutorial step, unlocks, run stats, achievements/coins).
- Meta UX: Main Menu overlay hosts Achievements and safe actions (Quit+Save with confirm, Restart Run post-tutorial, Restart Game to replay tutorial).
<!-- SUMMARY_END -->

---

## 1. Project Overview

- **Name:** Project Delta
- **Owner:** TBD
- **Purpose:** Ship a playable, iteratable card-puzzle with a tutorial-to-meta progression loop.
- **Primary Stack:** Unity + C# + Unity Test Framework (no backend).
- **Target Platforms:** Desktop (Unity Editor) initially.

---

## 2. Core Design Pillars

- Keep core rules deterministic and testable (pattern validation + deck tweaks + tutorial reliability).
- Prefer small, shippable iterations (tutorial steps, pattern gating, achievements, run mode).
- Make UI reactive and resilient via GameState events and clean rebind flows on Continue/Retry.

---

## 3. Technical Decisions & Constraints

- Language(s): C# (Unity).
- Framework(s): Unity 2022 LTS; Unity Test Framework (NUnit).
- Persistence: Local PlayerPrefs JSON (PlayerProfile) for tutorial step, unlocks, achievements, run stats.
- Backend: None currently; keep gameplay/progression fully offline-capable.
- Non-negotiable constraints:
  - Tutorial progression: 7 steps, advance on win only.
  - Early game uses Straight Runs; Suited Runs unlock later (post-tutorial gate).
  - Rules tiering: Mid tier starts at 7+ non-tutorial wins; Mid+ removes jokers and disables suit-agnostic straight runs (suited runs only).

---

## 4. Architecture Snapshot

- Runtime: `GameManager` orchestrates sessions; `GameState` owns rules/events/state; patterns implement `IPattern` and are filtered/scored via `PatternValidator`.
- Content: tutorial levels generated via `TutorialLevelFactory`; non-tutorial levels loaded from inspector sequence and/or `Assets/Resources/Levels/`, then runtime-variant difficulty is applied per `progressionStep`.
- Meta: `ProgressionService` + `PlayerProfile` manage tutorial step/unlocks; achievements + run mode are persisted locally.

---

## 5. Links & Related Docs

- Readme: `README.md`
- Roadmap / sprint status: `SPRINTS.md`
- Gameplay change log + tutorial arc notes: `SESSION_NOTES.md`
- Level progression + tutorial design: `levelprogressioninto.md`
- Project structure (high-level): `projectstructre.md`
- Developer workflow + architecture notes: `.github/copilot-instructions.md`
- Agent memory system (this repo’s local workflow): `docs/AGENT_SESSION_PROTOCOL.md`, `docs/MCP_LOCAL_DESIGN.md`

---

## 6. Naming Conventions (Docs)

- `SESSION_NOTES.md` (repo root) is game-focused notes: gameplay changes, tutorial arc, design decisions.
- `docs/SESSION_NOTES.md` is the agent session log for the local “files as memory” workflow (append-only).
  - Keep these separate to avoid mixing “what the game is” with “what we did in a coding session”.

---

## 7. Change Log (High-Level Decisions)

Use this section for **big decisions** only:

- `YYYY-MM-DD` – Decided on X instead of Y.
- `YYYY-MM-DD` – Switched primary deployment target to Z.
