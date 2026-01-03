# Web Escape-Room Ladder Blueprint

## Goals
- Ship a minimal-but-fun escape-room ladder that can launch in 2–4 weeks and grow seasonally.
- Keep validation server-side (JWT/HMAC tokens for progression) and avoid leaking solutions in client assets.
- Start with a minimal content schema and pipelines that support fast iteration.

## Level Definition (JSON)
Each level entry can live in a JSON (or YAML) file loaded by the server. Fields are intentionally small so the MVP can load from disk before moving to a DB.

```json
{
  "id": "level-03",
  "title": "Spectral Whisper",
  "prompt": "The waveform hides a single word.",
  "mechanics": ["audio-steg", "spectrogram"],
  "assetRefs": [{ "type": "audio", "path": "/assets/audio/level-03.wav" }],
  "validator": {
    "type": "exact",           // "exact" | "pattern" | "regex" | "hmac" | "script"
    "answer": "ECHO"           // For "hmac", store a server-side secret + hash; for "script", point to a validator function.
  },
  "nextId": "level-04",
  "hints": [
    "Look at the picture of the sound, not the sound itself.",
    "A standard spectrogram view reveals the word across the center band."
  ],
  "difficulty": 3,
  "metadata": {
    "rateLimit": { "windowSeconds": 120, "maxAttempts": 8 },
    "cooldownSeconds": 30,
    "tags": ["tutorial", "audio"]
  }
}
```

### Validator Types
- **exact**: Case-insensitive match on a normalized answer string.
- **pattern**: Accept a set of valid strings (useful for alternate phrasings or multi-part answers).
- **regex**: Server-side regex against normalized input (escape user input and anchor the pattern).
- **hmac**: Compare `HMAC(secret, answer)` server-side; avoids storing raw answers when using config files.
- **script**: Call a server-side function that can perform multi-step checks (e.g., verifying fragments collected). Keep functions pure and tested.

### Progress Tokens
- After validation, mint a signed token (JWT or HMAC) that encodes `{ levelId, userId/anonSession, issuedAt, expiresAt, nonce }`.
- Require the previous level’s token in the request body for unlocking the next page. Rotate the signing secret per season.
- Store minimal session progress server-side (token replay table or short database row keyed by session).

## Next.js Route Structure (App Router)
```
/app
  /(levels)
    /level/[id]/page.tsx          // Renders puzzle page using level JSON
  /api/levels/[id]/validate/route.ts  // POST { answer, token } → { ok, nextToken, nextId }
  /api/progress/route.ts          // GET current level state for session token
/assets                           // Public puzzle assets (audio/image); hide answers server-side
```
- Use `getLevel(id)` in a shared server module that pulls from JSON or SQLite.
- Normalize inputs (`trim`, `lowercase`, collapse whitespace) before validation.
- Rate-limit the validate route (e.g., Upstash, Vercel middleware).
- For client UI, keep a tiny state machine: `idle → submitting → success|error` with debounced submissions.

## Rails Route Structure (if you pivot)
```
# config/routes.rb
resources :levels, only: [:show] do
  post :validate, on: :member
end
resource :progress, only: [:show]
```
- `LevelsController#show` renders the puzzle view from `Level` records or JSON fixtures.
- `LevelsController#validate` runs server-side validators and returns `{ ok, next_token, next_id }` JSON.
- Use `ActiveSupport::MessageVerifier` or `MessageEncryptor` for signed tokens; rotate secrets per season.
- Add a simple `Progress` model keyed by anonymous session UUID for rate limiting and audit.

## Season 1 Puzzle Briefs (Onboarding + 10 Rooms)
Each room lists the answer, mechanic, assets, and a teaching goal so you can tune difficulty. Keep the copy short and atmospheric in the UI.

### Onboarding Micro-Arc (teaches "explore + inspect")
0) **Try Me** — _Click basics_
   - **Screen:** Black background, centered white text: `TRY ME`.
   - **Mechanic:** Clicking matters; anywhere on the text advances.
   - **Answer:** Any click on `TRY ME`.
   - **Teaching Goal:** Establish that interaction progresses the ladder.

