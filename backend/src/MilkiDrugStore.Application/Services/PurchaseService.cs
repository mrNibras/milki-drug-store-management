using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.DTOs.Purchase;
using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Catalog;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class PurchaseService : IPurchaseService
{
        private readonly IRepository<Purchase> _purchaseRepo;
        private readonly IRepository<PurchaseItem> _purchaseItemRepo;
        private readonly IRepository<Medicine> _medicineRepo;
        private readonly IRepository<MedicineBatch> _batchRepo;
        private readonly IRepository<Supplier> _supplierRepo;
        private readonly ICatalogService _catalog;
        private readonly IRepository<InventoryTransaction> _transactionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<PurchaseService> _logger;
        private readonly ICosmeticRepository _cosmeticRepo;
        private readonly IRepository<CosmeticBatch> _cosmeticBatchRepo;

        public PurchaseService(
            IRepository<Purchase> purchaseRepo,
            IRepository<PurchaseItem> purchaseItemRepo,
            IRepository<Medicine> medicineRepo,
            IRepository<MedicineBatch> batchRepo,
            IRepository<Supplier> supplierRepo,
            ICatalogService catalog,
            IRepository<InventoryTransaction> transactionRepo,
            IUnitOfWork unitOfWork,
            IAuditLogService auditLog,
            ILogger<PurchaseService> logger,
            ICosmeticRepository cosmeticRepo,
            IRepository<CosmeticBatch> cosmeticBatchRepo)
    {
        _purchaseRepo = purchaseRepo;
        _purchaseItemRepo = purchaseItemRepo;
        _medicineRepo = medicineRepo;
        _batchRepo = batchRepo;
        _supplierRepo = supplierRepo;
        _catalog = catalog;
        _transactionRepo = transactionRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _logger = logger;
        _cosmeticRepo = cosmeticRepo;
        _cosmeticBatchRepo = cosmeticBatchRepo;
    }

    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, int createdBy, int? branchId = null)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("At least one purchase item is required.");
        if (request.Items.Any(i => i.Quantity <= 0 || i.PurchasePrice <= 0 || i.SellingPrice <= 0 ||
                                   string.IsNullOrWhiteSpace(i.BatchNumber) || !i.ExpiryDate.HasValue ||
                                   (i.ProductType != "cosmetic" && i.ExpiryDate.Value.Date <= DateTime.UtcNow.Date)))
            throw new InvalidOperationException("Each item requires a batch number, positive quantity and prices, and a future expiry date.");

        var effectiveBranchId = branchId ?? 0;
        if (effectiveBranchId <= 0)
        {
            var branches = await _unitOfWork.Branches.GetAllAsync();
            var defaultBranch = branches.FirstOrDefault();
            if (defaultBranch != null)
            {
                effectiveBranchId = defaultBranch.BranchId;
            }
        }

        _logger.LogInformation("Creating purchase. UserId: {UserId}, BranchId: {BranchId}, SupplierId: {SupplierId}, ItemCount: {ItemCount}",
            createdBy, effectiveBranchId, request.SupplierId, request.Items.Count);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var purchaseCount = (await _purchaseRepo.GetAllAsync()).Count();
            var purchaseNumber = $"PUR-{DateTime.UtcNow.Year}-{purchaseCount + 1:D5}";

            _logger.LogInformation("Generated purchase number: {PurchaseNumber}", purchaseNumber);

            var supplier = (await _supplierRepo.GetAllAsync()).FirstOrDefault(s => s.SupplierId == request.SupplierId);
            if (supplier == null)
            {
                _logger.LogWarning("Supplier not found. SupplierId: {SupplierId}", request.SupplierId);
                throw new Exception("Supplier not found");
            }

            var totalAmount = request.Items.Sum(i => i.Quantity * i.PurchasePrice);
            if (request.AmountPaid < 0 || request.AmountPaid > totalAmount)
                throw new InvalidOperationException("Amount paid must be between zero and the purchase total.");

            var purchase = new Purchase
            {
                PurchaseNumber = purchaseNumber,
                BranchId = effectiveBranchId,
                SupplierId = request.SupplierId,
                PurchaseDate = ToUtc(request.PurchaseDate),
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

            _logger.LogInformation("Purchase created. PurchaseId: {PurchaseId}, PurchaseNumber: {PurchaseNumber}",
                purchase.PurchaseId, purchase.PurchaseNumber);

            foreach (var item in request.Items)
            {
                var isCosmetic = item.ProductType?.Equals("cosmetic", StringComparison.OrdinalIgnoreCase) == true;
                _logger.LogInformation("Processing item. ProductType: {ProductType}, ProductId: {ProductId}, BatchNumber: {BatchNumber}, Qty: {Quantity}",
                    item.ProductType, item.ProductId, item.BatchNumber, item.Quantity);

                if (isCosmetic)
                {
                    var purchaseItem = await ProcessCosmeticItemAsync(item, purchase.PurchaseId, effectiveBranchId, request.SupplierId, createdBy);
                    await _purchaseItemRepo.AddAsync(purchaseItem);
                    await _transactionRepo.AddAsync(new InventoryTransaction
                    {
                        CosmeticId = purchaseItem.CosmeticId,
                        CosmeticBatchId = purchaseItem.CosmeticBatchId,
                        TransactionType = TransactionType.Purchase.ToString(),
                        Quantity = item.Quantity,
                        UnitPrice = item.PurchasePrice,
                        ReferenceId = purchase.PurchaseId,
                        ReferenceType = "PURCHASE",
                        CreatedBy = createdBy
                    });
                }
                else
                {
                    var purchaseItem = await ProcessMedicineItemAsync(item, purchase.PurchaseId, effectiveBranchId, request.SupplierId, createdBy);
                    await _purchaseItemRepo.AddAsync(purchaseItem);
                    await _transactionRepo.AddAsync(new InventoryTransaction
                    {
                        ProductId = purchaseItem.ProductId,
                        BatchId = purchaseItem.BatchId,
                        TransactionType = TransactionType.Purchase.ToString(),
                        Quantity = item.Quantity,
                        UnitPrice = item.PurchasePrice,
                        ReferenceId = purchase.PurchaseId,
                        ReferenceType = "PURCHASE",
                        CreatedBy = createdBy
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Purchase completed successfully. PurchaseId: {PurchaseId}", purchase.PurchaseId);

            await _auditLog.LogAsync(createdBy, "Created Purchase", "Purchases", purchase.PurchaseId);

            var result = await GetByIdAsync(purchase.PurchaseId);
            return result ?? throw new Exception("Failed to create purchase");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create purchase. SupplierId: {SupplierId}, ItemCount: {ItemCount}, BranchId: {BranchId}",
                request.SupplierId, request.Items?.Count ?? 0, effectiveBranchId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<IEnumerable<PurchaseResponse>> GetAllAsync(int? branchId = null)
    {
        var purchases = (await _purchaseRepo.GetAllAsync()).ToList();
        var query = purchases.AsQueryable();
        if (branchId.HasValue)
            query = query.Where(p => p.BranchId == branchId.Value);
        var result = new List<PurchaseResponse>();

        foreach (var p in query.OrderByDescending(p => p.PurchaseDate))
        {
            var items = (await _purchaseItemRepo.FindAsync(pi => pi.PurchaseId == p.PurchaseId))
                .Include(pi => pi.Medicine)
                .Include(pi => pi.Cosmetic)
                .ToList();

            result.Add(new PurchaseResponse
            {
                PurchaseId = p.PurchaseId,
                PurchaseNumber = p.PurchaseNumber,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier?.SupplierName ?? "",
                BranchId = p.BranchId,
                PurchaseDate = p.PurchaseDate,
                TotalAmount = p.TotalAmount,
                AmountPaid = p.AmountPaid,
                AmountDue = p.AmountDue,
                PaymentStatus = p.PaymentStatus,
                PaymentMethod = p.PaymentMethod,
                 Items = items.Select(pi => new PurchaseItemResponse
                 {
                     PurchaseItemId = pi.PurchaseItemId,
                     ProductId = pi.ProductId,
                     ProductName = pi.Medicine?.BrandName ?? pi.Cosmetic?.ProductName ?? "",
                     ProductType = pi.CosmeticId.HasValue ? "cosmetic" : "medicine",
                     BrandName = pi.Medicine?.BrandName ?? pi.Cosmetic?.ProductName ?? "",
                     BatchNumber = pi.BatchNumber,
                     Quantity = pi.Quantity,
                     PurchasePrice = pi.PurchasePrice,
                     SubTotal = pi.SubTotal,
                     CosmeticId = pi.CosmeticId,
                     CosmeticBatchId = pi.CosmeticBatchId
                 }).ToList()
            });
        }

        return result;
    }

    public async Task<PurchaseResponse?> GetByIdAsync(int id, int? branchId = null)
    {
        var purchases = await _purchaseRepo.FindAsync(p => p.PurchaseId == id);
        var query = purchases.AsQueryable();
        if (branchId.HasValue)
            query = query.Where(p => p.BranchId == branchId.Value);
        var purchase = query
            .Include(p => p.Supplier)
            .Include(p => p.Items)
                .ThenInclude(i => i.Medicine)
            .Include(p => p.Items)
                .ThenInclude(i => i.Cosmetic)
            .FirstOrDefault();
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
                 ProductId = pi.ProductId,
                 ProductName = pi.Medicine?.BrandName ?? pi.Cosmetic?.ProductName ?? "",
                 ProductType = pi.CosmeticId.HasValue ? "cosmetic" : "medicine",
                 BrandName = pi.Medicine?.BrandName ?? pi.Cosmetic?.ProductName ?? "",
                 BatchNumber = pi.BatchNumber,
                 Quantity = pi.Quantity,
                 PurchasePrice = pi.PurchasePrice,
                 SubTotal = pi.SubTotal,
                 CosmeticId = pi.CosmeticId,
                 CosmeticBatchId = pi.CosmeticBatchId
             }).ToList()
        };
    }

    private async Task<PurchaseItem> ProcessMedicineItemAsync(PurchaseItemRequest item, int purchaseId, int branchId, int supplierId, int createdBy)
    {
        var medicine = (await _medicineRepo.FindAsync(m => m.ProductId == (item.ProductId ?? 0)))
            .FirstOrDefault()
            ?? (await _medicineRepo.FindAsync(m => !string.IsNullOrWhiteSpace(item.ProductCode) && m.ProductCode == item.ProductCode.Trim().ToUpper()))
            .FirstOrDefault()
            ?? (await _medicineRepo.FindAsync(m => !string.IsNullOrWhiteSpace(item.Barcode) && m.Barcode == item.Barcode.Trim()))
            .FirstOrDefault();

        if (medicine == null)
        {
            if (string.IsNullOrWhiteSpace(item.BrandName))
                throw new Exception($"Product ID {item.ProductId} not found and no name was provided to create it");

            int categoryId;
            if (item.CategoryId.HasValue && await _catalog.IsValidCategoryIdAsync(item.CategoryId.Value))
            {
                categoryId = item.CategoryId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(item.CategoryName))
            {
                var resolvedCategory = await _catalog.ResolveCategoryIdAsync(item.CategoryName, createdBy);
                if (!resolvedCategory.HasValue)
                    throw new Exception("No category exists to assign the new medicine");
                categoryId = resolvedCategory.Value;
            }
            else
            {
                categoryId = MedicineCatalog.Categories[0].Id;
            }

            int unitTypeId;
            if (!string.IsNullOrWhiteSpace(item.UnitType))
            {
                var resolvedUnitType = await _catalog.ResolveUnitTypeIdAsync(item.UnitType, createdBy);
                if (!resolvedUnitType.HasValue)
                    throw new Exception("No unit type exists to assign the new medicine");
                unitTypeId = resolvedUnitType.Value;
            }
            else
            {
                unitTypeId = MedicineCatalog.UnitTypes[0].Id;
            }

            medicine = new Medicine
            {
                ProductCode = await ResolveProductCodeAsync(item.ProductCode),
                BrandName = item.BrandName,
                GenericName = item.GenericName ?? item.BrandName,
                Strength = item.Strength,
                DosageForm = item.DosageForm,
                Barcode = item.Barcode,
                CategoryId = categoryId,
                UnitTypeId = unitTypeId,
                ReorderLevel = item.ReorderLevel > 0 ? item.ReorderLevel : 10,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            await _medicineRepo.AddAsync(medicine);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Auto-created medicine. ProductId: {ProductId}, BrandName: {BrandName}",
                medicine.ProductId, medicine.BrandName);

            await _auditLog.LogAsync(createdBy, $"Auto-created medicine: {medicine.BrandName}", "Medicines", medicine.ProductId);
        }

        var normalizedBatchNumber = item.BatchNumber.Trim();
        var batch = (await _batchRepo.FindAsync(b => b.ProductId == medicine.ProductId &&
            b.BranchId == branchId && b.BatchNumber == normalizedBatchNumber)).FirstOrDefault();

        if (batch == null)
        {
            batch = new MedicineBatch
            {
                ProductId = medicine.ProductId,
                BranchId = branchId,
                BatchNumber = normalizedBatchNumber,
                QuantityReceived = item.Quantity,
                PurchasePrice = item.PurchasePrice,
                SellingPrice = item.SellingPrice,
                ExpiryDate = ToUtc(item.ExpiryDate!.Value),
                ManufacturingDate = ToUtc(item.ManufacturingDate),
                SupplierId = item.SupplierId ?? supplierId,
                DateReceived = DateTime.UtcNow
            };
            await _batchRepo.AddAsync(batch);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created new batch. BatchId: {BatchId}, BatchNumber: {BatchNumber}, ProductId: {ProductId}",
                batch.BatchId, batch.BatchNumber, medicine.ProductId);
        }
        else
        {
            batch.QuantityReceived += item.Quantity;
            batch.PurchasePrice = item.PurchasePrice;
            batch.SellingPrice = item.SellingPrice;
            batch.ExpiryDate = ToUtc(item.ExpiryDate!.Value);
            batch.ManufacturingDate = ToUtc(item.ManufacturingDate);
            batch.SupplierId = item.SupplierId ?? supplierId;
            await _batchRepo.UpdateAsync(batch);

            _logger.LogInformation("Updated existing batch. BatchId: {BatchId}, NewQtyReceived: {Qty}",
                batch.BatchId, batch.QuantityReceived);
        }

        return new PurchaseItem
        {
            PurchaseId = purchaseId,
            ProductId = medicine.ProductId,
            BatchId = batch.BatchId,
            BatchNumber = item.BatchNumber,
            Quantity = item.Quantity,
            PurchasePrice = item.PurchasePrice,
            SubTotal = item.Quantity * item.PurchasePrice,
            ExpiryDate = ToUtc(item.ExpiryDate)
        };
    }

    private async Task<PurchaseItem> ProcessCosmeticItemAsync(PurchaseItemRequest item, int purchaseId, int branchId, int supplierId, int createdBy)
    {
        int categoryId;
        if (item.CategoryId.HasValue && (CosmeticCatalog.IsBuiltInCategoryId(item.CategoryId.Value) || await _catalog.IsValidCategoryIdAsync(item.CategoryId.Value)))
        {
            categoryId = item.CategoryId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(item.CategoryName))
        {
            var resolvedCategory = CosmeticCatalog.FindCategoryId(item.CategoryName);
            if (resolvedCategory.HasValue)
            {
                categoryId = resolvedCategory.Value;
            }
            else
            {
                var resolved = await _catalog.ResolveCategoryIdAsync(item.CategoryName, createdBy);
                if (!resolved.HasValue)
                    throw new Exception("No category exists to assign the new cosmetic");
                categoryId = resolved.Value;
            }
        }
        else
        {
            categoryId = CosmeticCatalog.Categories[0].Id;
        }

        var cosmetic = item.ProductId.HasValue
            ? (await _cosmeticRepo.FindAsync(c => c.CosmeticId == item.ProductId)).FirstOrDefault()
            : null;

        if (cosmetic == null)
        {
            if (string.IsNullOrWhiteSpace(item.BrandName))
                throw new Exception($"Cosmetic ID {item.ProductId} not found and no name was provided to create it");

            cosmetic = new Cosmetic
            {
                ProductName = item.BrandName,
                Description = item.GenericName ?? item.BrandName,
                CategoryId = categoryId,
                BranchId = branchId,
                SupplierId = item.SupplierId ?? supplierId,
                Price = item.SellingPrice,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _cosmeticRepo.AddAsync(cosmetic);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Auto-created cosmetic. CosmeticId: {CosmeticId}, ProductName: {ProductName}",
                cosmetic.CosmeticId, cosmetic.ProductName);

            await _auditLog.LogAsync(createdBy, $"Auto-created cosmetic: {cosmetic.ProductName}", "Cosmetics", cosmetic.CosmeticId);
        }

        var normalizedBatchNumber = item.BatchNumber.Trim();
        var batch = (await _cosmeticBatchRepo.FindAsync(b => b.CosmeticId == cosmetic.CosmeticId &&
            b.BranchId == branchId && b.BatchNumber == normalizedBatchNumber)).FirstOrDefault();

        if (batch == null)
        {
            batch = new CosmeticBatch
            {
                CosmeticId = cosmetic.CosmeticId,
                BranchId = branchId,
                BatchNumber = normalizedBatchNumber,
                QuantityReceived = item.Quantity,
                BuyingPrice = item.PurchasePrice,
                SellingPrice = item.SellingPrice,
                ExpiryDate = ToUtc(item.ExpiryDate),
                LowStockThreshold = item.ReorderLevel > 0 ? item.ReorderLevel : 10,
                SupplierId = item.SupplierId ?? supplierId,
                DateReceived = DateTime.UtcNow
            };
            await _cosmeticBatchRepo.AddAsync(batch);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created new cosmetic batch. BatchId: {BatchId}, BatchNumber: {BatchNumber}, CosmeticId: {CosmeticId}",
                batch.BatchId, batch.BatchNumber, cosmetic.CosmeticId);
        }
        else
        {
            batch.QuantityReceived += item.Quantity;
            batch.BuyingPrice = item.PurchasePrice;
            batch.SellingPrice = item.SellingPrice;
            batch.ExpiryDate = ToUtc(item.ExpiryDate);
            batch.LowStockThreshold = item.ReorderLevel > 0 ? item.ReorderLevel : 10;
            batch.SupplierId = item.SupplierId ?? supplierId;
            await _cosmeticBatchRepo.UpdateAsync(batch);

            _logger.LogInformation("Updated existing cosmetic batch. BatchId: {BatchId}, NewQtyReceived: {Qty}",
                batch.BatchId, batch.QuantityReceived);
        }

        return new PurchaseItem
        {
            PurchaseId = purchaseId,
            ProductId = cosmetic.CosmeticId,
            CosmeticId = cosmetic.CosmeticId,
            CosmeticBatchId = batch.BatchId,
            BatchNumber = item.BatchNumber,
            Quantity = item.Quantity,
            PurchasePrice = item.PurchasePrice,
            SubTotal = item.Quantity * item.PurchasePrice,
            ExpiryDate = ToUtc(item.ExpiryDate)
        };
    }

    private async Task<string> ResolveProductCodeAsync(string? requested)
    {
        if (!string.IsNullOrWhiteSpace(requested))
            return requested.Trim().ToUpper();

        var count = (await _medicineRepo.GetAllAsync()).Count();
        return $"MED-{(count + 1):D5}";
    }

    private static DateTime ToUtc(DateTime dt)
    {
        return dt.Kind == DateTimeKind.Utc
            ? dt
            : dt.Kind == DateTimeKind.Local
                ? dt.ToUniversalTime()
                : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    private static DateTime? ToUtc(DateTime? dt) => dt.HasValue ? ToUtc(dt.Value) : null;
}
