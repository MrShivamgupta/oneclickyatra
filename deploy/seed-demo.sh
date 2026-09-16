#!/usr/bin/env bash
# Seeds demo data + dev SuperAdmin (superadmin@oneclickyatra.dev / Admin@12345).
# WARNING: Only run on a fresh demo/staging server — change the password immediately after.
# Run from repo root:
#   chmod +x deploy/seed-demo.sh && ./deploy/seed-demo.sh

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT/docker"

echo "WARNING: This creates a known default admin password. Use only for demo."
read -r -p "Continue? [y/N] " confirm
if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
  echo "Cancelled."
  exit 0
fi

docker compose -f docker-compose.prod.yml run --rm migrate seed
echo "Seeded. Login: superadmin@oneclickyatra.dev / Admin@12345 — CHANGE THIS NOW."
