-- USERS (add this)
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY,
    email TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL
);

-- DEVICES (keep yours, it's good)
CREATE TABLE IF NOT EXISTS devices (
    id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    serial_number TEXT UNIQUE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- TELEMETRY (slight fix)
CREATE TABLE IF NOT EXISTS telemetry (
    id SERIAL PRIMARY KEY,
    device_id UUID REFERENCES devices(id),
    temperature DOUBLE PRECISION,
    speed DOUBLE PRECISION,
    battery DOUBLE PRECISION,
    timestamp TIMESTAMP
);