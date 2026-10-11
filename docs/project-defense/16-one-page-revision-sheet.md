# One-page revision sheet

## Project in one breath

**Milki Drug Store Management System:** React/TypeScript browser client → ASP.NET Core .NET 8 API → EF Core/Npgsql → PostgreSQL. Layered modular monolith; selected MediatR; not microservices or strict Clean Architecture.

## Purpose and users

Designed to support medicine/cosmetic catalog, batch and branch stock, supplier purchasing, POS sales, damage/expiry, roles, notifications and reports. Admin has management/report privileges; authenticated staff use operational flows. No external payment gateway, customer/prescription module, or verified regulatory workflow found.

## Core data

24 DbSets: Roles, Users, Branches, Settings, Categories, UnitTypes, Medicines, Cosmetics, MedicineBatches, CosmeticBatches, Suppliers, Purchases, PurchaseItems, Sales, SaleItems, InventoryTransactions, DamageRecords, ExpiredRecords, Notifications, AuditLogs, RefreshTokens, PasswordResets, AuditLogArchives, NotificationArchives.

## High-value business rules

- Batch balance = received − issued − damaged − expired.
- Purchases create batches/transactions inside a DB transaction.
- Sale uses batch allocation and FEFO ordering; pharmacist discount max is 5% per unit; discounts require a reason.
- Damage requires one batch ID, positive quantity, reason and available stock.
- Expiry job runs daily; notification job every 30 minutes.
- Auth uses BCrypt, JWT, server-side roles and active-user check.

## API reminders

`POST /api/auth/login`, `POST /api/sales`, `POST /api/purchases` (Admin), `POST /api/damages`, `GET /api/reports/*` (Admin), `GET /api/health` (source route). 401 = not authenticated; 403 = not authorized.

## Evidence and limitations to state honestly

- Fresh .NET run: **252 total; 235 passed; 17 failed; 0 skipped**. Failures are two test fixture groups missing `ICurrentUserService`, before assertions.
- `npm run build`: passed with bundle/import warnings.
- Vercel returned HTTP 200; Render API health returned 404/no-server on 2026-10-10. No end-to-end production confirmation.
- High priority: full login response logging; localStorage/raw token storage; CORS env key mismatch; missing validator registration; test DI failures.

## Five claims never to misstate

1. Do not say all 252 tests passed.
2. Do not say the API is currently working live; last probe showed no server.
3. Do not call the database backup/restore or production persistence verified.
4. Do not call the architecture microservices, strict Clean Architecture, or full CQRS.
5. Do not claim individual authorship without your own evidence.

## Before Q&A

Open `Program.cs`, `SaleService.cs`, `AppDbContext.cs`, `AuthService.cs`, `api.ts`, test report and diagrams. Explain one sale from UI to DB and name one risk plus a reasonable improvement.
