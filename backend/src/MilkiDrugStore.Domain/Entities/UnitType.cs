namespace MilkiDrugStore.Domain.Entities;

public class UnitType
{
    public int UnitTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Medicine> Medicines { get; set; } = new();
}
