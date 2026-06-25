# Milki Drug Store Management System (MDSMS)

## Project Overview

A comprehensive web-based drug store management system built with modern technologies.

## Project Structure

```
milki-drug-store-management-system/
│
├── frontend/                    # React Frontend Application
│   ├── src/
│   │   ├── app/
│   │   │   ├── App.tsx
│   │   │   └── main.tsx
│   │   │
│   │   ├── components/
│   │   │   ├── layout/
│   │   │   │   └── Sidebar.tsx
│   │   │   └── ui/
│   │   │       ├── Badge.tsx
│   │   │       ├── Button.tsx
│   │   │       ├── Modal.tsx
│   │   │       └── ThemeCard.tsx
│   │   │
│   │   ├── data/
│   │   │   └── mockData.ts
│   │   │
│   │   ├── layouts/
│   │   │   └── MainLayout.tsx
│   │   │
│   │   ├── pages/
│   │   │   ├── DashboardPage.tsx
│   │   │   ├── DamageExpiryPage.tsx
│   │   │   ├── InventoryPage.tsx
│   │   │   ├── LoginPage.tsx
│   │   │   ├── MedicinesPage.tsx
│   │   │   ├── NotificationsPage.tsx
│   │   │   ├── POSPage.tsx
│   │   │   ├── PurchasesPage.tsx
│   │   │   ├── ReportsPage.tsx
│   │   │   ├── SettingsPage.tsx
│   │   │   ├── SuppliersPage.tsx
│   │   │   └── UsersPage.tsx
│   │   │
│   │   ├── store/
│   │   │   ├── appStore.ts
│   │   │   └── themeStore.ts
│   │   │
│   │   ├── types/
│   │   │   └── index.ts
│   │   │
│   │   ├── utils/
│   │   │   ├── cn.ts
│   │   │   └── helpers.ts
│   │   │
│   │   └── index.css
│   │
│   ├── public/
│   ├── index.html
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
│
└── backend/                     # ASP.NET Core Backend (Future)
    ├── MilkiDrugStore.API/
    ├── MilkiDrugStore.Application/
    ├── MilkiDrugStore.Domain/
    └── MilkiDrugStore.Infrastructure/
```

## Technology Stack

### Frontend
- **React 19** with TypeScript
- **Vite** - Build tool
- **Tailwind CSS 4** - Styling
- **Zustand** - State management
- **React Router** - Navigation
- **Recharts** - Charts
- **Lucide React** - Icons
- **date-fns** - Date utilities

### Backend (Planned)
- **ASP.NET Core 9** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT Authentication** - Security
- **Clean Architecture** - Design pattern

## Features

### 1. Authentication & Authorization
- Login/Logout
- JWT Token Authentication
- Role-based access (Admin, Pharmacist)

### 2. Dashboard
- Sales overview
- Inventory summary
- Low stock alerts
- Expiry warnings
- Most selling items
- Recent activities

### 3. Medicine Management
- Register medicines
- Edit medicine information
- Category management
- Unit type management

### 4. Inventory Management
- FEFO (First Expired First Out)
- Batch tracking
- Stock levels
- Purchase price & selling price
- Expiry date tracking

### 5. Point of Sale (POS)
- Quick medicine search
- Cart management
- Discount support
- Multiple payment methods
- Receipt generation

### 6. Purchase Management
- Bulk purchase entry
- Supplier selection
- Payment tracking (Paid/Partial/Unpaid)
- Auto medicine creation

### 7. Supplier Management
- Supplier registration
- Contact management
- Financial summary (from purchases)

### 8. Reports
- Sales reports (Daily/Weekly/Monthly/Yearly)
- Inventory reports
- Profit reports
- Supplier reports
- Staff performance reports

### 9. Damage & Expiry Management
- Auto-detection of expiring items
- Manual damage recording
- Expiry alerts (every 15 days after 6 months)

### 10. Notifications
- Low stock alerts
- Expiry warnings
- Out of stock alerts

### 11. User Management
- Add/Edit/Delete users
- Role assignment
- Active/Inactive status

### 12. Settings
- Pharmacy information
- Inventory thresholds
- System configuration

## Database Schema (SQL Server)

### Tables
- Users
- Roles
- Categories
- Medicines
- MedicineBatches
- Suppliers
- Purchases
- PurchaseItems
- Sales
- SaleItems
- InventoryTransactions
- Notifications
- AuditLogs

## API Endpoints (Planned)

### Authentication
- POST /api/auth/login
- POST /api/auth/register
- POST /api/auth/refresh-token

### Medicines
- GET /api/medicines
- POST /api/medicines
- PUT /api/medicines/{id}
- DELETE /api/medicines/{id}

### Inventory
- GET /api/inventory
- GET /api/inventory/low-stock
- GET /api/inventory/expiring

### Purchases
- GET /api/purchases
- POST /api/purchases/bulk

### Sales
- GET /api/sales
- POST /api/sales

### Suppliers
- GET /api/suppliers
- POST /api/suppliers
- PUT /api/suppliers/{id}
- DELETE /api/suppliers/{id}

### Reports
- GET /api/reports/sales
- GET /api/reports/inventory
- GET /api/reports/profit

## Business Rules

### FEFO (First Expired First Out)
- Sell medicines from earliest expiry batch first
- Automatic batch selection in POS

### Discount Management
- Max 5% for pharmacists
- Unlimited for admins
- Reason required for all discounts

### Payment Tracking
- Payments tracked at purchase level
- Supplier financial summary calculated from purchases

### Expiry Notifications
- First alert at 6 months before expiry
- Then every 15 days until sold or expired

## Installation

### Frontend
```bash
cd frontend
npm install
npm run dev
```

### Backend (Future)
```bash
cd backend
dotnet restore
dotnet run
```

## Environment Variables

### Frontend (.env)
```
VITE_API_URL=http://localhost:5000/api
```

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MilkiDrugStoreDB;Trusted_Connection=true"
  },
  "JwtSettings": {
    "Secret": "your-secret-key",
    "ExpirationInMinutes": 60
  }
}
```

## License

© 2026 Milki Drug Store. All rights reserved.
