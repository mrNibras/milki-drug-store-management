namespace MilkiDrugStore.Domain.Entities;

public class Cosmetic
{
    public int CosmeticId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

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

    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<CosmeticBatch> Batches { get; set; } = new();
}
