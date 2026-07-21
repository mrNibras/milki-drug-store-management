using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Api;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await MigrateAsync(context);

        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Pharmacist" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            var mainBranch = context.Branches.FirstOrDefault(b => b.BranchName == "Main Branch");
            if (mainBranch == null)
            {
                mainBranch = new Branch { BranchName = "Main Branch", Location = "Main Store", IsActive = true, CreatedAt = DateTime.Now };
                context.Branches.Add(mainBranch);
                await context.SaveChangesAsync();
            }

            context.Users.Add(new User
            {
                FullName = "System Admin",
                Email = "admin@milki.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                RoleId = adminRole.RoleId,
                BranchId = mainBranch.BranchId,
                IsApproved = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await context.SaveChangesAsync();
        }

        if (!context.UnitTypes.Any())
        {
            context.UnitTypes.AddRange(
                new UnitType { Name = "Tablet", Description = "Solid dosage form" },
                new UnitType { Name = "Capsule", Description = "Gelatinous shell" },
                new UnitType { Name = "Bottle", Description = "Liquid container" },
                new UnitType { Name = "Tube", Description = "Ointment/cream tube" },
                new UnitType { Name = "Piece", Description = "Single item" },
                new UnitType { Name = "Strip", Description = "Blister strip" },
                new UnitType { Name = "Box", Description = "Box packaging" },
                new UnitType { Name = "Packet", Description = "Packet packaging" },
                new UnitType { Name = "Vial", Description = "Injectable vial" },
                new UnitType { Name = "Injection", Description = "Injectable form" },
                new UnitType { Name = "Cream", Description = "Topical cream" },
                new UnitType { Name = "Drops", Description = "Eye/ear drops" },
                new UnitType { Name = "Inhaler", Description = "Inhalation device" },
                new UnitType { Name = "Patch", Description = "Transdermal patch" },
                new UnitType { Name = "Syrup", Description = "Oral liquid" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Categories.Any())
        {
            var tablet = await context.UnitTypes.FirstAsync(u => u.Name == "Tablet");
            var capsule = await context.UnitTypes.FirstAsync(u => u.Name == "Capsule");
            var bottle = await context.UnitTypes.FirstAsync(u => u.Name == "Bottle");
            var tube = await context.UnitTypes.FirstAsync(u => u.Name == "Tube");
            var piece = await context.UnitTypes.FirstAsync(u => u.Name == "Piece");

            context.Categories.AddRange(
                new Category { Name = "Antibiotic", UnitTypeId = tablet.UnitTypeId },
                new Category { Name = "Pain Killer", UnitTypeId = tablet.UnitTypeId },
                new Category { Name = "Vitamin", UnitTypeId = tablet.UnitTypeId },
                new Category { Name = "Respiratory", UnitTypeId = capsule.UnitTypeId },
                new Category { Name = "Dermatology", UnitTypeId = tube.UnitTypeId },
                new Category { Name = "Cosmetics", UnitTypeId = tube.UnitTypeId },
                new Category { Name = "Baby Supplies", UnitTypeId = piece.UnitTypeId }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Settings.Any())
        {
            var mainBranch = context.Branches.FirstOrDefault(b => b.BranchName == "Main Branch");
            if (mainBranch == null)
            {
                mainBranch = new Branch { BranchName = "Main Branch", Location = "Main Store", IsActive = true, CreatedAt = DateTime.Now };
                context.Branches.Add(mainBranch);
                await context.SaveChangesAsync();
            }

            context.Settings.Add(new Settings
            {
                BranchId = mainBranch.BranchId,
                PharmacyName = "Milki Drug Store",
                Address = "",
                Phone = "",
                Email = "",
                Language = "English",
                LowStockThreshold = 10,
                ExpiryAlertMonths = 6,
                Currency = "ETB"
            });
            await context.SaveChangesAsync();
        }
    }

    private static async Task MigrateAsync(AppDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        if (connection is Microsoft.Data.Sqlite.SqliteConnection sqliteConnection)
        {
            await using var pragmaCmd = sqliteConnection.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA table_info(Sales);";
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await pragmaCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                    columns.Add(reader.GetString(reader.GetOrdinal("name")));
            }

            // Table may not exist yet on a brand-new database (EnsureCreated builds
            // the full current schema, so no column migration is needed in that case).
            if (columns.Count == 0)
                return;

            if (!columns.Contains("PaymentMethod"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sales ADD COLUMN PaymentMethod VARCHAR(20) NOT NULL DEFAULT 'cash';";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("PaymentStatus"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sales ADD COLUMN PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'paid';";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("AmountPaid"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sales ADD COLUMN AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0;";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("AmountDue"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sales ADD COLUMN AmountDue DECIMAL(18,2) NOT NULL DEFAULT 0;";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("ReferenceNumber"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sales ADD COLUMN ReferenceNumber VARCHAR(100);";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("TotalDiscount"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sales ADD COLUMN TotalDiscount DECIMAL(18,2) NOT NULL DEFAULT 0;";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("DiscountReason"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sales ADD COLUMN DiscountReason VARCHAR(500);";
                await cmd.ExecuteNonQueryAsync();
            }

            // Purchase table payment columns (added after initial schema).
            await using var purchasePragmaCmd = sqliteConnection.CreateCommand();
            purchasePragmaCmd.CommandText = "PRAGMA table_info(Purchases);";
            var purchaseColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var purchaseReader = await purchasePragmaCmd.ExecuteReaderAsync())
            {
                while (await purchaseReader.ReadAsync())
                {
                    if (!purchaseReader.IsDBNull(purchaseReader.GetOrdinal("name")))
                        purchaseColumns.Add(purchaseReader.GetString(purchaseReader.GetOrdinal("name")));
                }
            }

            if (purchaseColumns.Count > 0)
            {
                if (!purchaseColumns.Contains("AmountPaid"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Purchases ADD COLUMN AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0;";
                    await cmd.ExecuteNonQueryAsync();
                }
                if (!purchaseColumns.Contains("AmountDue"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Purchases ADD COLUMN AmountDue DECIMAL(18,2) NOT NULL DEFAULT 0;";
                    await cmd.ExecuteNonQueryAsync();
                }
                if (!purchaseColumns.Contains("PaymentStatus"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Purchases ADD COLUMN PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'unpaid';";
                    await cmd.ExecuteNonQueryAsync();
                }
                if (!purchaseColumns.Contains("PaymentMethod"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Purchases ADD COLUMN PaymentMethod VARCHAR(20);";
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // SaleItems column migration
            await using var itemPragmaCmd = sqliteConnection.CreateCommand();
            itemPragmaCmd.CommandText = "PRAGMA table_info(SaleItems);";
            var itemColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var itemReader = await itemPragmaCmd.ExecuteReaderAsync())
            {
                while (await itemReader.ReadAsync())
                {
                    if (!itemReader.IsDBNull(itemReader.GetOrdinal("name")))
                        itemColumns.Add(itemReader.GetString(itemReader.GetOrdinal("name")));
                }
            }

            if (itemColumns.Count > 0 && !itemColumns.Contains("DiscountAmount"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE SaleItems ADD COLUMN DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0;";
                await cmd.ExecuteNonQueryAsync();
            }

            if (purchaseColumns.Count > 0)
            {
                if (!purchaseColumns.Contains("PaymentStatus"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Purchases ADD COLUMN PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'unpaid';";
                    await cmd.ExecuteNonQueryAsync();
                }
                if (!purchaseColumns.Contains("PaymentMethod"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Purchases ADD COLUMN PaymentMethod VARCHAR(20);";
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await using var categoryPragmaCmd = sqliteConnection.CreateCommand();
            categoryPragmaCmd.CommandText = "PRAGMA table_info(Categories);";
            var categoryColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var categoryReader = await categoryPragmaCmd.ExecuteReaderAsync())
            {
                while (await categoryReader.ReadAsync())
                {
                    if (!categoryReader.IsDBNull(categoryReader.GetOrdinal("name")))
                        categoryColumns.Add(categoryReader.GetString(categoryReader.GetOrdinal("name")));
                }
            }

            if (categoryColumns.Count > 0)
            {
                if (!categoryColumns.Contains("UnitTypeId"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Categories ADD COLUMN UnitTypeId INTEGER NOT NULL DEFAULT 1;";
                    await cmd.ExecuteNonQueryAsync();
                }
                if (!categoryColumns.Contains("IsActive"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Categories ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;";
                    await cmd.ExecuteNonQueryAsync();
                }
                if (!categoryColumns.Contains("CreatedAt"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Categories ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT (datetime('now'));";
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await using var medicinePragmaCmd = sqliteConnection.CreateCommand();
            medicinePragmaCmd.CommandText = "PRAGMA table_info(Medicines);";
            var medicineColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var medicineReader = await medicinePragmaCmd.ExecuteReaderAsync())
            {
                while (await medicineReader.ReadAsync())
                {
                    if (!medicineReader.IsDBNull(medicineReader.GetOrdinal("name")))
                        medicineColumns.Add(medicineReader.GetString(medicineReader.GetOrdinal("name")));
                }
            }

            if (medicineColumns.Count > 0)
            {
                if (!medicineColumns.Contains("UnitTypeId"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Medicines ADD COLUMN UnitTypeId INTEGER NOT NULL DEFAULT 1;";
                    await cmd.ExecuteNonQueryAsync();
                }
                if (medicineColumns.Contains("UnitType"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = "ALTER TABLE Medicines DROP COLUMN UnitType;";
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await using var unitTypePragmaCmd = sqliteConnection.CreateCommand();
            unitTypePragmaCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='UnitTypes';";
            var unitTypeTableExists = false;
            await using (var unitTypeReader = await unitTypePragmaCmd.ExecuteReaderAsync())
            {
                if (await unitTypeReader.ReadAsync())
                {
                    unitTypeTableExists = true;
                }
            }

            if (!unitTypeTableExists)
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = @"CREATE TABLE UnitTypes (
                    UnitTypeId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name VARCHAR(50) NOT NULL UNIQUE,
                    Description VARCHAR(200),
                    IsActive INTEGER NOT NULL DEFAULT 1
                );";
                await cmd.ExecuteNonQueryAsync();
            }

            // Branch-related migrations
            await using var branchPragmaCmd = sqliteConnection.CreateCommand();
            branchPragmaCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Branches';";
            var branchTableExists = false;
            await using (var branchReader = await branchPragmaCmd.ExecuteReaderAsync())
            {
                if (await branchReader.ReadAsync())
                {
                    branchTableExists = true;
                }
            }

            if (!branchTableExists)
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = @"CREATE TABLE Branches (
                    BranchId INTEGER PRIMARY KEY AUTOINCREMENT,
                    BranchName VARCHAR(200) NOT NULL,
                    Location VARCHAR(200),
                    Phone VARCHAR(50),
                    Email VARCHAR(150),
                    Address VARCHAR(300),
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );";
                await cmd.ExecuteNonQueryAsync();

                await using var insertCmd = sqliteConnection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Branches (BranchName, Location, IsActive, CreatedAt) VALUES ('Main Branch', 'Main Store', 1, datetime('now'));";
                await insertCmd.ExecuteNonQueryAsync();
            }

            var tablesToAddBranch = new[] { "Users", "Sales", "Purchases", "MedicineBatches", "DamageRecords", "ExpiredRecords", "AuditLogs", "Notifications", "Settings" };
            foreach (var table in tablesToAddBranch)
            {
                await using var tablePragmaCmd = sqliteConnection.CreateCommand();
                tablePragmaCmd.CommandText = $"PRAGMA table_info({table});";
                var tableColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                await using (var tableReader = await tablePragmaCmd.ExecuteReaderAsync())
                {
                    while (await tableReader.ReadAsync())
                    {
                        if (!tableReader.IsDBNull(tableReader.GetOrdinal("name")))
                            tableColumns.Add(tableReader.GetString(tableReader.GetOrdinal("name")));
                    }
                }

                if (tableColumns.Count > 0 && !tableColumns.Contains("BranchId"))
                {
                    await using var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN BranchId INTEGER NOT NULL DEFAULT 1;";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
