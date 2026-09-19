using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Postgres;

/// <summary>
/// Design-time factory for the PostgreSQL-only migration assembly
/// (MilkiDrugStore.Persistence.Postgres). Keeps PostgreSQL schema
/// generation independent from the SQLite/SQL Server migrations in the
/// base Persistence assembly.
/// </summary>
public class PostgresDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MilkiDrugStore.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = DbProviderResolver.ResolveConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Local fallback used only when generating/maintaining the
            // PostgreSQL migrations on a developer machine.
            connectionString = "Host=localhost;Port=5432;Database=milki_drug_store;Username=postgres;Password=postgres";
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly("MilkiDrugStore.Persistence.Postgres"));

        return new AppDbContext(optionsBuilder.Options);
    }
}