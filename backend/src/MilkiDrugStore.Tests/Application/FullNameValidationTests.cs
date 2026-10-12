using Xunit;
using FluentAssertions;
using MilkiDrugStore.Application.Validators;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Moq;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Tests.Application;

public class FullNameValidatorTests
{
    [Theory]
    [InlineData("John Smith")]
    [InlineData("Mary-Jane O'Connor")]
    [InlineData("Abebe Bikila")]
    [InlineData("Ana María")]
    [InlineData("  Alice  Bob  ")]
    [InlineData("O'Neil")]
    [InlineData("Jean-Pierre")]
    public void IsValidFullName_ValidNames_ReturnsTrue(string name)
    {
        FullNameValidator.IsValidFullName(name).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void IsValidFullName_EmptyOrWhitespace_ReturnsFalse(string name)
    {
        FullNameValidator.IsValidFullName(name).Should().BeFalse();
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("User@Name")]
    [InlineData("Name#1")]
    [InlineData("Test.User")]
    [InlineData("Name!")]
    [InlineData("Name$")]
    [InlineData("Name%")]
    [InlineData("Name&")]
    [InlineData("Name*")]
    [InlineData("Name()")]
    [InlineData("Name[]")]
    [InlineData("Name{}")]
    [InlineData("Name<>")]
    [InlineData("Name\\")]
    [InlineData("Name/")]
    [InlineData("Name:")]
    [InlineData("Name;")]
    [InlineData("Name\"")]
    [InlineData("Name?")]
    [InlineData("Name|")]
    [InlineData("Name~")]
    [InlineData("Name=")]
    [InlineData("Name^")]
    public void IsValidFullName_InvalidCharacters_ReturnsFalse(string name)
    {
        FullNameValidator.IsValidFullName(name).Should().BeFalse();
    }

    [Fact]
    public void IsValidFullName_Digits_ReturnsFalse()
    {
        FullNameValidator.IsValidFullName("John 2 Smith").Should().BeFalse();
        FullNameValidator.IsValidFullName("123456").Should().BeFalse();
        FullNameValidator.IsValidFullName("User123").Should().BeFalse();
    }

    [Fact]
    public void IsValidFullName_TooLong_ReturnsFalse()
    {
        var longName = new string('A', 101);
        FullNameValidator.IsValidFullName(longName).Should().BeFalse();
    }

    [Fact]
    public void IsValidFullName_MaxLength100_ReturnsTrue()
    {
        var maxName = new string('A', 100);
        FullNameValidator.IsValidFullName(maxName).Should().BeTrue();
    }
}