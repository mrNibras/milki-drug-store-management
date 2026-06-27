using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class MedicineRepository : Repository<Medicine>, IMedicineRepository
{
    public MedicineRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Medicine>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(m => m.MedicineName.Contains(searchTerm) || m.GenericName.Contains(searchTerm))
            .Include(m => m.Category)
            .Include(m => m.Batches)
            .ToListAsync();
    }

    public async Task<IEnumerable<Medicine>> GetLowStockAsync()
    {
        return await _dbSet
            .Include(m => m.Batches)
            .Where(m => m.Batches.Sum(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired) <= m.LowStockThreshold
                     && m.Batches.Sum(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired) > 0)
            .ToListAsync();
    }

    public async Task<IEnumerable<Medicine>> GetExpiringAsync(int months)
    {
        var threshold = DateTime.Now.AddMonths(months);
        return await _dbSet
            .Include(m => m.Batches)
            .Where(m => m.Batches.Any(b => b.ExpiryDate <= threshold && (b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired) > 0))
            .ToListAsync();
    }

    public async Task<IEnumerable<Medicine>> GetOutOfStockAsync()
    {
        return await _dbSet
            .Include(m => m.Batches)
            .Where(m => m.Batches.All(b => b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired <= 0))
            .ToListAsync();
    }
}
