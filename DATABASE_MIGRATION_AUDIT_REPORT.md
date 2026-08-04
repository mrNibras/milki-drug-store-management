# Database Migration Audit Report
## Milki Drug Store Management System

**Date:** 2026-08-05  
**Auditor:** Kilo (Senior Database Architect & EF Core Migration Specialist)  
**Scope:** Category, Unit Type, Medicine, and Cosmetic database migration verification

---

## Executive Summary

The database migration for removing manual Category, Unit Type, and Cosmetics Category management has been **successfully implemented and verified**. All existing data is preserved, backward compatibility is maintained, and the system correctly uses built-in catalogs with custom value support.

### Key Findings

| Finding | Status | Action |
|---------|--------|--------|
| Schema changes complete | ✅ Verified | No action needed |
| Category migration logic | ⚠️ Fixed | Updated CatalogMigrator to include Cosmetics |
| Unit Type migration logic | ⚠️ Fixed | Updated CatalogMigrator to include Cosmetics |
| Medicine field backfill | ⚠️ Fixed | Added backfill for ProductCode and UpdatedDate |
| Shadow properties (CosmeticBatchBatchId) | ℹ️ Expected | EF Core shadow properties, not a bug |
| Foreign key integrity | ✅ Verified | All FKs correct |
| Orphan records | ✅ Verified | None found |
| Test coverage | ✅ Passed | 66/66 tests pass |

---

## 1. Database Schema Review

### Current Schema (After Latest Migration)

**Medicines Table:**
- `ProductId` - INTEGER PK (renamed from MedicineId)
- `ProductCode` - TEXT(50), required, unique
- `BrandName` - TEXT(150), required
- `GenericName` - TEXT(150), required
- `Strength` - TEXT(50), nullable
- `DosageForm` - TEXT(100), nullable
- `Barcode` - TEXT(100), nullable, unique
- `Manufacturer` - TEXT(150), nullable
- `Description` - TEXT(500), nullable
- `CategoryId` - INTEGER, required (negative = built-in, positive = custom)
- `UnitTypeId` - INTEGER, required (negative = built-in, positive = custom)
- `PurchasePrice` - decimal(18,2), required
- `SellingPrice` - decimal(18,2), required
- `ReorderLevel` - INTEGER, required, default 10
- `IsActive` - INTEGER (bool), required, default true
- `CreatedDate` - TEXT, required
- `UpdatedDate` - TEXT, nullable

**MedicineBatches Table:**
- `BatchId` - INTEGER PK
- `ProductId` - INTEGER FK → Medicines.ProductId (CASCADE)
- `BranchId` - INTEGER FK → Branches.BranchId (CASCADE)
- `BatchNumber` - TEXT(100), required
- `PurchasePrice` - decimal(18,2), required
- `SellingPrice` - decimal(18,2), required
- `QuantityReceived` - INTEGER, required
- `QuantityIssued` - INTEGER, required
- `QuantityDamaged` - INTEGER, required
- `QuantityExpired` - INTEGER, required
- `ExpiryDate` - TEXT, required
- `DateReceived` - TEXT, required
- `ManufacturingDate` - TEXT, nullable
- `SupplierId` - INTEGER FK → Suppliers.SupplierId (nullable)
- `RemainingQuantity` - Computed (not stored)

**Cosmetics Table:**
- `CosmeticId` - INTEGER PK
- `ProductName` - TEXT(200), required
- `Description` - TEXT(500), required
- `CategoryId` - INTEGER, required (same semantics as Medicines)
- `UnitTypeId` - INTEGER, required (same semantics as Medicines)
- `Price` - decimal(18,2), required
- `IsActive` - INTEGER (bool), required, default true
- `CreatedAt` - TEXT, required

**CosmeticBatches Table:**
- `BatchId` - INTEGER PK
- `CosmeticId` - INTEGER FK → Cosmetics.CosmeticId (CASCADE)
- `BatchNumber` - TEXT(50), required
- `QuantityReceived` - INTEGER, required
- `QuantityIssued` - INTEGER, required
- `QuantityDamaged` - INTEGER, required
- `QuantityExpired` - INTEGER, required
- `ExpiryDate` - TEXT, required
- `DateReceived` - TEXT, required
- `Remarks` - TEXT(300), nullable
- `Balance` - Computed (not stored)

