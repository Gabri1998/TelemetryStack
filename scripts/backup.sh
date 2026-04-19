#!/bin/bash
BACKUP_DIR="/backups/telemetry"
DATE=$(date +%Y%m%d_%H%M%S)

# Backup PostgreSQL
docker exec telemetry-postgres pg_dump -U admin telemetry > "$BACKUP_DIR/postgres_$DATE.sql"

# Backup Redis (optional)
docker exec telemetry-redis redis-cli SAVE
docker cp telemetry-redis:/data/dump.rdb "$BACKUP_DIR/redis_$DATE.rdb"

# Keep only last 7 days
find "$BACKUP_DIR" -type f -mtime +7 -delete