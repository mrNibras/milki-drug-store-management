using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Entities;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class BackupServiceIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly string _connectionString;

    public BackupServiceIntegrationTests()
    {
        _connectionString = $"Data Source={Path.GetTempFileName()}";
        var backupDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Test_Backups");
        Directory.CreateDirectory(backupDir);
        
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_connectionString, sql =>
            {
                sql.MigrationsAssembly("MilkiDrugStore.Persistence");
            }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IBackupService, BackupService>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", _connectionString),
                new KeyValuePair<string, string?>("BackupDirectory", backupDir)
            })
            .Build());
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }
        if (_serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _serviceProvider.Dispose();
        }

        try
        {
            var dbFile = new FileInfo(_connectionString.Replace("Data Source=", ""));
            if (dbFile.Exists)
            {
                dbFile.Delete();
            }
        }
        catch { }
    }

    [Fact]
    public async Task CreateBackup_Should_Create_Backup_File_For_Sqlite()
    {
        var backupService = _serviceProvider.GetRequiredService<IBackupService>();
        
        var backupPath = await backupService.CreateBackupAsync("test-backup");
        
        Assert.NotNull(backupPath);
        Assert.True(File.Exists(backupPath));
        Assert.EndsWith(".db", backupPath);
    }

    [Fact]
    public async Task GetBackupDirectory_Should_Return_Configured_Directory()
    {
        var backupService = _serviceProvider.GetRequiredService<IBackupService>();
        
        var directory = await backupService.GetBackupDirectoryAsync();
        
        Assert.NotNull(directory);
        Assert.NotEmpty(directory);
    }
}
