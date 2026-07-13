using Xunit;
using FluentAssertions;
using MilkiDrugStore.Api;
using MilkiDrugStore.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Reflection;

namespace MilkiDrugStore.Tests.Infrastructure;

public class DbSeederTests
{
    [Fact]
    public async Task MigrateAsync_AddsPaymentColumnsToSale_WhenMissing()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new AppDbContext(options))
        {
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE Sales (
                    SaleId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleNumber VARCHAR(50) NOT NULL,
                    SaleDate TEXT NOT NULL,
                    TotalAmount DECIMAL(18,2) NOT NULL,
                    TotalProfit DECIMAL(18,2) NOT NULL,
                    UserId INTEGER NOT NULL
                );
            ");
        }

        using (var context = new AppDbContext(options))
        {
            var migrateMethod = typeof(DbSeeder).GetMethod("MigrateAsync",
                BindingFlags.NonPublic | BindingFlags.Static);
            migrateMethod!.Invoke(null, new object[] { context });
        }

        var columns = await GetColumnsAsync(connection, "Sales");
        columns.Should().Contain("PaymentMethod");
        columns.Should().Contain("PaymentStatus");
        columns.Should().Contain("AmountPaid");
        columns.Should().Contain("AmountDue");
        columns.Should().Contain("ReferenceNumber");
        columns.Should().Contain("TotalDiscount");
        columns.Should().Contain("DiscountReason");
    }

    [Fact]
    public async Task MigrateAsync_DoesNotDuplicateColumns_WhenAlreadyExists()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new AppDbContext(options))
        {
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE Sales (
                    SaleId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleNumber VARCHAR(50) NOT NULL,
                    SaleDate TEXT NOT NULL,
                    TotalAmount DECIMAL(18,2) NOT NULL,
                    TotalProfit DECIMAL(18,2) NOT NULL,
                    UserId INTEGER NOT NULL
                );
            ");
        }

        using (var context = new AppDbContext(options))
        {
            var migrateMethod = typeof(DbSeeder).GetMethod("MigrateAsync",
                BindingFlags.NonPublic | BindingFlags.Static);
            migrateMethod!.Invoke(null, new object[] { context });
        }

        using (var context2 = new AppDbContext(options))
        {
            var migrateMethod2 = typeof(DbSeeder).GetMethod("MigrateAsync",
                BindingFlags.NonPublic | BindingFlags.Static);
            migrateMethod2!.Invoke(null, new object[] { context2 });
        }

        var columns = await GetColumnsAsync(connection, "Sales");
        columns.Count(c => c == "PaymentMethod").Should().Be(1);
    }

    private static async Task<List<string>> GetColumnsAsync(SqliteConnection connection, string tableName)
    {
        var columns = new List<string>();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info({tableName});";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(reader.GetOrdinal("name")))
                columns.Add(reader.GetString(reader.GetOrdinal("name")));
        }
        return columns;
    }
}
