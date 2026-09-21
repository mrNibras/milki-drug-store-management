using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class BackupServiceIntegrationTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public BackupServiceIntegrationTests(PostgreSqlFixture fixture)
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

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Pharmacist" }
            );
            await db.SaveChangesAsync();
        }

        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
        var backupPath = await backupService.CreateBackupAsync("integration-test-backup");

        Assert.NotNull(backupPath);
        Assert.True(File.Exists(backupPath));
        Assert.EndsWith(".dump", backupPath);
    }

    [Fact]
    public async Task GetBackupDirectory_Should_Return_Configured_Directory()
    {
        var backupDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Test_Backups_" + Guid.NewGuid().ToString("N"));

        var services = new ServiceCollection();
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
        Assert.NotNull(directory);
        Assert.NotEmpty(directory);
    }

    [Fact]
    public async Task RestoreBackup_Should_Restore_From_Dump()
    {
        var serviceProvider = _fixture.CreateServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        db.Roles.RemoveRange(await db.Roles.ToListAsync());
        await db.SaveChangesAsync();

        db.Roles.Add(new Role { Name = "Admin" });
        await db.SaveChangesAsync();

        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

        var backupPath = await backupService.CreateBackupAsync("restore-test");
        Assert.True(File.Exists(backupPath));

        var role = await db.Roles.FirstAsync();
        db.Roles.Remove(role);
        await db.SaveChangesAsync();
        Assert.False(await db.Roles.AnyAsync());

        var restored = await backupService.RestoreBackupAsync(backupPath);
        Assert.True(restored);

        scope.Dispose();

        using var verifyScope = serviceProvider.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await verifyDb.Roles.AnyAsync());
    }
}
