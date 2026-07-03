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

    public InventoryService(
        IRepository<InventoryTransaction> transactionRepo,
        IRepository<MedicineBatch> batchRepo,
        IRepository<Medicine> medicineRepo,
        IUnitOfWork unitOfWork)
    {
        _transactionRepo = transactionRepo;
        _batchRepo = batchRepo;
        _medicineRepo = medicineRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> GetCurrentStockAsync(int medicineId)
    {
        var batches = (await _batchRepo.FindAsync(b => b.MedicineId == medicineId)).ToList();
        return batches.Sum(b => b.Balance);
    }

    public async Task RecordDamageAsync(int batchId, int quantity, string reason, int recordedBy)
    {
        var batches = await _batchRepo.FindAsync(b => b.BatchId == batchId);
        var batch = batches.FirstOrDefault();
        if (batch == null) throw new Exception("Batch not found");
        if (quantity > batch.Balance) throw new Exception("Insufficient stock");

        batch.QuantityDamaged += quantity;
        await _batchRepo.UpdateAsync(batch);

        await _unitOfWork.InventoryTransactions.AddAsync(new InventoryTransaction
        {
            MedicineId = batch.MedicineId,
            BatchId = batchId,
            TransactionType = TransactionType.Damage.ToString(),
            Quantity = quantity,
            UnitPrice = batch.PurchasePrice,
            ReferenceType = "DAMAGE",
            CreatedBy = recordedBy
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecordExpiredAsync(int batchId, int quantity, int recordedBy)
    {
        var batches = await _batchRepo.FindAsync(b => b.BatchId == batchId);
        var batch = batches.FirstOrDefault();
        if (batch == null) throw new Exception("Batch not found");
        if (quantity > batch.Balance) throw new Exception("Insufficient stock");

        batch.QuantityExpired += quantity;
        await _batchRepo.UpdateAsync(batch);

        await _unitOfWork.InventoryTransactions.AddAsync(new InventoryTransaction
        {
            MedicineId = batch.MedicineId,
            BatchId = batchId,
            TransactionType = TransactionType.Expired.ToString(),
            Quantity = quantity,
            UnitPrice = batch.PurchasePrice,
            ReferenceType = "EXPIRED",
            CreatedBy = recordedBy
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<DamageRecord>> GetDamagesAsync()
    {
        return await _unitOfWork.DamageRecords.GetAllAsync();
    }

    public async Task<IEnumerable<ExpiredRecord>> GetExpiredAsync()
    {
        return await _unitOfWork.ExpiredRecords.GetAllAsync();
    }
}
