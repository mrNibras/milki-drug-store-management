using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId)
    {
        return await _dbSet
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(al => al.CreatedAt >= from && al.CreatedAt <= to)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync();
    }
}
