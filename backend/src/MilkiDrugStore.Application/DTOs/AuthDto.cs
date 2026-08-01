namespace MilkiDrugStore.Application.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int BranchId { get; set; }
    public bool? IsActive { get; set; }
}

public class UserResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int BranchId { get; set; }
}

public class UpdateSettingsRequest
{
    public string PharmacyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Language { get; set; } = "English";
    public int LowStockThreshold { get; set; } = 10;
    public int ExpiryAlertMonths { get; set; } = 6;
    public string Currency { get; set; } = "ETB";
    public string BatchSelectionMode { get; set; } = "AutomaticFefo";
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;
}
