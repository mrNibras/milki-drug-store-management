using MilkiDrugStore.Application.DTOs.Notification;

namespace MilkiDrugStore.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetAllAsync();
    Task<IEnumerable<NotificationResponse>> GetUnreadAsync();
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync();
    Task CheckAndCreateNotificationsAsync();
}