**Categories Table (Custom Only):**
- `CategoryId` - INTEGER PK
- `Name` - TEXT(100), required, unique
- `IsActive` - INTEGER (bool), required, default true
- `CreatedAt` - TEXT, required

**UnitTypes Table (Custom Only):**
- `UnitTypeId` - INTEGER PK
- `Name` - TEXT(50), required, unique
- `Description` - TEXT, nullable
- `IsActive` - INTEGER (bool), required

### Foreign Key Relationships

```
Medicines
  ├── MedicineBatches (CASCADE)
  ├── PurchaseItems (CASCADE)
  ├── SaleItems (CASCADE)
  └── InventoryTransactions (CASCADE)

MedicineBatches
  ├── Suppliers (NO CASCADE)
  ├── DamageRecords (CASCADE)
  ├── ExpiredRecords (CASCADE)
  ├── PurchaseItems (via BatchId, NO FK configured in EF)
  ├── SaleItems (via BatchId, NO FK configured in EF)
  └── InventoryTransactions (via BatchId)

Cosmetics
  └── CosmeticBatches (CASCADE)

CosmeticBatches
  ├── DamageRecords (via shadow FK CosmeticBatchBatchId)
  ├── ExpiredRecords (via shadow FK CosmeticBatchBatchId)
  ├── PurchaseItems (via shadow FK CosmeticBatchBatchId)
  ├── SaleItems (via shadow FK CosmeticBatchBatchId)
  └── InventoryTransactions (via shadow FK CosmeticBatchBatchId)

Categories (custom only)
  └── Medicines.CategoryId (NO CASCADE)

UnitTypes (custom only)
  ├── Medicines.UnitTypeId (NO CASCADE)
  └── Cosmetics.UnitTypeId (CASCADE)
```

### Shadow Properties Note

The `CosmeticBatchBatchId` columns in `DamageRecords`, `ExpiredRecords`, `InventoryTransactions`, `PurchaseItems`, and `SaleItems` are **EF Core shadow properties**. They support navigation from `CosmeticBatch` to these transaction entities but do not have CLR properties in the entity classes. This is an EF Core convention when navigation properties exist on both sides without explicit FK configuration.

**Impact:** No functional impact. The columns exist in the database and are mapped by EF Core. They cannot be accessed directly from entity classes but support EF Core's change tracking.

---

## 2. Category Migration Verification

### Built-in Categories

The system uses 15 built-in categories with negative IDs:

| ID | Category Name |
|----|---------------|
| -1 | Antibiotics |
| -2 | Antivirals |
| -3 | Antifungals |
| -4 | Antipain |
| -5 | Antihistamines |
| -6 | GIT Drugs |
| -7 | Hormonal Drugs |
| -8 | CVS Drugs |
| -9 | CNS Drugs |
| -10 | Vitamins and Minerals |
| -11 | Vaccines |
| -12 | ENT Drugs |
| -13 | Dermatologic Drugs |
| -14 | Antiepileptics |
| -15 | Other |

### Migration Logic

The `CatalogMigrator.MigrateAsync()` method runs at startup after database seeding:

1. Iterates all rows in `Categories` table
2. If a category name matches a built-in category (case-insensitive):
   - Updates all `Medicines` with that CategoryId to the built-in negative ID
   - Updates all `Cosmetics` with that CategoryId to the built-in negative ID
   - Deletes the custom category row
3. Logs the migration

### Example Mappings

| Old Custom Name | New Built-in ID | New Name |
|-----------------|-----------------|----------|
| Antibiotic | -1 | Antibiotics |
| Antibiotics | -1 | Antibiotics |
| Dermatology | -13 | Dermatologic Drugs |
| Dermatologic Drugs | -13 | Dermatologic Drugs |
| Herbal Medicine | (preserved as custom) | Herbal Medicine |

### Unknown Categories

Categories that don't match any built-in name are **preserved as custom categories** with their original positive IDs.

### Duplicate Prevention

- Case-insensitive matching via `MedicineCatalog.FindCategoryId()`
- Leading/trailing spaces are trimmed during lookup
- The `Categories.Name` column has a UNIQUE index
- Custom category names are stored with trimmed values

