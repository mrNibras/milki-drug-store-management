namespace MilkiDrugStore.Domain.Entities;

public class Supplier
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Purchase> Purchases { get; set; } = new();
}
