using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserAsync(int userId);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
}
