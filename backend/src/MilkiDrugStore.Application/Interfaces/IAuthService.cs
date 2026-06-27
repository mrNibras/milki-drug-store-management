using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<string> RegisterAsync(RegisterRequest request);
    Task<string> ApproveUserAsync(int userId);
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task DeleteUserAsync(int id);
    Task<Settings?> GetSettingsAsync();
    Task<Settings> UpdateSettingsAsync(UpdateSettingsRequest request);
}
