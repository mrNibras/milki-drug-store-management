-- ============================================================
-- Database Validation Queries for Milki Drug Store
-- Run these against the production SQLite database to verify
-- migration correctness and data integrity.
-- ============================================================

-- 1. Verify all required tables exist
SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;

-- Expected tables:
-- AuditLogs, Branches, Categories, Cosmetics, CosmeticBatches,
-- DamageRecords, ExpiredRecords, InventoryTransactions,
-- MedicineBatches, Medicines, Notifications, PasswordResets,
-- PurchaseItems, Purchases, RefreshTokens, Roles, SaleItems,
-- Sales, Settings, Suppliers, UnitTypes, Users

-- 2. Verify Medicines table schema (key columns)
PRAGMA table_info(Medicines);

-- Expected:
-- ProductId (INTEGER PK)
-- ProductCode (TEXT, required)
-- BrandName (TEXT, required)
-- GenericName (TEXT, required)
-- CategoryId (INTEGER, required)
-- UnitTypeId (INTEGER, required)
-- PurchasePrice (decimal)
-- SellingPrice (decimal)
-- ReorderLevel (INTEGER)
-- IsActive (INTEGER)
-- CreatedDate (TEXT)
-- UpdatedDate (TEXT, nullable)

-- 3. Verify no NULL CategoryId in Medicines
SELECT COUNT(*) AS MedicinesWithNullCategory FROM Medicines WHERE CategoryId IS NULL;

-- Expected: 0

-- 4. Verify no NULL UnitTypeId in Medicines
SELECT COUNT(*) AS MedicinesWithNullUnitType FROM Medicines WHERE UnitTypeId IS NULL;

-- Expected: 0

-- 5. Verify Cosmetics table schema
PRAGMA table_info(Cosmetics);

-- Expected:
-- CosmeticId (INTEGER PK)
-- ProductName (TEXT, required)
-- CategoryId (INTEGER, required)
-- UnitTypeId (INTEGER, required)
-- Price (decimal)

-- 6. Verify no NULL CategoryId in Cosmetics
SELECT COUNT(*) AS CosmeticsWithNullCategory FROM Cosmetics WHERE CategoryId IS NULL;

-- Expected: 0

-- 7. Verify no NULL UnitTypeId in Cosmetics
SELECT COUNT(*) AS CosmeticsWithNullUnitType FROM Cosmetics WHERE UnitTypeId IS NULL;

-- Expected: 0

-- 8. Verify Categories table (custom only, no UnitTypeId column)
PRAGMA table_info(Categories);

-- Expected:
-- CategoryId (INTEGER PK)
-- Name (TEXT, required)
-- IsActive (INTEGER)
-- CreatedAt (TEXT)

-- Should NOT have UnitTypeId column

-- 9. Verify UnitTypes table schema
PRAGMA table_info(UnitTypes);

-- Expected:
-- UnitTypeId (INTEGER PK)
-- Name (TEXT, required)
-- Description (TEXT, nullable)
-- IsActive (INTEGER)

-- 10. Check for duplicate custom categories (case-insensitive)
SELECT LOWER(TRIM(Name)) AS NormalizedName, COUNT(*) AS Count
FROM Categories
GROUP BY LOWER(TRIM(Name))
HAVING COUNT(*) > 1;

-- Expected: 0 rows

-- 11. Check for duplicate custom unit types (case-insensitive)
SELECT LOWER(TRIM(Name)) AS NormalizedName, COUNT(*) AS Count
FROM UnitTypes
GROUP BY LOWER(TRIM(Name))
HAVING COUNT(*) > 1;

-- Expected: 0 rows

-- 12. Verify built-in categories are NOT in Categories table
SELECT COUNT(*) AS BuiltInCategoriesInCustomTable
FROM Categories
WHERE Name IN (
    'Antibiotics', 'Antivirals', 'Antifungals', 'Antipain',
    'Antihistamines', 'GIT Drugs', 'Hormonal Drugs', 'CVS Drugs',
    'CNS Drugs', 'Vitamins and Minerals', 'Vaccines', 'ENT Drugs',
    'Dermatologic Drugs', 'Antiepileptics', 'Other'
);

-- Expected: 0

