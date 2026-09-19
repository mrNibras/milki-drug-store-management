namespace MilkiDrugStore.Application.Interfaces;

public interface IRetentionService
{
    Task<int> CleanupOldAuditLogsAsync();
    Task<int> CleanupOldNotificationsAsync();
    Task<(int auditLogsDeleted, int notificationsDeleted)> RunFullCleanupAsync();
}
