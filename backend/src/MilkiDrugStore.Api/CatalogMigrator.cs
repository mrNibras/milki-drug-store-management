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

        var categories = await db.Categories.ToListAsync();
        foreach (var cat in categories)
        {
            if (MedicineCatalog.FindCategoryId(cat.Name) is int builtInId && builtInId != cat.CategoryId)
            {
                var medicines = await db.Medicines.Where(m => m.CategoryId == cat.CategoryId).ToListAsync();
                foreach (var m in medicines)
                    m.CategoryId = builtInId;

                db.Categories.Remove(cat);
                logger.LogInformation("Migrated category '{Name}' (id {OldId}) -> built-in id {NewId}", cat.Name, cat.CategoryId, builtInId);
            }
        }

        var unitTypes = await db.UnitTypes.ToListAsync();
        foreach (var ut in unitTypes)
        {
            if (MedicineCatalog.FindUnitTypeId(ut.Name) is int builtInId && builtInId != ut.UnitTypeId)
            {
                var medicines = await db.Medicines.Where(m => m.UnitTypeId == ut.UnitTypeId).ToListAsync();
                foreach (var m in medicines)
                    m.UnitTypeId = builtInId;

                db.UnitTypes.Remove(ut);
                logger.LogInformation("Migrated unit type '{Name}' (id {OldId}) -> built-in id {NewId}", ut.Name, ut.UnitTypeId, builtInId);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Catalog data migration completed.");
    }
}
