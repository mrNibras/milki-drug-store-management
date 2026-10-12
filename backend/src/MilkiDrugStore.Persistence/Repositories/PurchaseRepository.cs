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

    public async Task<int> GetNextPurchaseSequenceAsync(int year)
    {
        // Use PostgreSQL advisory lock to prevent concurrent duplicate generation
        var lockKey = 2000000 + year; // Unique lock key per year (different from sale)
        await _context.Database.ExecuteSqlRawAsync($"SELECT pg_advisory_xact_lock({lockKey})");

        var maxSequence = await _dbSet
            .Where(p => p.PurchaseNumber.StartsWith($"PUR-{year}-"))
            .Select(p => p.PurchaseNumber)
            .ToListAsync();

        var maxSeq = 0;
        foreach (var pn in maxSequence)
        {
            var parts = pn.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var seq))
            {
                maxSeq = Math.Max(maxSeq, seq);
            }
        }

        return maxSeq + 1;
    }

    public async Task<IQueryable<Purchase>> GetBySupplierAsync(int supplierId, int? branchId = null)
    {
        var query = _dbSet
            .Include(p => p.Supplier)
            .Where(p => p.SupplierId == supplierId)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(p => p.BranchId == branchId.Value);

        return await Task.FromResult(query);
    }
}
