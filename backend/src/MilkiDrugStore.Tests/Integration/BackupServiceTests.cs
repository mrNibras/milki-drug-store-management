using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Domain.Entities;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class BackupServiceTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public BackupServiceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateBackup_Should_Create_Backup_File_For_PostgreSQL()
    {
        var serviceProvider = _fixture.CreateServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Seed some data
        db.Roles.Add(new Role { Name = "Admin" });
        await db.SaveChangesAsync();

        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
        var backupPath = await backupService.CreateBackupAsync("test-backup");

        Assert.NotNull(backupPath);
        Assert.True(File.Exists(backupPath));
        Assert.EndsWith(".dump", backupPath);
    }

    [Fact]
    public async Task GetBackupDirectory_Should_Return_Configured_Directory()
    {
        var services = new ServiceCollection();
        var backupDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Backup_Test_" + Guid.NewGuid().ToString("N"));
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", _fixture.ConnectionString),
                new KeyValuePair<string, string?>("BackupDirectory", backupDir)
            })
            .Build());
        services.AddLogging();
        services.AddScoped<IBackupService, BackupService>();

        var provider = services.BuildServiceProvider();
        var backupService = provider.GetRequiredService<IBackupService>();

        var directory = await backupService.GetBackupDirectoryAsync();
        Assert.Equal(backupDir, directory);
    }

    [Fact]
    public async Task CreateBackup_Should_Throw_For_NonExistent_Database()
    {
        var backupDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Backup_Test_" + Guid.NewGuid().ToString("N"));
        var badConn = "Host=nonexistent-host:5432;Database=db;Username=user;Password=pass";

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", badConn),
                new KeyValuePair<string, string?>("BackupDirectory", backupDir)
            })
            .Build());
        services.AddLogging();
        services.AddScoped<IBackupService, BackupService>();

        var provider = services.BuildServiceProvider();
        var backupService = provider.GetRequiredService<IBackupService>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => backupService.CreateBackupAsync("bad"));
    }

    [Theory]
    [InlineData("Host=localhost;Port=5432;Database=db;Username=postgres;Password=secret")]
    public async Task BackupService_Uses_PostgreSQL_Provider(string conn)
    {
        var backupDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Backup_Test_" + Guid.NewGuid().ToString("N"));

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", conn),
                new KeyValuePair<string, string?>("BackupDirectory", backupDir)
            })
            .Build());
        services.AddLogging();
        services.AddScoped<IBackupService, BackupService>();

        var provider = services.BuildServiceProvider();
        var backupService = provider.GetRequiredService<IBackupService>();

        var dir = await backupService.GetBackupDirectoryAsync();
        Assert.Equal(backupDir, dir);
    }
}
