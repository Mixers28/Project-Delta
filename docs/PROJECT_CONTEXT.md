# Project Context – Long-Term Memory (LTM)

> High-level design, tech decisions, constraints for this project.<br>
> This is the **source of truth** for agents and humans.

<!-- SUMMARY_START -->
**Summary (auto-maintained by Agent):**
- **Main branch (`main`):** Unity 2022.3.62f3 native desktop card-puzzle game ("Project Delta") focused on pattern-play, goals, and move-limited levels.
  - Core architecture: GameManager orchestration → GameState rules/events → PatternValidator + IPattern set → progression services (ProgressionService, tutorial, achievements, run mode).
  - Progression model: 7-step tutorial advances on wins only; post-tutorial unlocks include Run Mode and later Suited Runs (locked behind early real-game wins), with rules tiering driven by `nonTutorialWins` (Mid tier at 7+ wins).
  - Levels: loaded from inspector sequence or Assets/Resources/Levels/, with runtime difficulty variants via progressionStep.
  - Persistence: PlayerPrefs-backed PlayerProfile (tutorial step, unlocks, run stats, achievements/coins).
  - Meta UX: Main Menu overlay hosts Achievements and safe actions (Quit+Save with confirm, Restart Run post-tutorial, Restart Game to replay tutorial).

- **Web branch (`web/webgl-export`):** Parallel WebGL export variant with cloud persistence (Railway API + Supabase PostgreSQL, hosted on Vercel).
  - Same gameplay as main, compiled to WebGL for browser.
  - Backend: Express.js API on Railway (auth: device fingerprint → JWT; profile sync; run history).
  - Database: Supabase PostgreSQL (users, game_profiles, run_history tables).
  - Frontend: Vercel CDN + auto-deploy.
  - Phase 1 (Complete): Backend scaffold, API endpoints, Supabase schema, deployment configs.
  - Phase 2 (Next): C# web persistence services, anonymous auth UI, 60 FPS optimization.
  - Phase 3 (TBD): WebGL build, local testing, Vercel + Railway deployment.
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

## 7. Web Variant (Branch: `web/webgl-export`)

> Experimental parallel branch for WebGL browser deployment with cloud persistence.

### Overview
- **Goal:** Validate Project Delta as a playable WebGL game in browsers with cloud save functionality.
- **Branch:** `web/webgl-export` (permanent, kept separate from `main`)
- **Timeline:** Phase 1 ✅ Complete; Phase 2-3 in progress.

### Architecture

**Backend (Railway):**
- Node.js + Express.js server
- Endpoints:
  - `POST /auth/register` – anonymous user creation via device fingerprint
  - `GET/POST /api/profile` – fetch/save player progression (JWT required)
  - `GET /health` – health check for monitoring
- Database: Supabase PostgreSQL (users, game_profiles, run_history)
- Deployment: Auto-deploy on push to `web/webgl-export`

**Frontend (Vercel):**
- WebGL build compiled from Unity (2022.3.62f3)
- CDN delivery, auto-deploy on push
- Target: 60+ FPS in Chrome, Firefox, Safari (desktop)

**Persistence Flow:**
1. Player visits Vercel URL → WebGL loads
2. Browser auto-generates device fingerprint
3. Game calls `POST /auth/register` → receives JWT token
4. Player plays, wins levels
5. `GameOver` event triggers `POST /api/profile` → backend saves state
6. Browser refresh → fetch latest profile via `GET /api/profile`
7. Game resumes from saved state

### Phase Status

| Phase | Status | Details |
|-------|--------|---------|
| **1: Infrastructure** | ✅ Complete | Backend scaffold, API endpoints, Supabase schema, Railway/Vercel configs |
| **2: Integration** | 🔲 Next | C# web persistence services, LoginScreen UI, 60 FPS profiling |
| **3: Build & Test** | 🔲 TBD | WebGL export, local testing, deployment validation |
| **4: Validation** | 🔲 TBD | Profile sync, offline fallback, concurrent updates |
| **5: Documentation** | 🔲 TBD | BUILD_INSTRUCTIONS.md, DEPLOYMENT.md finalization |

### Key Decisions

- **Authentication:** Anonymous + device fingerprint (email login deferred to Phase 2)
- **Database:** Supabase PostgreSQL (independent from backend, better DX)
- **Frontend Hosting:** Vercel (CDN, auto-deploy, no need to serve from Railway)
- **Performance Target:** 60+ FPS (aggressive; optimize if below 50)
- **Mobile:** Deferred to Phase 2 (desktop-first MVP)

### Files & Documentation

- **Backend:** `backend/src/server.js` (450+ lines, fully documented)
- **Schema:** `backend/src/db/schema.sql` (ready for Supabase)
- **Deployment:** `web/DEPLOYMENT.md` (step-by-step setup)
- **Build Guide:** `web/BUILD_INSTRUCTIONS.md` (WebGL build + local testing)
- **Architecture:** `docs/spec.md` (comprehensive spec)

---

## 8. Change Log (High-Level Decisions)

Use this section for **big decisions** only:

- `2026-01-07` – Approved web/webgl-export branch with Railway + Supabase + Vercel stack; Phase 1 complete.
- `2026-01-07` – Decided on anonymous auth (device fingerprint) + optional email (Phase 2).
- `2026-01-07` – Chose Supabase for database (independent, SQL flexibility, better than Railway postgres).
- `2026-01-07` – Chose Vercel for frontend hosting (CDN, auto-deploy, cleaner than serving from Railway).
- `YYYY-MM-DD` – Decided on X instead of Y.
- `YYYY-MM-DD` – Switched primary deployment target to Z.
