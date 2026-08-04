using MilkiDrugStore.Application.DTOs.Catalog;

namespace MilkiDrugStore.Application.Interfaces;

/// <summary>
/// Provides the merged built-in + custom medicine catalog used by medicine,
/// cosmetic and purchase flows. Built-ins come from
/// <see cref="MilkiDrugStore.Domain.Catalog.MedicineCatalog"/>; custom values
/// are read from (and written to) the persisted category/unit-type stores.
/// </summary>
public interface ICatalogService
{
    Task<IEnumerable<CatalogOptionDto>> GetCategoryOptionsAsync();
    Task<IEnumerable<CatalogOptionDto>> GetUnitTypeOptionsAsync();

    /// <summary>
    /// Resolves a display name to a category id. Matches a built-in category
    /// by name (case-insensitive), otherwise matches an existing custom
    /// category, otherwise creates and persists a new custom category.
    /// Returns null when the name is blank.
    /// </summary>
    Task<int?> ResolveCategoryIdAsync(string? name, int userId);

    /// <summary>
    /// Resolves a display name to a unit type id. Matches a built-in unit type
    /// by name (case-insensitive), otherwise matches an existing custom unit
    /// type, otherwise creates and persists a new custom unit type.
    /// Returns null when the name is blank.
    /// </summary>
    Task<int?> ResolveUnitTypeIdAsync(string? name, int userId);

    Task<bool> IsValidCategoryIdAsync(int id);
    Task<bool> IsValidUnitTypeIdAsync(int id);

    Task<string> GetCategoryNameAsync(int id);
    Task<string> GetUnitTypeNameAsync(int id);
    Task<IDictionary<int, string>> GetCategoryNamesAsync(IEnumerable<int> ids);
    Task<IDictionary<int, string>> GetUnitTypeNamesAsync(IEnumerable<int> ids);
}
