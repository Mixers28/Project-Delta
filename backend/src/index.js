require('dotenv').config();
const express = require('express');
const cors = require('cors');

const authRoutes = require('./routes/auth');
const profileRoutes = require('./routes/profile');

const app = express();

app.use(express.json({ limit: '2mb' }));
app.use(express.urlencoded({ extended: true }));

const rawOrigins = (process.env.CORS_ORIGIN || '*')
  .split(',')
  .map((value) => value.trim())
  .filter(Boolean);

const normalizeOrigin = (value) => {
  if (!value) return value;
  return value.endsWith('/') ? value.slice(0, -1) : value;
};

const corsOptions = {
  origin(origin, callback) {
    if (!origin || rawOrigins.includes('*')) {
      return callback(null, true);
    }

    const normalized = normalizeOrigin(origin);
    const allowed = rawOrigins.some((entry) => normalizeOrigin(entry) === normalized);
    return callback(null, allowed);
  },
  credentials: true
};

app.use(cors(corsOptions));

app.get('/health', (req, res) => {
  res.status(200).json({ ok: true });
});

app.use('/auth', authRoutes);
app.use('/profile', profileRoutes);

app.use((req, res) => {
  res.status(404).json({ error: 'Not Found' });
});

// eslint-disable-next-line no-unused-vars
app.use((err, req, res, next) => {
  res.status(500).json({ error: 'Server error' });
});

const port = process.env.PORT || 3000;
app.listen(port, () => {
  console.log(`Server listening on ${port}`);
});
