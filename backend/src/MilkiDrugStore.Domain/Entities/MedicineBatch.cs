namespace MilkiDrugStore.Domain.Entities;

public class MedicineBatch
{
    public int BatchId { get; set; }
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityIssued { get; set; }
    public int QuantityDamaged { get; set; }
    public int QuantityExpired { get; set; }
    public int RemainingQuantity => QuantityReceived - QuantityIssued - QuantityDamaged - QuantityExpired;
    public DateTime ExpiryDate { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime DateReceived { get; set; } = DateTime.Now;
    public int? SupplierId { get; set; }
    public string? Remarks { get; set; }

    public Medicine? Medicine { get; set; }
    public Branch? Branch { get; set; }
    public Supplier? Supplier { get; set; }
    public List<SaleItem> SaleItems { get; set; } = new();
    public List<PurchaseItem> PurchaseItems { get; set; } = new();
    public List<InventoryTransaction> InventoryTransactions { get; set; } = new();
    public List<DamageRecord> DamageRecords { get; set; } = new();
    public List<ExpiredRecord> ExpiredRecords { get; set; } = new();
}
