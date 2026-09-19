using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Application.Services;

public class RetentionService : IRetentionService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RetentionService> _logger;
    private readonly int _auditLogRetentionDays;
    private readonly int _notificationRetentionDays;

    public RetentionService(
        AppDbContext dbContext,
        IConfiguration configuration,
        ILogger<RetentionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _auditLogRetentionDays = int.TryParse(configuration["Retention:AuditLogRetentionDays"], out var audit) ? audit : 1095;
        _notificationRetentionDays = int.TryParse(configuration["Retention:NotificationRetentionDays"], out var notif) ? notif : 180;
    }

    public async Task<int> CleanupOldAuditLogsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-_auditLogRetentionDays);
        var count = await _dbContext.AuditLogs
            .Where(al => al.CreatedAt < cutoff)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Deleted {Count} audit log entries older than {Cutoff}", count, cutoff);
        return count;
    }

    public async Task<int> CleanupOldNotificationsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-_notificationRetentionDays);
        var count = await _dbContext.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoff)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Deleted {Count} notification entries older than {Cutoff}", count, cutoff);
        return count;
    }

    public async Task<(int auditLogsDeleted, int notificationsDeleted)> RunFullCleanupAsync()
    {
        var auditLogsDeleted = await CleanupOldAuditLogsAsync();
        var notificationsDeleted = await CleanupOldNotificationsAsync();
        return (auditLogsDeleted, notificationsDeleted);
    }
}
