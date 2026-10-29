using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Common;
using MilkiDrugStore.Domain.Common;

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

                // Automatic expiry: write off every medicine/cosmetic batch whose
                // expiry date has passed. This replaces the removed manual
                // "Record Expired" action.
                var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
                var expiredResult = await inventoryService.ProcessExpiredInventoryAsync(stoppingToken);
                if (expiredResult.MedicineBatchesProcessed > 0 || expiredResult.CosmeticBatchesProcessed > 0)
                {
                    _logger.LogInformation(
                        "Automatic expiry processed {MedicineCount} medicine batch(es) and {CosmeticCount} cosmetic batch(es), {Units} unit(s) written off.",
                        expiredResult.MedicineBatchesProcessed,
                        expiredResult.CosmeticBatchesProcessed,
                        expiredResult.TotalUnitsExpired);
                }

                var medicineRepo = scope.ServiceProvider.GetRequiredService<IMedicineRepository>();
                var cosmeticRepo = scope.ServiceProvider.GetRequiredService<ICosmeticRepository>();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var branchRepo = scope.ServiceProvider.GetRequiredService<IRepository<Branch>>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var today = ExpiryCountdown.GetToday();
                var expiringMedicines = await medicineRepo.GetExpiringAsync(6);
                var expiringCosmetics = await cosmeticRepo.GetExpiringAsync(6);
                var branches = (await branchRepo.GetAllAsync()).ToList();
                if (!branches.Any())
                {
                    _logger.LogWarning("No branches found; skipping expiry notifications");
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    continue;
                }

                foreach (var medicine in expiringMedicines)
                {
                    // Determine the branch from the medicine's batches
                    var medicineBranchIds = medicine.Batches
                        .Where(b => DateOnly.FromDateTime(b.ExpiryDate) <= today.AddMonths(6) && b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired > 0)
                        .Select(b => b.BranchId)
                        .Distinct()
                        .ToList();

                    foreach (var branchId in medicineBranchIds)
                    {
                        var branch = branches.FirstOrDefault(b => b.BranchId == branchId);
                        if (branch == null)
                        {
                            _logger.LogWarning("Branch {BranchId} not found for medicine {MedicineId}; skipping notification", branchId, medicine.ProductId);
                            continue;
                        }

                        if (!await notificationRepo.HasNotificationTodayAsync(NotificationTypeStrings.ExpiryAlert, medicine.BrandName, branchId))
                        {
                            var batches = medicine.Batches.Where(b => b.BranchId == branchId && b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired > 0)
                                .OrderBy(b => b.ExpiryDate).ToList();
                            var message = batches.Any() 
                                ? $"{medicine.BrandName} - {ExpiryCountdown.FormatCountdown(DateOnly.FromDateTime(batches.First().ExpiryDate), today)}"
                                : $"{medicine.BrandName} is expiring within 6 months";
                            await notificationRepo.AddAsync(new Notification
                            {
                                BranchId = branchId,
                                Title = "Expiry Alert",
                                Message = message,
                                NotificationType = NotificationTypeStrings.ExpiryAlert
                            });
                        }
                    }
                }

                foreach (var cosmetic in expiringCosmetics)
                {
                    var cosmeticBranchIds = cosmetic.Batches
                        .Where(b => DateOnly.FromDateTime(b.ExpiryDate.Value) <= today.AddMonths(6) && b.Balance > 0)
                        .Select(b => b.BranchId)
                        .Distinct()
                        .ToList();

                    foreach (var branchId in cosmeticBranchIds)
                    {
                        var branch = branches.FirstOrDefault(b => b.BranchId == branchId);
                        if (branch == null)
                        {
                            _logger.LogWarning("Branch {BranchId} not found for cosmetic {CosmeticId}; skipping notification", branchId, cosmetic.CosmeticId);
                            continue;
                        }

                        if (!await notificationRepo.HasNotificationTodayAsync(NotificationTypeStrings.ExpiryAlert, cosmetic.ProductName, branchId))
                        {
                            var batches = cosmetic.Batches.Where(b => b.BranchId == branchId && b.Balance > 0)
                                .OrderBy(b => b.ExpiryDate).ToList();
                            var message = batches.Any()
                                ? $"{cosmetic.ProductName} - {ExpiryCountdown.FormatCountdown(DateOnly.FromDateTime(batches.First().ExpiryDate.Value), today)}"
                                : $"{cosmetic.ProductName} is expiring within 6 months";
                            await notificationRepo.AddAsync(new Notification
                            {
                                BranchId = branchId,
                                Title = "Cosmetic Expiry Alert",
                                Message = message,
                                NotificationType = NotificationTypeStrings.ExpiryAlert
                            });
                        }
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
