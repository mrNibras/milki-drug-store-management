using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUnreadAsync();
    Task<Notification?> GetRecentExpiryAlertAsync(string brandName, int days);
    Task<Notification?> GetActiveByTypeAsync(string notificationType);
    Task<Notification?> GetActiveByTypeAndMedicineAsync(string notificationType, string brandName, int? branchId = null);
}
