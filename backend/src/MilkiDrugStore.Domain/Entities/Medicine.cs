namespace MilkiDrugStore.Domain.Entities;

public class Medicine
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int UnitTypeId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }

    public Category? Category { get; set; }
    public UnitType? UnitType { get; set; }
    public List<MedicineBatch> Batches { get; set; } = new();
    public List<SaleItem> SaleItems { get; set; } = new();
    public List<PurchaseItem> PurchaseItems { get; set; } = new();
    public List<InventoryTransaction> InventoryTransactions { get; set; } = new();
}
