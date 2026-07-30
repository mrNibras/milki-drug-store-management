using Xunit;
using FluentAssertions;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Application.DTOs.Sale;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Moq;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Tests.Domain;

public class SaleEntityTests
{
    [Fact]
    public void Sale_DefaultValues_AreCorrect()
    {
        var sale = new Sale();
        sale.SaleNumber.Should().Be(string.Empty);
        sale.TotalAmount.Should().Be(0);
        sale.TotalProfit.Should().Be(0);
        sale.PaymentMethod.Should().Be("cash");
        sale.PaymentStatus.Should().Be("paid");
        sale.AmountPaid.Should().Be(0);
        sale.AmountDue.Should().Be(0);
        sale.Items.Should().BeEmpty();
    }
}

public class UserEntityTests
{
    [Fact]
    public void User_DefaultValues_AreCorrect()
    {
        var user = new User();
        user.FullName.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.PasswordHash.Should().Be(string.Empty);
        user.IsActive.Should().BeTrue();
        user.IsApproved.Should().BeFalse();
    }
}

public class MedicineEntityTests
{
  [Fact]
  public void Medicine_DefaultValues_AreCorrect()
  {
      var medicine = new Medicine();
      medicine.BrandName.Should().Be(string.Empty);
      medicine.GenericName.Should().Be(string.Empty);
      medicine.UnitTypeId.Should().Be(0);
      medicine.LowStockThreshold.Should().Be(10);
      medicine.IsActive.Should().BeTrue();
      medicine.Batches.Should().BeEmpty();
  }
}

public class MedicineBatchEntityTests
{
    [Fact]
    public void MedicineBatch_Balance_CalculatesCorrectly()
    {
        var batch = new MedicineBatch
        {
            QuantityReceived = 100,
            QuantityIssued = 30,
            QuantityDamaged = 5,
            QuantityExpired = 2
        };
        batch.Balance.Should().Be(63);
    }

    [Fact]
    public void MedicineBatch_Balance_ReturnsZeroWhenAllIssued()
    {
        var batch = new MedicineBatch
        {
            QuantityReceived = 50,
            QuantityIssued = 50,
            QuantityDamaged = 0,
            QuantityExpired = 0
        };
        batch.Balance.Should().Be(0);
    }
}

public class CategoryEntityTests
{
    [Fact]
    public void Category_DefaultValues_AreCorrect()
    {
        var category = new Category();
        category.Name.Should().Be(string.Empty);
    }
}

public class NotificationEntityTests
{
    [Fact]
    public void Notification_DefaultValues_AreCorrect()
    {
        var notification = new Notification();
        notification.Title.Should().Be(string.Empty);
        notification.Message.Should().Be(string.Empty);
        notification.NotificationType.Should().Be(string.Empty);
        notification.IsRead.Should().BeFalse();
    }
}

public class RoleEnumTests
{
    [Fact]
    public void RoleType_HasExpectedValues()
    {
        var values = Enum.GetValues<RoleType>();
        values.Should().Contain(RoleType.Admin);
        values.Should().Contain(RoleType.Pharmacist);
    }
}
