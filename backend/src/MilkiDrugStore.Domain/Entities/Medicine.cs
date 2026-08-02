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

    /// <summary>
    /// Built-in categories use negative ids (see <see cref="Catalog.MedicineCatalog"/>);
    /// custom categories use the positive id of a persisted <see cref="Category"/>.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Built-in unit types use negative ids (see <see cref="Catalog.MedicineCatalog"/>);
    /// custom unit types use the positive id of a persisted <see cref="UnitType"/>.
    /// </summary>
    public int UnitTypeId { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }

    public List<MedicineBatch> Batches { get; set; } = new();
    public List<SaleItem> SaleItems { get; set; } = new();
    public List<PurchaseItem> PurchaseItems { get; set; } = new();
    public List<InventoryTransaction> InventoryTransactions { get; set; } = new();
}
