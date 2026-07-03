using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Exceptions;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MilkiDrugStore.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IRepository<User> userRepo,
        IRepository<Role> roleRepo,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var users = await _userRepo.FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            return null;

        var role = await _roleRepo.GetByIdAsync(user.RoleId);
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

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            Role = role?.Name ?? "Pharmacist",
            UserId = user.UserId,
            FullName = user.FullName
        };
    }

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var existing = (await _userRepo.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
        if (existing != null)
            throw new Exception("Email already registered");

        var pharmacistRole = (await _roleRepo.FindAsync(r => r.Name == "Pharmacist")).FirstOrDefault();
        var roleId = pharmacistRole?.RoleId ?? (int)RoleType.Pharmacist;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = roleId,
            IsApproved = false,
            IsActive = true
        };

        await _userRepo.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var adminEmail = _configuration["AdminEmail"] ?? "admin@milki.com";
        await _emailService.SendApprovalEmailAsync(adminEmail, user.FullName);

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

        return "User approved successfully.";
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
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepo.GetAllAsync();
    }

    public async Task<User?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var users = await _userRepo.FindAsync(u => u.UserId == id);
        var user = users.FirstOrDefault();
        if (user == null) return null;

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.RoleId = request.RoleId;

        await _userRepo.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task DeleteUserAsync(int id)
    {
        var users = await _userRepo.FindAsync(u => u.UserId == id);
        var user = users.FirstOrDefault();
        if (user == null) return;

        user.IsActive = false;
        await _userRepo.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Settings?> GetSettingsAsync()
    {
        var settings = await _unitOfWork.Settings.GetAllAsync();
        return settings.FirstOrDefault();
    }

    public async Task<Settings> UpdateSettingsAsync(UpdateSettingsRequest request)
    {
        var settings = (await _unitOfWork.Settings.GetAllAsync()).FirstOrDefault();
        if (settings == null)
        {
            settings = new Settings();
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

        await _unitOfWork.Settings.UpdateAsync(settings);
        await _unitOfWork.SaveChangesAsync();
        return settings;
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == request.RefreshToken);
        var refreshToken = refreshTokens.FirstOrDefault();

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
            return null;

        var user = await _userRepo.GetByIdAsync(refreshToken.UserId);
        if (user == null || !user.IsActive)
            return null;

        refreshToken.IsRevoked = true;
        await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

        var role = await _roleRepo.GetByIdAsync(user.RoleId);
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
            FullName = user.FullName
        };
    }
}
