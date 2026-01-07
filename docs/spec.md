# Web/WebGL Export Specification

> Architecture plan for porting Project Delta to web via Unity WebGL export with backend persistence.
> **Branch:** `web/webgl-export`
> **Status:** Planning phase

---

## 1. Summary

Establish a parallel branch (`web/webgl-export`) to validate Project Delta as a WebGL-based browser game with cloud persistence (Railway backend + Supabase PostgreSQL, frontend on Vercel). The goal is to:

- **Minimize C# code changes** by exporting the existing Unity build to WebGL
- **Add backend persistence layer** for PlayerProfile (tutorial step, unlocks, run stats, achievements, coins) via REST API
- **Deploy backend to Railway** with Supabase PostgreSQL for structured user/game data
- **Deploy frontend (WebGL build) to Vercel** for fast CDN delivery and auto-deploy on branch push
- **Validate web UX** (60+ FPS performance, hand drag-to-reorder, UI responsiveness, desktop browsers)
- **Preserve main branch** as the native desktop reference implementation

**Success criteria:**
- WebGL build runs at 60+ FPS in modern browsers (Chrome, Firefox, Safari desktop)
- Tutorial and early-game progression fully playable with smooth hand interactions
- Run mode persists across browser refresh/close
- All achievements and coins sync to backend and restore on login
- Anonymous guest mode + optional email authentication working
- Full deployment pipeline: push to `web/webgl-export` → auto-deploy to Vercel (frontend) + Railway (backend)

---

## 2. Assumptions

