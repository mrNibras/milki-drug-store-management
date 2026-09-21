namespace MilkiDrugStore.Domain.Entities;

/// <summary>
/// A custom (user-defined) medicine/cosmetic category. Built-in categories are
/// defined in <see cref="Catalog.MedicineCatalog"/> and are not persisted here.
/// </summary>
public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
