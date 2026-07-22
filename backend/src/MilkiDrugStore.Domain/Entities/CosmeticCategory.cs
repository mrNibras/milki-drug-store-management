namespace MilkiDrugStore.Domain.Entities;

public class CosmeticCategory
{
    public int CosmeticCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Cosmetic> Cosmetics { get; set; } = new();
}
