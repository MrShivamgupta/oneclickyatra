# One Click Yatra — AI Travel CRM

Enterprise travel CRM platform. Stack: ASP.NET Core 9 Web API + Dapper (no EF) + SQL Server 2022,
Angular 22.1 (standalone + Signals), Redis, Hangfire, Serilog, JWT/RBAC, Swagger. See
`../docs/PROJECT_PLAN.md` for the full architecture, phase plan and Definition of Done, and
`../docs/api.md` for the current REST API endpoint inventory.

## Prerequisites

- .NET 9 SDK (this repo pins the SDK via `global.json`)
- Node.js + Angular CLI 22.1 (for `frontend/webapp` — a single Angular app serving the public site
  at `/` and the admin CRM at `/admin/*`, role-guarded; the exact version is pinned in
  `frontend/webapp/package.json`'s `@angular/core` dependency)
- SQL Server — either LocalDB (`sqllocaldb info` to check it's installed) or a full SQL Server 2022 instance
- A Redis-compatible server for local dev (e.g. [Memurai](https://www.memurai.com/get-memurai) on Windows) — optional for Phase 0; the API starts and serves requests without it, only `/ready` and cache-backed features need it running
- Docker + Docker Compose — only needed if you choose to run SQL Server/Redis in containers instead of natively (see `docker/docker-compose.yml`)

## Local setup (native SQL Server LocalDB — what this repo is configured for by default)

1. **Create the database** (one-time):
   ```powershell
   sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE OneClickYatra;"
   ```
2. **Set the JWT signing key** (never committed — stored in user-secrets locally):
   ```powershell
   cd backend/OneClickYatra.Api
   dotnet user-secrets init   # already done if OneClickYatra.Api.csproj has a UserSecretsId
   dotnet user-secrets set "Jwt:Key" "<a-random-64+-character-string>"
   ```
3. **Run migrations**:
   ```powershell
   cd backend/OneClickYatra.MigrationRunner
   dotnet run -- migrate
   ```
4. **Seed development data** (roles, permissions, a dev SuperAdmin user — dev/demo only, never run in Production):
   ```powershell
   dotnet run -- seed
   ```
   Seeded SuperAdmin login: `superadmin@oneclickyatra.dev` / `Admin@12345` — change or remove outside local dev.
5. **Run the API**:
   ```powershell
   cd ../OneClickYatra.Api
   dotnet run
   ```
   - Swagger UI: `https://localhost:<port>/swagger` (JWT "Authorize" button included)
   - Health: `GET /health` (liveness only) and `GET /ready` (liveness + SQL Server + Redis)
   - Hangfire dashboard: `/hangfire`

6. **Run the frontend** (in a separate terminal, with the API from step 5 still running):
   ```powershell
   cd frontend/webapp
   npm install
   npx ng serve
   ```
   - Dev server: `http://localhost:4200` — this repo's `angular.json`/`package.json` don't override
     the port, so Angular's own default applies. The public site is served at `/`, the admin CRM at
     `/admin/*` (role-guarded).
   - Run the frontend unit tests with `npx ng test --watch=false`.

If you don't have a local Redis-compatible server running yet, everything above still works —
`/ready` will report Redis as unhealthy until one is available; no other endpoint depends on it
in Phase 0.

## Migration runner CLI

`backend/OneClickYatra.MigrationRunner` is a small console tool (not Entity Framework — it just
executes the versioned `.sql` files under `database/Migrations` and `database/Seeds` in order,
tracked in `__SchemaVersions` / `__SeedVersions` tables):

```powershell
dotnet run -- migrate   # applies database/Migrations/*.sql not yet applied
dotnet run -- seed      # applies database/Seeds/*.sql not yet applied (dev/demo data only)
dotnet run -- hash <plainTextPassword>   # prints a BCrypt hash, useful for writing new seed users
```

Never edit an already-applied migration file — add a new `VNNN__Description.sql` instead.

## Running with Docker instead (optional, production-parity)

```powershell
cd docker
copy .env.example .env   # fill in real values, never commit .env
docker compose up -d
```

## Free public hosting (Oracle Cloud)

Full step-by-step guide: **[DEPLOY.md](./DEPLOY.md)**

Quick start on a Linux VM:

```bash
cd docker
cp .env.example .env   # set SQL_SA_PASSWORD, REDIS_PASSWORD, JWT_KEY, PUBLIC_DOMAIN
docker compose -f docker-compose.prod.yml up -d --build
```

## Tests

```powershell
dotnet test tests/UnitTests/OneClickYatra.UnitTests.csproj
```

## Project layout

See `docs/PROJECT_PLAN.md` §3 for the full architecture diagram and folder-by-folder breakdown.