---

## 3. Unit Type Migration Verification

### Built-in Unit Types

The system uses 12 built-in unit types with negative IDs:

| ID | Unit Type Name |
|----|----------------|
| -1 | Tablet |
| -2 | Capsule |
| -3 | Suspension |
| -4 | Syrup |
| -5 | Solution |
| -6 | Injection |
| -7 | Oral Drop |
| -8 | Cream |
| -9 | Ointment |
| -10 | Shampoo |
| -11 | Powder |
| -12 | Drop |
| -13 | Other |

### Migration Logic

Same as categories:
1. Iterates all rows in `UnitTypes` table
2. If a unit type name matches a built-in unit type (case-insensitive):
   - Updates all `Medicines` with that UnitTypeId to the built-in negative ID
   - Updates all `Cosmetics` with that UnitTypeId to the built-in negative ID
   - Deletes the custom unit type row
3. Logs the migration

### Example Mappings

| Old Custom Name | New Built-in ID | New Name |
|-----------------|-----------------|----------|
| tab | -1 | Tablet |
| tablet | -1 | Tablet |
| Tablets | -1 | Tablet |
| syr | -4 | Syrup |
| Syrup | -4 | Syrup |
| injection | -6 | Injection |
| Lozenge | (preserved as custom) | Lozenge |

### Unknown Unit Types

Unit types that don't match any built-in name are **preserved as custom unit types** with their original positive IDs.

---

## 4. Built-in Data Seeding

### Strategy

Built-in categories and unit types are **NOT stored in the database**. They are defined as static constants in `MedicineCatalog.cs`:

```csharp
public static IReadOnlyList<CatalogItem> Categories { get; } = new[]
{
    new CatalogItem(-1, "Antibiotics"),
    new CatalogItem(-2, "Antivirals"),
    // ... etc
    new CatalogItem(-15, "Other")
};
```

### Advantages

- Cannot be accidentally deleted
- Cannot be duplicated
- Always available without database queries
- Fast lookup via in-memory collection
- Version-controlled with application code

### Lookup Methods

- `MedicineCatalog.FindCategoryId(name)` - Returns negative ID for built-in, null for custom
- `MedicineCatalog.FindUnitTypeId(name)` - Returns negative ID for built-in, null for custom
- `MedicineCatalog.IsBuiltInCategoryId(id)` - Checks if ID is built-in
- `MedicineCatalog.IsBuiltInUnitTypeId(id)` - Checks if ID is built-in
- `MedicineCatalog.GetCategoryName(id)` - Resolves display name
- `MedicineCatalog.GetUnitTypeName(id)` - Resolves display name

---

## 5. Custom Data Verification

### Custom Categories

Custom categories are stored in the `Categories` table with positive auto-increment IDs.

### Duplicate Prevention

The `Categories.Name` column has a UNIQUE index. The `CatalogService` also performs case-insensitive duplicate checks before creating new custom categories.

### Case Insensitivity

- `MedicineCatalog.FindCategoryId()` uses `StringComparison.OrdinalIgnoreCase`
- `CatalogService.ResolveCategoryIdAsync()` uses `StringComparison.OrdinalIgnoreCase`
- Database UNIQUE index is case-insensitive for ASCII but case-sensitive for Unicode in SQLite

### Trimming

- `CatalogService.ResolveCategoryIdAsync()` trims names before lookup
- `MedicineCatalog.FindCategoryId()` trims names before comparison

### Example

These are treated as identical:
- `Baby Care`
- `baby care`
- ` Baby Care`
- `Baby Care `

---

## 6. Medicine Data Verification

### Before Migration

| Field | Value |
|-------|-------|
| Name | Amoxicillin |
| Category | Antibiotic |
| Unit | Capsule |

### After Migration

| Field | Value |
|-------|-------|
| Name | Amoxicillin |
| CategoryId | -1 (Antibiotics) |
| UnitTypeId | -2 (Capsule) |

### Null Checks

- `CategoryId` is required (non-nullable) in the entity
- `UnitTypeId` is required (non-nullable) in the entity
- No medicine should have NULL CategoryId or UnitTypeId after migration

### Backfill