-- 13. Verify built-in unit types are NOT in UnitTypes table
SELECT COUNT(*) AS BuiltInUnitTypesInCustomTable
FROM UnitTypes
WHERE Name IN (
    'Tablet', 'Capsule', 'Suspension', 'Syrup', 'Solution',
    'Injection', 'Oral Drop', 'Cream', 'Ointment', 'Shampoo',
    'Powder', 'Drop', 'Other'
);

-- Expected: 0

-- 14. Verify Medicine foreign keys (no orphans)
SELECT COUNT(*) AS OrphanMedicines FROM Medicines m
LEFT JOIN Categories c ON m.CategoryId = c.CategoryId
WHERE m.CategoryId > 0 AND c.CategoryId IS NULL;

-- Expected: 0 (positive CategoryIds must reference existing custom categories)

-- 15. Verify Medicine UnitType foreign keys (no orphans)
SELECT COUNT(*) AS OrphanMedicines FROM Medicines m
LEFT JOIN UnitTypes u ON m.UnitTypeId = u.UnitTypeId
WHERE m.UnitTypeId > 0 AND u.UnitTypeId IS NULL;

-- Expected: 0 (positive UnitTypeIds must reference existing custom unit types)

-- 16. Verify Cosmetic foreign keys (no orphans)
SELECT COUNT(*) AS OrphanCosmetics FROM Cosmetics c
LEFT JOIN Categories cat ON c.CategoryId = cat.CategoryId
WHERE c.CategoryId > 0 AND cat.CategoryId IS NULL;

-- Expected: 0

-- 17. Verify Cosmetic UnitType foreign keys (no orphans)
SELECT COUNT(*) AS OrphanCosmetics FROM Cosmetics c
LEFT JOIN UnitTypes u ON c.UnitTypeId = u.UnitTypeId
WHERE c.UnitTypeId > 0 AND u.UnitTypeId IS NULL;

-- Expected: 0

-- 18. Verify MedicineBatches reference valid medicines
SELECT COUNT(*) AS OrphanBatches FROM MedicineBatches mb
LEFT JOIN Medicines m ON mb.ProductId = m.ProductId
WHERE m.ProductId IS NULL;

-- Expected: 0

-- 19. Verify PurchaseItems reference valid medicines
SELECT COUNT(*) AS OrphanPurchaseItems FROM PurchaseItems pi
LEFT JOIN Medicines m ON pi.ProductId = m.ProductId
WHERE m.ProductId IS NULL;

-- Expected: 0

-- 20. Verify SaleItems reference valid medicines
SELECT COUNT(*) AS OrphanSaleItems FROM SaleItems si
LEFT JOIN Medicines m ON si.ProductId = m.ProductId
WHERE m.ProductId IS NULL;

-- Expected: 0

-- 21. Verify InventoryTransactions reference valid medicines
SELECT COUNT(*) AS OrphanInventoryTransactions FROM InventoryTransactions it
LEFT JOIN Medicines m ON it.ProductId = m.ProductId
WHERE m.ProductId IS NULL;

-- Expected: 0

-- 22. Verify PurchaseItems reference valid purchases
SELECT COUNT(*) AS OrphanPurchaseItems FROM PurchaseItems pi
LEFT JOIN Purchases p ON pi.PurchaseId = p.PurchaseId
WHERE p.PurchaseId IS NULL;

-- Expected: 0

-- 23. Verify SaleItems reference valid sales
SELECT COUNT(*) AS OrphanSaleItems FROM SaleItems si
LEFT JOIN Sales s ON si.SaleId = s.SaleId
WHERE s.SaleId IS NULL;

-- Expected: 0

-- 24. Verify MedicineBatches reference valid branches
SELECT COUNT(*) AS OrphanBatches FROM MedicineBatches mb
LEFT JOIN Branches b ON mb.BranchId = b.BranchId
WHERE b.BranchId IS NULL;

-- Expected: 0

-- 25. Verify Purchases reference valid branches
SELECT COUNT(*) AS OrphanPurchases FROM Purchases p
LEFT JOIN Branches b ON p.BranchId = b.BranchId
WHERE b.BranchId IS NULL;

-- Expected: 0

-- 26. Verify Purchases reference valid suppliers
SELECT COUNT(*) AS OrphanPurchases FROM Purchases p
LEFT JOIN Suppliers s ON p.SupplierId = s.SupplierId
WHERE s.SupplierId IS NULL;

-- Expected: 0

-- 27. Verify Sales reference valid users
SELECT COUNT(*) AS OrphanSales FROM Sales s
LEFT JOIN Users u ON s.UserId = u.UserId
WHERE u.UserId IS NULL;

