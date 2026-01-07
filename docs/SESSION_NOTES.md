# Session Notes – Session Memory (SM)

> Rolling log of what happened in each focused work session.<br>
> Append-only. Do not delete past sessions.

---

## Example Entry

### 2025-12-01

**Participants:** User,VS Code Agent, Chatgpt
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

---

## Recent Sessions (last 3-5)

### 2026-01-07 (Web Phase 1)

**Participants:** User, Reviewer/Architect/Coder Agent
**Branch:** web/webgl-export (new)

### What we worked on

- **Created `web/webgl-export` branch** from main as permanent parallel branch for WebGL experimentation.
- **Designed and approved architecture spec** (`docs/spec.md`): Railway backend + Supabase PostgreSQL + Vercel frontend.
- **Implemented Phase 1: Backend Scaffold**
  - Express.js server (`backend/src/server.js`) with 5 API endpoints: `/auth/register`, `/api/profile` (GET/POST), `/health`, `/api/runs` (placeholder)
  - JWT-based authentication with device fingerprint support for anonymous users
  - Supabase PostgreSQL schema (`backend/src/db/schema.sql`): users, game_profiles, run_history tables
  - Database connection pooling via `pg` library; CORS configured for Vercel domain
  - Railway deployment config (`backend/railway.json`) with auto-deploy
  - Vercel config (`vercel.json`) with CDN caching for WebGL assets
- **Created comprehensive documentation**
  - `web/README.md` – overview, tech stack, features, troubleshooting
  - `web/DEPLOYMENT.md` – step-by-step setup guide (Railway → Supabase → Vercel)
  - `web/BUILD_INSTRUCTIONS.md` – WebGL build configuration, local testing, performance profiling
  - `docs/spec.md` – full 400+ line architecture specification with decisions, timelines, rollback plans
- **Updated `.gitignore`** to handle web-specific files (web-build/, backend/node_modules/, .env, etc)
- **Cleaned repository** – removed untracked iOS build artifacts; confirmed clean working tree

### Files touched

- `backend/src/server.js` (450+ lines, fully commented)
- `backend/src/db/schema.sql` (complete schema ready for Supabase)
- `backend/package.json` (Node 18+, Express, JWT, pg, CORS, dotenv)
- `backend/railway.json` (Railway deployment config)
- `backend/.env.example` (template for env vars)
- `vercel.json` (Vercel CDN caching config)
- `web/README.md` (overview + quick start)
- `web/DEPLOYMENT.md` (step-by-step setup)
- `web/BUILD_INSTRUCTIONS.md` (WebGL build guide)
- `docs/spec.md` (architecture specification)
- `docs/PROJECT_CONTEXT.md` (added Section 7: Web Variant)
- `docs/NOW.md` (updated to track both main + web branches)
- `.gitignore` (updated for web development)

### Outcomes / Decisions

✅ **Tech Stack Finalized:**
- Backend: Node.js 18+ + Express.js (Railway)
- Database: Supabase PostgreSQL (not Railway postgres)
- Frontend: Vercel CDN + auto-deploy (not serving from Railway)
- Authentication: Anonymous via device fingerprint + optional email (Phase 2)
- Performance: 60+ FPS target (aggressive; optimize if < 50)
- Mobile: Deferred to Phase 2

✅ **Phase 1 Complete:**
- Backend scaffold fully functional and documented
- API endpoints ready for testing (can test locally with `npm run dev`)
- Supabase schema ready to import
- Deployment configs ready for Railway and Vercel

🔲 **Phase 2 (Next):**
- Create C# web persistence services (WebPersistenceManager, WebAuthManager, WebProfileSyncService)
- Create LoginScreen.cs for anonymous auth UI
- Configure GameManager for WebGL conditional compilation
- Create WebConfig.json template
- Local testing + FPS profiling

**Key Files for Reference:**
- API contract: [backend/src/server.js](backend/src/server.js) (fully documented endpoints)
- Database schema: [backend/src/db/schema.sql](backend/src/db/schema.sql)
- Setup guide: [web/DEPLOYMENT.md](web/DEPLOYMENT.md)
- Build guide: [web/BUILD_INSTRUCTIONS.md](web/BUILD_INSTRUCTIONS.md)

---

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

### [DATE – e.g. 2025-12-02]

**Participants:** [You, VS Code Agent, other agents]
**Branch:** [main / dev / feature-x]

### What we worked on
- (fill in)

### Files touched
- (fill in)

### Outcomes / Decisions
-

## Archive (do not load by default)
...
