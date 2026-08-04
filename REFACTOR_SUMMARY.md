# Milki Drug Store — Medicine & Catalog Refactor: Summary

## 1. What Was Implemented

### Backend — Medicine Model (committed in `9ca0f50e`)
| Area | Detail |
|------|--------|
| **Entity** | `Medicine` now uses `ProductId` (PK), `ProductCode` (unique), `BrandName`, `GenericName`, `Strength`, `DosageForm`, `Barcode`, `Manufacturer`, `Description`, `CategoryId`, `UnitTypeId`, `PurchasePrice`, `SellingPrice`, `ReorderLevel`, `IsActive`, `CreatedDate`, `UpdatedDate`. |
| **InventoryBatch** | `MedicineBatch` tracks `BatchId`, `ProductId`, `BranchId`, `BatchNumber`, `ManufacturingDate`, `ExpiryDate`, `QuantityReceived/Issued/Damaged/Expired`, computed `RemainingQuantity`, `PurchasePrice`, `SellingPrice`, `SupplierId`, `Remarks`. |
| **No quantity on Medicine** | Stock lives only in batches. `Medicine` has no `Stock`/`Quantity` field. |
| **Category / UnitType** | Now hold **custom** entries only. Built-in values are constants in `MedicineCatalog` (negative IDs so they never collide with DB-generated custom IDs). |
| **CatalogService** | `ResolveCategoryIdAsync`/`ResolveUnitTypeIdAsync` resolve a name → built-in ID or create a custom row. `GetCategoryOptions`/`GetUnitTypeOptions` return merged built-in + custom lists. `CreateCustomCategory`/`CreateCustomUnitType` persist new custom values. |
| **LookupsController** | `GET/POST /api/lookups/categories` and `GET/POST /api/lookups/unit-types`. Replaces the removed `/categories` endpoint. |
| **PurchaseService** | Auto-creates a new `Medicine` if `ProductId` lookup returns null. Every purchase creates a new `MedicineBatch`. Old batches are never overwritten. |
| **SaleService (FEFO)** | `GetAvailableBatchesAsync` returns batches for a product/branch ordered by `ExpiryDate` ascending (earliest first). Continues to the next batch when the current one empties. |
| **Manual batch selection** | `Settings.BatchSelectionMode` (`AutomaticFefo` | `ManualSelection`). When manual, `IsManualBatchMode` is true and the POS shows all available batches for pharmacist selection. |
| **Barcode** | `MedicineService.GetAllAsync` and `SearchAsync` include `Barcode`. Purchase auto-locates medicine by barcode. |
| **POS search response** | `MedicineSearchResponse` includes `BrandName`, `GenericName`, `Strength`, `DosageForm`, `Barcode`, `TotalStock`, `SellingPrice`. |
| **Reports** | `ReportService` groups by `ProductId`/`ProductCode`. |
| **EF Migration** | `20260802202529_RefactorMedicineProductModel` — schema refactor with all new columns, renamed keys, removed `CosmeticCategories` table, added `BatchSelectionMode` to `Settings`. |

### Backend — Data Migration Logic (this turn)
| File | Purpose |
|------|---------|
| `backend/src/MilkiDrugStore.Api/CatalogMigrator.cs` | On startup, scans existing `Categories`/`UnitTypes` rows. If a row's name matches a built-in catalog entry, all `Medicines` referencing it are re-pointed to the built-in negative ID and the custom row is removed. Unknown names are preserved as custom. |
| `Program.cs` | `CatalogMigrator.MigrateAsync` is called after `DbSeeder.SeedAsync`. |

### Backend — Test Fixes (this turn)
| File | Fixes |
|------|-------|
| `FEFOTests.cs`, `SaleServiceTests.cs`, `SaleServiceReferenceNumberTests.cs` | Added `Mock<IRepository<Settings>> _settingsRepo`; inserted `_settingsRepo.Object` into `SaleService` constructor call (ctor now requires 9 args including `IRepository<Settings>`). |
| `PurchaseServiceTests.cs` | Replaced `_categoryRepo`/`_unitTypeRepo` mocks with `_catalog` (`Mock<ICatalogService>`) to match new `PurchaseService` ctor signature. |
| `DatabaseIntegrationTests.cs` | `Category.UnitTypeId` removed (entity no longer has it). `Medicine.CreatedAt` → `CreatedDate`. `MedicineBatch` now requires `BranchId`. `Purchase` requires `BranchId`. `Sale` requires `BranchId`. Removed computed `RemainingQuantity` from EF query (use arithmetic). Fixed `Migrations_Should_Create_All_Tables` assertion to use `CountAsync() >= 0` (tables may be empty). Seeded a test User in fixture so `UserId=1` FK resolves. |
| `IntegrationTests.cs` | Removed `ICosmeticCategoryService` registration. Replaced `CosmeticCategory_Should_Be_Persisted` with `Category_Should_Be_Persisted` (new shared `Category` entity). Changed `CosmeticCategoryId = 1` → `CategoryId = 1`. |

