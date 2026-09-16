# One Click Yatra — Free Public Hosting (Oracle Cloud)

Deploy the full stack (Angular + API + SQL Server + Redis) on an **Oracle Cloud Always Free** VM using Docker.

**Total cost: ₹0** (Oracle Always Free tier)

---

## Architecture

```
Internet → VM port 80 (nginx/web)
              ├── /          → Angular SPA
              └── /api/v1/*  → ASP.NET API (internal port 8080)
                                    ├── SQL Server (Docker)
                                    └── Redis (Docker)
```

Frontend uses same-origin API (`/api/v1`) — no CORS issues.

---

## Prerequisites

| Item | Details |
|------|---------|
| Oracle Cloud account | [cloud.oracle.com](https://cloud.oracle.com) — Always Free |
| GitHub account | Code host karna hoga |
| VM size | **Minimum 4 GB RAM** (SQL Server + API + web). Ampere A1: 2 OCPU + 4 GB recommended |
| Domain (optional) | Free DNS via Cloudflare |

---

## Part 1 — GitHub par code push

Windows (PowerShell):

```powershell
cd D:\Shivam\Shivam\OneClickYatra
git init
git add .
git commit -m "Add production deploy setup"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/oneclickyatra.git
git push -u origin main
```

> `.env` files gitignore mein hain — secrets commit nahi honge.

---

## Part 2 — Oracle Cloud VM banao

1. **Oracle Cloud Console** → Compute → Instances → **Create Instance**
2. **Name:** `oneclickyatra`
3. **Image:** Ubuntu 22.04 or 24.04 (aarch64 / Ampere OK)
4. **Shape:** Ampere A1 — **2 OCPU, 4 GB RAM** (free tier ke andar)
5. **Networking:** Public IP assign karo
6. **SSH key:** Apna public key add karo (download private key)
7. **Boot volume:** 50 GB (free tier)
8. Create

### Security List (Firewall)

VCN → Security List → **Ingress Rules** add karo:

| Source | Protocol | Port |
|--------|----------|------|
| `0.0.0.0/0` | TCP | 22 (SSH) |
| `0.0.0.0/0` | TCP | 80 (HTTP) |
| `0.0.0.0/0` | TCP | 443 (HTTPS, optional) |

---

## Part 3 — VM par setup

SSH se connect karo:

```bash
ssh -i your-key.pem ubuntu@YOUR_VM_PUBLIC_IP
```

### 3.1 Docker install

```bash
git clone https://github.com/YOUR_USERNAME/oneclickyatra.git
cd oneclickyatra
chmod +x deploy/setup-vm.sh deploy/deploy.sh deploy/seed-demo.sh
./deploy/setup-vm.sh
```

Log out + log back in (Docker group ke liye):

```bash
exit
ssh -i your-key.pem ubuntu@YOUR_VM_PUBLIC_IP
```

### 3.2 Environment file

```bash
cd oneclickyatra/docker
cp .env.example .env
nano .env
```

`.env` mein ye values set karo:

```env
SQL_SA_PASSWORD=YourStrongSqlPassword1!
REDIS_PASSWORD=YourStrongRedisPassword2!
JWT_KEY=paste_a_long_random_64_char_string_here
PUBLIC_DOMAIN=YOUR_VM_PUBLIC_IP
```

JWT key generate karo:

```bash
openssl rand -base64 64
```

> `PUBLIC_DOMAIN` = VM ka public IP ya domain name (bina `http://`).

### 3.3 Deploy

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

Pehli baar **5–10 minute** lag sakte hain (SQL Server + Angular build).

Status check:

```bash
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f api
```

### 3.4 Demo data + admin user (optional)

```bash
cd ~/oneclickyatra
./deploy/seed-demo.sh
```

| Field | Value |
|-------|-------|
| Email | `superadmin@oneclickyatra.dev` |
| Password | `Admin@12345` |

**Public server par turant password change karo.**

---

## Part 4 — Site open karo

Browser mein:

```
http://YOUR_VM_PUBLIC_IP
```

- Public site: `/`
- Admin login: `/login`
- Admin panel: `/admin/dashboard`

---

## Part 5 — Custom domain + free HTTPS (Cloudflare)

1. [Cloudflare](https://dash.cloudflare.com) par free account
2. Apna domain add karo
3. DNS → **A record** → `YOUR_VM_PUBLIC_IP`
4. SSL/TLS → **Flexible** (ya Full with origin cert)
5. `.env` mein `PUBLIC_DOMAIN=yourdomain.com` update karo
6. Redeploy:

```bash
cd ~/oneclickyatra
./deploy/deploy.sh
```

---

## Updates (naya code deploy)

VM par:

```bash
cd ~/oneclickyatra
./deploy/deploy.sh
```

---

## Troubleshooting

### Site nahi khul rahi

```bash
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs web
docker compose -f docker-compose.prod.yml logs api
```

### SQL Server slow start (ARM VM)

Oracle Ampere par SQL Server **amd64 emulation** se chalta hai — pehli baar 2–3 minute wait karo:

```bash
docker compose -f docker-compose.prod.yml logs sqlserver
```

### Migrations dubara chalana

```bash
cd docker
docker compose -f docker-compose.prod.yml run --rm migrate
```

### RAM kam hai

VM ko **4 GB+ RAM** do. 1 GB par SQL Server fail ho jayega.

---

## Security checklist (public server)

- [ ] Default admin password change
- [ ] Strong `JWT_KEY` aur SQL/Redis passwords
- [ ] `.env` kabhi git mein commit mat karo
- [ ] Oracle Security List mein sirf 22, 80, 443 open
- [ ] Cloudflare HTTPS enable (recommended)
- [ ] Hangfire dashboard production mein disabled (default)

---

## File reference

| File | Purpose |
|------|---------|
| `docker/docker-compose.prod.yml` | Full production stack |
| `docker/.env.example` | Secrets template |
| `docker/Dockerfile.web` | Angular build + nginx |
| `docker/nginx.conf` | SPA + API reverse proxy |
| `deploy/setup-vm.sh` | One-time VM setup |
| `deploy/deploy.sh` | Pull + rebuild + migrate |
| `deploy/seed-demo.sh` | Demo data + admin user |
