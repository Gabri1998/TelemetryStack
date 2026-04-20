
# TelemetryStack

A production-ready IoT telemetry platform built with microservices architecture for real-time device data ingestion, processing, and visualization.

## Architecture

TelemetryStack consists of six microservices working together:

- **API Gateway** (.NET 10 + YARP) - Reverse proxy and authentication gateway
- **Auth Service** (.NET 10 + JWT) - User registration and authentication
- **Device Service** (.NET 10) - Device management CRUD operations
- **Telemetry Service** (.NET 10 + MQTT + SignalR) - Real-time telemetry processing
- **PostgreSQL 15** - Persistent data storage
- **Redis 7** - Caching and message queuing
- **Mosquitto 2** - MQTT broker for device communication
- **React Dashboard** - Real-time telemetry visualization

## Technology Stack

**Backend:**
- .NET 10
- PostgreSQL 15
- Redis 7
- MQTT (Eclipse Mosquitto)
- SignalR for real-time WebSocket communication
- JWT for authentication
- BCrypt for password hashing

**Frontend:**
- React 18 with TypeScript
- Vite build tool
- Tailwind CSS
- Recharts for data visualization
- SignalR client for real-time updates

**DevOps:**
- Docker and Docker Compose
- GitHub Actions CI/CD
- GitHub Container Registry

## Prerequisites

- Docker 24+
- Docker Compose 2.20+
- .NET 10 SDK (for local development)
- Node.js 20+ (for frontend development)
- Git

## Quick Start

Clone the repository:

bash
git clone https://github.com/Gabri1998/TelemetryStack-CSharp.NET.git
cd TelemetryStack-CSharp.NET

Start all services with Docker Compose:

bash
docker compose up -d


Verify services are running:

bash
docker compose ps


Access the services:

- API Gateway: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Frontend Dashboard: http://localhost:5173 (after starting frontend)

## Running the Frontend

bash
cd frontend/web-dashboard
npm install
npm run dev


## Testing the System

Register a new user:

bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"123456"}'


Login to get a JWT token:

bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"123456"}'


Run the telemetry simulator:

bash
cd scripts/telemetry-simulator
dotnet run


## Service Ports

 Service >> Port 
-----------------
 API Gateway >> 5000 
 Telemetry Service >> 5001 
 Device Service >> 5002 
 Auth Service >> 5003 
 PostgreSQL >> 5432 
 Redis >> 6379 
 MQTT Broker >> 1883 

## Development

### Running Individual Services

API Gateway:

cd apps/api-gateway
dotnet run


Auth Service:

cd apps/auth-service
dotnet run


Device Service:

cd apps/device-service
dotnet run


Telemetry Service:

cd apps/telemetry-service
dotnet run

### Building All Services


dotnet restore TelemetryStack-C#.NET.sln
dotnet build TelemetryStack-C#.NET.sln --configuration Release


## Fedora Linux Setup

For Fedora users, run the automated setup script:


cd scripts
chmod +x fedora-setup.sh
sudo ./fedora-setup.sh


This script will:
- Install Docker, .NET SDK, and Node.js
- Configure firewall rules
- Set up SELinux for containers
- Create systemd service for auto-start
- Deploy all services

Manual firewall configuration:

sudo firewall-cmd --permanent --add-port=5000/tcp
sudo firewall-cmd --permanent --add-port=5001/tcp
sudo firewall-cmd --permanent --add-port=5002/tcp
sudo firewall-cmd --permanent --add-port=5003/tcp
sudo firewall-cmd --reload

## Systemd Service (Auto-start on boot)

Install the systemd service:

sudo tee /etc/systemd/system/telemetry-stack.service > /dev/null <<'EOF'
[Unit]
Description=Telemetry Stack Docker Compose
Requires=docker.service
After=docker.service network-online.target
Wants=network-online.target

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/home/gabri/TelemetryStack-CSharp.NET
ExecStartPre=/usr/bin/docker compose pull
ExecStart=/usr/bin/docker compose up -d
ExecStop=/usr/bin/docker compose down
ExecReload=/usr/bin/docker compose restart
User=gabri
Group=docker
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable telemetry-stack.service
sudo systemctl start telemetry-stack.service

## CI/CD Pipeline

GitHub Actions workflows:

- `backend-build.yml` - Builds and tests .NET services
- `frontend-build.yml` - Builds React frontend
- `docker-build.yml` - Builds and pushes Docker images to GHCR
- `test.yml` - Runs integration tests
- `fedora-build.yml` - Tests compatibility with Fedora

## Backup and Restore

Create a backup:

sudo mkdir -p /backups/telemetry
sudo chown $USER:$USER /backups/telemetry
./scripts/backup.sh

Restore PostgreSQL database:

docker exec -i telemetry-postgres psql -U admin telemetry < backup.sql

## Monitoring

View service logs:

docker compose logs -f
docker compose logs -f telemetry-service
docker compose logs -f api-gateway

Check container status:

docker compose ps
docker stats

## Troubleshooting

**Port conflicts:** Change host ports in docker-compose.yml

**Database connection issues:** Ensure postgres container is healthy
docker compose ps postgres

**MQTT connection failures:** Check MQTT broker logs
docker logs telemetry-mqtt

**JWT validation errors:** Verify JWT secret matches across all services

**Permission denied errors:** Add user to docker group
sudo usermod -aG docker $USER
# Log out and back in

## Project Structure

TelemetryStack-CSharp.NET/
├── apps/
│   ├── api-gateway/
│   ├── auth-service/
│   ├── device-service/
│   └── telemetry-service/
├── frontend/
│   └── web-dashboard/
├── shared/
│   └── contracts-dotnet/
├── infrastructure/
│   ├── database/
│   ├── mqtt/
│   ├── nginx/
│   └── redis/
├── scripts/
│   ├── telemetry-simulator/
│   └── *.sh
├── .github/workflows/
├── docs/
└── docker-compose.yml

## License

This project is for educational purposes.

## Support

For issues and questions, please open a GitHub issue.