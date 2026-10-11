# Deployment and persistence

## Repository-declared topology

- `frontend/vercel.json` declares SPA fallback routing and response headers.
- `render.yaml` declares a Render Docker web service for the API, a PostgreSQL database, `/var/data` disk, `/api/health` health path, and S3 storage settings.
- `backend/Dockerfile` builds and publishes a .NET 8 API image and exposes port 8080.
- `Program.cs` reads connection string configuration (or `DATABASE_URL` through `DbProviderResolver`), persists ASP.NET Data Protection keys under the configured data directory, checks that directory is writable, applies EF migrations, and seeds data.

These files establish deployment preparation/configuration only.

## Build-time and runtime configuration

The frontend reads VITE_API_BASE_URL during Vite build; its source fallback is http://localhost:5000. Vite environment values are bundled into the client and are public configuration, not secrets. Vercel's actual build environment was not inspected, so the deployed bundle's API base is unverified.

The backend resolves ConnectionStrings:DefaultConnection first, then DATABASE_URL; production YAML declares ConnectionStrings__DefaultConnection from the Render database URL. appsettings.json/development include connection configuration, while the production overlay omits a connection string and expects deployment configuration. The live dashboard values were not read. Do not publish the values.

Other relevant configuration names include JwtSettings__Secret, Cors__AllowedOrigins (the expected hierarchical environment key), ASPNETCORE_ENVIRONMENT, DataDirectory, and FileStorage__Provider. Secret values must remain in hosting secret storage. Build-time frontend variables and backend runtime variables are different boundaries.

## Live verification performed

At **2026-10-10 11:36 UTC**:

- `https://milki-drug-store-management.vercel.app/` returned HTTP 200 from Vercel.
- `https://milki-drug-store-api.onrender.com/api/health` returned HTTP 404, header `x-render-routing: no-server`.

Conclusion: the frontend host responds, but the configured Render API did not route to a running service during the probe. Therefore a complete live login/API/database workflow is not verified, and it would be inaccurate to say the entire system is currently working in production. A 200 frontend response alone does not prove it contains the expected current app build or that its API is available.

## Configuration mismatches to inspect

- `Program.cs` binds `Cors:AllowedOrigins`; Render YAML sets `AllowedOrigins`, which is a different configuration key. Base configuration may still contain the frontend origin, but the intended Render override likely does not bind.
- `frontend/src/config.ts` defaults to localhost when `VITE_API_BASE_URL` is missing; verify the production build value before assuming the browser targets Render.
- Dockerfile and Render YAML use `ASPNETCORE_URLS=http+:8080`, a nonstandard URL form. Program appends a valid URL using the platform `PORT`; verify which binding wins in the deployed container. Do not infer this caused the no-server response.
- `backend/docker-compose.yml` should be reviewed before local compose use: API-in-container configuration appears to reference `localhost` for PostgreSQL rather than the `postgres` service name. Never publish development credentials.
- `backend/docker-entrypoint.sh` is not referenced by the Dockerfile entrypoint, so its setup code should not be assumed to execute.
- Render blueprint declares free DB and disk, but current dashboard plan, disk, values, backup policy, secret overrides and deployment state are not visible.

## Persistence and backup claims

The backend writes through EF Core/PostgreSQL and sale/purchase services use database transactions. `Program.cs` applies migrations at startup. Data Protection keys and local uploads can use `/var/data`; this does not persist PostgreSQL data by itself. A managed DB is separately declared in `render.yaml`. A code path/configuration does not prove live data survives redeployment or that backups restore correctly.

Settings backup/restore API routes and PostgreSQL client tools appear in code/image. No actual production backup, restore, disk persistence, or DB restart was exercised. Do not say “backups are working” without a successful restore test and evidence of schedule/retention.

## Safe verification procedure for an authorized operator

1. In the hosting dashboard, verify service state, latest deploy commit, logs, health status, environment variable names (not values in public screenshots), attached database, and disk mount.
2. Inspect API health from an approved environment; then verify a protected read request with a dedicated test account.
3. Use a disposable/non-production DB. Create a test product/purchase with clearly synthetic data, read it after a fresh DbContext or process restart, and verify the matching batch/transaction rows.
4. Run a backup, restore it into a separate disposable DB, and compare expected schema plus selected synthetic records.
5. Confirm database backups, retention, migration rollback strategy, and disk restore procedure with the operator.
6. Remove synthetic data only in that disposable test environment and retain logs/artifacts.

No production database was accessed or modified in this task.

## Deployment presentation answer

“The repository contains Vercel and Render configuration for a split frontend/API deployment, a PostgreSQL service and a persistent disk. I verified the Vercel endpoint answered HTTP 200, but on 10 October 2026 the Render API health check returned 404 with no server. I have not inspected the hosting dashboard or production database, so I describe deployment as configured in source with frontend response observed, but end-to-end production operation and persistence unverified.”
