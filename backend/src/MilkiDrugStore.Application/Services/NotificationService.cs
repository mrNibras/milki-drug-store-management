using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IRepository<Medicine> _medicineRepo;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(INotificationRepository notificationRepo, IRepository<Medicine> medicineRepo, IUnitOfWork unitOfWork)
    {
        _notificationRepo = notificationRepo;
        _medicineRepo = medicineRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _notificationRepo.GetAllAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadAsync()
    {
        return await _notificationRepo.GetUnreadAsync();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var notifications = await _notificationRepo.FindAsync(n => n.NotificationId == id);
        var notification = notifications.FirstOrDefault();
        if (notification == null) return;

        notification.IsRead = true;
        await _notificationRepo.UpdateAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync()
    {
        var unread = await _notificationRepo.GetUnreadAsync();
        foreach (var n in unread)
        {
            n.IsRead = true;
            await _notificationRepo.UpdateAsync(n);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckAndCreateNotificationsAsync()
    {
        var medicines = (await _medicineRepo.GetAllAsync()).ToList();

        foreach (var medicine in medicines)
        {
            var totalStock = medicine.Batches.Sum(b => b.Balance);

            if (totalStock == 0 && medicine.IsActive)
            {
                if (await _notificationRepo.GetActiveByTypeAsync("OUT_OF_STOCK") == null)
                {
                    await _notificationRepo.AddAsync(new Notification
                    {
                        Title = "Out of Stock",
                        Message = $"{medicine.MedicineName} is out of stock",
                        NotificationType = "OUT_OF_STOCK"
                    });
                }
            }
            else if (totalStock <= medicine.LowStockThreshold && totalStock > 0)
            {
                if (await _notificationRepo.GetActiveByTypeAsync("LOW_STOCK") == null)
                {
                    await _notificationRepo.AddAsync(new Notification
                    {
                        Title = "Low Stock",
                        Message = $"{medicine.MedicineName} stock is below threshold ({totalStock}/{medicine.LowStockThreshold})",
                        NotificationType = "LOW_STOCK"
                    });
                }
            }

            if (medicine.Batches.Any(b => b.Balance > 0 && b.ExpiryDate <= DateTime.Now.AddMonths(6)))
            {
                await _notificationRepo.AddAsync(new Notification
                {
                    Title = "Expiry Alert",
                    Message = $"{medicine.MedicineName} has batches expiring within 6 months",
                    NotificationType = "EXPIRY_ALERT"
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
