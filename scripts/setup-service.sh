#!/bin/bash
# setup-service.sh - Create and configure the systemd service

echo "Setting up TelemetryStack systemd service..."

# Create the service file
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

# Set proper permissions
sudo chmod 644 /etc/systemd/system/telemetry-stack.service

# Reload systemd
sudo systemctl daemon-reload

# Enable the service
sudo systemctl enable telemetry-stack.service

# Start the service
sudo systemctl start telemetry-stack.service

# Show status
sudo systemctl status telemetry-stack.service --no-pager

echo ""
echo "Service created successfully!"
echo "Commands to manage the service:"
echo "  sudo systemctl status telemetry-stack.service   # Check status"
echo "  sudo systemctl restart telemetry-stack.service  # Restart"
echo "  sudo systemctl stop telemetry-stack.service     # Stop"
echo "  sudo systemctl start telemetry-stack.service    # Start"
echo "  sudo journalctl -u telemetry-stack.service -f   # View logs"