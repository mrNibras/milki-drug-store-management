namespace MilkiDrugStore.Domain.Entities;

/// <summary>
/// A custom (user-defined) unit type. Built-in unit types are defined in
/// <see cref="Catalog.MedicineCatalog"/> and are not persisted here.
/// </summary>
public class UnitType
{
    public int UnitTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
