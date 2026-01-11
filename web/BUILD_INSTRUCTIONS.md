# WebGL Build Instructions

> How to export and test the Project Delta game as WebGL for web deployment.

---

## Prerequisites

- **Unity 2022.3.62f3** (same as main branch)
- **WebGL Build Support** installed (check in Hub: Editor → WebGL Build Support)
- **Backend running locally** (for testing): `npm run dev` in `backend/` folder
- **Local HTTP server** (Python 3 or similar)

---

## Step 1: Configure Unity Project

### 1.1 Update Include Scenes

1. Open Unity Editor
2. Go to **File → Build Settings**
3. In "Scenes In Build," ensure these are included:
   - `Assets/Scenes/StartupScreen.unity` (first)
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/GameScene.unity`
   - Any other playable scenes (tutorial, levels, etc.)
4. **Order matters:** StartupScreen should be index 0

### 1.2 WebGL Build Settings

1. Still in Build Settings:
2. Select **WebGL** from Platform list (left side)
3. Click **"Switch Platform"** (may take a few minutes)

### 1.3 Player Settings (WebGL Specific)

1. Click **Player Settings** (in Build Settings dialog)
2. Go to **Player → WebGL Settings:**

   **Compression Format:**
   - Set to **Brotli** (best compression for build size)
   - If hosting does not set `Content-Encoding: br`, enable **Decompression Fallback** in **Player Settings → Publishing Settings** for local testing or switch to **Gzip** / **Disabled** until headers are configured (see Step 7.2)

   **Debugging:**
   - Enable "Development Build" **only for local testing**
   - Disable for production builds (adds ~20 MB)

   **Quality:**
   - Go to **Quality** tab
   - Select "Fast" or "Simple" preset for WebGL
   - Adjust shadow distance, LOD bias, texture quality as needed

   **Scripting:**
   - Go to **Scripting** tab
   - API Compatibility Level: **.NET Standard 2.1**
   - IL2CPP Stripping Level: **Minimal** (for first build)

---

## Step 2: Configure WebGL Output

### 2.1 Build Output Path

1. In Build Settings, set **Build path** to: `web/build`
   (Relative to project root)

2. Create directory if needed:
   ```bash
   mkdir -p web/build
   ```

### 2.2 Backend Configuration in Unity

1. Create `Assets/Resources/Config/` folder if doesn't exist
2. Create file `Assets/Resources/Config/WebConfig.json`:
   ```json
   {
     "apiUrl": "http://localhost:3000",
     "environment": "development"
   }
   ```

3. In C# scripts, load config:
   ```csharp
   #if UNITY_WEBGL
   TextAsset configFile = Resources.Load<TextAsset>("Config/WebConfig");
   var config = JsonConvert.DeserializeObject<WebConfig>(configFile.text);
   string apiUrl = config.apiUrl;
   #endif
   ```
4. Persist `apiUrl` to browser storage under `api_url` (WebGL only) so the check in Step 7.4 works
   - Create `Assets/Plugins/WebGL/Storage.jslib`:
     ```javascript
     mergeInto(LibraryManager.library, {
       SetApiUrl: function (urlPtr) {
         const url = UTF8ToString(urlPtr);
         localStorage.setItem('api_url', url);
       }
     });
     ```
   - Call it from C#:
     ```csharp
     #if UNITY_WEBGL
     [DllImport("__Internal")] private static extern void SetApiUrl(string url);
     TextAsset configFile = Resources.Load<TextAsset>("Config/WebConfig");
     var config = JsonConvert.DeserializeObject<WebConfig>(configFile.text);
     SetApiUrl(config.apiUrl);
     #endif
     ```
   - Ensure the C# file has `using System.Runtime.InteropServices;`

---

## Step 3: Build WebGL

### 3.1 Build Locally

1. In Build Settings, click **Build** (not "Build & Run" yet)
2. Select `web/build` folder
3. Unity will compile and export (takes 5–15 minutes depending on project size)
4. Wait for completion. Build folder should contain:
   ```
   web/build/
   ├── Build/                  (WebGL runtime files)
   ├── TemplateData/           (HTML template, CSS, JS)
   ├── index.html              (Entry point)
   └── StreamingAssets/        (Custom assets if any)
   ```

### 3.2 Check Build Size

1. Run command:
   ```bash
   du -sh web/build
   ```
2. **Target:** < 100 MB uncompressed, < 30 MB gzipped
3. If too large:
   - Remove unused Scenes from Build Settings
   - Reduce texture resolutions
   - Disable unused 3D packages (ProBuilder, Terrain, etc.)

---

## Step 4: Local Testing

### 4.1 Start Local HTTP Server

```bash
cd web/build
python3 -m http.server 8000
```

Output should be:
```
Serving HTTP on 0.0.0.0 port 8000 (http://0.0.0.0:8000/) ...
```

### 4.2 Test in Browser

1. Open **Chrome** or **Firefox** (Safari may have restrictions)
2. Visit http://localhost:8000
3. Wait for build to load (check browser console for errors)
4. Verify:
   - [ ] Game loads without errors
   - [ ] No console errors (F12 → Console tab)
   - [ ] Anonymous login works (check localStorage for `jwt_token`)
   - [ ] FPS is 60+ (use Chrome DevTools → Performance tab)
   - [ ] Hand drag-to-reorder works with mouse
   - [ ] Quit/Restart buttons functional
   - [ ] Profile saves on game over

### 4.3 Performance Profiling

1. Open **Chrome DevTools** (F12)
2. Go to **Performance** tab
3. Click "Record"
4. Play for 30 seconds
5. Click "Stop"
6. Check metrics:
   - **FPS Graph:** Should stay above 50 FPS (target 60+)
   - **Main thread CPU time:** Should be < 16 ms per frame
   - **Render time:** Should be < 5 ms
   - **Script time:** Should be < 3 ms
7. If any metric is above target:
   - Identify bottleneck in "Bottom-up" tab
   - Optimize or reduce scene complexity

### 4.4 Memory Profiling

1. Open **Chrome DevTools** (F12)
2. Go to **Memory** tab
3. Take a heap snapshot
4. Check usage:
   - **Total heap:** Should be < 500 MB (target)
   - **Detached DOM nodes:** Should be 0–50
   - **Array buffers:** Check for large textures (should be < 200 MB)
5. If high memory:
   - Reduce texture resolution
   - Unload unused scenes
   - Check for memory leaks (repeated snapshots should not grow)

---

## Step 5: Test with Backend

### 5.1 Start Backend (Separate Terminal)

```bash
cd backend
npm run dev
```

Should output:
```
Project Delta API running on http://localhost:3000
```

### 5.2 Update WebGL Config

1. Edit `Assets/Resources/Config/WebConfig.json`:
   ```json
   {
     "apiUrl": "http://localhost:3000",
     "environment": "development"
   }
   ```

2. Rebuild WebGL:
   ```bash
   # In Unity: Build Settings → Build
   ```

### 5.3 Test End-to-End

1. Open http://localhost:8000 in browser
2. Game auto-registers anonymous user
3. Check Supabase console or backend logs:
   ```bash
   # Should see POST /auth/register in backend logs
   ```
4. Play and win a game
5. Check Supabase "game_profiles" table:
   ```bash
   # Tutorial step, coins should be updated
   ```

---

## Step 6: Release Build

### 6.1 Prepare for Production

1. In Unity Build Settings:
   - Uncheck **"Development Build"**
   - Verify compression is **Brotli**

2. Update WebConfig for production:
   ```json
   {
     "apiUrl": "https://project-delta-api.railway.app",
     "environment": "production"
   }
   ```

3. Rebuild:
   ```bash
   # In Unity: Build Settings → Build
   ```

### 6.2 Measure Release Build Size

```bash
du -sh web/build
# Should be < 100 MB
```

If too large, check:
- [ ] Any debug symbols included? (disable in Player Settings)
- [ ] Unused assets in build? (use Build Report)
- [ ] Textures at full resolution? (compress to 1024x1024 or smaller)

---

## Step 7: Deploy to Vercel

### 7.1 Prepare for Upload

1. Ensure `web/build/` is **not** in `.gitignore` on `web/webgl-export` branch
2. Commit and push:
   ```bash
   git add web/build/
   git commit -m "WebGL release build"
   git push origin web/webgl-export
   ```

### 7.2 Configure Brotli Headers (Vercel)

If you are serving `.br` files, you must send `Content-Encoding: br` and the right `Content-Type`.

1. Add `vercel.json` in the Vercel project root (this repo includes `web/vercel.json` for a `web/` project root; move/merge into root `vercel.json` if your Vercel root is the repository):
   ```json
   {
     "headers": [
       {
         "source": "/Build/(.*)\\.wasm\\.br",
         "headers": [
           { "key": "Content-Type", "value": "application/wasm" },
           { "key": "Content-Encoding", "value": "br" }
         ]
       },
       {
         "source": "/Build/(.*)\\.js\\.br",
         "headers": [
           { "key": "Content-Type", "value": "application/javascript" },
           { "key": "Content-Encoding", "value": "br" }
         ]
       },
       {
         "source": "/Build/(.*)\\.data\\.br",
         "headers": [
           { "key": "Content-Type", "value": "application/octet-stream" },
           { "key": "Content-Encoding", "value": "br" }
         ]
       }
     ]
   }
   ```

2. Re-deploy after adding headers.

### 7.3 Verify Auto-Deploy

1. Visit Vercel dashboard
2. Should see build in progress
3. Once complete, visit https://project-delta-web.vercel.app
4. Test end-to-end (register, play, save)

### 7.4 Verify Backend Connection

1. In browser console:
   ```javascript
   // Check API URL being used
   console.log(localStorage.getItem('api_url'));
   ```
2. Monitor backend logs:
   ```bash
   # In Railway dashboard: Service → Logs
   ```

---

## Troubleshooting

### Build Errors

**"WebGL is not installed"**
- **Solution:** In Unity Hub, click ... → Add Modules → select WebGL Support

**"Script compilation error in WebGL"**
- **Solution:** Check for platform-specific code. Use `#if UNITY_WEBGL` / `#endif` to hide native-only code

**"Build is 500+ MB"**
- **Solution:** Check Build Report (Window → Analysis → Build Report), remove unused assets/scenes, compress textures

### Runtime Errors

**"Blank white screen on load"**
- **Problem:** WebGL failing to initialize
- **Solution:** Check browser console (F12), look for shader compilation errors or missing assets
- **Debug:** Try in Chrome Incognito (no cache interference)

**"Loading stuck at 0%"**
- **Problem:** WebGL downloading assets too slowly
- **Solution:** Check network tab in DevTools, verify Vercel CDN is working
- **Debug:** Try local build first to isolate issue

**"Profile not saving"**
- **Problem:** API call failing
- **Solution:** Check CORS errors in console, verify backend is running, check JWT token in localStorage
- **Debug:** Test API manually:
  ```bash
  curl -X GET http://localhost:3000/health
  ```

**"FPS drops below 50"**
- **Problem:** Game too slow for WebGL
- **Solution:** Check Performance tab (see Step 4.3), optimize shaders or reduce draw calls
- **Debug:** Profile with Profiler window open (Window → Analysis → Profiler)

---

## Best Practices

1. **Test locally first:** Always do a full end-to-end test locally before deploying
2. **Monitor file sizes:** Keep textures at 1024x1024 or smaller
3. **Use staging:** Deploy to staging Vercel URL before production
4. **Check logs:** Monitor Railway logs for API errors
5. **Profile regularly:** Use Chrome DevTools Performance tab to catch regressions

---

## Next Steps

- [ ] Set up CI/CD pipeline to auto-build on commit
- [ ] Implement A/B testing for performance optimizations
- [ ] Set up error tracking (Sentry or similar)
- [ ] Add analytics to profile player behavior

---

## References

- **Unity WebGL Docs:** https://docs.unity3d.com/Manual/webgl-intro.html
- **WebGL Performance:** https://docs.unity3d.com/Manual/webgl-performance.html
- **Chrome DevTools:** https://developer.chrome.com/docs/devtools/
- **Vercel Deployment:** https://vercel.com/docs
