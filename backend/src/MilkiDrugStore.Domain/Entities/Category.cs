namespace MilkiDrugStore.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitTypeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public UnitType? UnitType { get; set; }
    public List<Medicine> Medicines { get; set; } = new();
}
