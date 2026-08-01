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

    public async Task<IEnumerable<MedicineResponse>> GetAllAsync(string? search = null, int? categoryId = null, int? branchId = null)
    {
        var query = (await _medicineRepo.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m => m.BrandName.Contains(term)
                || m.GenericName.Contains(term)
                || (m.ProductCode != null && m.ProductCode.Contains(term))
                || (m.Barcode != null && m.Barcode.Contains(term))
                || (m.Strength != null && m.Strength.Contains(term))
                || (m.DosageForm != null && m.DosageForm.Contains(term)));
        }

        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId.Value);

        var medicines = query
            .Include(m => m.Category)
            .Include(m => m.UnitType)
            .Include(m => m.Batches)
            .OrderBy(m => m.BrandName)
            .ToList();

        if (branchId.HasValue)
        {
            foreach (var m in medicines)
            {
                m.Batches = m.Batches.Where(b => b.BranchId == branchId.Value).ToList();
            }
        }

        return medicines.Select(MapToResponse);
    }

    public async Task<MedicineResponse?> GetByIdAsync(int id, int? branchId = null)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.ProductId == id);
        var medicine = medicines.Include(m => m.Category).Include(m => m.UnitType).Include(m => m.Batches).FirstOrDefault();
        if (medicine == null) return null;

        if (branchId.HasValue)
            medicine.Batches = medicine.Batches.Where(b => b.BranchId == branchId.Value).ToList();

        return MapToResponse(medicine);
    }

    public async Task<MedicineResponse> CreateAsync(CreateMedicineRequest request, int userId)
    {
        var medicine = new Medicine
        {
            ProductCode = await ResolveProductCodeAsync(request.ProductCode),
            BrandName = request.BrandName,
            GenericName = request.GenericName,
            Strength = request.Strength,
            DosageForm = request.DosageForm,
            Barcode = request.Barcode,
            Manufacturer = request.Manufacturer,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitTypeId = request.UnitTypeId,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            ReorderLevel = request.ReorderLevel,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _medicineRepo.AddAsync(medicine);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created medicine: {medicine.BrandName}", "Medicines", medicine.ProductId);

        return await GetByIdAsync(medicine.ProductId) ?? throw new Exception("Failed to create medicine");
    }

    public async Task<MedicineResponse?> UpdateAsync(int id, UpdateMedicineRequest request, int userId)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.ProductId == id);
        var medicine = medicines.FirstOrDefault();
        if (medicine == null) return null;

        medicine.ProductCode = string.IsNullOrWhiteSpace(request.ProductCode) ? medicine.ProductCode : request.ProductCode.Trim().ToUpper();
        medicine.BrandName = request.BrandName;
        medicine.GenericName = request.GenericName;
        medicine.Strength = request.Strength;
        medicine.DosageForm = request.DosageForm;
        medicine.Barcode = request.Barcode;
        medicine.Manufacturer = request.Manufacturer;
        medicine.Description = request.Description;
        medicine.CategoryId = request.CategoryId;
        medicine.UnitTypeId = request.UnitTypeId;
        medicine.PurchasePrice = request.PurchasePrice;
        medicine.SellingPrice = request.SellingPrice;
        medicine.ReorderLevel = request.ReorderLevel;
        if (request.IsActive.HasValue)
            medicine.IsActive = request.IsActive.Value;
        medicine.UpdatedDate = DateTime.Now;

        await _medicineRepo.UpdateAsync(medicine);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated medicine: {medicine.BrandName}", "Medicines", medicine.ProductId);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.ProductId == id);
        var medicine = medicines.FirstOrDefault();
        if (medicine == null) return;

        medicine.IsActive = false;
        await _medicineRepo.UpdateAsync(medicine);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Deleted medicine: {medicine.BrandName}", "Medicines", medicine.ProductId);
    }

    public async Task<MedicineResponse> AddBatchAsync(AddBatchRequest request, int userId, int? branchId = null)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.ProductId == request.ProductId);
        var medicine = medicines.Include(m => m.Batches).FirstOrDefault();
        if (medicine == null) throw new Exception("Medicine not found");

        var batch = new MedicineBatch
        {
            ProductId = request.ProductId,
            BranchId = branchId ?? 0,
            BatchNumber = request.BatchNumber,
            QuantityReceived = request.Quantity,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            ExpiryDate = request.ExpiryDate,
            ManufacturingDate = request.ManufacturingDate,
            SupplierId = request.SupplierId,
            DateReceived = DateTime.Now
        };

        await _batchRepo.AddAsync(batch);

        await _unitOfWork.InventoryTransactions.AddAsync(new Domain.Entities.InventoryTransaction
        {
            ProductId = request.ProductId,
            BatchId = batch.BatchId,
            TransactionType = TransactionType.Purchase.ToString(),
            Quantity = request.Quantity,
            UnitPrice = request.PurchasePrice,
            ReferenceType = "PURCHASE"
        });

        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Added batch {batch.BatchNumber} to {medicine.BrandName}", "MedicineBatches", batch.BatchId);

        return await GetByIdAsync(request.ProductId) ?? throw new Exception("Failed to add batch");
    }

    private async Task<string> ResolveProductCodeAsync(string? requested)
    {
        if (!string.IsNullOrWhiteSpace(requested))
            return requested.Trim().ToUpper();

        var count = (await _medicineRepo.GetAllAsync()).Count();
        return $"MED-{(count + 1):D5}";
    }

    private static MedicineResponse MapToResponse(Medicine m)
    {
        return new MedicineResponse
        {
            ProductId = m.ProductId,
            ProductCode = m.ProductCode,
            BrandName = m.BrandName,
            GenericName = m.GenericName,
            Strength = m.Strength,
            DosageForm = m.DosageForm,
            Barcode = m.Barcode,
            Manufacturer = m.Manufacturer,
            Description = m.Description,
            CategoryId = m.CategoryId,
            CategoryName = m.Category?.Name ?? "",
            UnitTypeId = m.UnitTypeId,
            UnitTypeName = m.UnitType?.Name ?? "",
            PurchasePrice = m.PurchasePrice,
            SellingPrice = m.SellingPrice,
            ReorderLevel = m.ReorderLevel,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt,
            UpdatedDate = m.UpdatedDate,
            Batches = m.Batches.Select(b => new BatchResponse
            {
                BatchId = b.BatchId,
                ProductId = b.ProductId,
                BatchNumber = b.BatchNumber,
                PurchasePrice = b.PurchasePrice,
                SellingPrice = b.SellingPrice,
                QuantityReceived = b.QuantityReceived,
                QuantityIssued = b.QuantityIssued,
                QuantityDamaged = b.QuantityDamaged,
                QuantityExpired = b.QuantityExpired,
                Balance = b.Balance,
                ExpiryDate = b.ExpiryDate,
                ManufacturingDate = b.ManufacturingDate,
                SupplierId = b.SupplierId,
                SupplierName = b.Supplier?.SupplierName ?? ""
            }).ToList()
        };
    }
}
