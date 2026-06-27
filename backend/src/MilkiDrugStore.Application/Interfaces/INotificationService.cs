using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<IEnumerable<Notification>> GetUnreadAsync();
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync();
    Task CheckAndCreateNotificationsAsync();
}
