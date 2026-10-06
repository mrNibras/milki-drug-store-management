using System.Security.Claims;
using System.Text.Json.Serialization.Metadata;

namespace MilkiDrugStore.Api.Infrastructure;

/// <summary>
/// Server-side redaction of supplier-cost values for non-admin accounts.
///
/// The pharmacy business rule is that only admins may see inventory valuation
/// and cost information. Hiding a column in React is not sufficient, because the
/// value would still travel to the browser and be readable in the Network tab,
/// so the restriction is applied to the serialized payload itself.
///
/// Cost properties are omitted from the JSON entirely for non-admins rather
/// than emitted as null, so a restricted value is not merely disguised as
/// "missing data".
///
/// Selling price is deliberately NOT restricted: the POS is reachable by
/// pharmacists and prices cart lines from the batch selling price, so removing it
/// would break completing an authorized sale. Purchase cost, however, is the
/// input to inventory valuation and profit, and is what this type removes.
/// </summary>
public static class RestrictedFinancialFields
{
    /// <summary>The role permitted to receive supplier cost values.</summary>
    public const string AdminRole = "Admin";

    /// <summary>
    /// Properties carrying what the pharmacy paid a supplier. These are the values
    /// behind inventory valuation and profit, and are admin-only.
    /// </summary>
    public static readonly IReadOnlySet<string> SupplierCostProperties =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "PurchasePrice",
            "BuyingPrice"
        };

    /// <summary>True when the given property may be serialized for this caller.</summary>
    public static bool CanSerialize(ClaimsPrincipal? user, string propertyName)
        => user is null
            || user.IsInRole(AdminRole)
            || !SupplierCostProperties.Contains(propertyName);

    /// <summary>
    /// JSON metadata modifier that drops supplier-cost properties for callers who
    /// are not admins. Admins and callers with no principal (background work,
    /// seeding) are left untouched.
    /// </summary>
    public static void Apply(JsonTypeInfo typeInfo, IHttpContextAccessor? accessor)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        var user = accessor?.HttpContext?.User;
        if (user is null || user.IsInRole(AdminRole))
            return;

        foreach (var name in SupplierCostProperties)
        {
            var property = typeInfo.Properties.FirstOrDefault(p => p.Name == name);
            if (property is not null)
                property.ShouldSerialize = static (_, _) => false;
        }
    }
}