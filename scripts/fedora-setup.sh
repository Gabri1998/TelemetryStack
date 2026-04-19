#!/bin/bash
# fedora-setup.sh - Complete setup for TelemetryStack on Fedora

set -e

echo " TelemetryStack Complete Setup for Fedora"
echo "=========================================="

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Function to print status
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Check Fedora version
if ! grep -q "Fedora" /etc/fedora-release; then
    print_error "This script is for Fedora only!"
    exit 1
fi

print_status "Fedora version: $(cat /etc/fedora-release)"

# Update system
print_status "Updating system packages..."
sudo dnf update -y

# Install dependencies
print_status "Installing dependencies..."

# Docker
if ! command -v docker &> /dev/null; then
    print_status "Installing Docker..."
    sudo dnf install -y dnf-plugins-core
    sudo dnf config-manager --add-repo https://download.docker.com/linux/fedora/docker-ce.repo
    sudo dnf install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
    sudo systemctl enable --now docker
    sudo usermod -aG docker $USER
    print_success "Docker installed"
else
    print_success "Docker already installed"
fi

# .NET SDK
if ! command -v dotnet &> /dev/null; then
    print_status "Installing .NET SDK 10.0..."
    sudo dnf install -y dotnet-sdk-10.0
    print_success ".NET SDK installed"
else
    print_success ".NET SDK already installed"
fi

# Node.js
if ! command -v node &> /dev/null; then
    print_status "Installing Node.js 20..."
    curl -fsSL https://rpm.nodesource.com/setup_20.x | sudo bash -
    sudo dnf install -y nodejs
    print_success "Node.js installed"
else
    print_success "Node.js already installed"
fi

# Git
if ! command -v git &> /dev/null; then
    print_status "Installing Git..."
    sudo dnf install -y git
    print_success "Git installed"
fi

# curl and wget
sudo dnf install -y curl wget vim htop

# Podman (optional)
print_status "Installing Podman (optional)..."
sudo dnf install -y podman podman-compose

# Configure firewall
print_status "Configuring firewall..."
sudo firewall-cmd --permanent --add-port=5000/tcp
sudo firewall-cmd --permanent --add-port=5001/tcp
sudo firewall-cmd --permanent --add-port=5002/tcp
sudo firewall-cmd --permanent --add-port=5003/tcp
sudo firewall-cmd --permanent --add-port=5432/tcp
sudo firewall-cmd --permanent --add-port=6379/tcp
sudo firewall-cmd --permanent --add-port=1883/tcp
sudo firewall-cmd --permanent --add-port=5173/tcp
sudo firewall-cmd --reload
print_success "Firewall configured"

# Configure SELinux
if command -v getenforce &> /dev/null; then
    if [ "$(getenforce)" != "Disabled" ]; then
        print_status "Configuring SELinux for containers..."
        sudo setsebool -P container_manage_cgroup on
        sudo setsebool -P httpd_can_network_connect on
        print_success "SELinux configured"
    fi
fi

# Create project directory
PROJECT_DIR="/opt/telemetry-stack"
if [ ! -d "$PROJECT_DIR" ]; then
    print_status "Creating project directory at $PROJECT_DIR"
    sudo mkdir -p $PROJECT_DIR
    sudo chown $USER:$USER $PROJECT_DIR
fi

# Clone or update repository
if [ ! -d "$PROJECT_DIR/.git" ]; then
    print_status "Cloning repository..."
    git clone https://github.com/yourusername/TelemetryStack-CSharp.NET.git $PROJECT_DIR
else
    print_status "Updating repository..."
    cd $PROJECT_DIR && git pull
fi

# Create data directories
print_status "Creating data directories..."
cd $PROJECT_DIR
mkdir -p data/postgres data/redis data/mosquitto logs
chmod 755 data/postgres data/redis data/mosquitto

# Build and start services
print_status "Building Docker images..."
docker compose build --parallel

print_status "Starting services..."
docker compose up -d

# Wait for services
print_status "Waiting for services to be ready..."
sleep 15

# Check service health
print_status "Checking service status..."
docker compose ps

