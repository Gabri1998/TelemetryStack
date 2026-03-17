CREATE TABLE devices (
    id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    serial_number TEXT UNIQUE,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE telemetry (
    id UUID PRIMARY KEY,
    device_id UUID REFERENCES devices(id),
    temperature FLOAT,
    speed FLOAT,
    battery FLOAT,
    timestamp TIMESTAMP
);