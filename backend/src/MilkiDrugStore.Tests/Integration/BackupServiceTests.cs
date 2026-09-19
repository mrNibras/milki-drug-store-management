using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Persistence.Context;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class BackupServiceTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _connectionString;
    private readonly string _backupDir;

    public BackupServiceTests()
    {
        _connectionString = $"Data Source={Path.GetTempFileName()}";
        _backupDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Backup_Test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_backupDir);

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_connectionString, sql =>
            {
                sql.MigrationsAssembly("MilkiDrugStore.Persistence");
            }));

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", _connectionString),
                new KeyValuePair<string, string?>("BackupDirectory", _backupDir),
                new KeyValuePair<string, string?>("DatabaseProvider", "sqlite")
            })
            .Build());

        services.AddLogging();
        services.AddScoped<IBackupService, BackupService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.DisposeAsync();

        if (_serviceProvider is IAsyncDisposable ad)
            await ad.DisposeAsync();
        else
            _serviceProvider.Dispose();

        try
        {
            var dbFile = new FileInfo(_connectionString.Replace("Data Source=", ""));
            if (dbFile.Exists) dbFile.Delete();
        }
        catch { }

        try { Directory.Delete(_backupDir, recursive: true); } catch { }
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

        Assert.Equal(_backupDir, directory);
    }

    [Fact]
    public async Task CreateBackup_Should_Throw_For_NonExistent_Database()
    {
        var badServices = new ServiceCollection();
        var badConn = "Data Source=/nonexistent/path/db.db";
        badServices.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", badConn),
                new KeyValuePair<string, string?>("BackupDirectory", _backupDir),
                new KeyValuePair<string, string?>("DatabaseProvider", "sqlite")
            })
            .Build());
        badServices.AddLogging();
        badServices.AddScoped<IBackupService, BackupService>();

        var badProvider = badServices.BuildServiceProvider();
        var backupService = badProvider.GetRequiredService<IBackupService>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => backupService.CreateBackupAsync("bad"));
    }

    [Theory]
    [InlineData("sqlite", "Data Source=/var/data/test.db")]
    [InlineData("postgresql", "Host=localhost;Port=5432;Database=db;Username=user;Password=pass")]
    [InlineData("sqlserver", "Server=localhost;Database=test;User Id=sa;Password=pass")]
    public async Task BackupService_Detects_Provider_From_Config(string provider, string conn)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", conn),
                new KeyValuePair<string, string?>("BackupDirectory", _backupDir),
                new KeyValuePair<string, string?>("DatabaseProvider", provider)
            })
            .Build());
        services.AddLogging();
        services.AddScoped<IBackupService, BackupService>();

        var providerFactory = services.BuildServiceProvider();
        var backupService = providerFactory.GetRequiredService<IBackupService>();

        var dir = await backupService.GetBackupDirectoryAsync();
        Assert.Equal(_backupDir, dir);
    }
}