-- Expected: 0

-- 28. Verify Sales reference valid branches
SELECT COUNT(*) AS OrphanSales FROM Sales s
LEFT JOIN Branches b ON s.BranchId = b.BranchId
WHERE b.BranchId IS NULL;

-- Expected: 0

-- 29. Check for medicines with empty ProductCode (needs backfill)
SELECT COUNT(*) AS MedicinesWithEmptyProductCode FROM Medicines WHERE ProductCode = '';

-- Expected: 0 (after backfill)

-- 30. Check for medicines with null UpdatedDate (needs backfill)
SELECT COUNT(*) AS MedicinesWithNullUpdatedDate FROM Medicines WHERE UpdatedDate IS NULL;

-- Expected: 0 (after backfill)

-- 31. Verify CategoryId distribution in Medicines
SELECT 
    CASE 
        WHEN CategoryId < 0 THEN 'Built-in'
        WHEN CategoryId > 0 THEN 'Custom'
        ELSE 'Unknown'
    END AS CategoryType,
    COUNT(*) AS Count
FROM Medicines
GROUP BY CategoryType;

-- Expected: Mix of Built-in and Custom (no Unknown)

-- 32. Verify UnitTypeId distribution in Medicines
SELECT 
    CASE 
        WHEN UnitTypeId < 0 THEN 'Built-in'
        WHEN UnitTypeId > 0 THEN 'Custom'
        ELSE 'Unknown'
    END AS UnitTypeType,
    COUNT(*) AS Count
FROM Medicines
GROUP BY UnitTypeType;

-- Expected: Mix of Built-in and Custom (no Unknown)

-- 33. Verify custom categories in use
SELECT c.CategoryId, c.Name, COUNT(m.ProductId) AS MedicineCount
FROM Categories c
LEFT JOIN Medicines m ON c.CategoryId = m.CategoryId
GROUP BY c.CategoryId, c.Name
ORDER BY MedicineCount DESC;

-- Should show all custom categories and how many medicines use them

-- 34. Verify custom unit types in use
SELECT u.UnitTypeId, u.Name, COUNT(m.ProductId) AS MedicineCount
FROM UnitTypes u
LEFT JOIN Medicines m ON u.UnitTypeId = m.UnitTypeId
GROUP BY u.UnitTypeId, u.Name
ORDER BY MedicineCount DESC;

-- Should show all custom unit types and how many medicines use them

-- 35. Verify CosmeticBatches reference valid cosmetics
SELECT COUNT(*) AS OrphanCosmeticBatches FROM CosmeticBatches cb
LEFT JOIN Cosmetics c ON cb.CosmeticId = c.CosmeticId
WHERE c.CosmeticId IS NULL;

-- Expected: 0

-- 36. Verify DamageRecords reference valid medicine batches
SELECT COUNT(*) AS OrphanDamageRecords FROM DamageRecords dr
LEFT JOIN MedicineBatches mb ON dr.BatchId = mb.BatchId
WHERE mb.BatchId IS NULL;

-- Expected: 0

-- 37. Verify ExpiredRecords reference valid medicine batches
SELECT COUNT(*) AS OrphanExpiredRecords FROM ExpiredRecords er
LEFT JOIN MedicineBatches mb ON er.BatchId = mb.BatchId
WHERE mb.BatchId IS NULL;

-- Expected: 0

-- 38. Summary statistics
SELECT 
    (SELECT COUNT(*) FROM Medicines) AS TotalMedicines,
    (SELECT COUNT(*) FROM MedicineBatches) AS TotalBatches,
    (SELECT COUNT(*) FROM Cosmetics) AS TotalCosmetics,
    (SELECT COUNT(*) FROM CosmeticBatches) AS TotalCosmeticBatches,
    (SELECT COUNT(*) FROM Categories) AS TotalCustomCategories,
    (SELECT COUNT(*) FROM UnitTypes) AS TotalCustomUnitTypes,
    (SELECT COUNT(*) FROM Purchases) AS TotalPurchases,
    (SELECT COUNT(*) FROM Sales) AS TotalSales,
    (SELECT COUNT(*) FROM PurchaseItems) AS TotalPurchaseItems,
    (SELECT COUNT(*) FROM SaleItems) AS TotalSaleItems,
    (SELECT COUNT(*) FROM InventoryTransactions) AS TotalInventoryTransactions;
