const { Client } = require('pg');
require('dotenv').config();

const dbName = 'cozy_ar_db';
const defaultDbUrl = process.env.DATABASE_URL_SYSTEM || 'postgresql://quackers@localhost:5432/postgres';
const targetDbUrl = process.env.DATABASE_URL || 'postgresql://quackers@localhost:5432/cozy_ar_db';

async function init() {
  console.log('Connecting to postgres database to check target database presence...');
  const client = new Client({ connectionString: defaultDbUrl });
  
  try {
    await client.connect();
    
    // Check if database exists
    const res = await client.query(`SELECT 1 FROM pg_database WHERE datname = $1`, [dbName]);
    if (res.rowCount === 0) {
      console.log(`Database "${dbName}" not found. Creating...`);
      // CREATE DATABASE cannot run inside a transaction block
      await client.query(`CREATE DATABASE ${dbName}`);
      console.log(`Database "${dbName}" created successfully.`);
    } else {
      console.log(`Database "${dbName}" already exists.`);
    }
  } catch (err) {
    console.error('Error during database check/creation:', err.message);
    process.exit(1);
  } finally {
    await client.end();
  }

  console.log(`Connecting directly to "${dbName}" to set up schema and seed default data...`);
  const targetClient = new Client({ connectionString: targetDbUrl });
  
  try {
    await targetClient.connect();
    
    // Create schema
    console.log('Creating characters table...');
    await targetClient.query(`
      CREATE TABLE IF NOT EXISTS characters (
        id SERIAL PRIMARY KEY,
        name VARCHAR(255) NOT NULL,
        thumbnail_url VARCHAR(512) NOT NULL,
        image_url VARCHAR(512) NOT NULL,
        metadata JSONB NOT NULL
      )
    `);
    
    // Clear existing characters to make it idempotent
    console.log('Clearing old characters data...');
    await targetClient.query('TRUNCATE TABLE characters RESTART IDENTITY CASCADE');

    // Seed default characters
    const seedData = [
      {
        name: 'Sprout',
        thumbnail_url: '/assets/sprout_thumb.png',
        image_url: '/assets/sprout_idle.png',
        metadata: {
          description: 'A cozy little leaf forest spirit who loves to sit on the grass and wave to passersby.',
          expressions: {
            idle: '/assets/sprout_idle.png',
            happy: '/assets/sprout_happy.png',
            sad: '/assets/sprout_sad.png',
            wave: '/assets/sprout_wave.png'
          }
        }
      },
      {
        name: 'Nimbus',
        thumbnail_url: '/assets/nimbus_thumb.png',
        image_url: '/assets/nimbus_idle.png',
        metadata: {
          description: 'A fluffy cloud puppy that floats peacefully in the air, bringing gentle breezes.',
          expressions: {
            idle: '/assets/nimbus_idle.png',
            happy: '/assets/nimbus_happy.png',
            sad: '/assets/nimbus_sad.png',
            wave: '/assets/nimbus_wave.png'
          }
        }
      },
      {
        name: 'Mocha',
        thumbnail_url: '/assets/mocha_thumb.png',
        image_url: '/assets/mocha_idle.png',
        metadata: {
          description: 'A round, sleepy bear holding a tiny teacup, looking for a warm spot to rest.',
          expressions: {
            idle: '/assets/mocha_idle.png',
            happy: '/assets/mocha_happy.png',
            sad: '/assets/mocha_sad.png',
            wave: '/assets/mocha_wave.png'
          }
        }
      }
    ];

    console.log('Seeding characters...');
    for (const char of seedData) {
      await targetClient.query(
        `INSERT INTO characters (name, thumbnail_url, image_url, metadata) VALUES ($1, $2, $3, $4)`,
        [char.name, char.thumbnail_url, char.image_url, JSON.stringify(char.metadata)]
      );
      console.log(`Seeded character: ${char.name}`);
    }
    
    console.log('Database initialization and seeding completed successfully!');
  } catch (err) {
    console.error('Error during database schema definition/seeding:', err.message);
    process.exit(1);
  } finally {
    await targetClient.end();
  }
}

init();
