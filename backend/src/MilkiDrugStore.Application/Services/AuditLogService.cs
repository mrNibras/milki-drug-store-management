using MilkiDrugStore.Application.DTOs;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task LogAsync(int userId, string action, string tableName, int? recordId = null, int? branchId = null)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                BranchId = branchId,
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

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync(int? branchId = null)
    {
        var query = _auditLogRepo.GetAllQueryable();
        if (branchId.HasValue)
            query = query.Where(al => al.BranchId == branchId.Value);
        var logs = await query // The query is now executed here
            .OrderByDescending(al => al.CreatedAt)
            .Include(al => al.User)
            .ToListAsync();
        
        return logs.Select(al => new AuditLogDto
        {
            AuditId = al.AuditId,
            UserId = al.UserId,
            UserName = al.User?.FullName ?? "System",
            Action = al.Action,
            TableName = al.TableName,
            RecordId = al.RecordId,
            CreatedAt = al.CreatedAt
        }).ToList();
    }

    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId, int? branchId = null)
    {
        var query = await _auditLogRepo.FindAsync(al => al.UserId == userId);
        if (branchId.HasValue)
            query = query.Where(al => al.BranchId == branchId.Value);
        var logs = await query
            .OrderByDescending(al => al.CreatedAt)
            .Include(al => al.User)
            .ToListAsync();

        return logs.Select(al => new AuditLogDto
        {
            AuditId = al.AuditId,
            UserId = al.UserId,
            UserName = al.User?.FullName ?? "System",
            Action = al.Action,
            TableName = al.TableName,
            RecordId = al.RecordId,
            CreatedAt = al.CreatedAt
        }).ToList();
    }

    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime from, DateTime to, int? branchId = null)
    {
        var query = await _auditLogRepo.GetAllAsync();
        if (branchId.HasValue)
            query = query.Where(al => al.BranchId == branchId.Value);
        var logs = await query
            .Where(al => al.CreatedAt >= from && al.CreatedAt <= to)
            .OrderByDescending(al => al.CreatedAt)
            .Include(al => al.User)
            .ToListAsync();

        return logs.Select(al => new AuditLogDto
        {
            AuditId = al.AuditId,
            UserId = al.UserId,
            UserName = al.User?.FullName ?? "System",
            Action = al.Action,
            TableName = al.TableName,
            RecordId = al.RecordId,
            CreatedAt = al.CreatedAt
        }).ToList();
    }
}
