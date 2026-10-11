# API reference (route groups)

Controller route prefixes are generally `/api/[controller]`. Roles reflect controller/action attributes inspected in source; verify any changed branch before demo. HTTP status details can differ where controllers catch errors locally.

| Route | Methods | Access | Purpose |
|---|---|---|---|
| `/api/auth/login` | POST | Anonymous | Issue access/refresh tokens |
| `/api/auth/register` | POST | Anonymous | Create registration request/user |
| `/api/auth/refresh` | POST | Anonymous | Rotate refresh token and issue token response |
| `/api/auth/forgot-password`, `/reset-password` | POST | Anonymous | Request/complete password reset |
| `/api/auth/change-password` | POST | Authenticated | Change current user's password |
| `/api/auth/approve/{userId}` | POST | Admin | Approve account |
| `/api/medicines` | GET, POST | Authenticated read; Admin write | List/create medicine |
| `/api/medicines/{id}` | GET, PUT, DELETE | Authenticated read; Admin write | Read/update/soft-delete medicine |
| `/api/medicines/search` | GET | Authenticated | Search medicines |
| `/api/medicines/batches` | POST | Admin | Add a medicine batch |
| `/api/cosmetics` | GET, POST | Authenticated read; Admin write | List/create cosmetics |
| `/api/cosmetics/{id}` | GET, PUT, DELETE | Authenticated read; Admin write | Read/update/soft-delete cosmetic |
| `/api/cosmetics/batches` | POST | Admin | Add cosmetic batch |
| `/api/sales` | GET, POST | Authenticated | Read/process sales |
| `/api/sales/{id}` | GET | Authenticated | Sale detail |
| `/api/purchases`, `/api/purchases/{id}` | GET, POST | Admin | List/read/create purchases |
| `/api/suppliers`, `/api/suppliers/{id}` | GET, POST, PUT, DELETE | Admin | Supplier management |
| `/api/damages` | GET, POST | Authenticated | Record/list damaged stock |
| `/api/expired` | GET | Authenticated | Read expiry write-off history |
| `/api/notifications`, `/unread` | GET | Admin or Pharmacist | List notifications |
| `/api/notifications/{id}/read`, `/read-all` | PUT | Admin or Pharmacist | Mark notifications read |
| `/api/reports/dashboard/summary`, `/sales/{period}`, `/sales/{period}/bounds`, `/inventory`, `/suppliers`, `/staff` | GET | Admin | Reports and periods |
| `/api/dashboard/summary` | GET | Authenticated route; action Admin | Dashboard summary |
| `/api/users` and `/api/users/{id}` | GET, POST, PUT, DELETE; approval action | Admin | User administration |
| `/api/branches` and `/api/branches/{id}` | GET, POST, PUT, DELETE | Admin | Branch administration |
| `/api/settings` and `/api/settings/backup`, `/restore` | GET, PUT, POST | Authenticated base; admin-only actions | Settings and backup operations |
| `/api/settings/public` | GET | Controller class currently authenticated | Public-settings data (route name does not make it anonymous) |
| `/api/audit-logs`, `/user/{id}`, `/range` | GET | Admin | Audit query |
| `/api/catalog/categories`, `/unit-types` | GET | Anonymous | Catalog lookups |
| `/api/lookups/categories`, `/unit-types` | GET | Authenticated | Authenticated lookup variants |
| `/api/health` | GET | Anonymous | Health endpoint in source; remote Render check returned 404/no-server |
| `/api/health/database` | GET | Admin | Database diagnostic endpoint |

## HTTP concepts to say aloud

- GET reads; POST creates or initiates; PUT updates; DELETE in medicine/cosmetics is soft deletion in service logic.
- 401 means missing/invalid authentication; 403 means authenticated principal lacks the required role/policy.
- 400 is used for some validation and controller-caught exceptions; a broad catch may also turn server failures into 400. Unhandled unexpected failures become 500 through the global filter.
- JSON uses camelCase and ignores null values by Program configuration.

## Route verification references

`backend/src/MilkiDrugStore.Api/Controllers/*.cs`; start with `AuthController`, `SalesController`, `PurchasesController`, `MedicinesController`, `CosmeticsController`, `ReportsController`, `HealthController`. Do not treat a client-side route or UI visibility as endpoint authorization evidence.
