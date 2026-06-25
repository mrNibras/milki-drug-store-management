# Milki Drug Store Management System - Backend

## ASP.NET Core 9 Web API

This is the backend API for the Milki Drug Store Management System.

## Architecture

The backend follows **Clean Architecture** with the following layers:

```
backend/
├── MilkiDrugStore.API/              # Presentation Layer (Controllers, Middleware)
├── MilkiDrugStore.Application/      # Application Layer (Services, DTOs, Interfaces)
├── MilkiDrugStore.Domain/           # Domain Layer (Entities, Enums, Business Rules)
└── MilkiDrugStore.Infrastructure/   # Infrastructure Layer (Database, Repositories, Email)
```

## Technology Stack

- **ASP.NET Core 9** - Web API Framework
- **Entity Framework Core 9** - ORM
- **SQL Server** - Database
- **JWT Authentication** - Security
- **AutoMapper** - Object Mapping
- **FluentValidation** - Input Validation
- **Serilog** - Logging
- **Swagger/OpenAPI** - API Documentation

## Database Tables

```sql
-- Core Tables
Users
Roles
Categories
Medicines
MedicineBatches

-- Transaction Tables
Suppliers
Purchases
PurchaseItems
Sales
SaleItems
InventoryTransactions

-- System Tables
Notifications
AuditLogs
```

## API Endpoints

### Authentication
```
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh-token
POST   /api/auth/change-password
```

### Medicines
```
GET    /api/medicines
GET    /api/medicines/{id}
POST   /api/medicines
PUT    /api/medicines/{id}
DELETE /api/medicines/{id}
GET    /api/medicines/search?q={query}
```

### Inventory
```
GET    /api/inventory
GET    /api/inventory/summary
GET    /api/inventory/low-stock
GET    /api/inventory/expiring
GET    /api/inventory/expired
```

### Purchases
```
GET    /api/purchases
GET    /api/purchases/{id}
POST   /api/purchases/bulk
GET    /api/purchases/supplier/{supplierId}
```

### Sales
```
GET    /api/sales
GET    /api/sales/{id}
POST   /api/sales
GET    /api/sales/today
GET    /api/sales/range?start={date}&end={date}
```

### Suppliers
```
GET    /api/suppliers
GET    /api/suppliers/{id}
POST   /api/suppliers
PUT    /api/suppliers/{id}
DELETE /api/suppliers/{id}
GET    /api/suppliers/{id}/purchases
```

### Reports
```
GET    /api/reports/sales?period={daily|weekly|monthly|yearly}
GET    /api/reports/inventory
GET    /api/reports/profit
GET    /api/reports/suppliers
GET    /api/reports/staff
GET    /api/reports/most-selling
```

### Notifications
```
GET    /api/notifications
PUT    /api/notifications/{id}/read
PUT    /api/notifications/read-all
```

### Users (Admin Only)
```
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
PUT    /api/users/{id}/toggle-active
```

## Setup Instructions

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Database Setup
```bash
# Create database
dotnet ef database update

# Seed data
dotnet run --seed
```

### Run API
```bash
cd MilkiDrugStore.API
dotnet run
```

The API will be available at `https://localhost:5001`

### Swagger
Navigate to `https://localhost:5001/swagger` for API documentation.

## Environment Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MilkiDrugStoreDB;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyHereMustBe32Characters!",
    "Issuer": "MilkiDrugStore",
    "Audience": "MilkiDrugStoreApp",
    "ExpirationInMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Future Enhancements

- [ ] Excel Import for medicines
- [ ] Barcode scanning
- [ ] Multi-branch support
- [ ] Mobile app API
- [ ] SMS notifications
- [ ] Cloud deployment
