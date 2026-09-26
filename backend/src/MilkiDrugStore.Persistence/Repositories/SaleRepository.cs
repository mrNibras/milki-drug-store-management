using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;
using System.Linq;

namespace MilkiDrugStore.Persistence.Repositories;

public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context) { }

    public override async Task<IQueryable<Sale>> GetAllAsync()
    {
        return await Task.FromResult(_dbSet
            .Include(s => s.Items)
                .ThenInclude(i => i.Medicine)
            .Include(s => s.Items)
                .ThenInclude(i => i.Cosmetic)
            .Include(s => s.Items)
                .ThenInclude(i => i.Batch)
            .Include(s => s.Items)
                .ThenInclude(i => i.CosmeticBatch)
            .Include(s => s.User)
            .AsQueryable());
    }

    public async Task<Sale?> GetWithItemsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Items)
                .ThenInclude(i => i.Medicine)
            .Include(s => s.Items)
                .ThenInclude(i => i.Cosmetic)
            .Include(s => s.Items)
                .ThenInclude(i => i.CosmeticBatch)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SaleId == id);
    }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(s => s.SaleDate >= from && s.SaleDate <= to)
            .Include(s => s.User)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<int> GetNextSaleSequenceAsync(int year)
    {
        // Use PostgreSQL advisory lock to prevent concurrent duplicate generation
        // Lock key is derived from year to allow concurrent sales in different years
        var lockKey = 1000000 + year; // Unique lock key per year
        await _context.Database.ExecuteSqlRawAsync($"SELECT pg_advisory_xact_lock({lockKey})");

        // Get the maximum sequence number for the current year from existing SaleNumbers
        // SaleNumber format: SAL-YYYY-NNNNN
        var maxSequence = await _dbSet
            .Where(s => s.SaleNumber.StartsWith($"SAL-{year}-"))
            .Select(s => s.SaleNumber)
            .ToListAsync();

        var maxSeq = 0;
        foreach (var sn in maxSequence)
        {
            var parts = sn.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var seq))
            {
                maxSeq = Math.Max(maxSeq, seq);
            }
        }

        return maxSeq + 1;
    }
}
