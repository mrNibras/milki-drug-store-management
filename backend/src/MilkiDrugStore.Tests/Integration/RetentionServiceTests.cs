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

public class RetentionServiceTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _connectionString;

    public RetentionServiceTests()
    {
        _connectionString = $"Data Source={Guid.NewGuid()}.db";

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_connectionString, sql =>
            {
                sql.MigrationsAssembly("MilkiDrugStore.Persistence");
            }));

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Retention:AuditLogRetentionDays", "90"),
                new KeyValuePair<string, string?>("Retention:NotificationRetentionDays", "30")
            })
            .Build());

        services.AddLogging();
        services.AddScoped<IRetentionService, RetentionService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await SeedTestDataAsync(db);
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
    }

    private async Task SeedTestDataAsync(AppDbContext db)
    {
        var role = new Role { Name = "Admin" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var branch = new Branch
        {
            BranchName = "Test Branch",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var user = new User
        {
            FullName = "Test User",
            Email = "test@test.com",
            PasswordHash = "hash",
            RoleId = role.RoleId,
            BranchId = branch.BranchId,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.AuditLogs.AddRange(
            new AuditLog { Action = "Old Action", TableName = "Test", CreatedAt = DateTime.UtcNow.AddDays(-100), UserId = user.UserId },
            new AuditLog { Action = "Recent Action", TableName = "Test", CreatedAt = DateTime.UtcNow.AddDays(-10), UserId = user.UserId },
            new AuditLog { Action = "Old Action 2", TableName = "Test", CreatedAt = DateTime.UtcNow.AddDays(-200), UserId = user.UserId }
        );

        db.Notifications.AddRange(
            new Notification { BranchId = branch.BranchId, Title = "Old Read", Message = "Test", NotificationType = "TEST", IsRead = true, CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new Notification { BranchId = branch.BranchId, Title = "Recent Read", Message = "Test", NotificationType = "TEST", IsRead = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Notification { BranchId = branch.BranchId, Title = "Old Unread", Message = "Test", NotificationType = "TEST", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new Notification { BranchId = branch.BranchId, Title = "Recent Unread", Message = "Test", NotificationType = "TEST", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CleanupOldAuditLogs_Should_Delete_Only_Old_Entries()
    {
        using var scope = _serviceProvider.CreateScope();
        var retention = scope.ServiceProvider.GetRequiredService<IRetentionService>();

        var deleted = await retention.CleanupOldAuditLogsAsync();
        Assert.Equal(2, deleted);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remaining = await db.AuditLogs.CountAsync();
        Assert.Equal(1, remaining);
    }

    [Fact]
    public async Task CleanupOldNotifications_Should_Delete_Only_Old_Read_Entries()
    {
        using var scope = _serviceProvider.CreateScope();
        var retention = scope.ServiceProvider.GetRequiredService<IRetentionService>();

        var deleted = await retention.CleanupOldNotificationsAsync();
        Assert.Equal(1, deleted);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remaining = await db.Notifications.CountAsync();
        Assert.Equal(3, remaining);
    }

    [Fact]
    public async Task RunFullCleanup_Should_Delete_Both_Old_AuditLogs_And_Notifications()
    {
        using var scope = _serviceProvider.CreateScope();
        var retention = scope.ServiceProvider.GetRequiredService<IRetentionService>();

        var (auditLogs, notifications) = await retention.RunFullCleanupAsync();
        Assert.Equal(2, auditLogs);
        Assert.Equal(1, notifications);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingAudit = await db.AuditLogs.CountAsync();
        var remainingNotif = await db.Notifications.CountAsync();
        Assert.Equal(1, remainingAudit);
        Assert.Equal(3, remainingNotif);
    }
}
