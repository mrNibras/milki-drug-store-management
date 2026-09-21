-- ============================================================
-- Database Validation Queries for Milki Drug Store (PostgreSQL)
-- Run these against the production PostgreSQL database to verify
-- migration correctness and data integrity.
-- ============================================================

-- 1. Verify all required tables exist
SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename;

-- Expected tables:
-- audit_logs, branches, categories, cosmetics, cosmetic_batches,
-- damage_records, expired_records, inventory_transactions,
-- medicine_batches, medicines, notifications, password_resets,
-- purchase_items, purchases, refresh_tokens, roles, sale_items,
-- sales, settings, suppliers, unit_types, users

-- 2. Verify Medicines table schema (key columns)
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'medicines'
ORDER BY ordinal_position;

-- Expected:
-- product_id (integer PK)
-- product_code (text, NOT NULL)
-- brand_name (text, NOT NULL)
-- generic_name (text, NOT NULL)
-- category_id (integer)
-- unit_type_id (integer)
-- purchase_price (numeric(18,2))
-- selling_price (numeric(18,2))
-- reorder_level (integer)
-- is_active (boolean)
-- created_date (timestamp with time zone)
-- updated_date (timestamp with time zone, nullable)

-- 3. Verify no NULL CategoryId in Medicines
SELECT COUNT(*) AS medicines_with_null_category FROM medicines WHERE category_id IS NULL;

-- Expected: 0

-- 4. Verify no NULL UnitTypeId in Medicines
SELECT COUNT(*) AS medicines_with_null_unit_type FROM medicines WHERE unit_type_id IS NULL;

-- Expected: 0

-- 5. Verify Cosmetics table schema
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'cosmetics'
ORDER BY ordinal_position;

-- Expected:
-- cosmetic_id (integer PK)
-- product_name (text, NOT NULL)
-- category_id (integer)
-- unit_type_id (integer)
-- price (numeric(18,2))

-- 6. Verify no NULL CategoryId in Cosmetics
SELECT COUNT(*) AS cosmetics_with_null_category FROM cosmetics WHERE category_id IS NULL;

-- Expected: 0

-- 7. Verify no NULL UnitTypeId in Cosmetics
SELECT COUNT(*) AS cosmetics_with_null_unit_type FROM cosmetics WHERE unit_type_id IS NULL;

-- Expected: 0

-- 8. Verify Categories table (custom only, no UnitTypeId column)
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'categories'
ORDER BY ordinal_position;

-- Expected columns:
-- category_id, name, is_active, created_at

-- Should NOT have unit_type_id column

-- 9. Verify UnitTypes table schema
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'unit_types'
ORDER BY ordinal_position;

-- Expected:
-- unit_type_id (integer PK)
-- name (text, NOT NULL)
-- description (text, nullable)
-- is_active (boolean)

-- 10. Check for duplicate custom categories (case-insensitive)
SELECT LOWER(TRIM(name)) AS normalized_name, COUNT(*) AS count
FROM categories
GROUP BY LOWER(TRIM(name))
HAVING COUNT(*) > 1;

-- Expected: 0 rows

-- 11. Check for duplicate custom unit types (case-insensitive)
SELECT LOWER(TRIM(name)) AS normalized_name, COUNT(*) AS count
FROM unit_types
GROUP BY LOWER(TRIM(name))
HAVING COUNT(*) > 1;

-- Expected: 0 rows

-- 12. Verify built-in categories are NOT in Categories table
SELECT COUNT(*) AS built_in_categories_in_custom_table
FROM categories
WHERE name IN (
    'Antibiotics', 'Antivirals', 'Antifungals', 'Antipain',
    'Antihistamines', 'GIT Drugs', 'Hormonal Drugs', 'CVS Drugs',
    'CNS Drugs', 'Vitamins and Minerals', 'Vaccines', 'ENT Drugs',
    'Dermatologic Drugs', 'Antiepileptics', 'Other'
);

-- Expected: 0

-- 13. Verify built-in unit types are NOT in UnitTypes table
SELECT COUNT(*) AS built_in_unit_types_in_custom_table
FROM unit_types
WHERE name IN (
    'Tablet', 'Capsule', 'Suspension', 'Syrup', 'Solution',
    'Injection', 'Oral Drop', 'Cream', 'Ointment', 'Shampoo',
    'Powder', 'Drop', 'Other'
);

