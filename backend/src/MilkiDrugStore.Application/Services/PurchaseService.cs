using MilkiDrugStore.Application.DTOs.Purchase;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class PurchaseService : IPurchaseService
{
        private readonly IRepository<Purchase> _purchaseRepo;
        private readonly IRepository<PurchaseItem> _purchaseItemRepo;
        private readonly IRepository<Medicine> _medicineRepo;
        private readonly IRepository<MedicineBatch> _batchRepo;
        private readonly IRepository<Supplier> _supplierRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<UnitType> _unitTypeRepo;
        private readonly IRepository<InventoryTransaction> _transactionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLog;

        public PurchaseService(
            IRepository<Purchase> purchaseRepo,
            IRepository<PurchaseItem> purchaseItemRepo,
            IRepository<Medicine> medicineRepo,
            IRepository<MedicineBatch> batchRepo,
            IRepository<Supplier> supplierRepo,
            IRepository<Category> categoryRepo,
            IRepository<UnitType> unitTypeRepo,
            IRepository<InventoryTransaction> transactionRepo,
            IUnitOfWork unitOfWork,
            IAuditLogService auditLog)
    {
        _purchaseRepo = purchaseRepo;
        _purchaseItemRepo = purchaseItemRepo;
        _medicineRepo = medicineRepo;
            _batchRepo = batchRepo;
            _supplierRepo = supplierRepo;
            _categoryRepo = categoryRepo;
            _unitTypeRepo = unitTypeRepo;
            _transactionRepo = transactionRepo;
            _unitOfWork = unitOfWork;
            _auditLog = auditLog;
    }

    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, int createdBy)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var purchaseCount = (await _purchaseRepo.GetAllAsync()).Count();
            var purchaseNumber = $"PUR-{DateTime.Now.Year}-{purchaseCount + 1:D5}";

            var supplier = (await _supplierRepo.GetAllAsync()).FirstOrDefault(s => s.SupplierId == request.SupplierId);
            if (supplier == null) throw new Exception("Supplier not found");

            var totalAmount = request.Items.Sum(i => i.Quantity * i.PurchasePrice);

            var purchase = new Purchase
            {
                PurchaseNumber = purchaseNumber,
                SupplierId = request.SupplierId,
                PurchaseDate = request.PurchaseDate,
                TotalAmount = totalAmount,
                AmountPaid = request.AmountPaid,
                AmountDue = totalAmount - request.AmountPaid,
                PaymentStatus = !string.IsNullOrWhiteSpace(request.PaymentStatus)
                    ? request.PaymentStatus!.ToLower()
                    : (request.AmountPaid <= 0
                        ? "unpaid"
                        : (request.AmountPaid >= totalAmount ? "paid" : "partial")),
                PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "cash" : request.PaymentMethod.ToLower(),
                CreatedBy = createdBy
            };

            await _purchaseRepo.AddAsync(purchase);
            await _unitOfWork.SaveChangesAsync();

            foreach (var item in request.Items)
            {
                var medicine = (await _medicineRepo.FindAsync(m => m.MedicineId == item.MedicineId)).FirstOrDefault();

                // Auto-create the medicine if it does not exist yet.
                if (medicine == null)
                {
                    if (string.IsNullOrWhiteSpace(item.MedicineName))
                        throw new Exception($"Medicine ID {item.MedicineId} not found and no name was provided to create it");

                    var categoryId = item.CategoryId;
                    if (categoryId == null || (await _categoryRepo.GetByIdAsync(categoryId.Value)) == null)
                    {
                        categoryId = (await _categoryRepo.GetAllAsync()).FirstOrDefault()?.CategoryId;
                        if (categoryId == null)
                            throw new Exception("No category exists to assign the new medicine");
                    }

                    var unitTypeId = (await _unitTypeRepo.GetAllAsync())
                        .FirstOrDefault(u => u.Name.Equals(item.UnitType, StringComparison.OrdinalIgnoreCase))?.UnitTypeId
                        ?? (await _unitTypeRepo.GetAllAsync()).FirstOrDefault()?.UnitTypeId
                        ?? 1;

                    medicine = new Medicine
                    {
                        MedicineName = item.MedicineName,
                        GenericName = item.GenericName ?? item.MedicineName,
                        CategoryId = categoryId.Value,
                        UnitTypeId = unitTypeId,
                        LowStockThreshold = item.LowStockThreshold > 0 ? item.LowStockThreshold : 10,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    await _medicineRepo.AddAsync(medicine);
                    await _unitOfWork.SaveChangesAsync();

                    await _auditLog.LogAsync(createdBy, $"Auto-created medicine: {medicine.MedicineName}", "Medicines", medicine.MedicineId);
                }

                var batch = new MedicineBatch
                {
                    MedicineId = medicine.MedicineId,
                    BatchNumber = item.BatchNumber,
                    QuantityReceived = item.Quantity,
                    PurchasePrice = item.PurchasePrice,
                    SellingPrice = item.SellingPrice,
                    ExpiryDate = item.ExpiryDate ?? DateTime.Now.AddYears(2),
                    DateReceived = DateTime.Now
                };

                await _batchRepo.AddAsync(batch);
                await _unitOfWork.SaveChangesAsync();

                var purchaseItem = new PurchaseItem
                {
                    PurchaseId = purchase.PurchaseId,
                    MedicineId = medicine.MedicineId,
                    BatchId = batch.BatchId,
                    BatchNumber = item.BatchNumber,
                    Quantity = item.Quantity,
                    PurchasePrice = item.PurchasePrice,
                    SubTotal = item.Quantity * item.PurchasePrice,
                    ExpiryDate = item.ExpiryDate
                };

                await _purchaseItemRepo.AddAsync(purchaseItem);

                await _transactionRepo.AddAsync(new InventoryTransaction
                {
                    MedicineId = medicine.MedicineId,
                    BatchId = batch.BatchId,
                    TransactionType = TransactionType.Purchase.ToString(),
                    Quantity = item.Quantity,
                    UnitPrice = item.PurchasePrice,
                    ReferenceId = purchase.PurchaseId,
                    ReferenceType = "PURCHASE",
                    CreatedBy = createdBy
                });
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            await _auditLog.LogAsync(createdBy, "Created Purchase", "Purchases", purchase.PurchaseId);

            var result = await GetByIdAsync(purchase.PurchaseId);
            return result ?? throw new Exception("Failed to create purchase");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<IEnumerable<PurchaseResponse>> GetAllAsync()
    {
        var purchases = await _purchaseRepo.GetAllAsync();
        var result = new List<PurchaseResponse>();

        foreach (var p in purchases.OrderByDescending(p => p.PurchaseDate))
        {
            var items = (await _purchaseItemRepo.FindAsync(pi => pi.PurchaseId == p.PurchaseId))
                .Include(pi => pi.Medicine).ToList();

            result.Add(new PurchaseResponse
            {
                PurchaseId = p.PurchaseId,
                PurchaseNumber = p.PurchaseNumber,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier?.SupplierName ?? "",
                PurchaseDate = p.PurchaseDate,
                TotalAmount = p.TotalAmount,
                AmountPaid = p.AmountPaid,
                AmountDue = p.AmountDue,
                PaymentStatus = p.PaymentStatus,
                PaymentMethod = p.PaymentMethod,
                Items = items.Select(pi => new PurchaseItemResponse
                {
                    PurchaseItemId = pi.PurchaseItemId,
                    MedicineId = pi.MedicineId,
                    MedicineName = pi.Medicine?.MedicineName ?? "",
                    BatchNumber = pi.BatchNumber,
                    Quantity = pi.Quantity,
                    PurchasePrice = pi.PurchasePrice,
                    SubTotal = pi.SubTotal
                }).ToList()
            });
        }

        return result;
    }

    public async Task<PurchaseResponse?> GetByIdAsync(int id)
    {
        var purchases = await _purchaseRepo.FindAsync(p => p.PurchaseId == id);
        var purchase = purchases.Include(p => p.Supplier).Include(p => p.Items).ThenInclude(i => i.Medicine).FirstOrDefault();
        if (purchase == null) return null;

        return new PurchaseResponse
        {
            PurchaseId = purchase.PurchaseId,
            PurchaseNumber = purchase.PurchaseNumber,
            SupplierId = purchase.SupplierId,
            SupplierName = purchase.Supplier?.SupplierName ?? "",
            PurchaseDate = purchase.PurchaseDate,
            TotalAmount = purchase.TotalAmount,
            AmountPaid = purchase.AmountPaid,
            AmountDue = purchase.AmountDue,
            PaymentStatus = purchase.PaymentStatus,
            PaymentMethod = purchase.PaymentMethod,
            Items = purchase.Items.Select(pi => new PurchaseItemResponse
            {
                PurchaseItemId = pi.PurchaseItemId,
                MedicineId = pi.MedicineId,
                MedicineName = pi.Medicine?.MedicineName ?? "",
                BatchNumber = pi.BatchNumber,
                Quantity = pi.Quantity,
                PurchasePrice = pi.PurchasePrice,
                SubTotal = pi.SubTotal
            }).ToList()
        };
    }
}
