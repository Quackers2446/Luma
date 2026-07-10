const { Pool } = require('pg');
require('dotenv').config();

const pool = new Pool({
  connectionString: process.env.DATABASE_URL || 'postgresql://quackers@localhost:5432/cozy_ar_db'
});

module.exports = {
  query: (text, params) => pool.query(text, params),
  pool
};
