using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging.Abstractions;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

/// <summary>
/// Runs the real PostgreSQL schema (including the damage/expiry migration) and
/// verifies that damage and automatic expiry persist correctly and that existing
/// medicine damage history survives the migration.
/// </summary>
public class PostgreSqlDamageExpiryTests : IClassFixture<IsolatedPostgreSqlDatabase>
{
    private readonly IsolatedPostgreSqlDatabase _server;

    // The database can be reused across runs, so every row created by a test is
    // tagged with a unique marker.
    private readonly string _marker = Guid.NewGuid().ToString("N").Substring(0, 12);

    public PostgreSqlDamageExpiryTests(IsolatedPostgreSqlDatabase server)
    {
        _server = server;
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_server.ConnectionString, npgsql => npgsql.MigrationsAssembly("MilkiDrugStore.Persistence"))
            .Options;
        return new AppDbContext(options);
    }

    private InventoryService CreateService(AppDbContext db)
    {
        return new InventoryService(
            new Repository<InventoryTransaction>(db),
            new Repository<MedicineBatch>(db),
            new Repository<Medicine>(db),
            new Repository<CosmeticBatch>(db),
            new UnitOfWork(db),
            new MilkiDrugStore.Application.Services.AuditLogService(
                new Repository<AuditLog>(db), new UnitOfWork(db), NullLogger<AuditLogService>.Instance));
    }

    [Fact]
    public async Task Migration_Applies_And_Preserves_ExistingMedicineDamageHistory()
    {
        using var db = CreateContext();
        await db.Database.MigrateAsync();

        var branch = await db.Branches.FirstOrDefaultAsync()
                     ?? await Seed(db, b => db.Branches.Add(b));

        // A medicine damage record that predates the cosmetic-damage migration.
        var legacy = new DamageRecord
        {
            BranchId = branch.BranchId,
            BatchId = null,
            Quantity = 3,
            Reason = "Legacy medicine damage (BatchId is null after migration)",
            RecordedBy = 0
        };
        db.DamageRecords.Add(legacy);
        await db.SaveChangesAsync();

        // Re-apply migrations to prove the schema is stable/idempotent.
        var pending = await db.Database.GetPendingMigrationsAsync();
        pending.Should().BeEmpty("all migrations including the new one must be applied");
    }

    private async Task<Branch> Seed(AppDbContext db, Action<Branch> add)
    {
        var branch = new Branch { BranchName = "PG Branch", IsActive = true, CreatedAt = DateTime.UtcNow };
        add(branch);
        await db.SaveChangesAsync();
        return branch;
    }

    [Fact]
    public async Task MedicineDamage_Persists_Across_PostgreSQL()
    {
        using var db = CreateContext();
        await db.Database.MigrateAsync();
        var service = CreateService(db);

        var (medicine, branch) = await SeedMedicine(db);
        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = branch.BranchId,
            BatchNumber = $"PG-MED-{Guid.NewGuid():N}".Substring(0, 20),
            PurchasePrice = 40m,
            SellingPrice = 90m,
            QuantityReceived = 100,
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        };
        db.MedicineBatches.Add(batch);
        await db.SaveChangesAsync();

        var reason = $"PG medicine damage {_marker}";
        await service.RecordDamageAsync(batch.BatchId, null, 10, reason, 1);

        db.ChangeTracker.Clear();
        var reloaded = await db.MedicineBatches.AsNoTracking().SingleAsync(b => b.BatchId == batch.BatchId);
        reloaded.RemainingQuantity.Should().Be(90);

        var damage = await db.DamageRecords.AsNoTracking().SingleAsync(d => d.Reason == reason);
        damage.BatchId.Should().Be(batch.BatchId);
        damage.CosmeticBatchId.Should().BeNull();

        var transaction = await db.InventoryTransactions.AsNoTracking().SingleAsync(t => t.ReferenceType == "DAMAGE" && t.BatchId == batch.BatchId);
        transaction.Quantity.Should().Be(10);
        transaction.CosmeticBatchId.Should().BeNull();
    }

    [Fact]
    public async Task CosmeticDamage_Persists_Across_PostgreSQL()
    {
        using var db = CreateContext();
        await db.Database.MigrateAsync();
        var service = CreateService(db);

        var (cosmetic, branch) = await SeedCosmetic(db);
        var batch = new CosmeticBatch
        {
            CosmeticId = cosmetic.CosmeticId,
            BranchId = branch.BranchId,
            BatchNumber = $"PG-COS-{Guid.NewGuid():N}".Substring(0, 20),
            QuantityReceived = 100,
            BuyingPrice = 100m,
            SellingPrice = 250m,
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        };
        db.CosmeticBatches.Add(batch);
        await db.SaveChangesAsync();

        var reason = $"PG cosmetic damage {_marker}";
        await service.RecordDamageAsync(null, batch.BatchId, 10, reason, 1);

        db.ChangeTracker.Clear();
        var reloaded = await db.CosmeticBatches.AsNoTracking().SingleAsync(b => b.BatchId == batch.BatchId);
        reloaded.Balance.Should().Be(90);

        var damage = await db.DamageRecords.AsNoTracking().SingleAsync(d => d.Reason == reason);
        damage.CosmeticBatchId.Should().Be(batch.BatchId);
        damage.CosmeticId.Should().Be(cosmetic.CosmeticId);
        damage.BatchId.Should().BeNull();

        var transaction = await db.InventoryTransactions.AsNoTracking()
            .SingleAsync(t => t.ReferenceType == "DAMAGE" && t.CosmeticBatchId == batch.BatchId);
        transaction.Quantity.Should().Be(10);
        transaction.CosmeticId.Should().Be(cosmetic.CosmeticId);
        transaction.ProductId.Should().BeNull();
    }

    [Fact]
    public async Task AutomaticExpiry_Persists_For_Medicine_And_Cosmetic_In_PostgreSQL()
    {
        using var db = CreateContext();
        await db.Database.MigrateAsync();
        var service = CreateService(db);

        var (medicine, branch) = await SeedMedicine(db);
        var medicineBatch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = branch.BranchId,
            BatchNumber = $"PG-EXPM-{Guid.NewGuid():N}".Substring(0, 20),
            PurchasePrice = 40m,
            SellingPrice = 90m,
            QuantityReceived = 50,
            ExpiryDate = DateTime.UtcNow.AddDays(-3)
        };
        var (cosmetic, _) = await SeedCosmetic(db);
        var cosmeticBatch = new CosmeticBatch
        {
            CosmeticId = cosmetic.CosmeticId,
            BranchId = branch.BranchId,
            BatchNumber = $"PG-EXPC-{Guid.NewGuid():N}".Substring(0, 20),
            QuantityReceived = 60,
            BuyingPrice = 100m,
            SellingPrice = 250m,
            ExpiryDate = DateTime.UtcNow.AddDays(-3)
        };
        db.MedicineBatches.Add(medicineBatch);
        db.CosmeticBatches.Add(cosmeticBatch);
        await db.SaveChangesAsync();

        var result = await service.ProcessExpiredInventoryAsync();
        result.MedicineBatchesProcessed.Should().BeGreaterThanOrEqualTo(1);
        result.CosmeticBatchesProcessed.Should().BeGreaterThanOrEqualTo(1);
        result.TotalUnitsExpired.Should().BeGreaterThanOrEqualTo(110);

        db.ChangeTracker.Clear();
        (await db.MedicineBatches.AsNoTracking().SingleAsync(b => b.BatchId == medicineBatch.BatchId))
            .RemainingQuantity.Should().Be(0);
        (await db.CosmeticBatches.AsNoTracking().SingleAsync(b => b.BatchId == cosmeticBatch.BatchId))
            .Balance.Should().Be(0);

        (await db.ExpiredRecords.AsNoTracking().CountAsync(e => e.BatchId == medicineBatch.BatchId))
            .Should().Be(1);
        (await db.ExpiredRecords.AsNoTracking().CountAsync(e => e.CosmeticBatchId == cosmeticBatch.BatchId))
            .Should().Be(1);

        // Second sweep must not duplicate anything.
        var second = await service.ProcessExpiredInventoryAsync();
        second.TotalUnitsExpired.Should().Be(0, "a second sweep must find nothing new");
        db.ChangeTracker.Clear();
        (await db.ExpiredRecords.AsNoTracking().CountAsync(e => e.BatchId == medicineBatch.BatchId))
            .Should().Be(1, "duplicate prevention must hold in PostgreSQL");
        (await db.ExpiredRecords.AsNoTracking().CountAsync(e => e.CosmeticBatchId == cosmeticBatch.BatchId))
            .Should().Be(1);
    }

    /// <summary>
    /// Upgrades a database that already contains medicine damage history from the
    /// previous migration to the new one and proves the history is preserved.
    /// </summary>
    [Fact]
    public async Task Migration_Upgrades_Existing_Database_Without_Losing_Damage_History()
    {
        var previousMigration = "20260923103402_MakeProductIdsNullableForCosmetics";
        var databaseName = $"upgrade_{Guid.NewGuid():N}".Substring(0, 20);

        var adminCs = _server.AdminConnectionStringBuilder;
        var dbNameCs = new Npgsql.NpgsqlConnectionStringBuilder(_server.ConnectionString) { Database = databaseName };
        var upgradeOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dbNameCs.ConnectionString, npgsql => npgsql.MigrationsAssembly("MilkiDrugStore.Persistence"))
            .Options;

        using (var admin = new Npgsql.NpgsqlConnection(adminCs.ConnectionString))
        {
            admin.Open();
            using var create = new Npgsql.NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
            create.ExecuteNonQuery();
        }

        try
        {
            // 1. Bring the database to the migration that predates cosmetic damage.
            using (var db = new AppDbContext(upgradeOptions))
            {
                await db.GetService<IMigrator>().MigrateAsync(previousMigration);
            }

            // 2. Insert pre-existing medicine damage history.
            using (var db = new AppDbContext(upgradeOptions))
            {
                db.Branches.Add(new Branch { BranchName = "Legacy", IsActive = true, CreatedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();

                var branch = await db.Branches.FirstAsync();
                var medicine = new Medicine
                {
                    BrandName = "Legacy Brand",
                    GenericName = "Legacy Generic",
                    ProductCode = "LEG-1",
                    CategoryId = -1,
                    UnitTypeId = -1,
                    IsActive = true
                };
                db.Medicines.Add(medicine);
                await db.SaveChangesAsync();

                var batch = new MedicineBatch
                {
                    ProductId = medicine.ProductId,
                    BranchId = branch.BranchId,
                    BatchNumber = "LEGACY-1",
                    PurchasePrice = 10m,
                    SellingPrice = 20m,
                    QuantityReceived = 100,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                };
                db.MedicineBatches.Add(batch);
                await db.SaveChangesAsync();

                // Raw SQL: this row is written against the OLD schema, which has
                // no CosmeticBatchId/CosmeticId columns yet.
                await db.Database.ExecuteSqlRawAsync(
                    "INSERT INTO \"DamageRecords\" (\"BranchId\", \"BatchId\", \"Quantity\", \"Reason\", \"RecordedBy\", \"RecordedDate\") " +
                    "VALUES ({0}, {1}, 7, 'Pre-migration medicine damage', 1, now())",
                    branch.BranchId, batch.BatchId);
            }

            // 3. Upgrade to the latest migration.
            using (var db = new AppDbContext(upgradeOptions))
            {
                await db.GetService<IMigrator>().MigrateAsync();
            }

            // 4. The history survived and is still readable through the new model.
            using (var db = new AppDbContext(upgradeOptions))
            {
                var service = CreateService(db);
                var damages = (await service.GetDamagesAsync()).ToList();

                damages.Should().ContainSingle("the pre-existing medicine damage record must survive the migration");
                damages[0].ProductType.Should().Be("medicine");
                damages[0].Quantity.Should().Be(7);
                damages[0].Reason.Should().Be("Pre-migration medicine damage");
                damages[0].BrandName.Should().Be("Legacy Brand");

                // Cosmetic damage works on the upgraded database.
                var cosmetic = new Cosmetic
                {
                    ProductName = "Post-migration Cosmetic",
                    CategoryId = -1,
                    UnitTypeId = -1,
                    Price = 100m,
                    IsActive = true,
                    BranchId = damages[0].BranchId
                };
                db.Cosmetics.Add(cosmetic);
                await db.SaveChangesAsync();

                var cosmeticBatch = new CosmeticBatch
                {
                    CosmeticId = cosmetic.CosmeticId,
                    BranchId = cosmetic.BranchId,
                    BatchNumber = "COS-UPG-1",
                    QuantityReceived = 50,
                    BuyingPrice = 40m,
                    SellingPrice = 100m,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                };
                db.CosmeticBatches.Add(cosmeticBatch);
                await db.SaveChangesAsync();

                await service.RecordDamageAsync(null, cosmeticBatch.BatchId, 5, "Cosmetic damage after upgrade", 1);

                db.ChangeTracker.Clear();
                (await db.CosmeticBatches.AsNoTracking().SingleAsync(b => b.BatchId == cosmeticBatch.BatchId))
                    .Balance.Should().Be(45);
                (await db.DamageRecords.AsNoTracking().CountAsync(d => d.CosmeticBatchId == cosmeticBatch.BatchId))
                    .Should().Be(1);

                // The legacy row is still intact after the cosmetic write.
                (await db.DamageRecords.AsNoTracking()
                        .SingleAsync(d => d.Reason == "Pre-migration medicine damage"))
                    .BatchId.Should().NotBeNull();
            }
        }
        finally
        {
            Npgsql.NpgsqlConnection.ClearAllPools();
            using var admin = new Npgsql.NpgsqlConnection(adminCs.ConnectionString);
            admin.Open();
            using var drop = new Npgsql.NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", admin);
            drop.ExecuteNonQuery();
        }
    }

    private static async Task<(Medicine, Branch)> SeedMedicine(AppDbContext db)
    {
        var branch = await db.Branches.FirstOrDefaultAsync();
        if (branch == null)
        {
            branch = new Branch { BranchName = "PG Branch", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Branches.Add(branch);
            await db.SaveChangesAsync();
        }

        var medicine = new Medicine
        {
            BrandName = $"PG Brand {Guid.NewGuid():N}".Substring(0, 20),
            GenericName = "PG Generic",
            ProductCode = $"PG-{Guid.NewGuid():N}".Substring(0, 12),
            CategoryId = -1,
            UnitTypeId = -1,
            IsActive = true
        };
        db.Medicines.Add(medicine);
        await db.SaveChangesAsync();
        return (medicine, branch);
    }

    private static async Task<(Cosmetic, Branch)> SeedCosmetic(AppDbContext db)
    {
        var branch = await db.Branches.FirstOrDefaultAsync();
        if (branch == null)
        {
            branch = new Branch { BranchName = "PG Branch", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Branches.Add(branch);
            await db.SaveChangesAsync();
        }

        var cosmetic = new Cosmetic
        {
            ProductName = $"PG Cosmetic {Guid.NewGuid():N}".Substring(0, 20),
            CategoryId = -1,
            UnitTypeId = -1,
            Price = 250m,
            IsActive = true,
            BranchId = branch.BranchId
        };
        db.Cosmetics.Add(cosmetic);
        await db.SaveChangesAsync();
        return (cosmetic, branch);
    }
}
