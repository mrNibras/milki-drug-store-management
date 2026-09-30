namespace MilkiDrugStore.Domain.Entities;

public class Cosmetic
{
    public List<DamageRecord> DamageRecords { get; set; } = new();
    public List<ExpiredRecord> ExpiredRecords { get; set; } = new();

    public int CosmeticId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Built-in cosmetic categories use negative ids (see <see cref="Catalog.CosmeticCatalog"/>);
    /// custom categories use the positive id of a persisted <see cref="Category"/>.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Built-in unit types use negative ids (see <see cref="Catalog.MedicineCatalog"/>);
    /// custom unit types use the positive id of a persisted <see cref="UnitType"/>.
    /// </summary>
    public int UnitTypeId { get; set; }

    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int BranchId { get; set; }
    public int? SupplierId { get; set; }

    public Branch? Branch { get; set; }
    public Supplier? Supplier { get; set; }
    public List<CosmeticBatch> Batches { get; set; } = new();
    public List<PurchaseItem> PurchaseItems { get; set; } = new();
    public List<SaleItem> SaleItems { get; set; } = new();
    public List<InventoryTransaction> InventoryTransactions { get; set; } = new();
}
