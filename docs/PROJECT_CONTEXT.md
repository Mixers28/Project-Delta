# Project Context – Long-Term Memory (LTM)

> High-level design, tech decisions, constraints for this project.<br>
> This is the **source of truth** for agents and humans.

<!-- SUMMARY_START -->
**Summary (auto-maintained by Agent):**
- Unity 2022.3.62f3 card-based puzzle game (“Project Delta”) focused on pattern-play, goals, and move-limited levels; target includes WebGL/mobile.
- Core architecture: GameManager orchestration → GameState rules/events → PatternValidator + IPattern set → progression services (ProgressionService, tutorial, achievements, run mode).
- Progression model: 7-step tutorial advances on wins only with explicit pattern intros (Pair, Three-of-a-Kind, Four-of-a-Kind, Suited Run 3, Straight Run 3, Color Run 3, Flush). Suit/Color Set patterns are removed. Explicit `allowedPatterns` overrides exclusions. Post-tutorial gating uses level index (non-tutorial wins + 1). Runs are suit-agnostic through post-tutorial level 9; from level 10+ runs of length 3/4 require suited or color runs; StraightRun5+ stays agnostic.
- Levels: loaded from inspector sequence or Assets/Resources/Levels/, with runtime difficulty variants via progressionStep.
- Persistence: PlayerPrefs-backed PlayerProfile plus cloud sync (JWT auth + profile CRUD) via Node/Express + Postgres on Railway.
- Meta UX: Main Menu overlay hosts Achievements and safe actions (Quit+Save with confirm, Restart Run post-tutorial, Restart Game to replay tutorial).
- Backlog: gameplay/UX ideas tracked in `docs/IDEAS_BACKLOG.md`.
<!-- SUMMARY_END -->

---

## 1. Project Overview

- **Name:** Project Delta
- **Owner:** mixers28
- **Purpose:** Ship a playable, iteratable card-puzzle with a tutorial-to-meta progression loop.
- **Primary Stack:** Unity + C# + Unity Test Framework + Node/Express backend + Postgres.
- **Target Platforms:** Desktop + WebGL (mobile-friendly).

---

## 2. Core Design Pillars

- Keep core rules deterministic and testable (pattern validation + deck tweaks + tutorial reliability).
- Prefer small, shippable iterations (tutorial steps, pattern gating, achievements, run mode).
- Make UI reactive and resilient via GameState events and clean rebind flows on Continue/Retry.

---

## 3. Technical Decisions & Constraints

- Language(s): C# (Unity).
- Framework(s): Unity 2022 LTS; Unity Test Framework (NUnit).
- Persistence: Local PlayerPrefs JSON (PlayerProfile) plus optional cloud sync (Railway backend).
- Backend: Node/Express + Postgres on Railway (JWT auth, profile CRUD).
- Non-negotiable constraints:
  - Tutorial progression: 7 steps, advance on win only, each step introduces a distinct pattern (Pair, Three-of-a-Kind, Four-of-a-Kind, Suited Run 3, Straight Run 3, Color Run 3, Flush).
  - Early post-tutorial: Straight runs through post-tutorial level 9.
  - Post-tutorial level 10+: runs of length 3/4 must be suited or color; StraightRun5+ remains agnostic.
  - Explicit `allowedPatterns` gates override exclusion rules (tutorial-specific patterns always valid).
  - Suit/Color Set patterns are retired and should not be reintroduced.

---

## 4. Architecture Snapshot

- Runtime: `GameManager` orchestrates sessions; `GameState` owns rules/events/state; patterns implement `IPattern` and are filtered/scored via `PatternValidator`.
- Content: tutorial levels generated via `TutorialLevelFactory`; non-tutorial levels loaded from inspector sequence and/or `Assets/Resources/Levels/`, then runtime-variant difficulty is applied per `progressionStep`.
- Meta: `ProgressionService` + `PlayerProfile` manage tutorial step/unlocks; achievements + run mode are persisted locally.
- Cloud: `AuthService` + `CloudProfileStore` + backend `/auth` + `/profile` endpoints for signup/login and profile sync.

---

## 5. Links & Related Docs

- Readme: `README.md`
- Roadmap / sprint status: `SPRINTS.md`
- Gameplay change log + tutorial arc notes: `SESSION_NOTES.md`
- Level progression + tutorial design: `levelprogressioninto.md`
- Ideas backlog: `docs/IDEAS_BACKLOG.md`
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
- `2026-01-28` – Added Railway backend for cloud persistence (JWT auth + profile CRUD) and updated post-tutorial run gating (straight runs through level 9; suited/color runs for 3/4 from level 10+).
