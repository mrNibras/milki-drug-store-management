namespace MilkiDrugStore.Application.DTOs.Cosmetic;

public class CosmeticCategoryResponse
{
    public int CosmeticCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCosmeticCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateCosmeticCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
