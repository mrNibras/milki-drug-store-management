# Milki Drug Store Management System - Backend

## ASP.NET Core 8 Web API

Backend API for the Milki Drug Store Management System.

## Architecture

```
backend/
├── MilkiDrugStore.Api/              # Presentation Layer
├── MilkiDrugStore.Application/      # Application Layer
├── MilkiDrugStore.Domain/           # Domain Layer
└── MilkiDrugStore.Infrastructure/   # Infrastructure Layer
```

## Tech Stack

- ASP.NET Core 8
- Entity Framework Core 8
- SQL Server
- JWT Authentication
- AutoMapper + FluentValidation
- Serilog + Swagger

## Quick Start

```bash
# Start SQL Server (requires Docker)
docker compose up -d sqlserver

# Run API
cd backend
dotnet run --project src/MilkiDrugStore.Api
```

API: `https://localhost:5001` / Swagger: `https://localhost:5001/swagger`

Default admin: `admin@milki.com` / `Admin123`

## Docker

```bash
docker compose up --build
```

## Connection String

Local: `Server=localhost;Database=MilkiDrugStoreDB;User Id=sa;Password=YourStrong@Pass123;TrustServerCertificate=true`
