namespace MilkiDrugStore.Domain.Entities;

public class Medicine
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string UnitType { get; set; } = "Tablet";
    public int LowStockThreshold { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Category? Category { get; set; }
    public List<MedicineBatch> Batches { get; set; } = new();
    public List<SaleItem> SaleItems { get; set; } = new();
    public List<PurchaseItem> PurchaseItems { get; set; } = new();
    public List<InventoryTransaction> InventoryTransactions { get; set; } = new();
}
