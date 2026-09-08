using MilkiDrugStore.Domain.Catalog;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Api;

public static class CatalogMigrator
{
    public static async Task MigrateAsync(AppDbContext db, ILogger logger)
    {
        logger.LogInformation("Starting catalog data migration...");

        // NOTE: This migrator is intentionally NON-DESTRUCTIVE.
        //
        // It does NOT delete, rename, or re-map any categories or unit types
        // stored in the database. User-created (custom) categories and unit
        // types are always preserved.
        //
        // Historically this method deleted DB rows whose name matched a
        // built-in catalog entry and reassigned medicines to the built-in
        // (negative) id. That behavior could destroy user data and was
        // unnecessary, because CatalogService.GetCategoryOptionsAsync /
        // GetUnitTypeOptionsAsync already deduplicate built-in vs custom
        // entries at read time. It has been removed to guarantee data safety.

        // Verify we can read the tables (ensures connectivity during startup).
        var categoryCount = await db.Categories.CountAsync();
        var unitTypeCount = await db.UnitTypes.CountAsync();
        var medicineCount = await db.Medicines.CountAsync();

        logger.LogInformation(
            "Catalog migration check complete: {CategoryCount} categories, {UnitTypeCount} unit types, {MedicineCount} medicines. Nothing was modified.",
            categoryCount, unitTypeCount, medicineCount);
    }
}
