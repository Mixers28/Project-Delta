# NOW - Working Memory (WM)

> This file captures the **current focus / sprint**.<br>
> It should always describe what we're doing *right now*.

<!-- SUMMARY_START -->
**Current Focus (auto-maintained by Agent):**
- Verify Railway backend deploy (main branch) and confirm `/health`, `/auth/signup`, `/auth/login` responses.
- Wire Unity Cloud Base URL to the backend domain (no trailing slash) and validate signup/login in WebGL.
- WebGL/mobile UX polish: prompt-based text input, safe-area alignment, and menu button persistence across levels.
- Confirm post-tutorial run gating (straight runs through level 9; suited/color runs for 3/4 from level 10+).
<!-- SUMMARY_END -->

---

## Current Objective

Ship a stable, iteratable Project Delta loop: tutorial → early real game ramp → run mode and unlocks, with reliable level solvability.

---

## Active Branch

- `main`

---

## What We Are Working On Right Now

- [ ] Railway backend: confirm service uses `main` branch and environment vars (`DATABASE_URL`, `JWT_SECRET`, `CORS_ORIGIN`) are set for build/runtime; `/health` returns `{"ok":true}`.
- [ ] Unity cloud auth: confirm Cloud Base URL uses backend domain; signup/login works on WebGL and mobile (keyboard prompt shows).
- [ ] WebGL UI: verify HUD safe-area alignment and menu button remains visible after entering/exiting levels.
- [ ] Progression rules: validate post-tutorial run gating (straight runs through level 9; suited/color runs for 3/4 from level 10+).
- [ ] Docs: keep `SESSION_NOTES.md` (game) vs `docs/SESSION_NOTES.md` (agent session log) in sync.

---

## Next Small Deliverables

- Backend service verified live on Railway with working signup/login.
- WebGL signup/login works on mobile (keyboard prompt appears).
- Safe-area alignment and menu visibility validated across devices.

---

## Notes / Scratchpad

- No blockers tracked here; keep this section for concrete issues (e.g., test failures, Unity version mismatches, or content/pacing questions).
