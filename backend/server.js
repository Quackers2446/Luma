const express = require('express');
const cors = require('cors');
const path = require('path');
const db = require('./db');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(express.json());

// Serve static assets directly (fallback)
app.use('/public', express.static(path.join(__dirname, 'public')));
app.use('/assets', express.static(path.join(__dirname, 'public', 'assets')));

// Helper to construct fully qualified URLs dynamically
const getFullUrl = (req, assetPath) => {
  if (!assetPath) return '';
  if (assetPath.startsWith('http://') || assetPath.startsWith('https://')) {
    return assetPath;
  }
  const host = req.get('host');
  const protocol = req.protocol;
  return `${protocol}://${host}${assetPath}`;
};

// Route: List all characters
app.get('/characters', async (req, res) => {
  try {
    const result = await db.query('SELECT id, name, thumbnail_url FROM characters ORDER BY id ASC');
    const characters = result.rows.map(char => ({
      id: char.id,
      name: char.name,
      thumbnail_url: getFullUrl(req, char.thumbnail_url)
    }));
    res.json(characters);
  } catch (err) {
    console.error('Error fetching characters:', err.message);
    res.status(500).json({ error: 'Database error fetching characters' });
  }
});

// Route: Get character metadata & expressions
app.get('/characters/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const result = await db.query('SELECT * FROM characters WHERE id = $1', [id]);
    
    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Character not found' });
    }
    
    const char = result.rows[0];
    
    // Construct dynamic expression and layer URLs inside the metadata JSON
    const metadata = { ...char.metadata };
    if (metadata.expressions) {
      const expressions = {};
      for (const [key, val] of Object.entries(metadata.expressions)) {
        expressions[key] = getFullUrl(req, val);
      }
      metadata.expressions = expressions;
    }
    if (metadata.layers) {
      const layers = {};
      for (const [key, val] of Object.entries(metadata.layers)) {
        layers[key] = getFullUrl(req, val);
      }
      metadata.layers = layers;
    }
    
    res.json({
      id: char.id,
      name: char.name,
      thumbnail_url: getFullUrl(req, char.thumbnail_url),
      image_url: getFullUrl(req, char.image_url),
      metadata: metadata
    });
  } catch (err) {
    console.error('Error fetching character details:', err.message);
    res.status(500).json({ error: 'Database error fetching character details' });
  }
});

// Route: Download PNG assets (GET /assets/:id)
// Serves image files from backend/public/assets
app.get('/assets/:filename', (req, res) => {
  const filename = req.params.filename;
  // Prevent directory traversal attacks
  const safeFilename = path.basename(filename);
  const filepath = path.join(__dirname, 'public', 'assets', safeFilename);
  
  res.sendFile(filepath, (err) => {
    if (err) {
      console.warn(`Asset not found: ${safeFilename}`);
      res.status(404).send('Asset not found');
    }
  });
});

app.listen(PORT, '0.0.0.0', () => {
  console.log(`Cozy AR Companion Backend running on http://0.0.0.0:${PORT}`);
});
