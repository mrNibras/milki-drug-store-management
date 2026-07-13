namespace MilkiDrugStore.Domain.Entities;

public class Purchase
{
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string PaymentStatus { get; set; } = "unpaid";
    public string? PaymentMethod { get; set; }
    public int CreatedBy { get; set; }

    public Supplier? Supplier { get; set; }
    public List<PurchaseItem> Items { get; set; } = new();
}
