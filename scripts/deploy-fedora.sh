# (paste the entire script above)
#!/bin/bash
# deploy-fedora.sh - Deployment script for Fedora

set -e

echo "TelemetryStack Deployment for Fedora"
echo "========================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Get the project root directory (where docker-compose.yml is)
PROJECT_ROOT="/home/gabri/TelemetryStack-CSharp.NET"

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
    echo -e "${YELLOW}Running as root. Consider using a regular user with docker permissions.${NC}"
fi

# Check Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Docker not found. Installing...${NC}"
    sudo dnf install -y docker-ce docker-ce-cli containerd.io
    sudo systemctl enable --now docker
    sudo usermod -aG docker $USER
    echo -e "${GREEN}Docker installed. Please log out and back in.${NC}"
    exit 1
fi

# Check Docker Compose
if ! command -v docker compose &> /dev/null; then
    echo -e "${YELLOW}Docker Compose not found. Installing...${NC}"
    sudo dnf install -y docker-compose-plugin
fi

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${YELLOW}.NET SDK not found. Installing...${NC}"
    sudo dnf install -y dotnet-sdk-10.0
fi

# Check Node.js
if ! command -v node &> /dev/null; then
    echo -e "${YELLOW}Node.js not found. Installing...${NC}"
    curl -fsSL https://rpm.nodesource.com/setup_20.x | sudo bash -
    sudo dnf install -y nodejs
fi

echo -e "${GREEN}All dependencies satisfied${NC}"

# Go to project root
cd $PROJECT_ROOT

# Create necessary directories
mkdir -p logs data/postgres data/redis data/mosquitto

# Set permissions for Fedora SELinux
if command -v getenforce &> /dev/null && [ "$(getenforce)" != "Disabled" ]; then
    echo -e "${YELLOW}Configuring SELinux...${NC}"
    sudo setsebool -P container_manage_cgroup on
    sudo chcon -Rt svirt_sandbox_file_t data/
fi

# Build and start services
echo -e "${GREEN}Building services...${NC}"
docker compose build --parallel

echo -e "${GREEN}Starting services...${NC}"
docker compose up -d

# Wait for services to be ready
echo -e "${YELLOW}Waiting for services to be ready...${NC}"
sleep 15

# Check service health
echo -e "${GREEN}Service Status:${NC}"
docker compose ps

# Check logs for errors
echo -e "${GREEN}Recent logs:${NC}"
docker compose logs --tail=20

# Frontend setup
echo -e "${GREEN}Setting up frontend...${NC}"
cd $PROJECT_ROOT/frontend/web-dashboard
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}Installing frontend dependencies...${NC}"
    npm install
fi

# Build frontend
echo -e "${GREEN}Building frontend...${NC}"
npm run build

echo -e "${GREEN}Deployment complete!${NC}"
echo ""
echo -e "${GREEN}Access your application:${NC}"
echo -e "   Frontend: ${YELLOW}http://localhost:5173${NC}"
echo -e "   API Gateway: ${YELLOW}http://localhost:5000${NC}"
echo -e "   Swagger UI: ${YELLOW}http://localhost:5000/swagger${NC}"
echo ""
echo -e "${YELLOW}Useful commands:${NC}"
echo -e "   View logs: ${YELLOW}docker compose logs -f${NC}"
echo -e "   Stop services: ${YELLOW}docker compose down${NC}"
echo -e "   Restart services: ${YELLOW}docker compose restart${NC}"