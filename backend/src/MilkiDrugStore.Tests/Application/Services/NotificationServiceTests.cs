using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepo = new();
    private readonly Mock<IRepository<Medicine>> _medicineRepo = new();
    private readonly Mock<ICosmeticRepository> _cosmeticRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _notificationRepo.Object,
            _medicineRepo.Object,
            _cosmeticRepo.Object,
            _unitOfWork.Object
        );
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Notifications()
    {
        var notifications = new List<Notification>
        {
            new Notification { NotificationId = 1, Title = "Low Stock", Message = "Aspirin is low", IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { NotificationId = 2, Title = "Out of Stock", Message = "Paracetamol is out", IsRead = true, CreatedAt = DateTime.UtcNow }
        };
        _notificationRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult((System.Linq.IQueryable<Notification>)notifications.AsQueryable()));

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task MarkAsReadAsync_Should_Mark_Notification_As_Read()
    {
        var notification = new Notification { NotificationId = 1, Title = "Test", IsRead = false };
        _notificationRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>()))
            .Returns(Task.FromResult((System.Linq.IQueryable<Notification>)new List<Notification> { notification }.AsQueryable()));

        await _sut.MarkAsReadAsync(1);

        Assert.True(notification.IsRead);
    }

    [Fact]
    public async Task GetUnreadAsync_Should_Return_Only_Unread_Notifications()
    {
        var notifications = new List<Notification>
        {
            new Notification { NotificationId = 1, Title = "Low Stock", Message = "Aspirin is low", IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { NotificationId = 2, Title = "Out of Stock", Message = "Paracetamol is out", IsRead = true, CreatedAt = DateTime.UtcNow }
        };
        _notificationRepo.Setup(r => r.GetUnreadAsync()).Returns(Task.FromResult((System.Collections.Generic.IEnumerable<Notification>)notifications.Where(n => !n.IsRead).ToList()));

        var result = await _sut.GetUnreadAsync();

        Assert.Single(result);
        Assert.False(result.First().IsRead);
    }
}