1. **Performance target:** 60+ FPS (aggressive; will optimize shader/asset complexity if WebGL perf falls below 50 FPS)
2. **Mobile web support:** Deferred to Phase 2; MVP targets desktop only (Chrome, Firefox, Safari)
3. **User authentication:** Anonymous guest mode (automatic device fingerprint) + optional email login (future)
4. **Backend persistence:** REST API layer (Node.js + Express) running on Railway
5. **Database:** Supabase PostgreSQL for user/game state (structured data, SQL queries, free tier sufficient for MVP)
6. **Frontend hosting:** Vercel for WebGL build CDN delivery; auto-deploy on push to `web/webgl-export`
7. **Asset handling:** WebGL loads Unity assets from Resources/ via UnityWebRequest
8. **PlayerPrefs limitations:** WebGL PlayerPrefs maps to browser IndexedDB; backend is source of truth for persistence
9. **No breaking changes to main:** Conditional compilation (`UNITY_WEBGL` #if) used where needed; `web/*` branch kept separate indefinitely

---

## 3. Implementation Plan

### Phase 1: Setup & Infrastructure (Week 1)
**Goal:** Establish branch, create backend + frontend infrastructure, configure CI/CD pipelines.

1. **Create branch `web/webgl-export` from `main`**
   - Keep `main` untouched; this is experimental
   - All web-specific files live on this branch

2. **Set up Railway project (Backend)**
   - Create new Railway app for Node.js backend
   - Configure environment variables: `PORT`, `DATABASE_URL` (Supabase), `JWT_SECRET`, `CORS_ORIGIN` (Vercel URL)
   - Reserve a public domain or use Railway's auto-generated URL (e.g., `project-delta-api.railway.app`)

3. **Set up Supabase project (Database)**
   - Create new Supabase organization/project
   - Create PostgreSQL database schema:
     - `users` (id, email, device_fingerprint, created_at)
     - `game_profiles` (user_id, tutorial_step, unlocks_json, run_stats_json, achievements_json, coins, updated_at)
     - `run_history` (id, user_id, level_id, started_at, ended_at, status, score)
   - Generate API keys (service_role for backend only)
   - Set Row Level Security (RLS) policies (optional for MVP)

4. **Set up Vercel project (Frontend)**
   - Create new Vercel project linked to GitHub repo
   - Configure build command: (TBD after WebGL build structure defined)
   - Configure environment variable: `REACT_APP_API_URL=https://project-delta-api.railway.app`
   - Enable auto-deploy on push to `web/webgl-export`

5. **Create backend scaffold** (`/backend` or separate repo)
   - Express.js server with auth + API endpoints:
     - `POST /auth/register` – create anonymous user with device fingerprint
     - `POST /auth/login-email` – email/password login (future)
     - `GET /api/profile` – fetch PlayerProfile JSON (requires JWT)
     - `POST /api/profile` – upsert PlayerProfile (merge strategy: backend = source of truth)
     - `GET /api/runs` – fetch run history (optional)
   - Connect to Supabase PostgreSQL via `pg` library
   - JWT token generation + validation middleware
   - CORS enabled for Vercel domain

6. **Create Railway + Vercel config files**
   - `backend/railway.json` – build + start commands
   - `backend/.env.example` – sample env vars (DATABASE_URL, JWT_SECRET, etc.)
   - `vercel.json` – build command, routes, rewrites (if needed)

7. **Create `web/DEPLOYMENT.md`**
   - Step-by-step: Git setup → Railway backend → Supabase schema → Vercel frontend
   - Environment variables checklist
   - Health check endpoints to validate setup

---

### Phase 2: C# Backend Integration (Week 1-2)
**Goal:** Add minimal C# changes to support web persistence.

1. **Create `Assets/Scripts/Web/` folder**
   - `WebPersistenceManager.cs` – orchestrates cloud sync
   - `WebAuthManager.cs` – handles login/registration flow
   - `WebProfileSyncService.cs` – syncs PlayerProfile to backend

2. **Modify `PlayerProfile.cs`**
   - Add `ToJson()` / `FromJson()` (already exists, ensure compatibility)
   - No breaking changes to existing fields

3. **Create `WebPersistenceConfig.cs`**
   - Store backend API URL (configurable via Inspector or build config)
   - Example: `https://project-delta-api.railway.app`
   - Use conditional compilation:
     ```csharp
     #if UNITY_WEBGL
     // Use web persistence
     #else
     // Use local PlayerPrefs
     #endif
     ```

4. **Modify `GameManager.cs`** (if needed)
   - On startup, detect WebGL build and initialize `WebPersistenceManager`
   - On `GameOver`, trigger async save to backend
   - Keep sync non-blocking (fire-and-forget with fallback to local)

5. **Create anonymous auth + login UI**
   - `Assets/Scripts/UI/LoginScreen.cs`
   - Auto-generates device fingerprint (browser/OS/GPU combo)
   - Creates anonymous user on first launch
   - Shows optional email login button (future enhancement, Phase 2)
   - Persists JWT token in browser localStorage

6. **Update build configuration**
   - Add WebGL build target to project
   - Ensure all Scenes are included in WebGL build

---

### Phase 3: WebGL Export & Testing (Week 2)
**Goal:** Generate WebGL build, test in browser, identify performance bottlenecks.

1. **Create WebGL build profile**
   - Build Settings → Target Platform: WebGL
   - Scene list: StartupScreen → Main Menu → GameScene(s)
   - Compression: Brotli (best compression)
   - Development build: No (once validated)

2. **Generate WebGL build locally**
   - Output to `/web-build/` (ignore from main branch)
   - Test in local server: `python3 -m http.server 8000` (or similar)

3. **Browser testing (local)**
   - Chrome DevTools: check console for errors, network requests
   - Verify drag-to-reorder hand works on desktop mouse
   - Verify Main Menu, Quit, Restart flows work
   - Check asset loading times (measure Time-to-Interactive)
   - Verify FPS meter shows 60+ FPS (target)

4. **Deploy to Vercel + Railway**
   - Push WebGL build to Vercel (auto-deploy on push to `web/webgl-export`)
   - Verify backend API health on Railway
   - Test end-to-end: register anonymous user → play → save profile → verify in Supabase

5. **Performance profiling & optimization**
   - Record FPS in Chrome DevTools Perf tab (target: 60+ FPS)
   - If FPS < 50: identify bottlenecks (shader complexity, draw calls, asset load time)
   - Optimize: bake lighting, reduce particle count, compress textures
   - Document findings in `DEPLOYMENT.md`

---

### Phase 4: Persistence Layer Validation (Week 2-3)
**Goal:** Ensure PlayerProfile syncs reliably to backend.

1. **Test auth flow**
   - Register new user → token stored in browser localStorage
   - Login existing user → token refreshed
   - Guest mode (optional) → device fingerprint stored

2. **Test profile persistence**
   - Play tutorial step → win → profile saves to backend
   - Close browser → relaunch → profile restored from backend
   - Run mode: start run → win → continue → quit → relaunch → run still active

3. **Test offline fallback**
   - Simulate offline (DevTools Network tab)
   - Game continues with local PlayerPrefs
   - On reconnect, sync pending changes to backend

4. **Test concurrent updates**
   - Play on two browsers simultaneously
   - Verify last-write-wins or merge strategy handles conflicts
   - No data corruption on simultaneous saves

---

### Phase 5: Documentation & Handoff (Week 3)
**Goal:** Document the web export process for future team members.

1. **Create `web/README.md`**
   - High-level overview of WebGL export + persistence architecture
   - Links to backend repo and Supabase dashboard

2. **Create `web/BUILD_INSTRUCTIONS.md`**
   - Step-by-step: Unity WebGL build → test locally → deploy to Railway
   - Environment variables needed (backend URL, auth keys)
   - Common issues and fixes

3. **Update `docs/DEPLOYMENT.md`**
   - Native build deployment (if any)
   - WebGL deployment via Railway
   - Switching between local/dev/prod backends

4. **Update `PROJECT_CONTEXT.md` (on `web/webgl-export`)**
   - Add "Web variant" section describing the branch's scope
   - Clarify that main remains the reference implementation

5. **Create issue template for WebGL-specific bugs**
   - Browser/OS/GPU context
   - Network conditions
   - LocalStorage/IndexedDB state

---

## 4. Files to Touch / Create

### New Files
```
web/                               # New folder (on web/webgl-export only)
├── README.md                       # Overview of web variant
├── BUILD_INSTRUCTIONS.md           # Step-by-step build + deploy guide
├── DEPLOYMENT.md                   # Production deployment checklist
└── .railwayapp.json                # Railway build config (optional)

backend/                            # New repo or folder (separate)
├── src/
│   ├── server.js                   # Express entry point
│   ├── routes/
│   │   ├── auth.js                 # /auth/register, /auth/login
│   │   └── api.js                  # /api/profile/*, /api/runs/*
│   ├── middleware/
│   │   └── auth.js                 # JWT verification
│   └── db/
│       └── schema.sql              # Supabase schema (if used)
├── package.json
├── .env.example
└── railway.json                    # Railway config

Assets/Scripts/Web/                 # New folder
├── WebPersistenceManager.cs
├── WebAuthManager.cs
├── WebProfileSyncService.cs
├── WebPersistenceConfig.cs
└── UI/
    └── LoginScreen.cs

Assets/Scripts/UI/
├── (modified) MainMenuScreen.cs    # Add login check
└── (modified) StartupScreen.cs     # Detect WebGL and initialize web services
```

### Modified Files (Minimal Changes)
```
Assets/Scripts/Managers/
├── (modify) GameManager.cs         # Initialize WebPersistenceManager on WebGL
└── (already exists) GameState.cs   # No changes needed

Assets/Scripts/Progression/
├── (already exists) PlayerProfile.cs # Ensure ToJson/FromJson intact
├── (already exists) ProgressionService.cs
└── (already exists) RunModeService.cs

Assets/Scripts/UI/
├── (modify) GameOverPanel.cs       # Trigger sync on game over
└── (already exists) AchievementsScreen.cs

ProjectSettings/
├── EditorBuildSettings.json        # Add WebGL build target
└── ProjectSettings.asset           # Configure WebGL settings (compression, etc.)

docs/
├── (new) spec.md                   # This file
├── (update) PROJECT_CONTEXT.md     # Add "Web Variant" section on web/* branch
└── (update) NOW.md                 # Track web branch progress
```

---

## 5. Test Plan

### Unit Tests (in Unity)
- **PlayerProfile serialization/deserialization** → ensure web JSON format matches local PlayerPrefs
- **WebProfileSyncService** → mock backend API, verify correct payloads sent
- **WebAuthManager** → token generation/validation, localStorage updates

### Integration Tests (WebGL Build)
- **Auth flow:** Register → Login → Logout → LocalStorage cleared
- **Profile persistence:** Save → Close → Reopen → Restored
- **Offline handling:** Network down → Save queued → Network restored → Synced
- **Run mode:** Start → Win → Continue → Save → Relaunch → Run active

### Browser Tests (Manual)
- **Chrome (latest):** Full playthrough, FPS monitor, DevTools console clear
- **Firefox (latest):** Hand drag, UI interactions, no WebGL errors
- **Safari (latest):** Performance, asset loading, IndexedDB access
- **Mobile browsers (optional future):** Test touch input, responsive layout

### Performance Tests
- **Initial load:** Time-to-Interactive < 10 seconds (with caching)
- **Gameplay FPS:** 30+ FPS average (accept dips to 25 on complex hand layouts)
- **API latency:** Profile sync < 2 seconds (include network + server time)
- **Memory usage:** < 500 MB WebGL heap (profiled in Chrome DevTools)

### Deployment Tests (Railway)
- **Backend health:** API endpoints respond 200 OK
- **Database:** Supabase query latency < 100ms
- **Auth:** JWT tokens validated server-side
- **Auto-deploy:** Push to `web/webgl-export` → Railway auto-builds within 5 min

---

## 6. Rollback Plan

### If WebGL Performance is Unacceptable
- **Decision point:** Week 2-3, after performance profiling
- **Action:** Document findings in `web/ROLLBACK_NOTES.md`
- **Keep branch alive:** May revisit with Unity 6 or newer WebGL improvements
- **Fallback:** Evaluate Godot HTML5 or web framework rewrite (separate planning)

### If Backend Persistence is Too Complex
- **Decision point:** Week 1-2, during backend scaffold phase
- **Action:** Revert to local PlayerPrefs only (web build still valid)
- **Impact:** No cloud sync, but game remains fully playable
- **Next step:** Revisit persistence later with simplified storage (e.g., Firebase)

### If Railway Deployment Fails
- **Fallback 1:** Deploy backend to Vercel (Node.js) or Netlify Functions
- **Fallback 2:** Use Firebase Realtime Database (no backend code needed)
- **Fallback 3:** Host WebGL build on GitHub Pages (no persistence)

### Branch Cleanup (if Abandoned)
```bash
git branch -d web/webgl-export         # Delete local branch
git push origin --delete web/webgl-export  # Delete remote (after team consensus)
```
- Keep this repo as reference in a `ROLLBACK_NOTES.md` archive
- Any learnings (performance, architecture) documented for future reference

---

## 7. Success Metrics

| Metric | Target | Validation |
|--------|--------|------------|
| **WebGL build size** | < 100 MB (gzipped: < 30 MB) | Measure in Release build, Vercel deployment |
| **Time-to-Interactive** | < 10 sec (cached: < 3 sec) | Chrome DevTools Lighthouse |
| **In-game FPS** | 60+ FPS avg (min 50 FPS) | DevTools Performance tab, logged stats |
| **Anonymous auth latency** | < 500 ms (device fingerprint → JWT) | Backend logs, browser Network tab |
| **Profile sync time** | < 1.5 sec (game over → backend write) | Measured on game over / quit |
| **Uptime (Railway)** | 99%+ | Railway dashboard |
| **Uptime (Vercel)** | 99.99% | Vercel analytics |
| **Database latency (Supabase)** | < 100 ms for profile queries | Query logs |
| **Test coverage** | Unit + integration tests green | Cypress/Jest (if added) |
| **Documentation** | All steps reproducible | New team member can build & deploy solo |

---

## 8. Dependencies & Context

### Libraries/Services
- **Unity WebGL:** Built-in (2022.3.62f3)
- **Railway:** Backend hosting + auto-deploy (free tier: 5 projects, 500 hours/month, $5/month per project after)
- **Supabase:** PostgreSQL database + auth (free tier: 500 MB, 2 GB bandwidth/month; scales to $25/month)
- **Vercel:** Frontend hosting + CDN + auto-deploy (free tier: 100 GB bandwidth/month, unlimited projects)
- **Node.js + Express:** Backend (Node 18+ LTS recommended)
- **jsonwebtoken (JWT):** Auth tokens (`npm install jsonwebtoken`)
- **pg (PostgreSQL client):** Database access (`npm install pg`)
- **cors:** Enable CORS for WebGL requests (`npm install cors`)
- **dotenv:** Environment variable management (`npm install dotenv`)

### Context References
- **Unity WebGL best practices:** https://docs.unity3d.com/Manual/webgl-intro.html
- **WebGL performance guide:** https://docs.unity3d.com/Manual/webgl-performance.html
- **IndexedDB API:** https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API
- **Railway docs:** https://railway.app/docs
- **Supabase docs:** https://supabase.com/docs
- **Vercel docs:** https://vercel.com/docs
- **Express.js guide:** https://expressjs.com/

---

## 9. Decisions Finalized

✅ **Authentication model:** Anonymous guest mode (automatic device fingerprint) + optional email login (Phase 2)

✅ **Database choice:** Supabase PostgreSQL (structured data, SQL queries, independent from backend, free tier sufficient)

✅ **Performance threshold:** 60+ FPS target (aggressive; optimize if below 50 FPS)

✅ **Mobile web support:** Deferred to Phase 2; MVP targets desktop browsers only (Chrome, Firefox, Safari)

✅ **Frontend hosting:** Vercel (CDN delivery, auto-deploy on push, better DX than serving from Railway)

✅ **Main branch sync:** Keep `web/webgl-export` branch permanently; cherry-pick critical fixes both directions as needed

---

## 10. Timeline & Milestones (Finalized)

| Week | Phase | Deliverable | Notes |
|------|-------|-------------|-------|
| **1** | **Phase 1: Setup** | Branch created; Railway + Supabase + Vercel projects configured; backend scaffold (Express + auth + profile API); Supabase schema defined | Architect + Backend Dev |
| **1-2** | **Phase 2: Integration** | C# web persistence services; anonymous auth + device fingerprint; WebPersistenceManager; LoginScreen UI; 60 FPS profiling | Unity Developer |
| **2** | **Phase 3: Build & Test** | WebGL export (Release build); local testing (60+ FPS validation); Vercel + Railway deployment; end-to-end flow (register → play → save → verify in DB) | Build Engineer + QA |
| **2-3** | **Phase 4: Persistence Validation** | Profile sync across browser refresh; run mode persistence; offline fallback; concurrent update handling; database query performance < 100 ms | Full Team |
| **3** | **Phase 5: Docs** | `web/DEPLOYMENT.md`, `web/BUILD_INSTRUCTIONS.md`, `web/README.md`; environment variable checklist; troubleshooting guide | Tech Lead |
| **3+** | **Ongoing** | Bug fixes, performance tuning, monitoring; mobile support (Phase 2); email auth (Phase 2) | Team |

---

## Approval & Sign-Off

- [x] Architect approves plan
- [x] User approves scope, timeline, and tech stack
- [x] Database: Supabase PostgreSQL (decided 2026-01-07)
- [x] Frontend hosting: Vercel (decided 2026-01-07)
- [x] Performance target: 60+ FPS (decided 2026-01-07)
- [x] Auth model: Anonymous guest mode + optional email (Phase 2)

**Created by:** Reviewer/Architect
**Date:** 2026-01-07
**Status:** Ready for Phase 1 implementation
**Branch:** `web/webgl-export` (to be created)

