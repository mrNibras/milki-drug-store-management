namespace MilkiDrugStore.Application.DTOs.Category;

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitTypeId { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
