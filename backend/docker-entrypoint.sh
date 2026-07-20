#!/bin/sh
set -e

DATA_DIR="/app/data"
mkdir -p "$DATA_DIR"
chmod 777 "$DATA_DIR"

if [ -f "$DATA_DIR/MilkiDrugStoreDB.db" ]; then
    chmod 666 "$DATA_DIR/MilkiDrugStoreDB.db" || true
fi

exec dotnet MilkiDrugStore.Api.dll
