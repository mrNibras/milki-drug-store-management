namespace MilkiDrugStore.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<Medicine> Medicines { get; set; } = new();
}