# Setup frontend
print_status "Setting up frontend..."
cd $PROJECT_DIR/frontend/web-dashboard
npm install
npm run build

# Create systemd service
print_status "Creating systemd service..."
sudo tee /etc/systemd/system/telemetry-stack.service > /dev/null <<EOF
[Unit]
Description=Telemetry Stack Docker Compose
Requires=docker.service
After=docker.service network-online.target
Wants=network-online.target

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=$PROJECT_DIR
ExecStartPre=/usr/bin/docker compose pull
ExecStart=/usr/bin/docker compose up -d
ExecStop=/usr/bin/docker compose down
ExecReload=/usr/bin/docker compose restart
User=$USER
Group=docker
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF

# Enable systemd service
sudo systemctl daemon-reload
sudo systemctl enable telemetry-stack.service

# Create backup script
print_status "Creating backup script..."
cat > $PROJECT_DIR/scripts/backup.sh <<'EOF'
#!/bin/bash
BACKUP_DIR="/backups/telemetry"
DATE=$(date +%Y%m%d_%H%M%S)
mkdir -p $BACKUP_DIR

# Backup PostgreSQL
docker exec telemetry-postgres pg_dump -U admin telemetry > "$BACKUP_DIR/postgres_$DATE.sql"

# Backup Redis
docker exec telemetry-redis redis-cli SAVE
docker cp telemetry-redis:/data/dump.rdb "$BACKUP_DIR/redis_$DATE.rdb"

# Keep last 7 days
find "$BACKUP_DIR" -type f -mtime +7 -delete

echo "Backup completed at $DATE" >> "$BACKUP_DIR/backup.log"
EOF

chmod +x $PROJECT_DIR/scripts/backup.sh

# Create monitoring script
print_status "Creating monitoring script..."
cat > $PROJECT_DIR/scripts/monitor.sh <<'EOF'
#!/bin/bash
echo "=== TelemetryStack Health Check ==="
echo "Time: $(date)"
echo ""

echo "--- Container Status ---"
docker compose ps

echo ""
echo "--- Resource Usage ---"
docker stats --no-stream

echo ""
echo "--- Recent Errors ---"
docker compose logs --tail=20 2>&1 | grep -i "error\|fail" || echo "No recent errors"
EOF

chmod +x $PROJECT_DIR/scripts/monitor.sh

# Create update script
print_status "Creating update script..."
cat > $PROJECT_DIR/scripts/update.sh <<'EOF'
#!/bin/bash
echo "Updating TelemetryStack..."
cd /opt/telemetry-stack

# Pull latest code
git pull

# Rebuild and restart
docker compose build --no-cache
docker compose up -d

# Clean up old images
docker image prune -f

echo "Update completed!"
EOF

chmod +x $PROJECT_DIR/scripts/update.sh

print_success "All scripts created"

# Display completion message
echo ""
echo "=========================================="
print_success "TelemetryStack Setup Complete!"
echo "=========================================="
echo ""
echo -e "${GREEN} Access your application:${NC}"
echo -e "   Frontend:     ${YELLOW}http://localhost:5173${NC}"
echo -e "   API Gateway:  ${YELLOW}http://localhost:5000${NC}"
echo -e "   Swagger UI:   ${YELLOW}http://localhost:5000/swagger${NC}"
echo -e "   MQTT Broker:  ${YELLOW}mqtt://localhost:1883${NC}"
echo ""
echo -e "${GREEN} Useful commands:${NC}"
echo -e "   View logs:     ${YELLOW}docker compose logs -f${NC}"
echo -e "   Stop services: ${YELLOW}docker compose down${NC}"
echo -e "   Restart:       ${YELLOW}sudo systemctl restart telemetry-stack${NC}"
echo -e "   Monitor:       ${YELLOW}./scripts/monitor.sh${NC}"
echo -e "   Backup:        ${YELLOW}./scripts/backup.sh${NC}"
echo -e "   Update:        ${YELLOW}./scripts/update.sh${NC}"
echo ""
echo -e "${YELLOW} IMPORTANT: Log out and back in for Docker group changes to take effect!${NC}"
echo ""

# Ask for reboot
read -p "Reboot now? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    sudo reboot
fi