namespace MilkiDrugStore.Application.DTOs.UnitType;

public class UnitTypeResponse
{
    public int UnitTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
