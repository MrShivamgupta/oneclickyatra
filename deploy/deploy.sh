#!/usr/bin/env bash
# Pull latest code and redeploy. Run from repo root on the VM.
#   chmod +x deploy/deploy.sh && ./deploy/deploy.sh

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

echo "==> Pulling latest code..."
git pull --ff-only

echo "==> Rebuilding and restarting containers..."
cd docker
docker compose -f docker-compose.prod.yml up -d --build

echo "==> Running migrations (safe to re-run)..."
docker compose -f docker-compose.prod.yml run --rm migrate

echo ""
echo "Deploy complete. Site: http://${PUBLIC_DOMAIN:-$(grep PUBLIC_DOMAIN .env | cut -d= -f2)}"