-- Expected: 0

-- 14. Verify Medicine foreign keys (no orphans)
SELECT COUNT(*) AS orphan_medicines FROM medicines m
LEFT JOIN categories c ON m.category_id = c.category_id
WHERE m.category_id > 0 AND c.category_id IS NULL;

-- Expected: 0 (positive CategoryIds must reference existing custom categories)

-- 15. Verify Medicine UnitType foreign keys (no orphans)
SELECT COUNT(*) AS orphan_medicines FROM medicines m
LEFT JOIN unit_types u ON m.unit_type_id = u.unit_type_id
WHERE m.unit_type_id > 0 AND u.unit_type_id IS NULL;

-- Expected: 0 (positive UnitTypeIds must reference existing custom unit types)

-- 16. Verify Cosmetic foreign keys (no orphans)
SELECT COUNT(*) AS orphan_cosmetics FROM cosmetics c
LEFT JOIN categories cat ON c.category_id = cat.category_id
WHERE c.category_id > 0 AND cat.category_id IS NULL;

-- Expected: 0

-- 17. Verify Cosmetic UnitType foreign keys (no orphans)
SELECT COUNT(*) AS orphan_cosmetics FROM cosmetics c
LEFT JOIN unit_types u ON c.unit_type_id = u.unit_type_id
WHERE c.unit_type_id > 0 AND u.unit_type_id IS NULL;

-- Expected: 0

-- 18. Verify MedicineBatches reference valid medicines
SELECT COUNT(*) AS orphan_batches FROM medicine_batches mb
LEFT JOIN medicines m ON mb.product_id = m.product_id
WHERE m.product_id IS NULL;

-- Expected: 0

-- 19. Verify PurchaseItems reference valid medicines
SELECT COUNT(*) AS orphan_purchase_items FROM purchase_items pi
LEFT JOIN medicines m ON pi.product_id = m.product_id
WHERE m.product_id IS NULL;

-- Expected: 0

-- 20. Verify SaleItems reference valid medicines
SELECT COUNT(*) AS orphan_sale_items FROM sale_items si
LEFT JOIN medicines m ON si.product_id = m.product_id
WHERE m.product_id IS NULL;

-- Expected: 0

-- 21. Verify InventoryTransactions reference valid medicines
SELECT COUNT(*) AS orphan_inventory_transactions FROM inventory_transactions it
LEFT JOIN medicines m ON it.product_id = m.product_id
WHERE m.product_id IS NULL;

-- Expected: 0

-- 22. Verify PurchaseItems reference valid purchases
SELECT COUNT(*) AS orphan_purchase_items FROM purchase_items pi
LEFT JOIN purchases p ON pi.purchase_id = p.purchase_id
WHERE p.purchase_id IS NULL;

-- Expected: 0

-- 23. Verify SaleItems reference valid sales
SELECT COUNT(*) AS orphan_sale_items FROM sale_items si
LEFT JOIN sales s ON si.sale_id = s.sale_id
WHERE s.sale_id IS NULL;

-- Expected: 0

-- 24. Verify MedicineBatches reference valid branches
SELECT COUNT(*) AS orphan_batches FROM medicine_batches mb
LEFT JOIN branches b ON mb.branch_id = b.branch_id
WHERE b.branch_id IS NULL;

-- Expected: 0

-- 25. Verify Purchases reference valid branches
SELECT COUNT(*) AS orphan_purchases FROM purchases p
LEFT JOIN branches b ON p.branch_id = b.branch_id
WHERE b.branch_id IS NULL;

-- Expected: 0

-- 26. Verify Purchases reference valid suppliers
SELECT COUNT(*) AS orphan_purchases FROM purchases p
LEFT JOIN suppliers s ON p.supplier_id = s.supplier_id
WHERE s.supplier_id IS NULL;

-- Expected: 0

-- 27. Verify Sales reference valid users
SELECT COUNT(*) AS orphan_sales FROM sales s
LEFT JOIN users u ON s.user_id = u.user_id
WHERE u.user_id IS NULL;

