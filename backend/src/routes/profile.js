const express = require('express');
const db = require('../db');
const auth = require('../middleware/auth');

const router = express.Router();

router.get('/', auth, async (req, res) => {
  try {
    const result = await db.query(
      'SELECT profile_json, updated_at FROM profiles WHERE user_id = $1',
      [req.user.userId]
    );

    if (result.rows.length === 0) {
      return res.status(200).json({ profile: null });
    }

    const row = result.rows[0];
    return res.status(200).json({
      profile: row.profile_json,
      serverUpdatedUtc: row.updated_at ? Math.floor(new Date(row.updated_at).getTime() / 1000) : 0
    });
  } catch (err) {
    return res.status(500).json({ error: 'Failed to load profile.' });
  }
});

router.put('/', auth, async (req, res) => {
  try {
    const profile = req.body.profile || {};

    await db.query(
      `INSERT INTO profiles (user_id, profile_json, updated_at)
       VALUES ($1, $2, NOW())
       ON CONFLICT (user_id)
       DO UPDATE SET profile_json = EXCLUDED.profile_json, updated_at = NOW()`,
      [req.user.userId, JSON.stringify(profile)]
    );

    return res.status(200).json({ ok: true });
  } catch (err) {
    return res.status(500).json({ error: 'Failed to save profile.' });
  }
});

module.exports = router;
