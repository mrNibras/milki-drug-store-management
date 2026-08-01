using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class InventoryTransactionRepository : Repository<InventoryTransaction>, IInventoryTransactionRepository
{
    public InventoryTransactionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<InventoryTransaction>> GetByMedicineAsync(int productId)
    {
        return await _dbSet
            .Where(it => it.ProductId == productId)
            .OrderByDescending(it => it.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(string transactionType)
    {
        return await _dbSet
            .Where(it => it.TransactionType == transactionType)
            .OrderByDescending(it => it.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(it => it.CreatedAt >= from && it.CreatedAt <= to)
            .OrderByDescending(it => it.CreatedAt)
            .ToListAsync();
    }
}
