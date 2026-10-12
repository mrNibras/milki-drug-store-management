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
using MilkiDrugStore.Application.DTOs.Auth;

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
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
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
            _unitOfWork.Object,
            _auditLog.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsException()
    {
        var request = new RegisterRequest { FullName = "Test", Email = "test@test.com", Password = "Pass123" };
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
            Password = "Pass123",
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
            Password = "Pass123",
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
            Password = "Pass123",
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
        var request = new RegisterRequest { FullName = "John 2 Smith", Email = "john2@example.com", Password = "Pass123" };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());

        Func<Task> act = () => _sut.RegisterAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }

    [Fact]
    public async Task RegisterAsync_InvalidFullNameWithSpecialChars_ThrowsException()
    {
        var request = new RegisterRequest { FullName = "User@Name", Email = "user2@example.com", Password = "Pass123" };
        _userRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
            .ReturnsAsync(new List<User>().AsQueryable());

        Func<Task> act = () => _sut.RegisterAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*letters*");
    }

    [Fact]
    public async Task RegisterAsync_ValidFullName_Succeeds()
    {
        var request = new RegisterRequest { FullName = "Abebe Bikila", Email = "abebe@example.com", Password = "Pass123" };
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
}
