using MilkiDrugStore.Application.DTOs.Notification;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
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

    public async Task<IEnumerable<NotificationResponse>> GetAllAsync(int? branchId = null)
    {
        var notifications = await _notificationRepo.GetAllAsync();
        var query = notifications.AsQueryable();
        if (branchId.HasValue)
            query = query.Where(n => n.BranchId == branchId.Value);
        return query.Select(n => new NotificationResponse
        {
            NotificationId = n.NotificationId,
            Title = n.Title,
            Message = n.Message,
            NotificationType = n.NotificationType,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<IEnumerable<NotificationResponse>> GetUnreadAsync(int? branchId = null)
    {
        var notifications = await _notificationRepo.GetUnreadAsync();
        var query = notifications.AsQueryable();
        if (branchId.HasValue)
            query = query.Where(n => n.BranchId == branchId.Value);
        return query.Select(n => new NotificationResponse
        {
            NotificationId = n.NotificationId,
            Title = n.Title,
            Message = n.Message,
            NotificationType = n.NotificationType,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
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

    public async Task MarkAllAsReadAsync(int? branchId = null)
    {
        var unread = await _notificationRepo.GetUnreadAsync();
        var query = unread.AsQueryable();
        if (branchId.HasValue)
            query = query.Where(n => n.BranchId == branchId.Value);
        foreach (var n in query)
        {
            n.IsRead = true;
            await _notificationRepo.UpdateAsync(n);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckAndCreateNotificationsAsync(int? branchId = null)
    {
        var medicines = (await _medicineRepo.GetAllAsync())
            .Include(m => m.Batches)
            .ToList();

        foreach (var medicine in medicines)
        {
            var batches = medicine.Batches.AsQueryable();
            if (branchId.HasValue)
                batches = batches.Where(b => b.BranchId == branchId.Value);
            var totalStock = batches.Sum(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired);

            if (totalStock == 0 && medicine.IsActive)
            {
                if (await _notificationRepo.GetActiveByTypeAndMedicineAsync(NotificationTypeStrings.OutOfStock, medicine.MedicineName) == null)
                {
                    var notification = new Notification
                    {
                        BranchId = branchId ?? 0,
                        Title = "Out of Stock",
                        Message = $"{medicine.MedicineName} is out of stock",
                        NotificationType = NotificationTypeStrings.OutOfStock
                    };
                    await _notificationRepo.AddAsync(notification);
                }
            }
            else if (totalStock <= medicine.LowStockThreshold && totalStock > 0)
            {
                if (await _notificationRepo.GetActiveByTypeAndMedicineAsync(NotificationTypeStrings.LowStock, medicine.MedicineName) == null)
                {
                    var notification = new Notification
                    {
                        BranchId = branchId ?? 0,
                        Title = "Low Stock",
                        Message = $"{medicine.MedicineName} stock is below threshold ({totalStock}/{medicine.LowStockThreshold})",
                        NotificationType = NotificationTypeStrings.LowStock
                    };
                    await _notificationRepo.AddAsync(notification);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
