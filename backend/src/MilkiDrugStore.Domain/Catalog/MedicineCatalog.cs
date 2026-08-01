namespace MilkiDrugStore.Domain.Catalog;

/// <summary>
/// System-defined medicine catalog values. Built-in categories and unit types
/// are defined here as constants and are never stored in the database.
/// Custom values entered via the "Other" option are persisted separately
/// and merged with these built-ins at the service layer.
///
/// Built-in items use negative ids so they can never collide with the
/// positive auto-generated ids of persisted custom values.
/// </summary>
public static class MedicineCatalog
{
    public const string OtherLabel = "Other";

    public readonly record struct CatalogItem(int Id, string Name);

    /// <summary>
    /// Built-in medicine categories, in display order. "Other" is always last.
    /// </summary>
    public static IReadOnlyList<CatalogItem> Categories { get; } = new[]
    {
        new CatalogItem(-1, "Antibiotics"),
        new CatalogItem(-2, "Antivirals"),
        new CatalogItem(-3, "Antifungals"),
        new CatalogItem(-4, "Antipain"),
        new CatalogItem(-5, "Antihistamines"),
        new CatalogItem(-6, "GIT Drugs"),
        new CatalogItem(-7, "Hormonal Drugs"),
        new CatalogItem(-8, "CVS Drugs"),
        new CatalogItem(-9, "CNS Drugs"),
        new CatalogItem(-10, "Vitamins and Minerals"),
        new CatalogItem(-11, "Vaccines"),
        new CatalogItem(-12, "ENT Drugs"),
        new CatalogItem(-13, "Dermatologic Drugs"),
        new CatalogItem(-14, "Antiepileptics"),
        new CatalogItem(-15, OtherLabel)
    };

    /// <summary>
    /// Built-in unit types, in display order. "Other" is always last.
    /// </summary>
    public static IReadOnlyList<CatalogItem> UnitTypes { get; } = new[]
    {
        new CatalogItem(-1, "Tablet"),
        new CatalogItem(-2, "Capsule"),
        new CatalogItem(-3, "Suspension"),
        new CatalogItem(-4, "Syrup"),
        new CatalogItem(-5, "Solution"),
        new CatalogItem(-6, "Injection"),
        new CatalogItem(-7, "Oral Drop"),
        new CatalogItem(-8, "Cream"),
        new CatalogItem(-9, "Ointment"),
        new CatalogItem(-10, "Shampoo"),
        new CatalogItem(-11, "Powder"),
        new CatalogItem(-12, "Drop"),
        new CatalogItem(-13, OtherLabel)
    };

    /// <summary>
    /// Looks up a built-in category id by name, ignoring case.
    /// </summary>
    public static int? FindCategoryId(string name)
        => FindId(Categories, name);

    /// <summary>
    /// Looks up a built-in unit type id by name, ignoring case.
    /// </summary>
    public static int? FindUnitTypeId(string name)
        => FindId(UnitTypes, name);

    /// <summary>
    /// Returns true when the id refers to one of the built-in categories.
    /// </summary>
    public static bool IsBuiltInCategoryId(int id)
        => IsBuiltInId(Categories, id);

    /// <summary>
    /// Returns true when the id refers to one of the built-in unit types.
    /// </summary>
    public static bool IsBuiltInUnitTypeId(int id)
        => IsBuiltInId(UnitTypes, id);

    public static string? GetCategoryName(int id)
        => GetName(Categories, id);

    public static string? GetUnitTypeName(int id)
        => GetName(UnitTypes, id);

    private static int? FindId(IReadOnlyList<CatalogItem> items, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalized = name.Trim();
        foreach (var item in items)
        {
            if (item.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                return item.Id;
        }

        return null;
    }

    private static bool IsBuiltInId(IReadOnlyList<CatalogItem> items, int id)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
                return true;
        }

        return false;
    }

    private static string? GetName(IReadOnlyList<CatalogItem> items, int id)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
                return item.Name;
        }

        return null;
    }
}