### Frontend — Category / Unit-Type Wiring (this turn)
| File | Change |
|------|--------|
| `frontend/src/types/index.ts` | Added `CatalogOption` interface (`id`, `name`, `isBuiltIn`). |
| `frontend/src/services/api.ts` | Added `CatalogOptionDto` + `CreateCatalogOptionRequest`. Updated `MedicineResponse` to match backend (`productId`, `productCode`, `brandName`, `genericName`, `strength`, `dosageForm`, `barcode`, `categoryName`, `unitTypeName`, `purchasePrice`, `sellingPrice`, `reorderLevel`, `totalStock`, `createdDate`, `updatedDate`). Updated `BatchResponse` (`productId`, `branchId`, `remainingQuantity`, `supplierId`, `supplierName`). |
| `frontend/src/store/appStore.ts` | `fetchCategories` → `GET /lookups/categories`. `fetchUnitTypes` → `GET /lookups/unit-types`. Added `createCustomCategory`/`createCustomUnitType` (POST to lookups). Removed dead `toCategory`/`toUnitType` mappers. Fixed `toMedicine` to map new field names (`productId`, `reorderLevel`, `remainingQuantity`, `createdDate`). Fixed `fetchMedicines` (was accidentally hitting `/suppliers`). `addMedicine`/`updateMedicine` now send `newCategoryName`/`newUnitTypeName` for "Other" entries. |
| `frontend/src/pages/MedicinesPage.tsx` | Added search inputs above category and unit-type dropdowns (filter options client-side). "Other" flow calls `createCustomCategory`/`createCustomUnitType` then re-fetches catalog. Handle save/update payloads now pass `categoryId`/`newCategoryName`/`unitTypeId`/`newUnitTypeName`. |

## 2. Sequence Diagrams

### 2.1 Medicine Registration with Category Resolution
```
User → MedicinesPage: Click "Add Medicine"
MedicinesPage → appStore: fetchCategories(), fetchUnitTypes()
appStore → API: GET /api/lookups/categories
API → CatalogService: GetCategoryOptions()
CatalogService → DB: Categories (custom only)
CatalogService → MedicineCatalog: built-in Categories
API ← CatalogService: merged list
appStore ← API: CatalogOption[]
 MedicinesPage ← appStore: render dropdowns

User selects "Pain Relief" + "Tablet"
User clicks "Register Medicine"
MedicinesPage → appStore: addMedicine({brandName, genericName, categoryId:-1, unitTypeId:-1, ...})
appStore → API: POST /api/medicines
API → MedicineService: CreateAsync(request)
MedicineService → CatalogService: ResolveCategoryIdAsync("Pain Relief") → -1 (built-in)
MedicineService → CatalogService: ResolveUnitTypeIdAsync("Tablet") → -1 (built-in)
MedicineService → DB: INSERT Medicine (CategoryId=-1, UnitTypeId=-1)
MedicineService → DB: INSERT MedicineBatch (new batch if purchase data provided)
API ← MedicineService: MedicineResponse
appStore ← API: ok
MedicinesPage ← appStore: refetch medicines
```

### 2.2 "Other" Category Creation Flow
```
User selects "Other..." in category dropdown
MedicinesPage → UI: show custom category input
User types "Pediatric" + selects unit type
User clicks "Register Medicine"
MedicinesPage → appStore: addMedicine({..., newCategoryName:"Pediatric"})
appStore → API: POST /api/medicines { categoryId:0, newCategoryName:"Pediatric", ... }
API → MedicineService: CreateAsync(request)
MedicineService → CatalogService: ResolveCategoryIdAsync("Pediatric")
  → not found in built-ins
  → CreateCustomCategory("Pediatric") → INSERT Category → returns new positive ID
MedicineService → DB: INSERT Medicine (CategoryId=newPositiveId)
MedicineService → DB: INSERT MedicineBatch
API ← MedicineService: MedicineResponse
appStore ← API: ok
appStore → API: GET /api/lookups/categories (refetch)
 MedicinesPage ← appStore: updated dropdowns
```

### 2.3 Purchase — Auto-Create Medicine + New Batch
```
User → PurchasesPage: add purchase item (scan barcode "MED-000001")
PurchasesPage → appStore: addPurchase(items)
appStore → API: POST /api/purchases { supplierId, items:[{productId, batchNumber, quantity, ...}] }
API → PurchaseService: CreateAsync(request)
PurchaseService → MedicineService: GetByIdAsync(productId)
  → null (new product)
PurchaseService → MedicineService: CreateAsync(newMedicine { ProductCode, BrandName, CategoryId, ... })
MedicineService → CatalogService: ResolveCategoryIdAsync/CreateCustom...
MedicineService → DB: INSERT Medicine
PurchaseService → DB: INSERT Purchase
PurchaseService → DB: INSERT PurchaseItem
PurchaseService → DB: INSERT MedicineBatch (new batch, never overwrite)
API ← PurchaseService: PurchaseResponse
appStore ← API: ok
```

