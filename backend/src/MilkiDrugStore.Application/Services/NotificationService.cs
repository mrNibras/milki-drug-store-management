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
    private readonly ICosmeticRepository _cosmeticRepo;
    private readonly IRepository<Branch> _branchRepo;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(INotificationRepository notificationRepo, IRepository<Medicine> medicineRepo, ICosmeticRepository cosmeticRepo, IUnitOfWork unitOfWork, IRepository<Branch> branchRepo)
    {
        _notificationRepo = notificationRepo;
        _medicineRepo = medicineRepo;
        _cosmeticRepo = cosmeticRepo;
        _unitOfWork = unitOfWork;
        _branchRepo = branchRepo;
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
        var branches = (await _branchRepo.GetAllAsync()).ToList();
        if (!branches.Any())
        {
            return;
        }

        var targetBranches = branchId.HasValue
            ? branches.Where(b => b.BranchId == branchId.Value).ToList()
            : branches;

        foreach (var branch in targetBranches)
        {
            await CheckAndCreateNotificationsForBranchAsync(branch.BranchId);
        }
    }

    private async Task CheckAndCreateNotificationsForBranchAsync(int branchId)
    {
        var medicines = (await _medicineRepo.GetAllAsync())
            .Include(m => m.Batches)
            .ToList();

        foreach (var medicine in medicines)
        {
            var batches = medicine.Batches.AsQueryable();
            batches = batches.Where(b => b.BranchId == branchId);
            var totalStock = batches.Sum(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired);

            if (totalStock == 0 && medicine.IsActive)
            {
                if (await _notificationRepo.GetActiveByTypeAndMedicineAsync(NotificationTypeStrings.OutOfStock, medicine.BrandName, branchId) == null)
                {
                    var notification = new Notification
                    {
                        BranchId = branchId,
                        Title = "Out of Stock",
                        Message = $"{medicine.BrandName} is out of stock",
                        NotificationType = NotificationTypeStrings.OutOfStock
                    };
                    await _notificationRepo.AddAsync(notification);
                }
            }
            else if (totalStock <= medicine.ReorderLevel && totalStock > 0)
            {
                if (await _notificationRepo.GetActiveByTypeAndMedicineAsync(NotificationTypeStrings.LowStock, medicine.BrandName, branchId) == null)
                {
                    var notification = new Notification
                    {
                        BranchId = branchId,
                        Title = "Low Stock",
                        Message = $"{medicine.BrandName} stock is below threshold ({totalStock}/{medicine.ReorderLevel})",
                        NotificationType = NotificationTypeStrings.LowStock
                    };
                    await _notificationRepo.AddAsync(notification);
                }
            }
        }

        var cosmetics = (await _cosmeticRepo.GetAllAsync())
            .Include(c => c.Batches)
            .ToList();

        foreach (var cosmetic in cosmetics)
        {
            var batches = cosmetic.Batches.AsQueryable();
            batches = batches.Where(b => b.BranchId == branchId);
            var totalStock = batches.Sum(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired);
            var lowStockThreshold = batches.Any() ? batches.Min(b => b.LowStockThreshold) : 0;

            if (totalStock == 0 && cosmetic.IsActive)
            {
                if (await _notificationRepo.GetActiveByTypeAndMedicineAsync(NotificationTypeStrings.OutOfStock, cosmetic.ProductName, branchId) == null)
                {
                    var notification = new Notification
                    {
                        BranchId = branchId,
                        Title = "Cosmetic Out of Stock",
                        Message = $"{cosmetic.ProductName} is out of stock",
                        NotificationType = NotificationTypeStrings.OutOfStock
                    };
                    await _notificationRepo.AddAsync(notification);
                }
            }
            else if (totalStock <= lowStockThreshold && totalStock > 0)
            {
                if (await _notificationRepo.GetActiveByTypeAndMedicineAsync(NotificationTypeStrings.LowStock, cosmetic.ProductName, branchId) == null)
                {
                    var notification = new Notification
                    {
                        BranchId = branchId,
                        Title = "Cosmetic Low Stock",
                        Message = $"{cosmetic.ProductName} stock is below threshold ({totalStock}/{lowStockThreshold})",
                        NotificationType = NotificationTypeStrings.LowStock
                    };
                    await _notificationRepo.AddAsync(notification);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
