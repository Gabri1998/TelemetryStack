#!/bin/bash
# fedora-firewall.sh - Configure firewall for TelemetryStack

set -e

echo " Configuring Firewall for TelemetryStack"

# Open required ports
sudo firewall-cmd --permanent --add-port=5000/tcp  # API Gateway
sudo firewall-cmd --permanent --add-port=5001/tcp  # Telemetry Service
sudo firewall-cmd --permanent --add-port=5002/tcp  # Device Service
sudo firewall-cmd --permanent --add-port=5003/tcp  # Auth Service
sudo firewall-cmd --permanent --add-port=5432/tcp  # PostgreSQL (optional)
sudo firewall-cmd --permanent --add-port=6379/tcp  # Redis (optional)
sudo firewall-cmd --permanent --add-port=1883/tcp  # MQTT
sudo firewall-cmd --permanent --add-port=5173/tcp  # Frontend dev server

# Reload firewall
sudo firewall-cmd --reload

echo " Firewall configured successfully"
sudo firewall-cmd --list-ports