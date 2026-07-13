using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Infrastructure.BackgroundJobs;

/// <summary>
/// Periodically generates Low Stock and Out of Stock notifications (in addition to the
/// ExpiryCheckBackgroundService which handles expiry alerts). Runs every 30 minutes.
/// </summary>
public class NotificationCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationCheckBackgroundService> _logger;

    public NotificationCheckBackgroundService(IServiceProvider serviceProvider, ILogger<NotificationCheckBackgroundService> logger)
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
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.CheckAndCreateNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification check background job");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
