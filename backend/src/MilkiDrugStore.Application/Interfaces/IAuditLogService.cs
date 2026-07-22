using MilkiDrugStore.Application.DTOs;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(int userId, string action, string tableName, int? recordId = null, int? branchId = null);
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId, int? branchId = null);
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime from, DateTime to, int? branchId = null);
    Task<IEnumerable<AuditLogDto>> GetAllAsync(int? branchId = null);
}
