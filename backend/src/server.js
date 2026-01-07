require('dotenv').config();
const express = require('express');
const cors = require('cors');
const jwt = require('jsonwebtoken');
const { Pool } = require('pg');

// ============================================================================
// SETUP
// ============================================================================

const app = express();
const PORT = process.env.PORT || 3000;
const JWT_SECRET = process.env.JWT_SECRET || 'dev-secret-change-in-prod';
const CORS_ORIGIN = process.env.CORS_ORIGIN || 'http://localhost:5173';

// Database connection pool (Supabase PostgreSQL)
const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }, // Required for Supabase
});

// Middleware
app.use(cors({ origin: CORS_ORIGIN, credentials: true }));
app.use(express.json());

// ============================================================================
// UTILITIES
// ============================================================================

/**
 * Generate JWT token for user
 */
function generateToken(userId) {
  return jwt.sign({ userId }, JWT_SECRET, { expiresIn: '30d' });
}

/**
 * Verify JWT token and extract userId
 */
function verifyToken(token) {
  try {
    const decoded = jwt.verify(token, JWT_SECRET);
    return decoded.userId;
  } catch (err) {
    return null;
  }
}

/**
 * Extract token from Authorization header
 */
function getTokenFromHeader(req) {
  const authHeader = req.headers.authorization || '';
  const match = authHeader.match(/^Bearer\s+(.+)$/);
  return match ? match[1] : null;
}

// ============================================================================
// MIDDLEWARE
// ============================================================================

/**
 * Require valid JWT token
 */
function requireAuth(req, res, next) {
  const token = getTokenFromHeader(req);
  if (!token) {
    return res.status(401).json({ error: 'Missing or invalid authorization token' });
  }

  const userId = verifyToken(token);
  if (!userId) {
    return res.status(401).json({ error: 'Invalid or expired token' });
  }

  req.userId = userId;
  next();
}

// ============================================================================
// ROUTES: Health Check
// ============================================================================

app.get('/health', (req, res) => {
  res.status(200).json({ status: 'ok', timestamp: new Date().toISOString() });
});

// ============================================================================
// ROUTES: Authentication
// ============================================================================

/**
 * POST /auth/register
 * Create an anonymous user with device fingerprint
 *
 * Request body:
 * {
 *   "deviceFingerprint": "string (e.g., browser/OS/GPU combo)"
 * }
 *
 * Response:
 * {
 *   "token": "JWT token",
 *   "userId": "UUID",
 *   "isNewUser": boolean
 * }
 */
app.post('/auth/register', async (req, res) => {
  try {
    const { deviceFingerprint } = req.body;

    if (!deviceFingerprint) {
      return res.status(400).json({ error: 'deviceFingerprint is required' });
    }

    // Check if user with this fingerprint already exists
    const existingUser = await pool.query(
      'SELECT id FROM users WHERE device_fingerprint = $1 LIMIT 1',
      [deviceFingerprint]
    );

    let userId;
    let isNewUser = false;

    if (existingUser.rows.length > 0) {
      // User already exists
      userId = existingUser.rows[0].id;
    } else {
      // Create new user
      const result = await pool.query(
        'INSERT INTO users (device_fingerprint, created_at) VALUES ($1, NOW()) RETURNING id',
        [deviceFingerprint]
      );
      userId = result.rows[0].id;
      isNewUser = true;

      // Create empty game profile for new user
      await pool.query(
        `INSERT INTO game_profiles (
          user_id, tutorial_step, unlocks_json, run_stats_json, achievements_json, coins, updated_at
        ) VALUES ($1, 0, '{}', '{}', '{}', 0, NOW())`,
        [userId]
      );
    }

    const token = generateToken(userId);

    res.status(200).json({
      token,
      userId,
      isNewUser,
    });
  } catch (err) {
    console.error('Error in /auth/register:', err);
    res.status(500).json({ error: 'Internal server error' });
  }
});

/**
 * POST /auth/login-email (Future - Phase 2)
 * Email/password login for registered users
 */
app.post('/auth/login-email', async (req, res) => {
  res.status(501).json({ error: 'Email login not yet implemented (Phase 2)' });
});

// ============================================================================
// ROUTES: Player Profile
// ============================================================================

/**
 * GET /api/profile
 * Fetch the user's game profile
 *
 * Authorization: Bearer <token>
 *
 * Response:
 * {
 *   "userId": "UUID",
 *   "tutorialStep": number,
 *   "unlocksJson": object,
 *   "runStatsJson": object,
 *   "achievementsJson": object,
 *   "coins": number,
 *   "updatedAt": "ISO timestamp"
 * }
 */
