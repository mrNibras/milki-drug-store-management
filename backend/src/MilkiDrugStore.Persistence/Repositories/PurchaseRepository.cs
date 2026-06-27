using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class PurchaseRepository : Repository<Purchase>, IPurchaseRepository
{
    public PurchaseRepository(AppDbContext context) : base(context) { }

    public async Task<Purchase?> GetWithItemsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Items)
            .ThenInclude(i => i.Medicine)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);
    }
}
