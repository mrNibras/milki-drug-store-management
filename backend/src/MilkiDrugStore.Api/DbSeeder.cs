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
            pragmaCmd.CommandText = "PRAGMA table_info(Sale);";
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await pragmaCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("name")))
                    columns.Add(reader.GetString(reader.GetOrdinal("name")));
            }

            if (!columns.Contains("PaymentMethod"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sale ADD COLUMN PaymentMethod VARCHAR(20) NOT NULL DEFAULT 'cash';";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("PaymentStatus"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sale ADD COLUMN PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'paid';";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("AmountPaid"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sale ADD COLUMN AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0;";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("AmountDue"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sale ADD COLUMN AmountDue DECIMAL(18,2) NOT NULL DEFAULT 0;";
                await cmd.ExecuteNonQueryAsync();
            }

            if (!columns.Contains("ReferenceNumber"))
            {
                await using var cmd = sqliteConnection.CreateCommand();
                cmd.CommandText = "ALTER TABLE Sale ADD COLUMN ReferenceNumber VARCHAR(100);";
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
