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
    private readonly IRepository<Sale> _saleRepo;
    private readonly IRepository<SaleItem> _saleItemRepo;
    private readonly IRepository<Medicine> _medicineRepo;
    private readonly IRepository<MedicineBatch> _batchRepo;
    private readonly IRepository<InventoryTransaction> _transactionRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly List<IDomainEvent> _domainEvents = new();

    public SaleService(
        IRepository<Sale> saleRepo,
        IRepository<SaleItem> saleItemRepo,
        IRepository<Medicine> medicineRepo,
        IRepository<MedicineBatch> batchRepo,
        IRepository<InventoryTransaction> transactionRepo,
        IRepository<Notification> notificationRepo,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog)
    {
        _saleRepo = saleRepo;
        _saleItemRepo = saleItemRepo;
        _medicineRepo = medicineRepo;
        _batchRepo = batchRepo;
        _transactionRepo = transactionRepo;
        _notificationRepo = notificationRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<SaleResponse> CreateAsync(CreateSaleRequest request, int userId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var saleCount = (await _saleRepo.GetAllAsync()).Count();
            var saleNumber = $"SAL-{DateTime.Now.Year}-{saleCount + 1:D5}";

            var totalAmount = 0m;
            var totalProfit = 0m;
            var saleItems = new List<SaleItem>();

            foreach (var item in request.Items)
            {
                var batches = await GetAvailableBatchesAsync(item.MedicineId);
                if (!batches.Any())
                    throw new InsufficientStockException($"No stock available for medicine ID {item.MedicineId}");

                var remainingQty = item.Quantity;

                foreach (var batch in batches)
                {
                    if (remainingQty <= 0) break;

                    var deductQty = Math.Min(remainingQty, batch.Balance);
                    if (deductQty <= 0) continue;

                    batch.QuantityIssued += deductQty;
                    remainingQty -= deductQty;

                    var profitPerUnit = batch.SellingPrice - batch.PurchasePrice;
                    var itemProfit = profitPerUnit * deductQty;
                    var itemTotal = batch.SellingPrice * deductQty;

                    totalAmount += itemTotal;
                    totalProfit += itemProfit;

                    saleItems.Add(new SaleItem
                    {
                        MedicineId = item.MedicineId,
                        BatchId = batch.BatchId,
                        Quantity = deductQty,
                        UnitPrice = batch.SellingPrice,
                        PurchasePrice = batch.PurchasePrice,
                        Profit = itemProfit,
                        SubTotal = itemTotal
                    });

                    await _transactionRepo.AddAsync(new InventoryTransaction
                    {
                        MedicineId = item.MedicineId,
                        BatchId = batch.BatchId,
                        TransactionType = TransactionType.Sale.ToString(),
                        Quantity = deductQty,
                        UnitPrice = batch.SellingPrice,
                        ReferenceId = null,
                        ReferenceType = "SALE",
                        CreatedBy = userId
                    });
                }

                if (remainingQty > 0)
                    throw new InsufficientStockException($"Insufficient stock for medicine ID {item.MedicineId}");
            }

            var sale = new Sale
            {
                SaleNumber = saleNumber,
                SaleDate = DateTime.Now,
                TotalAmount = totalAmount,
                TotalProfit = totalProfit,
                UserId = userId,
                Items = saleItems
            };

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
                var med = (await _medicineRepo.FindAsync(m => m.MedicineId == si.MedicineId)).FirstOrDefault();
                if (med != null)
                {
                    var batches = await GetAvailableBatchesAsync(med.MedicineId);
                    var currentStock = batches.Sum(b => b.Balance);
                    if (currentStock <= med.LowStockThreshold && currentStock > 0)
                    {
                        _domainEvents.Add(new StockLowEvent(med.MedicineId, currentStock));
                    }
                    if (currentStock == 0 && batches.Any())
                    {
                        _domainEvents.Add(new StockLowEvent(med.MedicineId, 0));
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

    public async Task<IEnumerable<SaleResponse>> GetAllAsync()
    {
        var sales = await _saleRepo.GetAllAsync();
        return sales.OrderByDescending(s => s.SaleDate).Select(MapToResponse);
    }

    public async Task<SaleResponse?> GetByIdAsync(int id)
    {
        var sale = await _saleRepo.GetByIdAsync(id);
        if (sale == null) return null;
        return MapToResponse(sale);
    }

    private async Task<List<MedicineBatch>> GetAvailableBatchesAsync(int medicineId)
    {
        var batches = (await _batchRepo.FindAsync(b => b.MedicineId == medicineId))
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.BatchId)
            .ToList();

        return batches.Where(b => b.Balance > 0).ToList();
    }

    private static SaleResponse MapToResponse(Sale s)
    {
        return new SaleResponse
        {
            SaleId = s.SaleId,
            SaleNumber = s.SaleNumber,
            SaleDate = s.SaleDate,
            TotalAmount = s.TotalAmount,
            TotalProfit = s.TotalProfit,
            UserId = s.UserId,
            UserName = s.User?.FullName ?? "",
            Items = s.Items.Select(i => new SaleItemResponse
            {
                SaleItemId = i.SaleItemId,
                MedicineId = i.MedicineId,
                MedicineName = i.Medicine?.MedicineName ?? "",
                BatchId = i.BatchId,
                BatchNumber = i.Batch?.BatchNumber ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void ClearDomainEvents() => _domainEvents.Clear();
}
