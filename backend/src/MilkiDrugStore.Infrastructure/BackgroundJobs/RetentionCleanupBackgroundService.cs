using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Infrastructure.BackgroundJobs;

public class RetentionCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RetentionCleanupBackgroundService> _logger;

    public RetentionCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<RetentionCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var retentionService = scope.ServiceProvider.GetRequiredService<IRetentionService>();
                var (auditLogs, notifications) = await retentionService.RunFullCleanupAsync();

                if (auditLogs > 0 || notifications > 0)
                {
                    _logger.LogInformation("Retention cleanup: deleted {AuditLogs} audit logs, {Notifications} notifications",
                        auditLogs, notifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in retention cleanup background job");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
