# NOW - Working Memory (WM)

> This file captures the **current focus / sprint**.<br>
> It should always describe what we're doing *right now*.

<!-- SUMMARY_START -->
**Current Focus (auto-maintained by Agent):**
- Finish repo hygiene (ensure Unity-generated folders stay untracked/ignored; keep diffs quiet after opening Unity).
- Validate run mode persistence across win/continue/quit/resume; reset only on fail or explicit restart/new run.
- Playtest tutorial pacing/solvability + early→mid rules transition (mid tier at 7 non-tutorial wins; no jokers; suited runs only).
- Polish core UX loops: stable drag-to-reorder hand, achievements access via Main Menu, and safe reset/quit flows.
- WebGL/mobile UI polish: safe-area overlays, win/lose continue button placement, and startup screen backdrop sizing.
<!-- SUMMARY_END -->

---

## Current Objective

Ship a stable, iteratable Project Delta loop: tutorial → early real game ramp → run mode and unlocks, with reliable level solvability.

---

## Active Branch

- `main`

---

## What We Are Working On Right Now

- [ ] Git hygiene: confirm `Library/`, `Logs/`, `obj/`, `.vs/`, `UserSettings/`, `Temp/` stay ignored and untracked; remove any remaining tracked/generated files (use `git rm` when needed; avoid deleting other local folders unless requested).
- [ ] Run mode: confirm run persists through wins and “Continue”, and across “Quit (Save)” → relaunch; reset only on loss or explicit “Restart Run”.
- [ ] Tutorial + progression: validate steps 1–7 targets (~2–3 min), and verify Mid tier behavior at 7 non-tutorial wins (no jokers; suit-agnostic runs disabled; suited runs only).
- [ ] Hand UX: fix mobile drag jitter in grid reorder; verify drag-to-reorder is stable (no flicker/jitter), stays within hand container bounds, and highlights insertion slot clearly.
- [ ] WebGL UI: confirm win/lose overlay buttons + continue label are legible and on-screen; startup splash backdrop fits parent without clipping.
- [ ] Main menu UX: Achievements lives in menu; confirm quit dialog shows correct note; “Restart Game (Tutorial)” correctly replays intro/tutorial flow.
- [ ] Run Edit/PlayMode tests in Unity Test Runner and keep `Assets/Tests` green.
- [ ] Keep docs consistent: `SESSION_NOTES.md` (game) vs `docs/SESSION_NOTES.md` (agent session log); avoid cross-contamination.

---

## Next Small Deliverables

- Clean git status after opening Unity (no generated folders tracked; minimal diffs).
- Tutorial feels consistent and solvable across replays (deterministic/preset deck tweaks where needed).
- Run mode persistence verified (including quit/resume), with correct “Run Ended”/“New Run” behavior on loss.
- Confirmed green test run in Unity Test Runner (Edit Mode + Play Mode).

---

## Notes / Scratchpad

- No blockers tracked here; keep this section for concrete issues (e.g., test failures, Unity version mismatches, or content/pacing questions).
