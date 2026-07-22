using MilkiDrugStore.Application.DTOs.Notification;

namespace MilkiDrugStore.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetAllAsync(int? branchId = null);
    Task<IEnumerable<NotificationResponse>> GetUnreadAsync(int? branchId = null);
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync(int? branchId = null);
    Task CheckAndCreateNotificationsAsync(int? branchId = null);
}
