using Microsoft.AspNetCore.Http;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Services;

/// <summary>
/// Reads the caller's role from the JWT principal, so the existing
/// role claim is the only source of truth and no second role system
/// is introduced.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool CanViewFinancialCosts
        => _httpContextAccessor.HttpContext?.User is not { } user
            || user.IsInRole("Admin");
}