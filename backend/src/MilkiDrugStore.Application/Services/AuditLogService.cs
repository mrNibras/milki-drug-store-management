using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IRepository<AuditLog> _auditLogRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IRepository<AuditLog> auditLogRepo, IUnitOfWork unitOfWork, ILogger<AuditLogService> logger)
    {
        _auditLogRepo = auditLogRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogAsync(int userId, string action, string tableName, int? recordId = null)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                TableName = tableName,
                RecordId = recordId,
                CreatedAt = DateTime.Now
            };

            await _auditLogRepo.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log");
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId)
    {
        var logs = await _auditLogRepo.FindAsync(al => al.UserId == userId);
        return logs.OrderByDescending(al => al.CreatedAt);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var logs = await _auditLogRepo.GetAllAsync();
        return logs.Where(al => al.CreatedAt >= from && al.CreatedAt <= to).OrderByDescending(al => al.CreatedAt);
    }
}
