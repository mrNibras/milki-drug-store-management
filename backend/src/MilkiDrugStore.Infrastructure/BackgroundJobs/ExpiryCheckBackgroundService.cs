using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Infrastructure.BackgroundJobs;

public class ExpiryCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiryCheckBackgroundService> _logger;

    public ExpiryCheckBackgroundService(IServiceProvider serviceProvider, ILogger<ExpiryCheckBackgroundService> logger)
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
                var medicineRepo = scope.ServiceProvider.GetRequiredService<IMedicineRepository>();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expiringMedicines = await medicineRepo.GetExpiringAsync(6);
                foreach (var medicine in expiringMedicines)
                {
                    var recentAlert = await notificationRepo.GetRecentExpiryAlertAsync(medicine.BrandName, 15);
                    if (recentAlert == null)
                    {
                        await notificationRepo.AddAsync(new Notification
                        {
                            Title = "Expiry Alert",
                            Message = $"{medicine.BrandName} is expiring within 6 months",
                            NotificationType = NotificationTypeStrings.ExpiryAlert
                        });
                    }
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in expiry check background job");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
