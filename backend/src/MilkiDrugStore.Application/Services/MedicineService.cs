using MilkiDrugStore.Application.DTOs.Medicine;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class MedicineService : IMedicineService
{
    private readonly IRepository<Medicine> _medicineRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<MedicineBatch> _batchRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public MedicineService(IRepository<Medicine> medicineRepo, IRepository<Category> categoryRepo,
        IRepository<MedicineBatch> batchRepo, IUnitOfWork unitOfWork, IAuditLogService auditLog)
    {
        _medicineRepo = medicineRepo;
        _categoryRepo = categoryRepo;
        _batchRepo = batchRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<IEnumerable<MedicineResponse>> GetAllAsync(string? search = null, int? categoryId = null)
    {
        var query = (await _medicineRepo.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.MedicineName.Contains(search) || m.GenericName.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId.Value);

        var medicines = query
            .Include(m => m.Category)
            .Include(m => m.UnitType)
            .Include(m => m.Batches)
            .OrderBy(m => m.MedicineName)
            .ToList();

        return medicines.Select(MapToResponse);
    }

    public async Task<MedicineResponse?> GetByIdAsync(int id)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.MedicineId == id);
        var medicine = medicines.Include(m => m.Category).Include(m => m.UnitType).Include(m => m.Batches).FirstOrDefault();
        if (medicine == null) return null;
        return MapToResponse(medicine);
    }

    public async Task<MedicineResponse> CreateAsync(CreateMedicineRequest request, int userId)
    {
        var medicine = new Medicine
        {
            MedicineName = request.MedicineName,
            GenericName = request.GenericName,
            CategoryId = request.CategoryId,
            UnitTypeId = request.UnitTypeId,
            LowStockThreshold = request.LowStockThreshold,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _medicineRepo.AddAsync(medicine);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created medicine: {medicine.MedicineName}", "Medicines", medicine.MedicineId);

        return await GetByIdAsync(medicine.MedicineId) ?? throw new Exception("Failed to create medicine");
    }

    public async Task<MedicineResponse?> UpdateAsync(int id, UpdateMedicineRequest request, int userId)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.MedicineId == id);
        var medicine = medicines.FirstOrDefault();
        if (medicine == null) return null;

        medicine.MedicineName = request.MedicineName;
        medicine.GenericName = request.GenericName;
        medicine.CategoryId = request.CategoryId;
        medicine.UnitTypeId = request.UnitTypeId;
        medicine.LowStockThreshold = request.LowStockThreshold;
        if (request.IsActive.HasValue)
            medicine.IsActive = request.IsActive.Value;

        await _medicineRepo.UpdateAsync(medicine);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated medicine: {medicine.MedicineName}", "Medicines", medicine.MedicineId);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.MedicineId == id);
        var medicine = medicines.FirstOrDefault();
        if (medicine == null) return;

        medicine.IsActive = false;
        await _medicineRepo.UpdateAsync(medicine);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Deleted medicine: {medicine.MedicineName}", "Medicines", medicine.MedicineId);
    }

    public async Task<MedicineResponse> AddBatchAsync(AddBatchRequest request, int userId)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.MedicineId == request.MedicineId);
        var medicine = medicines.Include(m => m.Batches).FirstOrDefault();
        if (medicine == null) throw new Exception("Medicine not found");

        var batch = new MedicineBatch
        {
            MedicineId = request.MedicineId,
            BatchNumber = request.BatchNumber,
            QuantityReceived = request.Quantity,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            ExpiryDate = request.ExpiryDate,
            DateReceived = DateTime.Now
        };

        await _batchRepo.AddAsync(batch);

        await _unitOfWork.InventoryTransactions.AddAsync(new Domain.Entities.InventoryTransaction
        {
            MedicineId = request.MedicineId,
            BatchId = batch.BatchId,
            TransactionType = TransactionType.Purchase.ToString(),
            Quantity = request.Quantity,
            UnitPrice = request.PurchasePrice,
            ReferenceType = "PURCHASE"
        });

        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Added batch {batch.BatchNumber} to {medicine.MedicineName}", "MedicineBatches", batch.BatchId);

        return await GetByIdAsync(request.MedicineId) ?? throw new Exception("Failed to add batch");
    }

    private static MedicineResponse MapToResponse(Medicine m)
    {
        return new MedicineResponse
        {
            MedicineId = m.MedicineId,
            MedicineName = m.MedicineName,
            GenericName = m.GenericName,
            CategoryId = m.CategoryId,
            CategoryName = m.Category?.Name ?? "",
            UnitTypeId = m.UnitTypeId,
            UnitTypeName = m.UnitType?.Name ?? "",
            LowStockThreshold = m.LowStockThreshold,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt,
            Batches = m.Batches.Select(b => new BatchResponse
            {
                BatchId = b.BatchId,
                BatchNumber = b.BatchNumber,
                PurchasePrice = b.PurchasePrice,
                SellingPrice = b.SellingPrice,
                QuantityReceived = b.QuantityReceived,
                QuantityIssued = b.QuantityIssued,
                QuantityDamaged = b.QuantityDamaged,
                QuantityExpired = b.QuantityExpired,
                Balance = b.Balance,
                ExpiryDate = b.ExpiryDate
            }).ToList()
        };
    }
}
