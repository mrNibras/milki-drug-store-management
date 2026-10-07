namespace MilkiDrugStore.Application.Interfaces;

/// <summary>
/// Reports the authenticated caller so application services can apply
/// role-based rules without depending on ASP.NET Core.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// True when the caller is privileged to view supplier cost and
    /// inventory valuation. Admins are; authenticated pharmacists are
    /// not; callers with no principal (background work, seeding) are
    /// trusted and therefore treated as privileged.
    /// </summary>
    bool CanViewFinancialCosts { get; }
}