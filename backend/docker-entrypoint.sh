#!/bin/sh
set -e

# IMPORTANT: The Render Persistent Disk is mounted at /var/data.
# The SQLite database must be written to /var/data so it survives
# container restarts and redeployments. Writing anywhere else (e.g.
# /app/data) places the database on the ephemeral container filesystem
# which is wiped on every restart -- silently losing all production data.
#
# The current Dockerfile uses `ENTRYPOINT ["dotnet", "MilkiDrugStore.Api.dll"]`
# directly and does NOT invoke this script. This script is kept for
# reference/backward compatibility and MUST always target /var/data.
DATA_DIR="/var/data"
mkdir -p "$DATA_DIR"
chmod 777 "$DATA_DIR"

if [ -f "$DATA_DIR/MilkiDrugStoreDB.db" ]; then
    chmod 666 "$DATA_DIR/MilkiDrugStoreDB.db" || true
fi

exec dotnet MilkiDrugStore.Api.dll
