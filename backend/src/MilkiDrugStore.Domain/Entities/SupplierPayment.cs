namespace MilkiDrugStore.Domain.Entities;

/// <summary>
/// Records an additional payment made to a supplier against outstanding
/// purchase debt. Each payment is an independent transaction; the original
/// purchase amounts and previous payment records are never overwritten.
/// </summary>
public class SupplierPayment
{
    public int SupplierPaymentId { get; set; }
    public int SupplierId { get; set; }
    public int BranchId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "cash";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Supplier? Supplier { get; set; }
    public Branch? Branch { get; set; }
    public User? CreatedByUser { get; set; }
    public List<SupplierPaymentItem> Items { get; set; } = new();
}

/// <summary>
/// Allocates a portion of a SupplierPayment to one specific outstanding
/// purchase. The sum of allocations for a payment must equal the payment
/// amount, so a payment is never applied ambiguously across purchases.
/// </summary>
public class SupplierPaymentItem
{
    public int SupplierPaymentItemId { get; set; }
    public int SupplierPaymentId { get; set; }
    public int PurchaseId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SupplierPayment? SupplierPayment { get; set; }
    public Purchase? Purchase { get; set; }
}