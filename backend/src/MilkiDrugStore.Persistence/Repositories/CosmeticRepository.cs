using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class CosmeticRepository : Repository<Cosmetic>, ICosmeticRepository
{
    public CosmeticRepository(AppDbContext context) : base(context) { }

    // CosmeticBatch.Balance is a computed CLR property that the provider cannot
    // translate, so stock is always expressed with the underlying columns.

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
            .Where(c => c.Batches
                .Where(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired > 0)
                .Sum(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired) <= 10)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cosmetic>> GetExpiringAsync(int months)
    {
        var threshold = DateTime.UtcNow.AddMonths(months);
        return await _dbSet
            .Include(c => c.Batches)
            .Where(c => c.Batches.Any(b =>
                b.ExpiryDate != null
                && b.ExpiryDate <= threshold
                && b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired > 0))
            .ToListAsync();
    }

    public async Task<IEnumerable<Cosmetic>> GetOutOfStockAsync()
    {
        return await _dbSet
            .Include(c => c.Batches)
            .Where(c => !c.Batches.Any(b =>
                b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired > 0))
            .ToListAsync();
    }
}