### 2.4 FEFO Sale Flow
```
User → POSPage: select medicine, quantity=2
POSPage → appStore: addSale(items)
appStore → API: POST /api/sales { items:[{productId, quantity}] }
API → SaleService: CreateAsync(request)
SaleService → GetAvailableBatchesAsync(productId, branchId)
  → WHERE ProductId=X AND BranchId=Y AND RemainingQuantity>0
  → ORDER BY ExpiryDate ASC
  → returns batch with earliest expiry
SaleService → DB: UPDATE MedicineBatch SET QuantityIssued += 2
SaleService → DB: INSERT Sale, SaleItem, InventoryTransaction
API ← SaleService: SaleResponse
appStore ← API: ok
```

### 2.5 Startup Data Migration
```
Program starts
Program → DbSeeder.SeedAsync() — seed Roles, Admin, Branches, Settings
Program → CatalogMigrator.MigrateAsync()
  FOR EACH Category row:
    IF name matches built-in → UPDATE Medicines referencing it → DELETE custom Category row
  FOR EACH UnitType row:
    IF name matches built-in → UPDATE Medicines referencing it → DELETE custom UnitType row
Program → App starts listening
```

## 3. Architectural Changes Explained

### Why Built-In Catalog Constants Instead of DB Seeding
Built-in categories and unit types (Antibiotics, Tablet, Capsule, …) are defined as **code constants** in `MedicineCatalog` with negative IDs. This guarantees:
- Zero seed-time DB writes for built-ins.
- No risk of a DB admin accidentally deleting a built-in.
- Fast, deterministic resolution via in-memory `FindCategoryId`/`FindUnitTypeId`.
- Custom entries use positive auto-increment IDs — never collides with built-ins.

### Why CatalogService Resolves by Name
The UI sends `NewCategoryName`/`NewUnitTypeName` (free text from the "Other" input). The service layer resolves the name to a stable integer ID before persisting the `Medicine`. This keeps the `Medicine` FK clean and lets the same service be reused by Purchase, Medicine Registration, and future import tools.

### Why MedicineBatch Uses Computed `RemainingQuantity`
`RemainingQuantity` is a computed property (`QuantityReceived - QuantityIssued - QuantityDamaged - QuantityExpired`), not a stored column. This eliminates stale-state bugs where a developer updates `QuantityIssued` but forgets to also update `RemainingQuantity`. EF Core maps computed properties as read-only.

### Why Category and UnitType Tables Hold Only Custom Rows
The old model treated `Category` and `UnitType` as the source of truth for ALL values, including built-ins. After the refactor they hold only user-defined custom values. Built-ins are immutable code constants. This simplifies:
- Seeding (no built-in seed data).
- Migrations (no need to protect built-in rows from deletion).
- UI dropdowns (merged list from `CatalogService.GetCategoryOptions`).

### Clean Architecture / SOLID Compliance
- **SRP**: `CatalogService` owns all name→ID resolution. `MedicineService` owns medicine CRUD. `PurchaseService` owns purchase orchestration. `SaleService` owns sale + FEFO logic.
- **OCP**: Adding a new built-in category or unit type requires only adding a line to `MedicineCatalog`. No service or controller changes.
- **DIP**: Services depend on `ICatalogService` interface, not a concrete class. Tests mock `ICatalogService` (see `PurchaseServiceTests`).
- **Separation of Concerns**: `MedicineCatalog` is pure domain constants. `CatalogService` is application-layer orchestration. `LookupsController` is infrastructure (HTTP). The database only stores custom rows.

## 4. Remaining Work (Future Tasks)

| Item | Notes |
|------|-------|
| **GET /api/inventory** endpoint | Not yet implemented. Inventory is reachable via `GET /api/medicines` (includes batches + `TotalStock`). |
| **POS / Purchase / Inventory pages alignment** | These pages still reference old medicine field names (`medicineId`, `lowStockThreshold`, `balance`). A follow-up frontend task should align them to the new `MedicineResponse` DTO. |
| **Reports product-code grouping verification** | Backend `ReportService` has `ProductCode` fields; grouping logic should be verified end-to-end for all report types. |
| **Production data migration** | `CatalogMigrator` handles category/unit-type migration. A separate one-time script should backfill `ProductCode` (auto-generate from `ProductId` prefix) and other new fields for any existing medicines created before the migration. |
