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
        thumbnail_url: '/assets/sprout/head.png',
        image_url: '/assets/sprout/body.png',
        metadata: {
          description: 'A cozy little leaf forest spirit who loves to sit on the grass and wave to passersby.',
          config: {
            idleBobAmplitude: 0.02,
            idleBobSpeed: 1.1,
            breathingScale: 0.015,
            blinkIntervalMin: 2.5,
            blinkIntervalMax: 6.0,
            headRotationRange: 4.0,
            hairSpring: 0.3,
            hairDamping: 0.8
          },
          layers: {
            body: '/assets/sprout/body.png',
            head: '/assets/sprout/head.png',
            left_arm: '/assets/sprout/left_arm.png',
            right_arm: '/assets/sprout/right_arm.png',
            hair: '/assets/sprout/hair.png',
            eyes_open: '/assets/sprout/eyes_open.png',
            eyes_closed: '/assets/sprout/eyes_closed.png',
            mouth: '/assets/sprout/mouth.png',
            mouth_happy: '/assets/sprout/mouth_happy.png',
            mouth_sad: '/assets/sprout/mouth_sad.png',
            shadow: '/assets/sprout/shadow.png'
          }
        }
      },
      {
        name: 'Nimbus',
        thumbnail_url: '/assets/nimbus/head.png',
        image_url: '/assets/nimbus/body.png',
        metadata: {
          description: 'A fluffy cloud puppy that floats peacefully in the air, bringing gentle breezes.',
          config: {
            idleBobAmplitude: 0.04,
            idleBobSpeed: 0.8,
            breathingScale: 0.02,
            blinkIntervalMin: 3.0,
            blinkIntervalMax: 7.0,
            headRotationRange: 3.0,
            hairSpring: 0.25,
            hairDamping: 0.85
          },
          layers: {
            body: '/assets/nimbus/body.png',
            head: '/assets/nimbus/head.png',
            left_arm: '/assets/nimbus/left_arm.png',
            right_arm: '/assets/nimbus/right_arm.png',
            hair: '/assets/nimbus/hair.png',
            eyes_open: '/assets/nimbus/eyes_open.png',
            eyes_closed: '/assets/nimbus/eyes_closed.png',
            mouth: '/assets/nimbus/mouth.png',
            mouth_happy: '/assets/nimbus/mouth_happy.png',
            mouth_sad: '/assets/nimbus/mouth_sad.png',
            shadow: '/assets/nimbus/shadow.png'
          }
        }
      },
      {
        name: 'Mocha',
        thumbnail_url: '/assets/mocha/head.png',
        image_url: '/assets/mocha/body.png',
        metadata: {
          description: 'A round, sleepy bear holding a tiny teacup, looking for a warm spot to rest.',
          config: {
            idleBobAmplitude: 0.015,
            idleBobSpeed: 0.6,
            breathingScale: 0.012,
            blinkIntervalMin: 4.0,
            blinkIntervalMax: 10.0,
            headRotationRange: 2.0,
            hairSpring: 0.4,
            hairDamping: 0.75
          },
          layers: {
            body: '/assets/mocha/body.png',
            head: '/assets/mocha/head.png',
            left_arm: '/assets/mocha/left_arm.png',
            right_arm: '/assets/mocha/right_arm.png',
            hair: '/assets/mocha/hair.png',
            eyes_open: '/assets/mocha/eyes_open.png',
            eyes_closed: '/assets/mocha/eyes_closed.png',
            mouth: '/assets/mocha/mouth.png',
            mouth_happy: '/assets/mocha/mouth_happy.png',
            mouth_sad: '/assets/mocha/mouth_sad.png',
            shadow: '/assets/mocha/shadow.png',
            accessory: '/assets/mocha/accessory.png'
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
