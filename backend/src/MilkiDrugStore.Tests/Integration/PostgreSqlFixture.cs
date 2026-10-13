using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Testcontainers.PostgreSql;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class PostgreSqlFixture : IAsyncLifetime
{
    /// <summary>
    /// When set, the tests run against this already-running PostgreSQL instance
    /// instead of starting a Testcontainers container. Useful in environments
    /// where the Docker daemon can start containers but not stop them.
    /// </summary>
    public const string ExternalConnectionStringVariable = "MILKI_TEST_POSTGRES";

    public PostgreSqlContainer Container { get; private set; } = null!;
    public string ConnectionString { get; private set; } = string.Empty;
    private bool _ownsContainer;

    public async Task InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable(ExternalConnectionStringVariable);
        if (!string.IsNullOrWhiteSpace(external))
        {
            ConnectionString = external;
            _ownsContainer = false;
            return;
        }

        Container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("milki_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
        _ownsContainer = true;

        // Run migrations on the fresh database
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(ConnectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MilkiDrugStore.Persistence");
            }));
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await serviceProvider.DisposeAsync();
    }

    public async Task DisposeAsync()
    {
        if (_ownsContainer)
            await Container.DisposeAsync();
    }

    public IServiceProvider CreateServiceProvider()
    {
        var backupDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Backup_" + Guid.NewGuid().ToString("N"));
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(ConnectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MilkiDrugStore.Persistence");
            }));

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", ConnectionString),
                new KeyValuePair<string, string?>("BackupDirectory", backupDir)
            })
            .Build());

        services.AddLogging();
        services.AddScoped<IBackupService, BackupService>();
        
        // Register ICurrentUserService for tests (mock implementation)
        services.AddScoped<ICurrentUserService, TestCurrentUserService>();

        return services.BuildServiceProvider();
    }
    
    // Simple test implementation of ICurrentUserService
    private class TestCurrentUserService : ICurrentUserService
    {
        public int UserId => 1;
        public string Email => "test@test.com";
        public string Role => "Admin";
        public int BranchId => 1;
        public bool CanViewFinancialCosts => true;
    }
}
