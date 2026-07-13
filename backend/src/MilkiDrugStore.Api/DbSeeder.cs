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
            context.Users.Add(new User
            {
                FullName = "System Admin",
                Email = "admin@milki.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                RoleId = adminRole.RoleId,
                IsApproved = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await context.SaveChangesAsync();
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Antibiotic" },
                new Category { Name = "Pain Killer" },
                new Category { Name = "Vitamin" },
                new Category { Name = "Respiratory" },
                new Category { Name = "Dermatology" },
                new Category { Name = "Cosmetics" },
                new Category { Name = "Baby Supplies" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Settings.Any())
        {
            context.Settings.Add(new Settings
            {
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
        }
    }
}
