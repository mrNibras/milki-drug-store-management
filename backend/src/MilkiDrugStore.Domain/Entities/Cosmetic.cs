namespace MilkiDrugStore.Domain.Entities;

public class Cosmetic
{
    public int CosmeticId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CosmeticCategoryId { get; set; }
    public int UnitTypeId { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public CosmeticCategory? CosmeticCategory { get; set; }
    public UnitType? UnitType { get; set; }
    public List<CosmeticBatch> Batches { get; set; } = new();
}
