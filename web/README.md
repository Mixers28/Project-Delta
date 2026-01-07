# Project Delta – WebGL Edition

> Browser-based version of Project Delta with cloud persistence (Railway API + Supabase PostgreSQL, hosted on Vercel).

---

## Quick Start

This branch (`web/webgl-export`) contains the WebGL export configuration and backend infrastructure for Project Delta as a web game.

### For Developers

1. **Backend Setup:** Follow [DEPLOYMENT.md](./DEPLOYMENT.md)
2. **Local Testing:**
   ```bash
   # Terminal 1: Backend
   cd backend
   npm install
   npm run dev    # Starts on http://localhost:3000

   # Terminal 2: Frontend (after WebGL build)
   cd web-build
   python3 -m http.server 8000  # Opens http://localhost:8000
   ```

3. **Environment Variables:**
   - Copy `backend/.env.example` → `backend/.env`
   - Fill in `DATABASE_URL` and `JWT_SECRET`

### For Testers

1. Visit https://project-delta-web.vercel.app
2. Play through tutorial (should auto-save progress)
3. Report any FPS drops, loading issues, or sync failures

---

## Architecture

```
Project Delta (main branch)
    ↓
web/webgl-export (this branch)
    ├── backend/                    # Node.js + Express API
    │   ├── src/
    │   │   ├── server.js          # Main server (auth, profile, run history)
    │   │   ├── routes/            # API route handlers
    │   │   ├── middleware/        # JWT validation, CORS
    │   │   └── db/schema.sql      # PostgreSQL schema
    │   ├── package.json
    │   ├── railway.json           # Railway deployment config
    │   └── .env.example
    │
    ├── web/                        # Documentation (this folder)
    │   ├── README.md              # This file
    │   ├── DEPLOYMENT.md          # Setup instructions
    │   └── BUILD_INSTRUCTIONS.md  # WebGL export guide
    │
    └── vercel.json                # Vercel deployment config

    Hosted On:
    - Frontend (WebGL): Vercel CDN
    - Backend (API): Railway
    - Database: Supabase PostgreSQL
```

---

## Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | Unity WebGL 2022.3.62f3 | Game runtime in browser |
| **Backend** | Node.js 18 + Express | REST API (auth, profiles, progression) |
| **Database** | Supabase PostgreSQL | User data, game state, run history |
| **Hosting** | Vercel (frontend) + Railway (backend) | CDN delivery, auto-deploy |

---

## Key Features

### Phase 1 (MVP - Current)
- ✅ WebGL export with 60+ FPS target
- ✅ Anonymous user registration (device fingerprint)
- ✅ Profile persistence (tutorial step, unlocks, achievements, coins)
- ✅ Run mode persistence across browser refresh
- ✅ Auto-deploy on push to `web/webgl-export`

### Phase 2 (Future)
- 🔲 Email/password authentication
- 🔲 Mobile web support (touch drag-to-reorder)
- 🔲 Run history analytics
- 🔲 Leaderboards

### Phase 3+ (Nice to Have)
- 🔲 Cross-device sync (cloud save on multiple browsers)
- 🔲 Social features (share scores, friend challenges)
- 🔲 Offline mode (service workers)

---

## Performance Targets

| Metric | Target | Status |
|--------|--------|--------|
| **In-game FPS** | 60+ (min 50) | TBD (Phase 3) |
| **Time-to-Interactive** | < 10 sec | TBD (Phase 3) |
| **API latency** | < 500 ms | TBD (Phase 3) |
| **Profile sync time** | < 1.5 sec | TBD (Phase 4) |
| **Build size** | < 100 MB (gzipped < 30 MB) | TBD (Phase 3) |

---

## Development Workflow

### Adding a Feature

1. **Design:** Sketch API changes in `spec.md`
2. **Backend:** Implement endpoint in `backend/src/server.js`
3. **Database:** Add schema migration if needed
4. **Frontend:** Call API from Unity C# scripts
5. **Test:** Local WebGL build + Supabase console
6. **Deploy:** Push to `web/webgl-export` → auto-deploy

### Example: Adding a New API Endpoint

```javascript
// backend/src/server.js
app.post('/api/custom-endpoint', requireAuth, async (req, res) => {
  const { data } = req.body;
  // ... implement endpoint
  res.status(200).json({ result: 'success' });
});
```

```csharp
// Assets/Scripts/Web/WebPersistenceManager.cs
private async void CallCustomEndpoint(string data) {
  var response = await httpClient.PostAsync(
    $"{API_URL}/api/custom-endpoint",
    new StringContent(JsonConvert.SerializeObject(new { data }))
  );
}
```

---

## Troubleshooting

### Common Issues

**Q: "CORS error when calling API"**
- A: Check Railway `CORS_ORIGIN` env var matches your Vercel domain

**Q: "WebGL takes 30+ seconds to load"**
- A: Check browser cache, bundle size in Release build, network throttling

**Q: "Profile not saving after win"**
- A: Check browser console for API errors, verify Supabase table has data, check JWT token in localStorage

**Q: "60 FPS not achievable"**
- A: Profile in Chrome DevTools → Performance tab, check for bottlenecks (shaders, draw calls, GC pauses)

See [DEPLOYMENT.md](./DEPLOYMENT.md#troubleshooting) for more details.

---

## Useful Links

- **Unity WebGL:** https://docs.unity3d.com/Manual/webgl-intro.html
- **WebGL Performance:** https://docs.unity3d.com/Manual/webgl-performance.html
- **Railway Docs:** https://railway.app/docs
- **Supabase Docs:** https://supabase.com/docs
- **Vercel Docs:** https://vercel.com/docs

---

## Contributing

1. Work on `web/webgl-export` branch
2. Test locally (backend + WebGL build)
3. Push to GitHub → auto-deploys to Vercel + Railway
4. Monitor logs in dashboards
5. Cherry-pick important fixes back to `main` if needed

---

## Support

- **Issues:** Report in GitHub Issues (tag with `web/` prefix)
- **Questions:** Check [DEPLOYMENT.md](./DEPLOYMENT.md) or [BUILD_INSTRUCTIONS.md](./BUILD_INSTRUCTIONS.md)
- **Performance:** Profile in Chrome DevTools, share results in issue

---

## License

Same as Project Delta main branch.

