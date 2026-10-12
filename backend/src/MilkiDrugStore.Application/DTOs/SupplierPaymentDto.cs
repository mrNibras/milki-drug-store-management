namespace MilkiDrugStore.Application.DTOs.SupplierPayment;

/// <summary>
/// Request to record a new supplier payment with explicit per-purchase
/// allocations. The sum of allocations must equal the payment amount.
/// </summary>
public class CreateSupplierPaymentRequest
{
    public int SupplierId { get; set; }
    public int BranchId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "cash";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public List<SupplierPaymentAllocationRequest> Allocations { get; set; } = new();
}

public class SupplierPaymentAllocationRequest
{
    public int PurchaseId { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public class SupplierPaymentResponse
{
    public int SupplierPaymentId { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SupplierPaymentAllocationResponse> Allocations { get; set; } = new();
}

public class SupplierPaymentAllocationResponse
{
    public int SupplierPaymentItemId { get; set; }
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
}

/// <summary>
/// Summary of a supplier's outstanding debt, computed from authoritative
/// purchase data (TotalAmount - AmountPaid), not from any stored balance.
/// </summary>
public class SupplierDebtSummaryResponse
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingDebt { get; set; }
    public int OutstandingPurchaseCount { get; set; }
    public List<SupplierDebtPurchaseResponse> OutstandingPurchases { get; set; } = new();
}

public class SupplierDebtPurchaseResponse
{
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
}