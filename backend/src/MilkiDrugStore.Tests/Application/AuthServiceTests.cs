using Xunit;
using FluentAssertions;
using Moq;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Tests.Application;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<Role>> _roleRepo = new();
    private readonly Mock<IRepository<Branch>> _branchRepo = new();
    private readonly Mock<IRepository<PasswordReset>> _passwordResetRepo = new();
    private readonly Mock<IJwtTokenService> _jwtService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly Mock<IUnitOfWork> _UnitOfWork = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var settingsSection = new Mock<IConfigurationSection>();
        settingsSection.Setup(s => s["Secret"]).Returns("SuperSecretKey12345SuperSecretKey12345");
        settingsSection.Setup(s => s["Issuer"]).Returns("MilkiDrugStore");
        settingsSection.Setup(s => s["Audience"]).Returns("MilkiDrugStoreClient");

        _configuration.Setup(c => c.GetSection("JwtSettings")).Returns(settingsSection.Object);
        _configuration.Setup(c => c["AdminEmail"]).Returns("admin@milki.com");

        _sut = new AuthService(
            _userRepo.Object,
            _roleRepo.Object,
            _branchRepo.Object,
            _passwordResetRepo.Object,
            _jwtService.Object,
            _emailService.Object,
            _configuration.Object,
            _UnitOfWork.Object,
            _auditLog.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsException()
    {
        var request = new RegisterRequest { FullName = "Test", Email = "test@test.com", Password = "Pass123!" };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { new() { Email = "test@test.com" } }.AsQueryable());

        Func<Task> act = () => _sut.RegisterAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*Email already registered*");
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsNull()
    {
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> {}.AsQueryable());

        var result = await _sut.LoginAsync(new LoginRequest { Email = "test@test.com", Password = "wrong" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task ApproveUserAsync_UserNotFound_ThrowsException()
    {
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> {}.AsQueryable());

        Func<Task> act = () => _sut.ApproveUserAsync(999);
        await act.Should().ThrowAsync<Exception>().WithMessage("*User not found*");
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsException()
    {
        var user = new User { UserId = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct") };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsQueryable());

        Func<Task> act = () => _sut.ChangePasswordAsync(1, "wrong", "newpass");
        await act.Should().ThrowAsync<Exception>().WithMessage("*Current password is incorrect*");
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        var user = new User { UserId = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"), IsActive = false, IsApproved = true };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsQueryable());
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Role { RoleId = 2, Name = "Pharmacist" });

        var result = await _sut.LoginAsync(new LoginRequest { Email = "test@test.com", Password = "pass" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UnapprovedUser_ReturnsNull()
    {
        var user = new User { UserId = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"), IsActive = true, IsApproved = false };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsQueryable());
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Role { RoleId = 2, Name = "Pharmacist" });

        var result = await _sut.LoginAsync(new LoginRequest { Email = "test@test.com", Password = "pass" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_InactiveUser_ReturnsNull()
    {
        var refreshToken = new RefreshToken { Token = "rt1", UserId = 1, ExpiryDate = DateTime.UtcNow.AddDays(7), IsRevoked = false };
        var user = new User { UserId = 1, IsActive = false, IsApproved = true };
        _unitOfWork.Setup(u => u.RefreshTokens.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken }.AsQueryable());
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "rt1" });
        result.Should().BeNull();
    }

[Fact]
    public async Task RefreshTokenAsync_UnapprovedUser_ReturnsNull()
    {
        var refreshToken = new RefreshToken { Token = "rt1", UserId = 1, ExpiryDate = DateTime.UtcNow.AddDays(7), IsRevoked = false };
        var user = new User { UserId = 1, IsActive = true, IsApproved = false };
        _unitOfWork.Setup(u => u.RefreshTokens.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken }.AsQueryable());
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "rt1" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_InvalidFullNameWithDigits_ThrowsException()
    {
        var request = new CreateUserRequest
        {
            FullName = "John 2 Smith",
            Email = "john@example.com",
            Password = "Pass123!",
            RoleId = 1,
            BranchId = 1
        };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());

        Func<Task> act = () => _sut.CreateUserAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }

    [Fact]
    public async Task CreateUserAsync_InvalidFullNameWithSpecialChars_ThrowsException()
    {
        var request = new CreateUserRequest
        {
            FullName = "User@Name",
            Email = "user@example.com",
            Password = "Pass123!",
            RoleId = 1,
            BranchId = 1
        };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());

        Func<Task> act = () => _sut.CreateUserAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }

    [Fact]
    public async Task CreateUserAsync_ValidFullName_Succeeds()
    {
        var request = new CreateUserRequest
        {
            FullName = "Mary-Jane O'Connor",
            Email = "mary@example.com",
            Password = "Pass123!",
            RoleId = 1,
            BranchId = 1
        };
        var role = new Role { RoleId = 1, Name = "Pharmacist" };
        var branch = new Branch { BranchId = 1, BranchName = "Main" };
        var createdUser = new User { UserId = 1, FullName = request.FullName, Email = request.Email };

        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());
        _roleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(role);
        _branchRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Branch> { branch }.AsQueryable());
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateUserAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_InvalidFullNameWithDigits_ThrowsException()
    {
        var request = new RegisterRequest { FullName = "John 2 Smith", Email = "john2@example.com", Password = "Pass123!" };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());

        Func<Task> act = () => _sut.RegisterAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }

    [Fact]
    public async Task RegisterAsync_InvalidFullNameWithSpecialChars_ThrowsException()
    {
        var request = new RegisterRequest { FullName = "User@Name", Email = "user2@example.com", Password = "Pass123!" };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());

        Func<Task> act = () => _sut.RegisterAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }

    [Fact]
    public async Task RegisterAsync_ValidFullName_Succeeds()
    {
        var request = new RegisterRequest { FullName = "Abebe Bikila", Email = "abebe@example.com", Password = "Pass123!" };
        var role = new Role { RoleId = 2, Name = "Pharmacist" };
        var branch = new Branch { BranchId = 1, BranchName = "Main" };
        var createdUser = new User { UserId = 1, FullName = request.FullName, Email = request.Email };

        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());
        _roleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Role, bool>>>()))
            .ReturnsAsync(new List<Role> { role }.AsQueryable());
        _branchRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Branch> { branch }.AsQueryable());
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.RegisterAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateUserAsync_InvalidFullNameWithDigits_ThrowsException()
    {
        var user = new User { UserId = 1, FullName = "Old Name", Email = "old@example.com" };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsQueryable());

        var request = new UpdateUserRequest { FullName = "John 2 Smith", Email = "new@example.com" };

        Func<Task> act = () => _sut.UpdateUserAsync(1, request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }

    [Fact]
    public async Task UpdateUserAsync_InvalidFullNameWithSpecialChars_ThrowsException()
    {
        var user = new User { UserId = 1, FullName = "Old Name", Email = "old@example.com" };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsQueryable());

        var request = new UpdateUserRequest { FullName = "User@Name", Email = "new@example.com" };

        Func<Task> act = () => _sut.UpdateUserAsync(1, request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }
    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ResetsPassword()
    {
        var token = "validtoken123456789012345678901234";
        var tokenHash = ComputeTokenHashTest(token);
        var user = new User { UserId = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPass123!"), IsActive = true, IsApproved = true };
        var reset = new PasswordReset { PasswordResetId = 1, TokenHash = tokenHash, UserId = 1, ExpiryDate = DateTime.UtcNow.AddHours(1), IsUsed = false, CreatedAt = DateTime.UtcNow };
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset> { reset }.AsQueryable());
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

        var result = await _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = token, NewPassword = "NewPass123!" });
        result.Should().Contain("success");
    }


    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_Throws()
    {
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset>().AsQueryable());
        Func<Task> act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "bad", NewPassword = "NewPass123!" });
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_Throws()
    {
        var token = "expiredtoken123456789012345678901234";
        var tokenHash = ComputeTokenHashTest(token);
        var reset = new PasswordReset { PasswordResetId = 1, TokenHash = tokenHash, UserId = 1, ExpiryDate = DateTime.UtcNow.AddHours(-1), IsUsed = false, CreatedAt = DateTime.UtcNow };
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset> { reset }.AsQueryable());
        Func<Task> act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = token, NewPassword = "NewPass123!" });
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ResetPasswordAsync_AlreadyConsumedToken_Throws()
    {
        var token = "usedtoken123456789012345678901234";
        var tokenHash = ComputeTokenHashTest(token);
        var reset = new PasswordReset { PasswordResetId = 1, TokenHash = tokenHash, UserId = 1, ExpiryDate = DateTime.UtcNow.AddHours(1), IsUsed = true, CreatedAt = DateTime.UtcNow };
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset>().AsQueryable());
        Func<Task> act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = token, NewPassword = "NewPass123!" });
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ResetPasswordAsync_WeakPassword_Throws()
    {
        var token = "validtoken123456789012345678901234";
        var tokenHash = ComputeTokenHashTest(token);
        var user = new User { UserId = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPass123!"), IsActive = true, IsApproved = true };
        var reset = new PasswordReset { PasswordResetId = 1, TokenHash = tokenHash, UserId = 1, ExpiryDate = DateTime.UtcNow.AddHours(1), IsUsed = false, CreatedAt = DateTime.UtcNow };
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset> { reset }.AsQueryable());
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        Func<Task> act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = token, NewPassword = "weak" });
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ResetPasswordAsync_EmptyPassword_Throws()
    {
        var token = "validtoken123456789012345678901234";
        var tokenHash = ComputeTokenHashTest(token);
        var reset = new PasswordReset { PasswordResetId = 1, TokenHash = tokenHash, UserId = 1, ExpiryDate = DateTime.UtcNow.AddHours(1), IsUsed = false, CreatedAt = DateTime.UtcNow };
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset> { reset }.AsQueryable());
        Func<Task> act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = token, NewPassword = "" });
        await act.Should().ThrowAsync<Exception>();
    }

    private static string ComputeTokenHashTest(string token)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    

    [Fact]
    public async Task ResetPasswordAsync_MissingToken_Throws()
    {
        Func<Task> act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "", NewPassword = "NewPass123!" });
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ResetPasswordAsync_PasswordPolicyViolation_Throws()
    {
        var token = "validtoken123456789012345678901234";
        var tokenHash = ComputeTokenHashTest(token);
        var user = new User { UserId = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPass123!"), IsActive = true, IsApproved = true };
        var reset = new PasswordReset { PasswordResetId = 1, TokenHash = tokenHash, UserId = 1, ExpiryDate = DateTime.UtcNow.AddHours(1), IsUsed = false, CreatedAt = DateTime.UtcNow };
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset> { reset }.AsQueryable());
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        Func<Task> act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = token, NewPassword = "nouppercase" });
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ResetPasswordAsync_OldPasswordRejectedAfterReset()
    {
        var token = "validtoken123456789012345678901234";
        var tokenHash = ComputeTokenHashTest(token);
        var user = new User { UserId = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPass123!"), IsActive = true, IsApproved = true };
        var reset = new PasswordReset { PasswordResetId = 1, TokenHash = tokenHash, UserId = 1, ExpiryDate = DateTime.UtcNow.AddHours(1), IsUsed = false, CreatedAt = DateTime.UtcNow };
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset> { reset }.AsQueryable());
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

        var result = await _sut.ResetPasswordAsync(new ResetPasswordRequest { Token = token, NewPassword = "NewPass123!" });
        result.Should().Contain("success");

        var updatedUser = new User { UserId = 1, Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("NewPass123!"), IsActive = true, IsApproved = true };
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(updatedUser);
        var oldValid = BCrypt.Net.BCrypt.Verify("oldPass123!", updatedUser.PasswordHash);
        oldValid.Should().BeFalse();
        var newValid = BCrypt.Net.BCrypt.Verify("NewPass123!", updatedUser.PasswordHash);
        newValid.Should().BeTrue();
    }

    [Fact]
    public async Task RequestPasswordResetAsync_EmailDeliveryFailure_InvalidatesToken()
    {
        var email = "test@test.com";
        var user = new User { UserId = 1, Email = email, IsActive = true, IsApproved = true };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user }.AsQueryable());
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset>().AsQueryable());
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _emailService.Setup(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var result = await _sut.RequestPasswordResetAsync(new ForgotPasswordRequest { Email = email });
        result.Message.Should().Be("If an account with that email exists, a reset link has been sent.");
    }

    [Fact]
    public async Task ResetPasswordAsync_HashStoredInsteadOfPlaintext()
    {
        var token = "verifystorehash123456789012345678901234";
        var expectedHash = ComputeTokenHashTest(token);
        _passwordResetRepo.Setup(r => r.AddAsync(It.IsAny<PasswordReset>())).Callback<PasswordReset>(r =>
        {
            r.TokenHash.Should().Be(expectedHash);
            r.TokenHash.Should().NotBe(token);
        });
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _passwordResetRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PasswordReset, bool>>>()))
            .ReturnsAsync(new List<PasswordReset>().AsQueryable());

        await _sut.RequestPasswordResetAsync(new ForgotPasswordRequest { Email = "test@test.com" });
    }
}
