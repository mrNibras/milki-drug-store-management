#!/bin/sh
set -e

# The Render Persistent Disk is mounted at /var/data.
# Data Protection keys and file uploads are stored under this path
# so they survive container restarts and redeployments.
#
# The current Dockerfile uses `ENTRYPOINT ["dotnet", "MilkiDrugStore.Api.dll"]`
# directly and does NOT invoke this script. This script is kept for
# reference/backward compatibility.
DATA_DIR="/var/data"
mkdir -p "$DATA_DIR"
chmod 777 "$DATA_DIR"

exec dotnet MilkiDrugStore.Api.dll
