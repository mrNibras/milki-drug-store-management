using MilkiDrugStore.Application.DTOs;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(int userId, string action, string tableName, int? recordId = null);
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId);
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<AuditLogDto>> GetAllAsync();
}
