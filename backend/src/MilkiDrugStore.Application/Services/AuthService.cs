using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Exceptions;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<Branch> _branchRepo;
    private readonly IRepository<PasswordReset> _passwordResetRepo;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> userRepo,
        IRepository<Role> roleRepo,
        IRepository<Branch> branchRepo,
        IRepository<PasswordReset> passwordResetRepo,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _branchRepo = branchRepo;
        _passwordResetRepo = passwordResetRepo;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _logger = logger;
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        _logger.LogInformation("Login attempt for email: {Email}", normalizedEmail);

        var users = await _userRepo.FindAsync(u => u.Email == normalizedEmail);
        var user = users.FirstOrDefault();

        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found for email {Email}", normalizedEmail);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for user {UserId} ({Email})", user.UserId, normalizedEmail);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: user {UserId} ({Email}) is inactive", user.UserId, normalizedEmail);
            return null;
        }

        if (!user.IsApproved)
        {
            _logger.LogWarning("Login failed: user {UserId} ({Email}) is not approved", user.UserId, normalizedEmail);
            return null;
        }

        var role = await _roleRepo.GetByIdAsync(user.RoleId);
        if (role == null)
        {
            _logger.LogWarning("Login failed: user {UserId} ({Email}) has no valid role assigned (RoleId={RoleId})", user.UserId, normalizedEmail, user.RoleId);
            return null;
        }

        var branch = await _branchRepo.GetByIdAsync(user.BranchId);
        user.Role = role;
        var token = _jwtTokenService.GenerateToken(user);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Login successful for user {UserId} ({Email}) with role {Role}", user.UserId, normalizedEmail, role.Name);

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            Role = role.Name,
            UserId = user.UserId,
            FullName = user.FullName,
            BranchId = user.BranchId,
            BranchName = branch?.BranchName ?? string.Empty
        };
    }

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existing = (await _userRepo.FindAsync(u => u.Email == normalizedEmail)).FirstOrDefault();
        if (existing != null)
            throw new Exception("Email already registered");

        var pharmacistRole = (await _roleRepo.FindAsync(r => r.Name == "Pharmacist")).FirstOrDefault();
        var roleId = pharmacistRole?.RoleId ?? (int)RoleType.Pharmacist;

        var defaultBranch = (await _branchRepo.GetAllAsync()).FirstOrDefault();
        if (defaultBranch == null)
            throw new Exception("No branch available for registration");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = roleId,
            BranchId = defaultBranch.BranchId,
            IsApproved = false,
            IsActive = true
        };

        await _userRepo.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var adminEmail = _configuration["AdminEmail"] ?? "admin@milki.com";
        try
        {
            await _emailService.SendApprovalEmailAsync(adminEmail, user.FullName);
        }
        catch
        {
        }

        return "Registration submitted for admin approval.";
    }

    public async Task<string> ApproveUserAsync(int userId)
    {
        var users = await _userRepo.FindAsync(u => u.UserId == userId);
        var user = users.FirstOrDefault();
        if (user == null) throw new Exception("User not found");

        user.IsApproved = true;
        await _userRepo.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Approved user: {user.FullName}", "Users", user.UserId);

        return "User approved successfully.";
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new Exception("Password is required");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new Exception("Full name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("Email is required");

        var normalizedEmail = NormalizeEmail(request.Email);

        var existing = (await _userRepo.FindAsync(u => u.Email == normalizedEmail)).FirstOrDefault();
        if (existing != null)
            throw new Exception("A user with this email already exists.");

        var branchId = request.BranchId;
        if (branchId <= 0)
        {
            var defaultBranch = (await _branchRepo.GetAllAsync()).FirstOrDefault();
            if (defaultBranch == null)
                throw new Exception("No branch available. Please create a branch first.");
            branchId = defaultBranch.BranchId;
        }

        var role = await _roleRepo.GetByIdAsync(request.RoleId);
        if (role == null)
            throw new Exception("Invalid role specified.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            BranchId = branchId,
            IsApproved = true,
            IsActive = true
        };

        await _userRepo.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(user.UserId, $"Created user: {user.FullName} with role {role.Name}", "Users", user.UserId);

        _logger.LogInformation("User created: {UserId} ({Email}) with role {Role}", user.UserId, normalizedEmail, role.Name);

        return "User created successfully.";
    }

    public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var users = await _userRepo.FindAsync(u => u.UserId == userId);
        var user = users.FirstOrDefault();
        if (user == null) throw new Exception("User not found");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new Exception("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepo.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, "Changed password", "Users", user.UserId);
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        var usersWithRoles = await users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .ToListAsync();
        return usersWithRoles.Select(u => new UserResponse
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            RoleId = u.RoleId,
            RoleName = u.Role != null ? u.Role.Name : "",
            BranchId = u.BranchId,
            BranchName = u.Branch != null ? u.Branch.BranchName : "",
            IsApproved = u.IsApproved,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var users = await _userRepo.FindAsync(u => u.UserId == id);
        var user = users.FirstOrDefault();
        if (user == null) return null;

        user.FullName = request.FullName.Trim();
        user.Email = NormalizeEmail(request.Email);
        if (request.RoleId > 0)
            user.RoleId = request.RoleId;
        if (request.BranchId > 0)
            user.BranchId = request.BranchId;
        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        await _userRepo.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(id, $"Updated user: {user.FullName}", "Users", user.UserId);

        var updatedUsers = await _userRepo.FindAsync(u => u.UserId == id);
        var updated = updatedUsers.Include(u => u.Role).Include(u => u.Branch).FirstOrDefault();
        if (updated == null) return null;

        return new UserResponse
        {
            UserId = updated.UserId,
            FullName = updated.FullName,
            Email = updated.Email,
            RoleId = updated.RoleId,
            RoleName = updated.Role != null ? updated.Role.Name : "",
            BranchId = updated.BranchId,
            BranchName = updated.Branch != null ? updated.Branch.BranchName : "",
            IsApproved = updated.IsApproved,
            IsActive = updated.IsActive,
            CreatedAt = updated.CreatedAt
        };
    }

    public async Task DeleteUserAsync(int id)
    {
        var users = await _userRepo.FindAsync(u => u.UserId == id);
        var user = users.FirstOrDefault();
        if (user == null) return;

        user.IsActive = false;
        await _userRepo.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(id, $"Deactivated user: {user.FullName}", "Users", user.UserId);
    }

    public async Task<Settings?> GetSettingsAsync(int? branchId = null)
    {
        var query = (await _unitOfWork.Settings.GetAllAsync()).AsQueryable();
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        return query.FirstOrDefault();
    }

    public async Task<PublicSettingsResponse?> GetPublicSettingsAsync(int? branchId = null)
    {
        var query = (await _unitOfWork.Settings.GetAllAsync()).AsQueryable();
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        var settings = await query.FirstOrDefaultAsync();
        if (settings == null)
            return null;

        return new PublicSettingsResponse
        {
            PharmacyName = settings.PharmacyName,
            Language = settings.Language,
            Currency = settings.Currency
        };
    }

    public async Task<Settings> UpdateSettingsAsync(UpdateSettingsRequest request, int userId, int? branchId = null)
    {
        var query = (await _unitOfWork.Settings.GetAllAsync()).AsQueryable();
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        var settings = query.FirstOrDefault();
        if (settings == null)
        {
            settings = new Settings { BranchId = branchId ?? 0 };
            await _unitOfWork.Settings.AddAsync(settings);
        }

        settings.PharmacyName = request.PharmacyName;
        settings.Address = request.Address;
        settings.Phone = request.Phone;
        settings.Email = request.Email;
        settings.Language = request.Language;
        settings.LowStockThreshold = request.LowStockThreshold;
        settings.ExpiryAlertMonths = request.ExpiryAlertMonths;
        settings.Currency = request.Currency;
        settings.BatchSelectionMode = Enum.TryParse<BatchSelectionMode>(request.BatchSelectionMode, true, out var mode)
            ? mode
            : BatchSelectionMode.AutomaticFefo;

        await _unitOfWork.Settings.UpdateAsync(settings);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, "Updated system settings", "Settings", settings.SettingId);

        return settings;
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == request.RefreshToken);
        var refreshToken = refreshTokens.FirstOrDefault();

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
            return null;

        var user = await _userRepo.GetByIdAsync(refreshToken.UserId);
        if (user == null || !user.IsActive || !user.IsApproved)
            return null;

        refreshToken.IsRevoked = true;
        await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

        var role = await _roleRepo.GetByIdAsync(user.RoleId);
        var branch = await _branchRepo.GetByIdAsync(user.BranchId);
        var newToken = _jwtTokenService.GenerateToken(user);

        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken.Token,
            Role = role?.Name ?? "Pharmacist",
            UserId = user.UserId,
            FullName = user.FullName,
            BranchId = user.BranchId,
            BranchName = branch?.BranchName ?? string.Empty
        };
    }

    public async Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var users = await _userRepo.FindAsync(u => u.Email == normalizedEmail);
        var user = users.FirstOrDefault();
        if (user == null || !user.IsActive || !user.IsApproved)
            return new ForgotPasswordResponse { Message = "If an account with that email exists, a reset link has been sent." };

        var existing = await _passwordResetRepo.FindAsync(pr => pr.UserId == user.UserId && !pr.IsUsed && pr.ExpiryDate > DateTime.UtcNow);
        foreach (var pr in existing)
        {
            pr.IsUsed = true;
            await _passwordResetRepo.UpdateAsync(pr);
        }

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var expiry = DateTime.UtcNow.AddHours(1);

        await _passwordResetRepo.AddAsync(new PasswordReset
        {
            Token = token,
            UserId = user.UserId,
            ExpiryDate = expiry,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={token}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);
        }
        catch
        {
        }

        return new ForgotPasswordResponse { Message = "If an account with that email exists, a reset link has been sent." };
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resets = await _passwordResetRepo.FindAsync(pr => pr.Token == request.Token && !pr.IsUsed);
        var reset = resets.FirstOrDefault();
        if (reset == null || reset.ExpiryDate < DateTime.UtcNow)
            throw new Exception("Invalid or expired reset token.");

        var user = await _userRepo.GetByIdAsync(reset.UserId);
        if (user == null || !user.IsActive || !user.IsApproved)
            throw new Exception("User account is not active.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepo.UpdateAsync(user);

        reset.IsUsed = true;
        await _passwordResetRepo.UpdateAsync(reset);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(user.UserId, "Reset password via email link", "Users", user.UserId);

        return "Password has been reset successfully.";
    }
}