-- Expected: 0

-- 28. Verify Sales reference valid branches
SELECT COUNT(*) AS orphan_sales FROM sales s
LEFT JOIN branches b ON s.branch_id = b.branch_id
WHERE b.branch_id IS NULL;

-- Expected: 0

-- 29. Check for medicines with empty ProductCode (needs backfill)
SELECT COUNT(*) AS medicines_with_empty_product_code FROM medicines WHERE product_code = '';

-- Expected: 0 (after backfill)

-- 30. Check for medicines with null UpdatedDate (needs backfill)
SELECT COUNT(*) AS medicines_with_null_updated_date FROM medicines WHERE updated_date IS NULL;

-- Expected: 0 (after backfill)

-- 31. Verify CategoryId distribution in Medicines
SELECT 
    CASE 
        WHEN category_id < 0 THEN 'Built-in'
        WHEN category_id > 0 THEN 'Custom'
        ELSE 'Unknown'
    END AS category_type,
    COUNT(*) AS count
FROM medicines
GROUP BY category_type;

-- Expected: Mix of Built-in and Custom (no Unknown)

-- 32. Verify UnitTypeId distribution in Medicines
SELECT 
    CASE 
        WHEN unit_type_id < 0 THEN 'Built-in'
        WHEN unit_type_id > 0 THEN 'Custom'
        ELSE 'Unknown'
    END AS unit_type_type,
    COUNT(*) AS count
FROM medicines
GROUP BY unit_type_type;

-- Expected: Mix of Built-in and Custom (no Unknown)

-- 33. Verify custom categories in use
SELECT c.category_id, c.name, COUNT(m.product_id) AS medicine_count
FROM categories c
LEFT JOIN medicines m ON c.category_id = m.category_id
GROUP BY c.category_id, c.name
ORDER BY medicine_count DESC;

-- Should show all custom categories and how many medicines use them

-- 34. Verify custom unit types in use
SELECT u.unit_type_id, u.name, COUNT(m.product_id) AS medicine_count
FROM unit_types u
LEFT JOIN medicines m ON u.unit_type_id = m.unit_type_id
GROUP BY u.unit_type_id, u.name
ORDER BY medicine_count DESC;

-- Should show all custom unit types and how many medicines use them

-- 35. Verify CosmeticBatches reference valid cosmetics
SELECT COUNT(*) AS orphan_cosmetic_batches FROM cosmetic_batches cb
LEFT JOIN cosmetics c ON cb.cosmetic_id = c.cosmetic_id
WHERE c.cosmetic_id IS NULL;

-- Expected: 0

-- 36. Verify DamageRecords reference valid medicine batches
SELECT COUNT(*) AS orphan_damage_records FROM damage_records dr
LEFT JOIN medicine_batches mb ON dr.batch_id = mb.batch_id
WHERE mb.batch_id IS NULL;

-- Expected: 0

-- 37. Verify ExpiredRecords reference valid medicine batches
SELECT COUNT(*) AS orphan_expired_records FROM expired_records er
LEFT JOIN medicine_batches mb ON er.batch_id = mb.batch_id
WHERE mb.batch_id IS NULL;

-- Expected: 0

-- 38. Summary statistics
SELECT 
    (SELECT COUNT(*) FROM medicines) AS total_medicines,
    (SELECT COUNT(*) FROM medicine_batches) AS total_batches,
    (SELECT COUNT(*) FROM cosmetics) AS total_cosmetics,
    (SELECT COUNT(*) FROM cosmetic_batches) AS total_cosmetic_batches,
    (SELECT COUNT(*) FROM categories) AS total_custom_categories,
    (SELECT COUNT(*) FROM unit_types) AS total_custom_unit_types,
    (SELECT COUNT(*) FROM purchases) AS total_purchases,
    (SELECT COUNT(*) FROM sales) AS total_sales,
    (SELECT COUNT(*) FROM purchase_items) AS total_purchase_items,
    (SELECT COUNT(*) FROM sale_items) AS total_sale_items,
    (SELECT COUNT(*) FROM inventory_transactions) AS total_inventory_transactions;
