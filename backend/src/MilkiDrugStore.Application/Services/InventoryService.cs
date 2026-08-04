using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Application.DTOs.Sale;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;

namespace MilkiDrugStore.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IRepository<InventoryTransaction> _transactionRepo;
    private readonly IRepository<MedicineBatch> _batchRepo;
    private readonly IRepository<Medicine> _medicineRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public InventoryService(
        IRepository<InventoryTransaction> transactionRepo,
        IRepository<MedicineBatch> batchRepo,
        IRepository<Medicine> medicineRepo,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog)
    {
        _transactionRepo = transactionRepo;
        _batchRepo = batchRepo;
        _medicineRepo = medicineRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<int> GetCurrentStockAsync(int productId, int? branchId = null)
    {
        var batches = (await _batchRepo.FindAsync(b => b.ProductId == productId)).ToList();
        if (branchId.HasValue)
            batches = batches.Where(b => b.BranchId == branchId.Value).ToList();
        return batches.Sum(b => b.RemainingQuantity);
    }

    public async Task RecordDamageAsync(int batchId, int quantity, string reason, int recordedBy)
    {
        var batches = await _batchRepo.FindAsync(b => b.BatchId == batchId);
        var batch = batches.FirstOrDefault();
        if (batch == null) throw new Exception("Batch not found");
        if (quantity > batch.RemainingQuantity) throw new Exception("Insufficient stock");

        batch.QuantityDamaged += quantity;
        await _batchRepo.UpdateAsync(batch);

        await _unitOfWork.InventoryTransactions.AddAsync(new InventoryTransaction
        {
            ProductId = batch.ProductId,
            BatchId = batchId,
            TransactionType = TransactionType.Damage.ToString(),
            Quantity = quantity,
            UnitPrice = batch.PurchasePrice,
            ReferenceType = "DAMAGE",
            CreatedBy = recordedBy
        });

        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(recordedBy, $"Recorded damage: {quantity} units for batch {batch.BatchNumber}", "DamageRecords", batchId);
    }

    public async Task RecordExpiredAsync(int batchId, int quantity, int recordedBy)
    {
        var batches = await _batchRepo.FindAsync(b => b.BatchId == batchId);
        var batch = batches.FirstOrDefault();
        if (batch == null) throw new Exception("Batch not found");
        if (quantity > batch.RemainingQuantity) throw new Exception("Insufficient stock");

        batch.QuantityExpired += quantity;
        await _batchRepo.UpdateAsync(batch);

        await _unitOfWork.InventoryTransactions.AddAsync(new InventoryTransaction
        {
            ProductId = batch.ProductId,
            BatchId = batchId,
            TransactionType = TransactionType.Expired.ToString(),
            Quantity = quantity,
            UnitPrice = batch.PurchasePrice,
            ReferenceType = "EXPIRED",
            CreatedBy = recordedBy
        });

        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(recordedBy, $"Recorded expired: {quantity} units for batch {batch.BatchNumber}", "ExpiredRecords", batchId);
    }

    public async Task<IEnumerable<DamageRecordResponse>> GetDamagesAsync(int? branchId = null)
    {
        var query = await _unitOfWork.DamageRecords.GetAllAsync();
        var damages = await query
            .Include(d => d.Batch)
            .ThenInclude(b => b.Medicine)
            .ToListAsync();

        if (branchId.HasValue)
            damages = damages.Where(d => d.BranchId == branchId.Value).ToList();

        return damages.Select(d => new DamageRecordResponse
        {
            DamageId = d.DamageId,
            BatchId = d.BatchId,
            BatchNumber = d.Batch?.BatchNumber ?? string.Empty,
            BrandName = d.Batch?.Medicine?.BrandName ?? string.Empty,
            Quantity = d.Quantity,
            Reason = d.Reason,
            RecordedBy = d.RecordedBy,
            RecordedDate = d.RecordedDate,
            BranchId = d.BranchId
        }).ToList();
    }

    public async Task<IEnumerable<ExpiredRecordResponse>> GetExpiredAsync(int? branchId = null)
    {
        var query = await _unitOfWork.ExpiredRecords.GetAllAsync();
        var expired = await query
            .Include(e => e.Batch)
            .ThenInclude(b => b.Medicine)
            .ToListAsync();

        if (branchId.HasValue)
            expired = expired.Where(e => e.BranchId == branchId.Value).ToList();

        return expired.Select(e => new ExpiredRecordResponse
        {
            ExpiredId = e.ExpiredId,
            BatchId = e.BatchId,
            BatchNumber = e.Batch?.BatchNumber ?? string.Empty,
            BrandName = e.Batch?.Medicine?.BrandName ?? string.Empty,
            Quantity = e.Quantity,
            RecordedDate = e.RecordedDate,
            RecordedBy = e.RecordedBy,
            BranchId = e.BranchId
        }).ToList();
    }
}
