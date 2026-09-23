namespace MilkiDrugStore.Domain.Catalog;

public static class CosmeticCatalog
{
    public const string OtherLabel = "Other";

    public readonly record struct CatalogItem(int Id, string Name);

    public static IReadOnlyList<CatalogItem> Categories { get; } = new[]
    {
        new CatalogItem(-100, "Hair Care"),
        new CatalogItem(-101, "Skin Care"),
        new CatalogItem(-102, "Bath & Body"),
        new CatalogItem(-103, "Oral Care"),
        new CatalogItem(-104, "Baby Care"),
        new CatalogItem(-105, "Makeup"),
        new CatalogItem(-106, "Fragrance"),
        new CatalogItem(-107, "Feminine Care"),
        new CatalogItem(-108, OtherLabel)
    };

    public static int? FindCategoryId(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalized = name.Trim();
        foreach (var item in Categories)
        {
            if (item.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                return item.Id;
        }

        return null;
    }

    public static bool IsBuiltInCategoryId(int id)
    {
        foreach (var item in Categories)
        {
            if (item.Id == id)
                return true;
        }

        return false;
    }

    public static string? GetCategoryName(int id)
    {
        foreach (var item in Categories)
        {
            if (item.Id == id)
                return item.Name;
        }

        return null;
    }
}