1) **Nothing Here** — _Hidden UI discovery_
   - **Screen:** White page with small top-left copy: "Find the way forward. Not everything is visible at first." Optional hint after 20s idle.
   - **Mechanic:** Invisible "next" icon/button in bottom-right with generous hitbox (≥80×80px). Use hover/nearby reveal (opacity 0→1) or long-press outline mode on mobile. CSS-selection variant is a fallback.
   - **Answer:** Click/tap the revealed icon.
   - **Teaching Goal:** Encourage scanning edges, hovering, and long-pressing; give near-miss feedback (cursor change/shadow).

2) **Source Code** — _Inspect element intro_
   - **Screen:** Minimal page that hints: "Interact with the page." Add comment like `<!-- you're getting warmer -->` or a meta tag with the token.
   - **Mechanic:** Clue is in HTML source/metadata.
   - **Answer:** `view-source` or the revealed token.
   - **Teaching Goal:** Teach players to check source/metadata early.

### Main Season Run (rooms 3–10)
3) **Door Code** — _Pattern intro_
   - **Mechanic:** Number pattern.
   - **Prompt:** Sequence of 4 clocks showing 1:05, 2:10, 3:20, ?
   - **Answer:** `4:40` (doubling minutes).
   - **Teaching Goal:** Establish "find the rule" expectation.

4) **Rotunda** — _Caesar warmup_
   - **Mechanic:** Caesar cipher (+3).
   - **Prompt:** "Krod wudyhohuph!" with a bronze relief showing +3.
   - **Answer:** `Hail traveler`.
   - **Teaching Goal:** Introduce ciphers and hint placement.

5) **Chime** — _Audio spectrogram_
   - **Mechanic:** Audio file where the word appears in spectrogram.
   - **Prompt:** A bell tone; caption "see the sound".
   - **Answer:** `ECHO`.
   - **Teaching Goal:** Introduce light audio-steg.

6) **Exposure** — _Image EXIF_
   - **Mechanic:** JPEG with EXIF Artist field containing clue.
   - **Prompt:** Photograph of an empty hallway.
   - **Answer:** `SHUTTER`.
   - **Teaching Goal:** Nudge players to inspect metadata.

7) **Gridlock** — _Logic grid_
   - **Mechanic:** 4x4 grid deduction (colors vs. shapes).
   - **Prompt:** Clues lead to single remaining coordinate.
   - **Answer:** `B3`.
   - **Teaching Goal:** Practice structured deduction.

8) **Fragments** — _Multi-step_
   - **Mechanic:** Collect 3 fragments across prior rooms (one per solved page) and order them.
   - **Prompt:** UI has three empty slots; previous rooms each reveal one letter on success screen.
   - **Answer:** `KEY`.
   - **Teaching Goal:** Show cross-room dependency.

9) **Circuit** — _State machine switches_
   - **Mechanic:** Toggle switches with rules (e.g., toggling A flips B and C).
   - **Prompt:** Goal state diagram shown faintly.
   - **Answer:** `CAB` (final switch order to reach goal).
   - **Teaching Goal:** Interactive puzzle with simple state reasoning.

10) **Vigenère** — _Keyed cipher_
    - **Mechanic:** Vigenère with key provided by Level 4’s flavor text.
    - **Prompt:** Ciphertext and hint "the rotunda’s motto guides you".
    - **Answer:** `ASCEND` (key: TRAVELER).
    - **Teaching Goal:** Deeper cipher with cross-level clue.

11) **Finale** — _Mixed meta_
    - **Mechanic:** Combine callbacks: need the Level 2 meta tag, Level 6 EXIF word, and a short Caesar shift to unlock.
    - **Prompt:** Door with three recesses and "shift them together" note.
    - **Answer:** `VISTA` (concat tokens, apply -1 Caesar).
    - **Teaching Goal:** Payoff meta that rewards note-taking.

## Safety & Anti-Cheat Notes
- Do not embed raw answers in bundled JS; use API validation only.
- Serve puzzle assets from public paths but keep hints in separate routes gated by tokens + cooldowns.
- Log validation attempts per IP/session to tune rate limits and detect brute force.
- Rotate signing secrets between seasons; invalidate tokens on rotation and force a re-auth handshake.

## Post-MVP Backlog
- **Hint economy:** Cooldown-based hints (free), optional paid "season pass" for bonus rooms.
- **Notebook:** Client-side saved notes + collected fragments; sync to session token.
- **Creator mode:** Authenticated admin UI to add/edit levels, upload assets, preview validators.
- **Leaderboards:** Optional timed runs; store start/end timestamps and number of hints used.
