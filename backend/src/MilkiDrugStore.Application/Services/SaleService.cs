using MilkiDrugStore.Application.DTOs.Sale;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Events;
using MilkiDrugStore.Domain.Exceptions;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepo;
    private readonly IRepository<SaleItem> _saleItemRepo;
    private readonly IRepository<Medicine> _medicineRepo;
    private readonly IRepository<MedicineBatch> _batchRepo;
    private readonly ICosmeticRepository _cosmeticRepo;
    private readonly IRepository<CosmeticBatch> _cosmeticBatchRepo;
    private readonly IRepository<InventoryTransaction> _transactionRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IRepository<Settings> _settingsRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Maximum discount a non-admin (pharmacist) may apply, as a fraction of unit price.</summary>
    private const decimal MaxPharmacistDiscountRate = 0.05m;

    public SaleService(
        ISaleRepository saleRepo,
        IRepository<SaleItem> saleItemRepo,
        IRepository<Medicine> medicineRepo,
        IRepository<MedicineBatch> batchRepo,
        ICosmeticRepository cosmeticRepo,
        IRepository<CosmeticBatch> cosmeticBatchRepo,
        IRepository<InventoryTransaction> transactionRepo,
        IRepository<Notification> notificationRepo,
        IRepository<Settings> settingsRepo,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog)
    {
        _saleRepo = saleRepo;
        _saleItemRepo = saleItemRepo;
        _medicineRepo = medicineRepo;
        _batchRepo = batchRepo;
        _cosmeticRepo = cosmeticRepo;
        _cosmeticBatchRepo = cosmeticBatchRepo;
        _transactionRepo = transactionRepo;
        _notificationRepo = notificationRepo;
        _settingsRepo = settingsRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<SaleResponse> CreateAsync(CreateSaleRequest request, int userId, string userRole, int? branchId = null)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);

            var currentYear = DateTime.UtcNow.Year;
            var nextSequence = await _saleRepo.GetNextSaleSequenceAsync(currentYear);
            var saleNumber = $"SAL-{currentYear}-{nextSequence:D5}";

            var totalAmount = 0m;
            var totalProfit = 0m;
            var totalDiscount = 0m;
            var saleItems = new List<SaleItem>();

            foreach (var item in request.Items)
            {
                var isCosmetic = item.ProductType?.Equals("cosmetic", StringComparison.OrdinalIgnoreCase) == true;

                if (isCosmetic)
                {
                    var cosmeticBatches = await GetAvailableCosmeticBatchesAsync(item.CosmeticId ?? item.ProductId, branchId, item.CosmeticBatchId);
                    if (!cosmeticBatches.Any())
                        throw new InsufficientStockException($"No stock available for cosmetic ID {item.CosmeticId ?? item.ProductId}");

                    var remainingQty = item.Quantity;
                    var requestedDiscount = Math.Max(0m, item.DiscountAmount);

                    foreach (var batch in cosmeticBatches)
                    {
                        if (remainingQty <= 0) break;

                        var deductQty = Math.Min(remainingQty, batch.Balance);
                        if (deductQty <= 0) continue;

                        batch.QuantityIssued += deductQty;
                        remainingQty -= deductQty;

                        var maxDiscountPerUnit = isAdmin ? batch.SellingPrice : batch.SellingPrice * MaxPharmacistDiscountRate;
                        var discountPerUnit = Math.Min(requestedDiscount, maxDiscountPerUnit);
                        if (discountPerUnit < 0) discountPerUnit = 0;

                        var effectiveUnitPrice = batch.SellingPrice - discountPerUnit;
                        var profitPerUnit = effectiveUnitPrice - batch.BuyingPrice;
                        var itemProfit = profitPerUnit * deductQty;
                        var itemTotal = effectiveUnitPrice * deductQty;

                        totalAmount += itemTotal;
                        totalProfit += itemProfit;
                        totalDiscount += discountPerUnit * deductQty;

                        saleItems.Add(new SaleItem
                        {
                            CosmeticId = batch.CosmeticId,
                            CosmeticBatchId = batch.BatchId,
                            Quantity = deductQty,
                            UnitPrice = batch.SellingPrice,
                            DiscountAmount = discountPerUnit,
                            PurchasePrice = batch.BuyingPrice,
                            Profit = itemProfit,
                            SubTotal = itemTotal
                        });

                        await _transactionRepo.AddAsync(new InventoryTransaction
                        {
                            CosmeticId = batch.CosmeticId,
                            CosmeticBatchId = batch.BatchId,
                            TransactionType = TransactionType.Sale.ToString(),
                            Quantity = deductQty,
                            UnitPrice = effectiveUnitPrice,
                            ReferenceType = "SALE",
                            CreatedBy = userId
                        });
                    }

                    if (remainingQty > 0)
                        throw new InsufficientStockException($"Insufficient stock for cosmetic ID {item.CosmeticId ?? item.ProductId}");
                }
                else
                {
                    var manualBatchMode = await IsManualBatchModeAsync(branchId);
                    var batches = await GetAvailableBatchesAsync(item.ProductId ?? 0, branchId, manualBatchMode ? item.BatchId : null);
                    if (!batches.Any())
                        throw new InsufficientStockException($"No stock available for product ID {item.ProductId}");

                    var remainingQty = item.Quantity;
                    var requestedDiscount = Math.Max(0m, item.DiscountAmount);

                    foreach (var batch in batches)
                    {
                        if (remainingQty <= 0) break;

                        var deductQty = Math.Min(remainingQty, batch.RemainingQuantity);
                        if (deductQty <= 0) continue;

                        batch.QuantityIssued += deductQty;
                        remainingQty -= deductQty;

                        // Enforce discount rules server-side: pharmacists are capped, admins are not.
                        var maxDiscountPerUnit = isAdmin ? batch.SellingPrice : batch.SellingPrice * MaxPharmacistDiscountRate;
                        var discountPerUnit = Math.Min(requestedDiscount, maxDiscountPerUnit);
                        if (discountPerUnit < 0) discountPerUnit = 0;

                        var effectiveUnitPrice = batch.SellingPrice - discountPerUnit;
                        var profitPerUnit = effectiveUnitPrice - batch.PurchasePrice;
                        var itemProfit = profitPerUnit * deductQty;
                        var itemTotal = effectiveUnitPrice * deductQty;

                        totalAmount += itemTotal;
                        totalProfit += itemProfit;
                        totalDiscount += discountPerUnit * deductQty;

                    saleItems.Add(new SaleItem
                    {
                        ProductId = item.ProductId,
                        BatchId = batch.BatchId,
                        Quantity = deductQty,
                        UnitPrice = batch.SellingPrice,
                        DiscountAmount = discountPerUnit,
                        PurchasePrice = batch.PurchasePrice,
                        Profit = itemProfit,
                        SubTotal = itemTotal
                    });

                        await _transactionRepo.AddAsync(new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            BatchId = batch.BatchId,
                            TransactionType = TransactionType.Sale.ToString(),
                            Quantity = deductQty,
                            UnitPrice = effectiveUnitPrice,
                            ReferenceId = null,
                            ReferenceType = "SALE",
                            CreatedBy = userId
                        });
                    }

                    if (remainingQty > 0)
                        throw new InsufficientStockException($"Insufficient stock for product ID {item.ProductId}");
                }
            }

            // A reason must accompany any applied discount.
            if (totalDiscount > 0 && string.IsNullOrWhiteSpace(request.DiscountReason))
                throw new Exception("A discount reason is required when applying a discount.");

            var sale = new Sale
            {
                SaleNumber = saleNumber,
                BranchId = branchId ?? 0,
                SaleDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                TotalProfit = totalProfit,
                TotalDiscount = totalDiscount,
                DiscountReason = totalDiscount > 0 ? request.DiscountReason : null,
                UserId = userId,
                PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "cash" : request.PaymentMethod.ToLower(),
                AmountPaid = request.AmountPaid,
                AmountDue = totalAmount - request.AmountPaid,
                ReferenceNumber = request.ReferenceNumber,
                Items = saleItems
            };

            if (sale.AmountPaid <= 0)
                sale.PaymentStatus = "unpaid";
            else if (sale.AmountPaid >= totalAmount)
            {
                sale.PaymentStatus = "paid";
                sale.AmountDue = 0;
            }
            else
                sale.PaymentStatus = "partial";

            await _saleRepo.AddAsync(sale);
            await _unitOfWork.SaveChangesAsync();

            foreach (var si in saleItems)
            {
                si.SaleId = sale.SaleId;
                await _saleItemRepo.UpdateAsync(si);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            await _auditLog.LogAsync(userId, $"Created Sale {saleNumber}", "Sales", sale.SaleId);

            _domainEvents.Add(new SaleCreatedEvent(sale.SaleId, totalAmount, userId));

            foreach (var si in saleItems)
            {
                var med = (await _medicineRepo.FindAsync(m => m.ProductId == si.ProductId)).FirstOrDefault();
                if (med != null)
                {
                    var batches = await GetAvailableBatchesAsync(med.ProductId, branchId);
                    var currentStock = batches.Sum(b => b.RemainingQuantity);
                    if (currentStock <= med.ReorderLevel && currentStock > 0)
                    {
                        _domainEvents.Add(new StockLowEvent(med.ProductId, currentStock));
                    }
                    if (currentStock == 0 && batches.Any())
                    {
                        _domainEvents.Add(new StockLowEvent(med.ProductId, 0));
                    }
                }

                if (si.CosmeticId.HasValue)
                {
                    var cosmeticBatches = await GetAvailableCosmeticBatchesAsync(si.CosmeticId, branchId);
                    var currentStock = cosmeticBatches.Sum(b => b.Balance);
                    if (currentStock <= cosmeticBatches.FirstOrDefault()?.LowStockThreshold && currentStock > 0)
                    {
                        _domainEvents.Add(new StockLowEvent(si.CosmeticId.Value, currentStock));
                    }
                    if (currentStock == 0 && cosmeticBatches.Any())
                    {
                        _domainEvents.Add(new StockLowEvent(si.CosmeticId.Value, 0));
                    }
                }
            }

            return MapToResponse(sale);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<IEnumerable<SaleResponse>> GetAllAsync(int? branchId = null)
    {
        var sales = await _saleRepo.GetAllAsync();
        var query = sales.AsQueryable();
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        var list = await query.OrderByDescending(s => s.SaleDate).ToListAsync();
        return list.Select(MapToResponse);
    }

    public async Task<SaleResponse?> GetByIdAsync(int id, int? branchId = null)
    {
        var sales = await _saleRepo.FindAsync(s => s.SaleId == id);
        var query = sales.AsQueryable();
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        var sale = query.FirstOrDefault();
        if (sale == null) return null;
        return MapToResponse(sale);
    }

    private async Task<bool> IsManualBatchModeAsync(int? branchId = null)
    {
        var settings = (await _settingsRepo.FindAsync(s => s.BranchId == (branchId ?? 0))).FirstOrDefault()
            ?? (await _settingsRepo.GetAllAsync()).FirstOrDefault();
        return settings?.BatchSelectionMode == BatchSelectionMode.ManualSelection;
    }

    private async Task<List<MedicineBatch>> GetAvailableBatchesAsync(int productId, int? branchId = null, int? manualBatchId = null)
    {
        var batches = (await _batchRepo.FindAsync(b => b.ProductId == productId))
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.BatchId)
            .ToList();

        if (manualBatchId.HasValue)
        {
            var selected = batches.FirstOrDefault(b => b.BatchId == manualBatchId.Value);
            if (selected != null && selected.RemainingQuantity > 0)
            {
                if (branchId.HasValue && selected.BranchId != branchId.Value)
                    return new List<MedicineBatch>();
                return new List<MedicineBatch> { selected };
            }
            return new List<MedicineBatch>();
        }

        if (branchId.HasValue)
            batches = batches.Where(b => b.BranchId == branchId.Value).ToList();

        batches = batches.Where(b => b.RemainingQuantity > 0).ToList();

        return batches;
    }

    private async Task<List<CosmeticBatch>> GetAvailableCosmeticBatchesAsync(int? cosmeticId, int? branchId = null, int? manualBatchId = null)
    {
        if (!cosmeticId.HasValue)
            return new List<CosmeticBatch>();

        var batches = (await _cosmeticBatchRepo.FindAsync(b => b.CosmeticId == cosmeticId.Value))
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.BatchId)
            .ToList();

        if (manualBatchId.HasValue)
        {
            var selected = batches.FirstOrDefault(b => b.BatchId == manualBatchId.Value);
            if (selected != null && selected.Balance > 0)
            {
                if (branchId.HasValue && selected.BranchId != branchId.Value)
                    return new List<CosmeticBatch>();
                return new List<CosmeticBatch> { selected };
            }
            return new List<CosmeticBatch>();
        }

        if (branchId.HasValue)
            batches = batches.Where(b => b.BranchId == branchId.Value).ToList();

        batches = batches.Where(b => b.Balance > 0).ToList();

        return batches;
    }

    private static SaleResponse MapToResponse(Sale s)
    {
        return new SaleResponse
        {
            SaleId = s.SaleId,
            SaleNumber = s.SaleNumber,
            BranchId = s.BranchId,
            SaleDate = s.SaleDate,
            TotalAmount = s.TotalAmount,
            TotalProfit = s.TotalProfit,
            TotalDiscount = s.TotalDiscount,
            DiscountReason = s.DiscountReason,
            UserId = s.UserId,
            UserName = s.User?.FullName ?? "",
            PaymentMethod = s.PaymentMethod,
            PaymentStatus = s.PaymentStatus,
            AmountPaid = s.AmountPaid,
            AmountDue = s.AmountDue,
            ReferenceNumber = s.ReferenceNumber,
            Items = s.Items.Select(i => new SaleItemResponse
            {
                SaleItemId = i.SaleItemId,
                ProductId = i.ProductId,
                ProductType = i.CosmeticId.HasValue ? "cosmetic" : "medicine",
                BrandName = i.Medicine?.BrandName ?? i.Cosmetic?.ProductName ?? "",
                BatchId = i.BatchId,
                CosmeticId = i.CosmeticId,
                CosmeticBatchId = i.CosmeticBatchId,
                BatchNumber = i.Batch?.BatchNumber ?? i.CosmeticBatch?.BatchNumber ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountAmount = i.DiscountAmount,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void ClearDomainEvents() => _domainEvents.Clear();
}
