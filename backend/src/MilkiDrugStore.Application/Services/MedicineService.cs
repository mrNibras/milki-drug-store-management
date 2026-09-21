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
    private readonly IRepository<MedicineBatch> _batchRepo;
    private readonly ICatalogService _catalog;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public MedicineService(IRepository<Medicine> medicineRepo,
        IRepository<MedicineBatch> batchRepo, ICatalogService catalog,
        IUnitOfWork unitOfWork, IAuditLogService auditLog)
    {
        _medicineRepo = medicineRepo;
        _batchRepo = batchRepo;
        _catalog = catalog;
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

        return await MapToResponsesAsync(medicines);
    }

    public async Task<MedicineResponse?> GetByIdAsync(int id, int? branchId = null)
    {
        var medicines = await _medicineRepo.FindAsync(m => m.ProductId == id);
        var medicine = medicines.Include(m => m.Batches).FirstOrDefault();
        if (medicine == null) return null;

        if (branchId.HasValue)
            medicine.Batches = medicine.Batches.Where(b => b.BranchId == branchId.Value).ToList();

        return await MapToResponseAsync(medicine);
    }

    public async Task<MedicineResponse> CreateAsync(CreateMedicineRequest request, int userId)
    {
        var categoryId = await ResolveCategoryIdAsync(request.CategoryId, request.NewCategoryName, userId);
        var unitTypeId = await ResolveUnitTypeIdAsync(request.UnitTypeId, request.NewUnitTypeName, userId);

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
            CategoryId = categoryId,
            UnitTypeId = unitTypeId,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            ReorderLevel = request.ReorderLevel,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
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

        var categoryId = await ResolveCategoryIdAsync(request.CategoryId, request.NewCategoryName, userId);
        var unitTypeId = await ResolveUnitTypeIdAsync(request.UnitTypeId, request.NewUnitTypeName, userId);

        medicine.ProductCode = string.IsNullOrWhiteSpace(request.ProductCode) ? medicine.ProductCode : request.ProductCode.Trim().ToUpper();
        medicine.BrandName = request.BrandName;
        medicine.GenericName = request.GenericName;
        medicine.Strength = request.Strength;
        medicine.DosageForm = request.DosageForm;
        medicine.Barcode = request.Barcode;
        medicine.Manufacturer = request.Manufacturer;
        medicine.Description = request.Description;
        medicine.CategoryId = categoryId;
        medicine.UnitTypeId = unitTypeId;
        medicine.PurchasePrice = request.PurchasePrice;
        medicine.SellingPrice = request.SellingPrice;
        medicine.ReorderLevel = request.ReorderLevel;
        if (request.IsActive.HasValue)
            medicine.IsActive = request.IsActive.Value;
        medicine.UpdatedDate = DateTime.UtcNow;

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
            DateReceived = DateTime.UtcNow
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

    private async Task<int> ResolveCategoryIdAsync(int requested, string? newName, int userId)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            var resolved = await _catalog.ResolveCategoryIdAsync(newName, userId);
            if (resolved.HasValue)
                return resolved.Value;
        }

        if (await _catalog.IsValidCategoryIdAsync(requested))
            return requested;

        throw new Exception("A valid category is required.");
    }

    private async Task<int> ResolveUnitTypeIdAsync(int requested, string? newName, int userId)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            var resolved = await _catalog.ResolveUnitTypeIdAsync(newName, userId);
            if (resolved.HasValue)
                return resolved.Value;
        }

        if (await _catalog.IsValidUnitTypeIdAsync(requested))
            return requested;

        throw new Exception("A valid unit type is required.");
    }

    public async Task<IEnumerable<MedicineSearchResponse>> SearchAsync(string query, int? branchId = null)
    {
        var allMedicines = await _medicineRepo.GetAllAsync();
        var medicines = allMedicines.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            medicines = medicines.Where(m =>
                m.BrandName.Contains(term)
                || m.GenericName.Contains(term)
                || (m.ProductCode != null && m.ProductCode.Contains(term))
                || (m.Barcode != null && m.Barcode.Contains(term))
                || (m.Strength != null && m.Strength.Contains(term))
                || (m.DosageForm != null && m.DosageForm.Contains(term)));
        }

        var filtered = medicines.ToList();

        if (branchId.HasValue)
        {
            foreach (var m in filtered)
            {
                m.Batches = m.Batches.Where(b => b.BranchId == branchId.Value).ToList();
            }
        }

        return filtered.Select(m => new MedicineSearchResponse
        {
            ProductId = m.ProductId,
            ProductCode = m.ProductCode,
            BrandName = m.BrandName,
            GenericName = m.GenericName,
            Strength = m.Strength,
            DosageForm = m.DosageForm,
            Barcode = m.Barcode,
            TotalStock = m.Batches.Sum(b => b.RemainingQuantity),
            SellingPrice = m.SellingPrice
        }).ToList();
    }

    private async Task<string> ResolveProductCodeAsync(string? requested)
    {
        if (!string.IsNullOrWhiteSpace(requested))
            return requested.Trim().ToUpper();

        var count = (await _medicineRepo.GetAllAsync()).Count();
        return $"MED-{(count + 1):D5}";
    }

    private async Task<List<MedicineResponse>> MapToResponsesAsync(IEnumerable<Medicine> medicines)
    {
        var list = medicines.ToList();
        var categoryNames = await _catalog.GetCategoryNamesAsync(list.Select(m => m.CategoryId));
        var unitTypeNames = await _catalog.GetUnitTypeNamesAsync(list.Select(m => m.UnitTypeId));

        return list.Select(m => MapToResponse(m, categoryNames, unitTypeNames)).ToList();
    }

    private async Task<MedicineResponse> MapToResponseAsync(Medicine m)
    {
        var categoryNames = await _catalog.GetCategoryNamesAsync(new[] { m.CategoryId });
        var unitTypeNames = await _catalog.GetUnitTypeNamesAsync(new[] { m.UnitTypeId });
        return MapToResponse(m, categoryNames, unitTypeNames);
    }

    private static MedicineResponse MapToResponse(Medicine m, IDictionary<int, string> categoryNames, IDictionary<int, string> unitTypeNames)
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
            CategoryName = categoryNames.TryGetValue(m.CategoryId, out var categoryName) ? categoryName : "",
            UnitTypeId = m.UnitTypeId,
            UnitTypeName = unitTypeNames.TryGetValue(m.UnitTypeId, out var unitTypeName) ? unitTypeName : "",
            PurchasePrice = m.PurchasePrice,
            SellingPrice = m.SellingPrice,
            ReorderLevel = m.ReorderLevel,
             IsActive = m.IsActive,
             CreatedDate = m.CreatedDate,
             UpdatedDate = m.UpdatedDate,
             TotalStock = m.Batches.Sum(b => b.RemainingQuantity),
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
                RemainingQuantity = b.RemainingQuantity,
                ExpiryDate = b.ExpiryDate,
                ManufacturingDate = b.ManufacturingDate,
                SupplierId = b.SupplierId,
                SupplierName = b.Supplier?.SupplierName ?? ""
            }).ToList()
        };
    }
}
