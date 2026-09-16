#!/usr/bin/env bash
# One-time setup on a fresh Ubuntu 22.04/24.04 VM (Oracle Cloud Always Free).
# Run as a normal user with sudo:
#   chmod +x deploy/setup-vm.sh && ./deploy/setup-vm.sh

set -euo pipefail

echo "==> Installing Docker..."
sudo apt-get update -y
sudo apt-get install -y ca-certificates curl git ufw
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "${VERSION_CODENAME}") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update -y
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
sudo usermod -aG docker "$USER" || true

echo "==> Opening firewall ports 80 and 443..."
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
echo "y" | sudo ufw enable || true

echo ""
echo "Done. Log out and back in (or run: newgrp docker) so Docker group applies."
echo "Next steps:"
echo "  1. git clone <your-repo-url> && cd OneClickYatra/docker"
echo "  2. cp .env.example .env   # edit passwords + PUBLIC_DOMAIN"
echo "  3. docker compose -f docker-compose.prod.yml up -d --build"
echo "  4. docker compose -f docker-compose.prod.yml run --rm migrate   # if migrate service already finished"
echo "  5. ./deploy/seed-demo.sh   # optional demo data + admin user"
