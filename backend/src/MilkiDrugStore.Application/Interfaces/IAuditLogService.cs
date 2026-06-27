using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(int userId, string action, string tableName, int? recordId = null);
    Task<IEnumerable<AuditLog>> GetByUserAsync(int userId);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
}