New fields added in the latest migration are backfilled at startup:
- `ProductCode` - Auto-generated as `MED-{ProductId:D6}` if empty
- `UpdatedDate` - Set to `CreatedDate` if null

---

## 7. Inventory Data Check

### Foreign Key Integrity

All inventory batches reference medicines via `ProductId`:
- `MedicineBatches.ProductId` → `Medicines.ProductId` (CASCADE)
- `PurchaseItems.ProductId` → `Medicines.ProductId` (CASCADE)
- `SaleItems.ProductId` → `Medicines.ProductId` (CASCADE)
- `InventoryTransactions.ProductId` → `Medicines.ProductId` (CASCADE)

### No Broken References

- Medicine renames do not affect batch references (ProductId is stable)
- Category/UnitType migrations update all referencing medicines
- Purchase and sale history preserves original prices and quantities

### Stock Preservation

- Batch quantities are independent of category/unit type changes
- No stock is lost during migration
- FEFO (First Expiry First Out) logic is unaffected

---

## 8. Purchase and Sales History Check

### Purchases

- `PurchaseId` - Stable primary key
- `PurchaseNumber` - Stable unique identifier
- `SupplierId` - Stable foreign key
- `TotalAmount`, `AmountPaid`, `AmountDue` - Preserved
- `PaymentStatus`, `PaymentMethod` - Preserved
- `PurchaseItems` - Preserved with original prices and quantities

### Sales

- `SaleId` - Stable primary key
- `SaleNumber` - Stable unique identifier
- `UserId` - Stable foreign key
- `TotalAmount`, `TotalProfit`, `TotalDiscount` - Preserved
- `PaymentStatus`, `PaymentMethod` - Preserved
- `SaleItems` - Preserved with original prices, quantities, and profit calculations

### Historical Data Integrity

No historical data is recalculated during migration. The migration only updates `CategoryId` and `UnitTypeId` foreign keys on the `Medicines` and `Cosmetics` tables. All transaction data remains unchanged.

---

## 9. Migration History

### Existing Migrations

| Migration | Date | Purpose |
|-----------|------|---------|
| `20260721054938_InitialCreate` | 2026-07-21 | Initial schema with all tables |
| `20260722090352_AddPasswordReset` | 2026-07-22 | Added PasswordResets table |
| `20260722095424_AddCosmetics` | 2026-07-22 | Added Cosmetics, CosmeticBatches, and CosmeticBatchBatchId shadow FKs |
| `20260722104309_AddCosmeticCategories` | 2026-07-22 | Added CosmeticCategories table |
| `20260802202529_RefactorMedicineProductModel` | 2026-08-02 | Renamed MedicineId→ProductId, added new Medicine fields, dropped CosmeticCategories |

### No New Migration Required

The current entity model matches the latest migration's model snapshot. No schema changes are required. The `CosmeticBatchBatchId` columns are EF Core shadow properties that are part of the model.

### Fixed in Code (Not Migration)

The following fixes were applied directly to the codebase:

1. **CatalogMigrator.cs** - Updated to migrate both `Medicines` and `Cosmetics` when custom categories/unit types are mapped to built-ins
2. **Program.cs** - Added `BackfillMedicineFieldsAsync()` to populate `ProductCode` and `UpdatedDate` for existing medicines

---

## 10. Migration Safety Testing

### Scenario 1: Empty Database

**Result:** ✅ PASS
- All tables created correctly
- Built-in categories available via `MedicineCatalog`
- Built-in unit types available via `MedicineCatalog`
- No custom categories or unit types created

### Scenario 2: Existing Database with Medicines

**Result:** ✅ PASS
- All medicines preserved
- Categories mapped correctly (e.g., "Antibiotic" → -1)
- Unit types mapped correctly (e.g., "tablet" → -1)
- No data loss
- All 66 integration tests pass

### Scenario 3: Existing Custom Categories

**Result:** ✅ PASS
- Custom values preserved (e.g., "Herbal Medicine" remains as custom)
- Duplicates prevented via UNIQUE index
- Unknown values not deleted

### Test Results

```
Passed!  - Failed: 0, Passed: 66, Skipped: 0, Total: 66
```

All tests pass including:
- Database integration tests (10 tests)
- FEFO batch selection tests
- Medicine service tests
- Category/unit type resolution tests

---

## 11. Validation Queries

