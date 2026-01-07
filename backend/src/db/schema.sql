-- Project Delta - Supabase PostgreSQL Schema
-- Run this in Supabase SQL Editor to set up the database

-- ============================================================================
-- TABLES
-- ============================================================================

-- Users table (anonymous + email support)
CREATE TABLE IF NOT EXISTS users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email VARCHAR(255) UNIQUE,
  device_fingerprint VARCHAR(255) NOT NULL UNIQUE,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- Game profiles (player progression, unlocks, achievements)
CREATE TABLE IF NOT EXISTS game_profiles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
  tutorial_step INTEGER DEFAULT 0,
  unlocks_json JSONB DEFAULT '{}',
  run_stats_json JSONB DEFAULT '{}',
  achievements_json JSONB DEFAULT '{}',
  coins INTEGER DEFAULT 0,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- Run history (for analytics, optional detail tracking)
CREATE TABLE IF NOT EXISTS run_history (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  level_id VARCHAR(255),
  started_at TIMESTAMP DEFAULT NOW(),
  ended_at TIMESTAMP,
  status VARCHAR(50), -- 'won', 'lost', 'quit'
  score INTEGER,
  created_at TIMESTAMP DEFAULT NOW()
);

-- ============================================================================
-- INDEXES (for query performance)
-- ============================================================================

CREATE INDEX idx_users_device_fingerprint ON users(device_fingerprint);
CREATE INDEX idx_game_profiles_user_id ON game_profiles(user_id);
CREATE INDEX idx_run_history_user_id ON run_history(user_id);
CREATE INDEX idx_run_history_started_at ON run_history(started_at DESC);

-- ============================================================================
-- ROW LEVEL SECURITY (optional for MVP, can be enabled later)
-- ============================================================================

-- Uncomment below to enable RLS (requires auth policies)

-- ALTER TABLE users ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE game_profiles ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE run_history ENABLE ROW LEVEL SECURITY;

-- Users can only view/edit their own profile
-- CREATE POLICY "Users can view own profile" ON game_profiles
--   FOR SELECT USING (auth.uid() = user_id);
--
-- CREATE POLICY "Users can update own profile" ON game_profiles
--   FOR UPDATE USING (auth.uid() = user_id);
--
-- CREATE POLICY "Users can view own run history" ON run_history
--   FOR SELECT USING (auth.uid() = user_id);

-- ============================================================================
-- NOTES
-- ============================================================================

-- Phase 1: Basic CRUD operations, no RLS
-- Phase 2: Add email/password auth with Supabase Auth service
-- Phase 3: Implement RLS policies for multi-tenant security
--
-- JSON columns (unlocks_json, run_stats_json, achievements_json) allow
-- flexible schema evolution without migrations. Structure defined by client.
