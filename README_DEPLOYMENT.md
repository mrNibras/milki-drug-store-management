# Milki Drug Store Management System - Deployment Guide

## Table of Contents
1. [Local Development](#local-development)
2. [Docker Build & Run](#docker-build--run)
3. [Render Deployment (Backend)](#render-deployment-backend)
4. [Vercel Deployment (Frontend)](#vercel-deployment-frontend)
5. [Environment Variables](#environment-variables)
6. [Database Setup](#database-setup)
7. [Migrations](#migrations)
8. [Backup Instructions](#backup-instructions)
9. [Health Check](#health-check)
10. [Troubleshooting](#troubleshooting)

---

## Local Development

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQLite (included with .NET)

### Backend Setup

```bash
cd backend

# Restore dependencies
dotnet restore

# Run migrations (if using SQL Server in development)
dotnet ef database update --project src/MilkiDrugStore.Persistence --startup-project src/MilkiDrugStore.Api

# Run the application
dotnet run --project src/MilkiDrugStore.Api
```

The API will be available at: `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

### Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at: `http://localhost:5173`

---

## Docker Build & Run

### Build the Docker Image

```bash
cd backend
docker build -t milki-drug-store-api .
```

### Run the Container

```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/MilkiDrugStoreDB.db" \
  -e JwtSettings__Secret="your-secret-key-here" \
  -e JwtSettings__Issuer="MilkiDrugStore" \
  -e JwtSettings__Audience="MilkiDrugStoreClient" \
  -e AllowedOrigins__0="http://localhost:5173" \
  -v milki-data:/app/data \
  --name milki-api \
  milki-drug-store-api
```

### Using Docker Compose

```bash
cd backend
docker-compose up -d
```

---

## Render Deployment (Backend)

### Option 1: Using render.yaml (Blueprint)

1. Push your code to GitHub
2. Go to [Render Dashboard](https://dashboard.render.com)
3. Click **New** → **Blueprint**
4. Connect your GitHub repository
5. Render will auto-detect `render.yaml` and configure the service

### Option 2: Manual Web Service Setup

1. **Create a new Web Service:**
   - Go to Render Dashboard → **New** → **Web Service**
   - Connect your GitHub repo
   - Select the branch to deploy

2. **Configure the service:**
   - **Name:** `milki-drug-store-api`
   - **Runtime:** Docker
   - **Dockerfile Path:** `backend/Dockerfile`
   - **Docker Context:** `backend`
   - **Plan:** Starter ($7/month) or Free

3. **Add Environment Variables:**

| Key | Value | Notes |
|-----|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | |
| `ASPNETCORE_URLS` | `http://+:8080` | |
| `JwtSettings__Secret` | `(generate a strong random key)` | Use a 32+ character random string |
| `JwtSettings__Issuer` | `MilkiDrugStore` | |
| `JwtSettings__Audience` | `MilkiDrugStoreClient` | |
| `JwtSettings__ExpiryMinutes` | `60` | |
| `AdminEmail` | `admin@milki.com` | For approval emails |
| `ConnectionStrings__DefaultConnection` | `Data Source=/var/data/MilkiDrugStoreDB.db` | For SQLite (development) |
| `AllowedOrigins__0` | `https://your-app.vercel.app` | Your Vercel frontend URL |

4. **Add Persistent Disk:**
   - Click **Disks** → **Add Disk**
   - **Name:** `milki-data`
   - **Mount Path:** `/var/data`
   - **Size:** 1 GB

5. **Deploy:**
   - Click **Create Web Service**
   - Wait for the build to complete (2-5 minutes)
   - Your API will be available at: `https://milki-drug-store-api.onrender.com`

### Health Check

Render will automatically ping `GET /health` to verify the service is running.

### Important Notes

- **Free tier** on Render spins down after 15 minutes of inactivity. First request after inactivity takes ~30 seconds.
- **SQLite** is fine for small stores. For production with multiple users, consider upgrading to SQL Server or PostgreSQL.
- **CORS** is configured via `AllowedOrigins` environment variable. Make sure to add your Vercel domain.

---

## Vercel Deployment (Frontend)

### Prerequisites
- Vercel account
- Backend API deployed on Render

### Setup

1. **Push your code to GitHub**

2. **Import project to Vercel:**
   - Go to [Vercel Dashboard](https://vercel.com)
   - Click **Add New** → **Project**
   - Import your GitHub repository

3. **Configure the project:**
   - **Framework Preset:** Vite
   - **Root Directory:** `frontend`
   - **Build Command:** `npm run build`
   - **Output Directory:** `dist`

4. **Add Environment Variables:**
   - Go to **Settings** → **Environment Variables**
   - Add the following:

| Key | Value | Environment |
|-----|-------|-------------|
| `VITE_API_BASE_URL` | `https://milki-drug-store-api.onrender.com` | Production |
| `VITE_APP_NAME` | `Milki Drug Store` | Production |
| `VITE_APP_VERSION` | `1.0.0` | Production |

5. **Deploy:**
   - Click **Deploy**
   - Wait for the build to complete (1-2 minutes)
   - Your app will be available at: `https://your-app.vercel.app`

### Important Notes

- **SPA Routing:** The `vercel.json` file rewrites all routes to `index.html` for React Router support.
- **API URL:** Make sure `VITE_API_BASE_URL` points to your Render backend.
- **CORS:** Add your Vercel domain to the `AllowedOrigins` environment variable on Render.

---

## Environment Variables

### Backend (.NET)

| Variable | Required | Description | Default |
|----------|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Yes | Environment (Development/Production) | `Development` |
| `ASPNETCORE_URLS` | No | Server binding URL | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Yes | Database connection string | SQLite in `/var/data` |
| `JwtSettings__Secret` | Yes | JWT signing key (32+ chars) | `CHANGE_ME...` |
| `JwtSettings__Issuer` | Yes | JWT issuer | `MilkiDrugStore` |
| `JwtSettings__Audience` | Yes | JWT audience | `MilkiDrugStoreClient` |
| `JwtSettings__ExpiryMinutes` | No | JWT token expiry | `60` |
| `AllowedOrigins__0` | Yes | CORS origin (repeatable) | `http://localhost:5173` |
| `Email__Host` | No | SMTP host | `smtp.gmail.com` |
| `Email__Port` | No | SMTP port | `587` |
| `Email__Username` | No | SMTP username | - |
| `Email__Password` | No | SMTP password | - |
| `Email__EnableSsl` | No | Enable SSL for SMTP | `true` |
| `AdminEmail` | No | Admin email for notifications | `admin@milki.com` |
| `DataDirectory` | No | SQLite data directory | `/var/data` |
| `BackupDirectory` | No | Backup storage directory | `/var/data/backups` |

### Frontend (Vite)

| Variable | Required | Description | Default |
|----------|----------|-------------|---------|
| `VITE_API_BASE_URL` | Yes | Backend API URL | `http://localhost:5000` |
| `VITE_APP_NAME` | No | Application name | `Milki Drug Store` |
| `VITE_APP_VERSION` | No | Application version | `1.0.0` |

---

## Database Setup

### SQLite (Development)

No setup required. The database is created automatically on first run.

**Connection String:**
```
Data Source=/var/data/MilkiDrugStoreDB.db
```

### SQL Server (Production)

1. **Provision SQL Server:**
   - Azure SQL Database
   - AWS RDS SQL Server
   - Google Cloud SQL

2. **Update Connection String:**
   ```
   Server=your-server.database.windows.net;Database=MilkiDrugStoreDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
   ```

3. **Set Environment Variable:**
   ```
   ConnectionStrings__DefaultConnection=Server=...;Database=...;...
   ```

### PostgreSQL (Alternative)

Install the Npgsql.EntityFrameworkCore.PostgreSQL package and update Program.cs:

```csharp
if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
{
    options.UseNpgsql(connectionString);
}
```

---

## Migrations

### Create a New Migration

```bash
cd backend
dotnet ef migrations add <MigrationName> --project src/MilkiDrugStore.Persistence --startup-project src/MilkiDrugStore.Api
```

### Apply Migrations

```bash
# Apply all pending migrations
dotnet ef database update --project src/MilkiDrugStore.Persistence --startup-project src/MilkiDrugStore.Api
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project src/MilkiDrugStore.Persistence --startup-project src/MilkiDrugStore.Api
```

### Production Migrations

Migrations are applied automatically on application startup in `Program.cs`:

```csharp
db.Database.Migrate();
```

---

## Backup Instructions

### Manual Backup via API

```bash
# Create backup
curl -X POST https://your-api.onrender.com/api/settings/backup \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# The backup file will be downloaded as a .db file
```

### Backup Service

The `BackupService` abstraction supports:
- SQLite file copy (implemented)
- SQL Server backup (not yet implemented, throws `NotSupportedException`)

### Scheduled Backups (Future)

To add scheduled backups:
1. Use a cron job or scheduled task
2. Call the backup API endpoint daily
3. Store backups in cloud storage (S3, Azure Blob, etc.)

---

## Health Check

### Endpoint

```
GET /health
```

### Response

```json
{
  "status": "Healthy",
  "time": "2026-07-21T04:53:40.1234567Z"
}
```

### Usage

- **Render:** Automatically pings `/health` every 30 seconds
- **Load Balancers:** Use `/health` for health checks
- **Monitoring:** Integrate with UptimeRobot, Pingdom, etc.

---

## Troubleshooting

### SQLite Database Locked Error

**Symptom:** `SQLite Error 5: database is locked`

**Solution:**
- Ensure only one instance of the app is running
- Check for long-running transactions
- Consider switching to SQL Server for production

### Render Deployment Fails

**Symptom:** Build fails or app crashes on startup

**Solutions:**
1. Check Render logs for specific errors
2. Verify all environment variables are set
3. Ensure the `/var/data` disk is mounted
4. Check that `PORT` environment variable is set to `8080`

### CORS Errors

**Symptom:** `Access to fetch at '...' has been blocked by CORS policy`

**Solutions:**
1. Add your frontend URL to `AllowedOrigins__0` on Render
2. For multiple origins, use `AllowedOrigins__0`, `AllowedOrigins__1`, etc.
3. Ensure `UseCors` is called before `UseAuthorization`

### JWT Token Expired

**Symptom:** `401 Unauthorized` after some time

**Solutions:**
1. Increase `JwtSettings__ExpiryMinutes`
2. Implement refresh token rotation
3. Check client-side token refresh logic

### Database Migrations Fail

**Symptom:** Migration errors on startup

**Solutions:**
1. Check database permissions
2. Verify connection string is correct
3. For SQLite, ensure the directory exists and is writable
4. For SQL Server, ensure the database exists and the user has permissions

---

## Security Checklist

- [ ] JWT secret is a strong random 32+ character string
- [ ] JWT secret is stored in environment variables, not code
- [ ] CORS origins are restricted to your actual domains
- [ ] HTTPS is enabled in production
- [ ] Security headers are enabled (X-Content-Type-Options, X-Frame-Options, etc.)
- [ ] Database connection strings are not hardcoded
- [ ] Admin credentials are changed from defaults
- [ ] SMTP credentials are stored securely
- [ ] Error messages don't expose sensitive information in production
- [ ] Database backups are encrypted at rest

---

## Performance Optimization

- [ ] Enable response compression (`AddResponseCompression`)
- [ ] Use Redis for caching (if needed)
- [ ] Enable database query caching
- [ ] Use CDN for frontend assets (Vercel handles this)
- [ ] Optimize database indexes
- [ ] Implement rate limiting
- [ ] Use connection pooling

---

## Monitoring

- [ ] Set up application monitoring (Application Insights, Sentry, etc.)
- [ ] Monitor database performance
- [ ] Set up uptime monitoring (UptimeRobot, Pingdom)
- [ ] Monitor Render service metrics
- [ ] Set up error alerting

---

## Support

For issues or questions:
- Check the troubleshooting section above
- Review Render logs: `render logs -f milki-drug-store-api`
- Review Vercel logs: Vercel Dashboard → Your Project → Logs
