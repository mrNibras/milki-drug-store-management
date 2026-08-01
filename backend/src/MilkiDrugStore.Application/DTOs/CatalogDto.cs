namespace MilkiDrugStore.Application.DTOs.Catalog;

public class CatalogOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
}

public class CreateCatalogOptionRequest
{
    public string Name { get; set; } = string.Empty;
}
