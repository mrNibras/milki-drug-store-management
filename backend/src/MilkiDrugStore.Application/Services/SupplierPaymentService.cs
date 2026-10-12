using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.SupplierPayment;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class SupplierPaymentService : ISupplierPaymentService
{
    private readonly ISupplierPaymentRepository _paymentRepo;
    private readonly ISupplierService _supplierService;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IUnitOfWork _UnitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ILogger<SupplierPaymentService> _logger;
    private readonly ICurrentUserService _currentUser;

    public SupplierPaymentService(
        ISupplierPaymentRepository paymentRepo,
        ISupplierService supplierService,
        IPurchaseRepository purchaseRepo,
        IUnitOfWork UnitOfWork,
        IAuditLogService auditLog,
        ILogger<SupplierPaymentService> logger,
        ICurrentUserService currentUser)
    {
        _paymentRepo = paymentRepo;
        _supplierService = supplierService;
        _purchaseRepo = purchaseRepo;
        _UnitOfWork = UnitOfWork;
        _auditLog = auditLog;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IList<SupplierDebtSummaryResponse>> GetOutstandingDebtsAsync(int? branchId = null)
    {
        var suppliers = await _supplierService.GetAllAsync();
        var result = new List<SupplierDebtSummaryResponse>();

        foreach (var supplier in suppliers)
        {
            var purchases = await _purchaseRepo.GetBySupplierAsync(supplier.SupplierId, branchId);
            var outstandingPurchases = new List<SupplierDebtPurchaseResponse>();
            decimal totalPurchases = 0m;
            decimal totalPaid = 0m;

            foreach (var purchase in purchases)
            {
                var amountDue = purchase.TotalAmount - purchase.AmountPaid;
                if (amountDue <= 0) continue;

                totalPurchases += purchase.TotalAmount;
                totalPaid += purchase.AmountPaid;
                outstandingPurchases.Add(new SupplierDebtPurchaseResponse
                {
                    PurchaseId = purchase.PurchaseId,
                    PurchaseNumber = purchase.PurchaseNumber,
                    PurchaseDate = purchase.PurchaseDate,
                    TotalAmount = purchase.TotalAmount,
                    AmountPaid = purchase.AmountPaid,
                    AmountDue = amountDue,
                    PaymentStatus = purchase.PaymentStatus,
                    PaymentMethod = purchase.PaymentMethod
                });
            }

            if (outstandingPurchases.Count == 0) continue;

            result.Add(new SupplierDebtSummaryResponse
            {
                SupplierId = supplier.SupplierId,
                SupplierName = supplier.SupplierName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                TotalPurchases = totalPurchases,
                TotalPaid = totalPaid,
                OutstandingDebt = totalPurchases - totalPaid,
                OutstandingPurchaseCount = outstandingPurchases.Count,
                OutstandingPurchases = outstandingPurchases
            });
        }

        return result;
    }

    public async Task<SupplierDebtSummaryResponse?> GetSupplierDebtAsync(int supplierId, int? branchId = null)
    {
        var supplier = await _supplierService.GetByIdAsync(supplierId);
        if (supplier == null) return null;

        var purchases = await _purchaseRepo.GetBySupplierAsync(supplierId, branchId);
        var outstandingPurchases = new List<SupplierDebtPurchaseResponse>();
        decimal totalPurchases = 0m;
        decimal totalPaid = 0m;

        foreach (var purchase in purchases)
        {
            var amountDue = purchase.TotalAmount - purchase.AmountPaid;
            if (amountDue <= 0) continue;

            totalPurchases += purchase.TotalAmount;
            totalPaid += purchase.AmountPaid;
            outstandingPurchases.Add(new SupplierDebtPurchaseResponse
            {
                PurchaseId = purchase.PurchaseId,
                PurchaseNumber = purchase.PurchaseNumber,
                PurchaseDate = purchase.PurchaseDate,
                TotalAmount = purchase.TotalAmount,
                AmountPaid = purchase.AmountPaid,
                AmountDue = amountDue,
                PaymentStatus = purchase.PaymentStatus,
                PaymentMethod = purchase.PaymentMethod
            });
        }

        if (outstandingPurchases.Count == 0) return null;

        return new SupplierDebtSummaryResponse
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            TotalPurchases = totalPurchases,
            TotalPaid = totalPaid,
            OutstandingDebt = totalPurchases - totalPaid,
            OutstandingPurchaseCount = outstandingPurchases.Count,
            OutstandingPurchases = outstandingPurchases
        };
    }

    public async Task<IList<SupplierDebtPurchaseResponse>> GetOutstandingPurchasesAsync(int supplierId, int? branchId = null)
    {
        var purchases = await _purchaseRepo.GetBySupplierAsync(supplierId, branchId);
        var result = new List<SupplierDebtPurchaseResponse>();

        foreach (var purchase in purchases)
        {
            var amountDue = purchase.TotalAmount - purchase.AmountPaid;
            if (amountDue <= 0) continue;

            result.Add(new SupplierDebtPurchaseResponse
            {
                PurchaseId = purchase.PurchaseId,
                PurchaseNumber = purchase.PurchaseNumber,
                PurchaseDate = purchase.PurchaseDate,
                TotalAmount = purchase.TotalAmount,
                AmountPaid = purchase.AmountPaid,
                AmountDue = amountDue,
                PaymentStatus = purchase.PaymentStatus,
                PaymentMethod = purchase.PaymentMethod
            });
        }

        return result;
    }

    public async Task<IList<SupplierPaymentResponse>> GetPaymentHistoryAsync(int supplierId, int? branchId = null)
    {
        var payments = await _paymentRepo.GetBySupplierAsync(supplierId, branchId);
        var result = new List<SupplierPaymentResponse>();

        foreach (var payment in payments)
        {
            var response = new SupplierPaymentResponse
            {
                SupplierPaymentId = payment.SupplierPaymentId,
                SupplierId = payment.SupplierId,
                SupplierName = payment.Supplier?.SupplierName ?? string.Empty,
                BranchId = payment.BranchId,
                BranchName = payment.Branch?.BranchName ?? string.Empty,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                CreatedBy = payment.CreatedBy,
                CreatedByName = payment.CreatedByUser?.FullName ?? string.Empty,
                CreatedAt = payment.CreatedAt,
                Allocations = new List<SupplierPaymentAllocationResponse>()
            };

            foreach (var item in payment.Items)
            {
                response.Allocations.Add(new SupplierPaymentAllocationResponse
                {
                    SupplierPaymentItemId = item.SupplierPaymentItemId,
                    PurchaseId = item.PurchaseId,
                    PurchaseNumber = item.Purchase?.PurchaseNumber ?? string.Empty,
                    AllocatedAmount = item.AllocatedAmount
                });
            }

            result.Add(response);
        }

        return result;
    }

    public async Task<SupplierPaymentResponse> RecordPaymentAsync(CreateSupplierPaymentRequest request, int createdBy)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Payment amount must be greater than zero.");

        if (request.Allocations == null || request.Allocations.Count == 0)
            throw new InvalidOperationException("At least one purchase allocation is required.");

        var allocationSum = request.Allocations.Sum(a => a.AllocatedAmount);
        if (allocationSum != request.Amount)
            throw new InvalidOperationException(
                $"The sum of allocations ({allocationSum}) must equal the payment amount ({request.Amount}).");

        var supplier = await _supplierService.GetByIdAsync(request.SupplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID {request.SupplierId} was not found.");

        // Load outstanding purchases with concurrency-safe totals.
        var purchases = await _purchaseRepo.GetBySupplierAsync(request.SupplierId, request.BranchId);
        var purchaseDict = purchases.ToDictionary(p => p.PurchaseId);

        decimal totalOutstanding = 0m;
        foreach (var alloc in request.Allocations)
        {
            if (!purchaseDict.TryGetValue(alloc.PurchaseId, out var purchase))
                throw new KeyNotFoundException($"Purchase with ID {alloc.PurchaseId} was not found.");

            if (purchase.SupplierId != request.SupplierId)
                throw new InvalidOperationException(
                    $"Purchase {purchase.PurchaseNumber} does not belong to the selected supplier.");

            if (purchase.BranchId != request.BranchId)
                throw new InvalidOperationException(
                    $"Purchase {purchase.PurchaseNumber} does not belong to the selected branch.");

            var outstanding = purchase.TotalAmount - purchase.AmountPaid;
            if (alloc.AllocatedAmount > outstanding)
                throw new InvalidOperationException(
                    $"The allocation for purchase {purchase.PurchaseNumber} exceeds its outstanding balance of {outstanding}.");

            totalOutstanding += outstanding;
        }

        if (request.Amount > totalOutstanding)
            throw new InvalidOperationException(
                $"The payment amount exceeds the total outstanding debt by {request.Amount - totalOutstanding}. " +
                $"The maximum permitted payment is {totalOutstanding}.");

        // Idempotency: reject if an identical payment already exists for this supplier/branch/amount.
        var existing = await _paymentRepo.GetBySupplierAsync(request.SupplierId, request.BranchId);
        foreach (var payment in existing)
        {
            if (payment.Amount == request.Amount
                && payment.PaymentMethod == request.PaymentMethod
                && payment.ReferenceNumber == request.ReferenceNumber
                && Math.Abs((payment.PaymentDate - request.PaymentDate).TotalDays) < 1)
            {
                throw new InvalidOperationException(
                    "A duplicate payment with the same amount, method, reference, and date was already recorded.");
            }
        }

        await _UnitOfWork.BeginTransactionAsync();

        try
        {
            var payment = new SupplierPayment
            {
                SupplierId = request.SupplierId,
                BranchId = request.BranchId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate,
                PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber,
                Notes = request.Notes,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            var addedPayment = await _paymentRepo.AddAsync(payment);

            foreach (var alloc in request.Allocations)
            {
                var item = new SupplierPaymentItem
                {
                    SupplierPaymentId = addedPayment.SupplierPaymentId,
                    PurchaseId = alloc.PurchaseId,
                    AllocatedAmount = alloc.AllocatedAmount,
                    CreatedAt = DateTime.UtcNow
                };
                await _paymentRepo.AddPaymentItemAsync(item);
            }

            // Update each affected purchase's paid amount and status.
            foreach (var alloc in request.Allocations)
            {
                var purchase = await _purchaseRepo.GetByIdAsync(alloc.PurchaseId);
                if (purchase == null) continue;

                purchase.AmountPaid += alloc.AllocatedAmount;
                purchase.AmountDue = purchase.TotalAmount - purchase.AmountPaid;
                purchase.PaymentStatus = purchase.AmountDue <= 0
                    ? "paid"
                    : (purchase.AmountPaid > 0 ? "partial" : "unpaid");

                await _purchaseRepo.UpdateAsync(purchase);
            }

            await _UnitOfWork.CommitTransactionAsync();

            await _auditLog.LogAsync(
                userId: createdBy,
                action: "CreateSupplierPayment",
                tableName: "SupplierPayments",
                recordId: addedPayment.SupplierPaymentId);

            return await BuildPaymentResponseAsync(addedPayment.SupplierPaymentId);
        }
        catch
        {
            await _UnitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<SupplierPaymentResponse> BuildPaymentResponseAsync(int supplierPaymentId)
    {
        var payment = await _paymentRepo.GetWithItemsAsync(supplierPaymentId);
        if (payment == null)
            throw new KeyNotFoundException($"SupplierPayment with ID {supplierPaymentId} was not found.");

        var response = new SupplierPaymentResponse
        {
            SupplierPaymentId = payment.SupplierPaymentId,
            SupplierId = payment.SupplierId,
            SupplierName = payment.Supplier?.SupplierName ?? string.Empty,
            BranchId = payment.BranchId,
            BranchName = payment.Branch?.BranchName ?? string.Empty,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedBy = payment.CreatedBy,
            CreatedByName = payment.CreatedByUser?.FullName ?? string.Empty,
            CreatedAt = payment.CreatedAt,
            Allocations = new List<SupplierPaymentAllocationResponse>()
        };

        foreach (var item in payment.Items)
        {
            response.Allocations.Add(new SupplierPaymentAllocationResponse
            {
                SupplierPaymentItemId = item.SupplierPaymentItemId,
                PurchaseId = item.PurchaseId,
                PurchaseNumber = item.Purchase?.PurchaseNumber ?? string.Empty,
                AllocatedAmount = item.AllocatedAmount
            });
        }

        return response;
    }
}
