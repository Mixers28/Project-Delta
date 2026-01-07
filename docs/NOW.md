# NOW - Working Memory (WM)

> This file captures the **current focus / sprint**.<br>
> It should always describe what we're doing *right now*.

<!-- SUMMARY_START -->
**Current Focus (auto-maintained by Agent):**
- **Main branch (`main`):** Polish and stabilize native game (tutorial pacing, run mode persistence, UX flows, tests).
- **Web branch (`web/webgl-export`):** Implement WebGL export with cloud persistence; Phase 1 complete, Phase 2 in progress.
  - Phase 1 ✅ Done: Backend scaffold (Express.js, JWT auth, profile API), Supabase schema, Railway/Vercel configs.
  - Phase 2 (Next): C# web persistence services, anonymous login UI, 60 FPS profiling, local WebGL build testing.
<!-- SUMMARY_END -->

---

## Current Objective

- **Main:** Ship a stable, iteratable Project Delta loop: tutorial → early real game ramp → run mode and unlocks, with reliable level solvability.
- **Web:** Validate WebGL deployment with cloud persistence; proof-of-concept playable in browser with cross-session save functionality.

---

## Active Branches

- `main` – Native desktop game development
- `web/webgl-export` – WebGL browser variant with cloud persistence (permanent parallel branch)

---

## What We Are Working On Right Now

### Main Branch (`main`)

- [ ] Git hygiene: confirm `Library/`, `Logs/`, `obj/`, `.vs/`, `UserSettings/`, `Temp/` stay ignored and untracked; remove any remaining tracked/generated files (no local deletes).
- [ ] Run mode: confirm run persists through wins and "Continue", and across "Quit (Save)" → relaunch; reset only on loss or explicit "Restart Run".
- [ ] Tutorial + progression: validate steps 1–7 targets (~2–3 min), and verify Mid tier behavior at 7 non-tutorial wins (no jokers; suit-agnostic runs disabled; suited runs only).
- [ ] Hand UX: verify drag-to-reorder is stable (no flicker/jitter), stays within hand container bounds, and highlights insertion slot clearly.
- [ ] Main menu UX: Achievements lives in menu; confirm quit dialog shows correct note; "Restart Game (Tutorial)" correctly replays intro/tutorial flow.
- [ ] Run Edit/PlayMode tests in Unity Test Runner and keep `Assets/Tests` green.

### Web Branch (`web/webgl-export`)

- [ ] **Phase 2 (In Progress):**
  - [ ] Create `Assets/Scripts/Web/` folder with persistence services (WebPersistenceManager, WebAuthManager, WebProfileSyncService)
  - [ ] Create `Assets/Scripts/UI/LoginScreen.cs` – auto-register anonymous users via device fingerprint
  - [ ] Add conditional compilation (`#if UNITY_WEBGL`) to GameManager for web initialization
  - [ ] Create `Assets/Resources/Config/WebConfig.json` template (API URL, environment)
  - [ ] Profile WebGL build for 60+ FPS targets; optimize if needed

- [ ] **Phase 3 (TBD):**
  - [ ] Configure Unity WebGL build settings (compression, scenes, player settings)
  - [ ] Generate local WebGL build → test in Python HTTP server
  - [ ] Test end-to-end: register → play → save → verify in Supabase
  - [ ] Deploy to Vercel + Railway; validate cross-domain requests

---

## Next Small Deliverables

### Main
- Clean git status after opening Unity (no generated folders tracked; minimal diffs).
- Tutorial feels consistent and solvable across replays (deterministic/preset deck tweaks where needed).
- Run mode persistence verified (including quit/resume), with correct "Run Ended"/"New Run" behavior on loss.
- Confirmed green test run in Unity Test Runner (Edit Mode + Play Mode).

### Web
- C# web persistence services implemented and tested locally
- Anonymous login flow working (device fingerprint → JWT token in localStorage)
- WebGL build running at 60+ FPS in Chrome DevTools Performance tab
- End-to-end test: register → play → game over → profile saved to Supabase

---

## Notes / Scratchpad

- **Main branch:** No blockers; keep focused on polish and stability.
- **Web branch:** Awaiting manual Railway/Supabase/Vercel project creation (can test backend locally with `npm run dev`).
- **Docs:** Keep `SESSION_NOTES.md` (game notes) vs `docs/SESSION_NOTES.md` (session log) separate to avoid drift.
