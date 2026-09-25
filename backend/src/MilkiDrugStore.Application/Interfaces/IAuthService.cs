using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<string> RegisterAsync(RegisterRequest request);
    Task<string> ApproveUserAsync(int userId);
    Task<string> CreateUserAsync(CreateUserRequest request);
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task DeleteUserAsync(int id);
    Task<Settings?> GetSettingsAsync(int? branchId = null);
    Task<PublicSettingsResponse?> GetPublicSettingsAsync(int? branchId = null);
    Task<Settings> UpdateSettingsAsync(UpdateSettingsRequest request, int userId, int? branchId = null);
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<string> ResetPasswordAsync(ResetPasswordRequest request);
}
