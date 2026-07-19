using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetUnreadAsync()
    {
        return await _dbSet.Where(n => !n.IsRead).OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<Notification?> GetRecentExpiryAlertAsync(string medicineName, int days)
    {
        var since = DateTime.Now.AddDays(-days);
        return await _dbSet
            .Where(n => n.NotificationType == NotificationTypeStrings.ExpiryAlert
                     && n.Title.Contains(medicineName)
                     && n.CreatedAt >= since)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Notification?> GetActiveByTypeAsync(string notificationType)
    {
        return await _dbSet
            .Where(n => n.NotificationType == notificationType && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Notification?> GetActiveByTypeAndMedicineAsync(string notificationType, string medicineName)
    {
        return await _dbSet
            .Where(n => n.NotificationType == notificationType
                     && !n.IsRead
                     && n.Title.Contains(medicineName))
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
