namespace MilkiDrugStore.Domain.Entities;

public class CosmeticBatch
{
    public int BatchId { get; set; }
    public int CosmeticId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityReceived { get; set; }
    public int QuantityIssued { get; set; }
    public int QuantityDamaged { get; set; }
    public int QuantityExpired { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime DateReceived { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }

    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int LowStockThreshold { get; set; } = 10;

    public int BranchId { get; set; }
    public int? SupplierId { get; set; }

    public Cosmetic? Cosmetic { get; set; }
    public Branch? Branch { get; set; }
    public Supplier? Supplier { get; set; }
    public List<SaleItem> SaleItems { get; set; } = new();
    public List<PurchaseItem> PurchaseItems { get; set; } = new();
    public List<InventoryTransaction> InventoryTransactions { get; set; } = new();
    public List<DamageRecord> DamageRecords { get; set; } = new();
    public List<ExpiredRecord> ExpiredRecords { get; set; } = new();

    public int Balance => QuantityReceived - QuantityIssued - QuantityDamaged - QuantityExpired;
}
