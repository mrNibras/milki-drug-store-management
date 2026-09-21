using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class CosmeticRepository : Repository<Cosmetic>, ICosmeticRepository
{
    public CosmeticRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Cosmetic>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(c => c.ProductName.Contains(searchTerm) || c.Description.Contains(searchTerm))
            .Include(c => c.Batches)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cosmetic>> GetLowStockAsync()
    {
        return await _dbSet
            .Include(c => c.Batches)
            .Where(c => c.Batches.Sum(b => b.Balance) <= 10 && c.Batches.Sum(b => b.Balance) > 0)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cosmetic>> GetExpiringAsync(int months)
    {
        var threshold = DateTime.UtcNow.AddMonths(months);
        return await _dbSet
            .Include(c => c.Batches)
            .Where(c => c.Batches.Any(b => b.ExpiryDate <= threshold && b.Balance > 0))
            .ToListAsync();
    }

    public async Task<IEnumerable<Cosmetic>> GetOutOfStockAsync()
    {
        return await _dbSet
            .Include(c => c.Batches)
            .Where(c => c.Batches.All(b => b.Balance <= 0))
            .ToListAsync();
    }
}
