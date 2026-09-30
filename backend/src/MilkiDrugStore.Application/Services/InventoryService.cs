using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Application.DTOs.Sale;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Common;
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
    private readonly IRepository<CosmeticBatch> _cosmeticBatchRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    /// <summary>RecordedBy/createdBy value for entries written by the automatic expiry sweep.</summary>
    public const int SystemUserId = 0;

    public InventoryService(
        IRepository<InventoryTransaction> transactionRepo,
        IRepository<MedicineBatch> batchRepo,
        IRepository<Medicine> medicineRepo,
        IRepository<CosmeticBatch> cosmeticBatchRepo,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog)
    {
        _transactionRepo = transactionRepo;
        _batchRepo = batchRepo;
        _medicineRepo = medicineRepo;
        _cosmeticBatchRepo = cosmeticBatchRepo;
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

    public async Task RecordDamageAsync(int? batchId, int? cosmeticBatchId, int quantity, string reason, int recordedBy)
    {
        // Exactly one batch identifier must identify the affected batch.
        if (batchId.HasValue == cosmeticBatchId.HasValue)
        {
            throw new ArgumentException("Provide either a medicine batch or a cosmetic batch, not both.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Damage quantity must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required for damage.");
        }

        if (batchId.HasValue)
        {
            await RecordMedicineDamageAsync(batchId.Value, quantity, reason.Trim(), recordedBy);
        }
        else
        {
            await RecordCosmeticDamageAsync(cosmeticBatchId!.Value, quantity, reason.Trim(), recordedBy);
        }
    }

    private async Task RecordMedicineDamageAsync(int batchId, int quantity, string reason, int recordedBy)
    {
        var batch = await _batchRepo.GetByIdAsync(batchId);
        if (batch == null)
            throw new ArgumentException("Medicine batch not found.");

        var available = batch.RemainingQuantity;
        if (available <= 0)
            throw new ArgumentException("Medicine batch has no stock available to damage.");
        if (quantity > available)
            throw new ArgumentException($"Insufficient stock. Only {available} unit(s) available in this batch.");

        batch.QuantityDamaged += quantity;
        await _batchRepo.UpdateAsync(batch);

        var damageRecord = new DamageRecord
        {
            BranchId = batch.BranchId,
            BatchId = batch.BatchId,
            Quantity = quantity,
            Reason = reason,
            RecordedBy = recordedBy,
            RecordedDate = DateTime.UtcNow
        };
        await _unitOfWork.DamageRecords.AddAsync(damageRecord);

        await _transactionRepo.AddAsync(new InventoryTransaction
        {
            ProductId = batch.ProductId,
            BatchId = batchId,
            TransactionType = TransactionType.Damage.ToString(),
            Quantity = quantity,
            UnitPrice = batch.PurchasePrice,
            ReferenceType = "DAMAGE",
            CreatedBy = recordedBy,
            CreatedAt = DateTime.UtcNow
        });

        // Batch update, DamageRecord and InventoryTransaction persist together.
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(recordedBy, $"Recorded damage: {quantity} units for medicine batch {batch.BatchNumber}", "DamageRecords", batchId);
    }

    private async Task RecordCosmeticDamageAsync(int cosmeticBatchId, int quantity, string reason, int recordedBy)
    {
        var batch = await _cosmeticBatchRepo.GetByIdAsync(cosmeticBatchId);
        if (batch == null)
            throw new ArgumentException("Cosmetic batch not found.");

        var available = batch.Balance;
        if (available <= 0)
            throw new ArgumentException("Cosmetic batch has no stock available to damage.");
        if (quantity > available)
            throw new ArgumentException($"Insufficient stock. Only {available} unit(s) available in this batch.");

        batch.QuantityDamaged += quantity;
        await _cosmeticBatchRepo.UpdateAsync(batch);

        var damageRecord = new DamageRecord
        {
            BranchId = batch.BranchId,
            CosmeticId = batch.CosmeticId,
            CosmeticBatchId = batch.BatchId,
            Quantity = quantity,
            Reason = reason,
            RecordedBy = recordedBy,
            RecordedDate = DateTime.UtcNow
        };
        await _unitOfWork.DamageRecords.AddAsync(damageRecord);

        await _transactionRepo.AddAsync(new InventoryTransaction
        {
            CosmeticId = batch.CosmeticId,
            CosmeticBatchId = batch.BatchId,
            TransactionType = TransactionType.Damage.ToString(),
            Quantity = quantity,
            UnitPrice = batch.BuyingPrice,
            ReferenceType = "DAMAGE",
            CreatedBy = recordedBy,
            CreatedAt = DateTime.UtcNow
        });

        // Batch update, DamageRecord and InventoryTransaction persist together.
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(recordedBy, $"Recorded damage: {quantity} units for cosmetic batch {batch.BatchNumber}", "DamageRecords", cosmeticBatchId);
    }

    /// <summary>
    /// Automatic expiry sweep. Every batch whose expiry date has passed (Ethiopian
    /// business time) and which still holds stock is written off, together with a
    /// durable ExpiredRecord and InventoryTransaction. Re-running is safe: a batch
    /// is only processed while it still has stock, and its stock drops to zero as
    /// part of the same save.
    /// </summary>
    public async Task<ExpiredProcessingResult> ProcessExpiredInventoryAsync(CancellationToken cancellationToken = default)
    {
        // Expiry is compared against the Ethiopian business time, while every
        // persisted timestamp stays UTC to match the rest of the schema.
        var businessNow = BusinessClock.BusinessNowAsUtc;
        var result = new ExpiredProcessingResult();

        // RemainingQuantity is a computed CLR property and cannot be translated,
        // so the stock test is expressed with translatable columns and refined
        // in memory.
        var medicineBatchQuery = await _batchRepo
            .FindAsync(b => b.ExpiryDate <= businessNow
                            && b.QuantityIssued + b.QuantityDamaged + b.QuantityExpired < b.QuantityReceived);
        var medicineBatches = await medicineBatchQuery.ToListAsync(cancellationToken);

        foreach (var batch in medicineBatches)
        {
            var quantity = batch.RemainingQuantity;
            if (quantity <= 0)
                continue;

            // Guard against reprocessing a batch that already has expiry history.
            var medicineHistory = await _unitOfWork.ExpiredRecords
                .FindAsync(e => e.BatchId == batch.BatchId);
            if (await medicineHistory.AnyAsync(cancellationToken))
                continue;

            batch.QuantityExpired += quantity;

            await _unitOfWork.ExpiredRecords.AddAsync(new ExpiredRecord
            {
                BranchId = batch.BranchId,
                BatchId = batch.BatchId,
                Quantity = quantity,
                RecordedBy = SystemUserId,
                RecordedDate = DateTime.UtcNow
            });

            await _unitOfWork.InventoryTransactions.AddAsync(new InventoryTransaction
            {
                ProductId = batch.ProductId,
                BatchId = batch.BatchId,
                TransactionType = TransactionType.Expired.ToString(),
                Quantity = quantity,
                UnitPrice = batch.PurchasePrice,
                ReferenceType = "EXPIRED",
                CreatedBy = SystemUserId,
                CreatedAt = DateTime.UtcNow
            });

            result.MedicineBatchesProcessed++;
            result.TotalUnitsExpired += quantity;
        }

        var cosmeticBatchQuery = await _cosmeticBatchRepo
            .FindAsync(b => b.ExpiryDate != null
                            && b.ExpiryDate <= businessNow
                            && b.QuantityIssued + b.QuantityDamaged + b.QuantityExpired < b.QuantityReceived);
        var cosmeticBatches = await cosmeticBatchQuery.ToListAsync(cancellationToken);

        foreach (var batch in cosmeticBatches)
        {
            var quantity = batch.Balance;
            if (quantity <= 0)
                continue;

            var cosmeticHistory = await _unitOfWork.ExpiredRecords
                .FindAsync(e => e.CosmeticBatchId == batch.BatchId);
            if (await cosmeticHistory.AnyAsync(cancellationToken))
                continue;

            batch.QuantityExpired += quantity;

            await _unitOfWork.ExpiredRecords.AddAsync(new ExpiredRecord
            {
                BranchId = batch.BranchId,
                CosmeticId = batch.CosmeticId,
                CosmeticBatchId = batch.BatchId,
                Quantity = quantity,
                RecordedBy = SystemUserId,
                RecordedDate = DateTime.UtcNow
            });

            await _unitOfWork.InventoryTransactions.AddAsync(new InventoryTransaction
            {
                CosmeticId = batch.CosmeticId,
                CosmeticBatchId = batch.BatchId,
                TransactionType = TransactionType.Expired.ToString(),
                Quantity = quantity,
                UnitPrice = batch.BuyingPrice,
                ReferenceType = "EXPIRED",
                CreatedBy = SystemUserId,
                CreatedAt = DateTime.UtcNow
            });

            result.CosmeticBatchesProcessed++;
            result.TotalUnitsExpired += quantity;
        }

        if (result.MedicineBatchesProcessed > 0 || result.CosmeticBatchesProcessed > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task<IEnumerable<DamageRecordResponse>> GetDamagesAsync(int? branchId = null)
    {
        var query = await _unitOfWork.DamageRecords.GetAllAsync();
#pragma warning disable CS8602
        var damages = await query
            .Include(d => d.Batch)
            .ThenInclude(b => b.Medicine)
            .Include(d => d.Cosmetic)
            .Include(d => d.CosmeticBatch)
            .ToListAsync();
#pragma warning restore CS8602

        if (branchId.HasValue)
            damages = damages.Where(d => d.BranchId == branchId.Value).ToList();

        return damages
            .OrderByDescending(d => d.RecordedDate)
            .Select(d => new DamageRecordResponse
            {
                DamageId = d.DamageId,
                ProductType = d.CosmeticBatchId.HasValue ? "cosmetic" : "medicine",
                ProductId = d.CosmeticBatchId.HasValue ? d.CosmeticId : d.Batch?.ProductId,
                BatchId = d.BatchId,
                CosmeticId = d.CosmeticId,
                CosmeticBatchId = d.CosmeticBatchId,
                BatchNumber = d.CosmeticBatchId.HasValue
                    ? d.CosmeticBatch?.BatchNumber ?? string.Empty
                    : d.Batch?.BatchNumber ?? string.Empty,
                BrandName = d.CosmeticBatchId.HasValue
                    ? d.Cosmetic?.ProductName ?? string.Empty
                    : d.Batch?.Medicine?.BrandName ?? string.Empty,
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
#pragma warning disable CS8602
        var expired = await query
            .Include(e => e.Batch)
            .ThenInclude(b => b.Medicine)
            .Include(e => e.Cosmetic)
            .Include(e => e.CosmeticBatch)
            .ToListAsync();
#pragma warning restore CS8602

        if (branchId.HasValue)
            expired = expired.Where(e => e.BranchId == branchId.Value).ToList();

        return expired
            .OrderByDescending(e => e.RecordedDate)
            .Select(e => new ExpiredRecordResponse
            {
                ExpiredId = e.ExpiredId,
                ProductType = e.CosmeticBatchId.HasValue ? "cosmetic" : "medicine",
                ProductId = e.CosmeticBatchId.HasValue ? e.CosmeticId : e.Batch?.ProductId,
                BatchId = e.BatchId,
                CosmeticId = e.CosmeticId,
                CosmeticBatchId = e.CosmeticBatchId,
                BatchNumber = e.CosmeticBatchId.HasValue
                    ? e.CosmeticBatch?.BatchNumber ?? string.Empty
                    : e.Batch?.BatchNumber ?? string.Empty,
                BrandName = e.CosmeticBatchId.HasValue
                    ? e.Cosmetic?.ProductName ?? string.Empty
                    : e.Batch?.Medicine?.BrandName ?? string.Empty,
                Quantity = e.Quantity,
                RecordedDate = e.RecordedDate,
                RecordedBy = e.RecordedBy,
                BranchId = e.BranchId
            }).ToList();
    }
}
