namespace MilkiDrugStore.Domain.Entities;

public class Sale
{
    public int SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalDiscount { get; set; }
    public string? DiscountReason { get; set; }
    public int UserId { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public string PaymentStatus { get; set; } = "paid";
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string? ReferenceNumber { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}