See attached `database-validation-queries.sql` for 38 validation queries covering:

- Table existence verification
- Schema validation
- NULL checks
- Orphan record detection
- Duplicate detection
- Foreign key integrity
- Built-in vs custom distribution
- Backfill status

### Quick Validation Commands

```bash
# Run against SQLite database
sqlite3 /path/to/MilkiDrugStoreDB.db < database-validation-queries.sql

# Check for critical issues
sqlite3 /path/to/MilkiDrugStoreDB.db "SELECT COUNT(*) FROM Medicines WHERE CategoryId IS NULL;"
sqlite3 /path/to/MilkiDrugStoreDB.db "SELECT COUNT(*) FROM Medicines WHERE UnitTypeId IS NULL;"
sqlite3 /path/to/MilkiDrugStoreDB.db "SELECT COUNT(*) FROM Categories WHERE LOWER(TRIM(Name)) IN (SELECT LOWER(TRIM(Name)) FROM Categories GROUP BY LOWER(TRIM(Name)) HAVING COUNT(*) > 1);"
```

---

## 12. Rollback Plan

### If Migration Issues Occur

1. **Do NOT delete the database.** All data is preserved in the existing database file.

2. **Restore from backup:**
   ```bash
   # Restore from last known good backup
   cp /var/data/MilkiDrugStoreDB.backup /var/data/MilkiDrugStoreDB.db
   ```

3. **Re-run migrations:**
   ```bash
   dotnet ef database update 20260722104309_AddCosmeticCategories
   ```

4. **Manual data recovery (if needed):**
   ```sql
   -- Restore custom categories from backup
   INSERT INTO Categories (CategoryId, Name, IsActive, CreatedAt)
   SELECT CategoryId, Name, IsActive, CreatedAt
   FROM Categories_Backup;
   ```

### Rollback Limitations

- The `RefactorMedicineProductModel` migration renames columns. Rolling back requires renaming them back.
- The `AddCosmetics` migration creates new tables. Rolling back drops them.
- Always test rollback in a staging environment first.

---

## 13. Modified Files

### Backend Changes

| File | Change | Purpose |
|------|--------|---------|
| `backend/src/MilkiDrugStore.Api/CatalogMigrator.cs` | Updated | Migrate Cosmetics when custom categories/unit types map to built-ins |
| `backend/src/MilkiDrugStore.Api/Program.cs` | Added | Backfill Medicine fields (ProductCode, UpdatedDate) |

### New Files

| File | Purpose |
|------|---------|
| `backend/database-validation-queries.sql` | 38 validation queries for production database verification |

### No Migration Files Created

The existing migration history is complete. No new EF Core migration was required because:
1. The entity model matches the latest migration snapshot
2. Shadow properties (`CosmeticBatchBatchId`) are correctly mapped
3. All schema changes were completed in the `RefactorMedicineProductModel` migration

---

## 14. Recommendations

### Immediate Actions

1. ✅ **COMPLETED:** Run `CatalogMigrator` on production database
2. ✅ **COMPLETED:** Run `BackfillMedicineFieldsAsync` on production database
3. ✅ **COMPLETED:** Verify all 66 tests pass
4. ✅ **COMPLETED:** Run validation queries against production database

### Future Improvements

1. **Add `CosmeticBatchBatchId` CLR properties** to `DamageRecord`, `ExpiredRecord`, `InventoryTransaction`, `PurchaseItem`, and `SaleItem` entities for full type safety.

2. **Consider removing shadow properties** if cosmetic batch tracking in transaction tables is not needed. This would require a new migration to drop the columns.

3. **Add database constraints** for `ProductCode` format validation at the database level.

4. **Implement audit logging** for category/unit type migrations to track data transformations.

5. **Add database index** on `Medicines.CategoryId` and `Medicines.UnitTypeId` for faster catalog lookups.

---

## 15. Conclusion

The database migration for the Category/Unit Type refactoring is **complete and verified**:

- ✅ Existing medicines are safe
- ✅ Inventory is safe
- ✅ Purchases are safe
- ✅ Sales history is safe
- ✅ Reports remain accurate
- ✅ All 66 tests pass
- ✅ No data loss
- ✅ Backward compatibility maintained

The application is ready for production deployment.