app.get('/api/profile', requireAuth, async (req, res) => {
  try {
    const result = await pool.query(
      `SELECT
        id, user_id, tutorial_step, unlocks_json, run_stats_json, achievements_json, coins, updated_at
      FROM game_profiles
      WHERE user_id = $1`,
      [req.userId]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Profile not found' });
    }

    const profile = result.rows[0];
    res.status(200).json({
      userId: profile.user_id,
      tutorialStep: profile.tutorial_step,
      unlocksJson: profile.unlocks_json || {},
      runStatsJson: profile.run_stats_json || {},
      achievementsJson: profile.achievements_json || {},
      coins: profile.coins || 0,
      updatedAt: profile.updated_at,
    });
  } catch (err) {
    console.error('Error in GET /api/profile:', err);
    res.status(500).json({ error: 'Internal server error' });
  }
});

/**
 * POST /api/profile
 * Upsert (merge) the user's game profile
 *
 * Authorization: Bearer <token>
 *
 * Request body:
 * {
 *   "tutorialStep": number (optional),
 *   "unlocksJson": object (optional),
 *   "runStatsJson": object (optional),
 *   "achievementsJson": object (optional),
 *   "coins": number (optional)
 * }
 *
 * Response: Updated profile object
 */
app.post('/api/profile', requireAuth, async (req, res) => {
  try {
    const {
      tutorialStep,
      unlocksJson,
      runStatsJson,
      achievementsJson,
      coins,
    } = req.body;

    // Fetch current profile
    const currentResult = await pool.query(
      'SELECT * FROM game_profiles WHERE user_id = $1',
      [req.userId]
    );

    if (currentResult.rows.length === 0) {
      return res.status(404).json({ error: 'Profile not found' });
    }

    const current = currentResult.rows[0];

    // Merge strategy: use provided values, fall back to current
    const mergedProfile = {
      tutorial_step: tutorialStep !== undefined ? tutorialStep : current.tutorial_step,
      unlocks_json: unlocksJson !== undefined ? unlocksJson : current.unlocks_json,
      run_stats_json: runStatsJson !== undefined ? runStatsJson : current.run_stats_json,
      achievements_json: achievementsJson !== undefined ? achievementsJson : current.achievements_json,
      coins: coins !== undefined ? coins : current.coins,
    };

    // Update database
    const updateResult = await pool.query(
      `UPDATE game_profiles
       SET tutorial_step = $1, unlocks_json = $2, run_stats_json = $3, achievements_json = $4, coins = $5, updated_at = NOW()
       WHERE user_id = $6
       RETURNING *`,
      [
        mergedProfile.tutorial_step,
        JSON.stringify(mergedProfile.unlocks_json),
        JSON.stringify(mergedProfile.run_stats_json),
        JSON.stringify(mergedProfile.achievements_json),
        mergedProfile.coins,
        req.userId,
      ]
    );

    const updated = updateResult.rows[0];
    res.status(200).json({
      userId: updated.user_id,
      tutorialStep: updated.tutorial_step,
      unlocksJson: updated.unlocks_json || {},
      runStatsJson: updated.run_stats_json || {},
      achievementsJson: updated.achievements_json || {},
      coins: updated.coins || 0,
      updatedAt: updated.updated_at,
    });
  } catch (err) {
    console.error('Error in POST /api/profile:', err);
    res.status(500).json({ error: 'Internal server error' });
  }
});

// ============================================================================
// ROUTES: Run History (Future - Phase 2)
// ============================================================================

/**
 * GET /api/runs
 * Fetch user's run history (optional detail tracking)
 *
 * Authorization: Bearer <token>
 */
app.get('/api/runs', requireAuth, async (req, res) => {
  res.status(501).json({ error: 'Run history not yet implemented (Phase 2)' });
});

// ============================================================================
// Error Handling & Server Start
// ============================================================================

app.use((err, req, res, next) => {
  console.error('Unhandled error:', err);
  res.status(500).json({ error: 'Internal server error' });
});

app.listen(PORT, () => {
  console.log(`Project Delta API running on http://localhost:${PORT}`);
  console.log(`Environment: ${process.env.NODE_ENV || 'development'}`);
  console.log(`CORS origin: ${CORS_ORIGIN}`);
});
