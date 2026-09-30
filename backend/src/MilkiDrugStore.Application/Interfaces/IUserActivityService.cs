namespace MilkiDrugStore.Application.Interfaces;

/// <summary>
/// Server-side account state lookup used to revoke access of tokens that were
/// issued while an account was still active. JWTs are stateless, so the database
/// remains the single source of truth for whether a principal may act.
/// </summary>
public interface IUserActivityService
{
    /// <summary>
    /// Returns true only when the user still exists, is active and is approved.
    /// </summary>
    Task<bool> IsActiveAsync(int userId, CancellationToken cancellationToken = default);
}
