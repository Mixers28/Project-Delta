# WebGL Deployment Guide

> Step-by-step instructions for deploying Project Delta WebGL to production (Railway + Supabase + Vercel)

---

## Overview

- **Frontend (WebGL):** Vercel (CDN-hosted static build)
- **Backend (API):** Railway (Node.js Express server)
- **Database:** Supabase (PostgreSQL)
- **Branch:** `web/webgl-export`

---

## Phase 1: Database Setup (Supabase)

### 1. Create Supabase Project

1. Go to [supabase.com](https://supabase.com)
2. Click "New Project"
3. Fill in:
   - **Project name:** `project-delta` (or similar)
   - **Database password:** Generate strong password (save it!)
   - **Region:** Choose closest to your players (e.g., `us-east-1`)
4. Click "Create new project" and wait ~2 minutes

### 2. Run Schema SQL

1. In Supabase dashboard, click "SQL Editor"
2. Click "New Query"
3. Copy entire contents of `backend/src/db/schema.sql`
4. Paste into SQL editor
5. Click "Run"
6. Verify tables created: check "Tables" in left sidebar

### 3. Get Connection String

1. In Supabase, click "Settings" (bottom left)
2. Click "Database"
3. Under "Connection string," select "Connection pooling" (Supabase uses PgBouncer)
4. Copy the connection string (looks like: `postgresql://user:password@host:5432/postgres`)
5. **Save this as `DATABASE_URL` for step below**

---

## Phase 2: Backend Setup (Railway)

### 1. Create Railway Project

1. Go to [railway.app](https://railway.app)
2. Click "Create a new project"
3. Select "GitHub Repo"
4. Connect your GitHub account and select this repo
5. Name it `project-delta-api`

### 2. Configure Environment Variables

1. In Railway dashboard, click your project
2. Go to "Variables"
3. Add the following:

   ```
   DATABASE_URL=<paste Supabase connection string from above>
   JWT_SECRET=<generate: openssl rand -base64 32>
   CORS_ORIGIN=https://project-delta-web.vercel.app
   NODE_ENV=production
   ```

4. Click "Save"

### 3. Configure Build + Deploy Settings

1. In Railway, click your service (should be auto-detected as Node.js)
2. Go to "Settings"
3. Verify:
   - **Start Command:** `npm start`
   - **Build Command:** `npm install`
   - **Watch Paths:** (leave empty)
4. Generate a public domain:
   - Click "Networking"
   - Enable "Public Networking"
   - Copy the domain (e.g., `project-delta-api-production-xxxx.up.railway.app`)
   - **Save this as your API URL**

### 4. Deploy

1. Click "Deploy"
2. Wait for build to complete (watch logs)
3. Test health check:
   ```bash
   curl https://<your-railway-domain>/health
   # Should respond: {"status":"ok","timestamp":"..."}
   ```

---

## Phase 3: Frontend Setup (Vercel)

### 1. Create Vercel Project

1. Go to [vercel.com](https://vercel.com)
2. Click "Add New"
3. Select "Project"
4. Import from Git → select this repo
5. Name it `project-delta-web`

### 2. Configure Environment Variables

1. In Vercel, go to "Settings" → "Environment Variables"
2. Add:
   ```
   REACT_APP_API_URL=<your Railway domain from above>
   ```
3. Click "Save"

### 3. Configure Build Settings

1. Go to "Settings" → "Build & Development Settings"
2. **Build Command:** `npm run build` (will be defined once WebGL build structure is ready)
3. **Output Directory:** `./web-build` (or wherever WebGL is exported)
4. Save and redeploy

### 4. Test Deployment

1. Wait for build to complete
2. Visit your Vercel domain (e.g., `https://project-delta-web.vercel.app`)
3. Should load WebGL build
4. Check browser console for API calls (should hit your Railway backend)

---

## Phase 4: Unity WebGL Integration

### 1. Update Backend URL in Unity

1. In Unity Editor, create `Assets/Resources/Config/WebConfig.json`:
   ```json
   {
     "apiUrl": "https://your-railway-domain"
   }
   ```

2. Or hardcode in `Assets/Scripts/Web/WebPersistenceConfig.cs`:
   ```csharp
   #if UNITY_WEBGL
   public string apiUrl = "https://your-railway-domain";
   #endif
   ```

### 2. Build WebGL

1. Unity → File → Build Settings
2. Switch Platform to WebGL
3. Player Settings → WebGL → Compression Format: Brotli
4. Build into `/web-build/` folder
5. Test locally:
   ```bash
   cd web-build
   python3 -m http.server 8000
   # Visit http://localhost:8000
   ```

---

## Health Checks & Validation

### Backend Health
```bash
# Check if API is running
curl https://<your-railway-domain>/health

# Response should be:
{"status":"ok","timestamp":"2026-01-07T..."}
```

### Database Health
```bash
# Login to Supabase SQL editor, run:
SELECT COUNT(*) FROM users;
SELECT COUNT(*) FROM game_profiles;
```

### End-to-End Test
1. Visit WebGL build on Vercel
2. Game should auto-register anonymous user
3. Check Supabase "users" table for new entry
4. Play a game, win, quit
5. Check Supabase "game_profiles" table - profile should be updated

---

## Troubleshooting

### "CORS error" or "403 Forbidden"

**Problem:** WebGL can't reach backend
- **Solution:** Check `CORS_ORIGIN` env var on Railway matches your Vercel domain
- **Debug:** Open browser DevTools → Network tab, check CORS headers

### "401 Unauthorized" on API calls

**Problem:** JWT token is invalid or expired
- **Solution:** Clear browser localStorage, reload page to re-register
- **Debug:** Check token in localStorage: `console.log(localStorage.getItem('jwt_token'))`

### "Database connection refused"

**Problem:** Railway can't reach Supabase
- **Solution:** Verify `DATABASE_URL` is correct in Railway env vars
- **Debug:** Check Railway logs: go to service → View Logs

### WebGL build too large (> 100 MB)

**Problem:** Build size exceeds limits
- **Solution:** Optimize assets, enable Brotli compression, reduce included scenes
- **Debug:** Use `Build → Report` in Unity to analyze

---

## Auto-Deploy Configuration

### GitHub to Railway
Railway auto-detects pushes to `web/webgl-export` branch and deploys automatically.

**Disable auto-deploy (if needed):**
1. Go to Railway project → Settings
2. Disable "Auto Deploy"

### GitHub to Vercel
Vercel auto-deploys on push to the branch specified during setup.

**Verify branch:** Vercel → Settings → Git → Deploy Branch should be `web/webgl-export`

---

## Monitoring & Logs

### Railway Logs
1. Dashboard → Service → Logs
2. Filter by: error, warning, info
3. Example: search for `POST /api/profile` to see sync requests

### Vercel Analytics
1. Dashboard → Analytics
2. Monitor build times, deployment history, bandwidth

### Supabase Logs
1. Dashboard → Logs
2. View recent queries to database
3. Check latency metrics

---

## Rollback Instructions

### Rollback Vercel
```bash
git push origin :web/webgl-export  # Delete remote branch
# OR
# Manually revert commit in Vercel dashboard
```

### Rollback Railway
1. Dashboard → Service
2. Click "Revert to Previous Deployment"
3. Select stable version from history

### Rollback Database (Supabase)
1. Supabase backup → Restore from date
2. (Requires paid plan; free tier has daily backups)

---

## Next Steps

- [ ] Set up monitoring alerts (Railway uptime, API error rate)
- [ ] Configure CDN caching headers for WebGL assets
- [ ] Add analytics tracking (game events → backend logs)
- [ ] Implement email auth (Phase 2)
- [ ] Add mobile touch support (Phase 2)

