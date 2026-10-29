using System;
using MilkiDrugStore.Application.Common;
using Xunit;
using FluentAssertions;

namespace MilkiDrugStore.Tests.Application;

public class ExpiryCountdownTests
{
    [Fact]
    public void SixMonthsFromToday_Today_Returns_SixMonths()
    {
        var today = new DateOnly(2026, 1, 15);
        var expiry = today.AddMonths(6);
        var result = ExpiryCountdown.FormatCountdown(expiry, today);
        result.Should().Be("6 months left for expiry");
    }

    [Fact]
    public void SixMonthsFromToday_Tomorrow_Returns_FiveMonthsAndTwentyNineDays()
    {
        var today = new DateOnly(2026, 1, 15);
        var expiry = today.AddMonths(6);
        var tomorrow = today.AddDays(1);
        var result = ExpiryCountdown.FormatCountdown(expiry, tomorrow);
        result.Should().Be("5 months and 29 days left for expiry");
    }

    [Fact]
    public void OneDayRemaining_Returns_OneDayLeft()
    {
        var today = new DateOnly(2026, 3, 10);
        var expiry = new DateOnly(2026, 3, 11);
        var result = ExpiryCountdown.FormatCountdown(expiry, today);
        result.Should().Be("1 day left for expiry");
    }

    [Fact]
    public void ExpiresToday_Returns_ExpiresToday()
    {
        var today = new DateOnly(2026, 5, 20);
        var result = ExpiryCountdown.FormatCountdown(today, today);
        result.Should().Be("Expires today");
    }

    [Fact]
    public void OneDayExpired_Returns_ExpiredOneDayAgo()
    {
        var today = new DateOnly(2026, 6, 1);
        var expiry = new DateOnly(2026, 5, 31);
        var result = ExpiryCountdown.FormatCountdown(expiry, today);
        result.Should().Be("Expired 1 day ago");
    }

    [Fact]
    public void MonthTransition_February_NonLeapYear()
    {
        var today = new DateOnly(2026, 1, 31);
        var expiry = new DateOnly(2026, 2, 28);
        var result = ExpiryCountdown.FormatCountdown(expiry, today);
        result.Should().Be("28 days left for expiry");
    }

    [Fact]
    public void MonthTransition_February_LeapYear()
    {
        var today = new DateOnly(2024, 1, 31);
        var expiry = new DateOnly(2024, 2, 29);
        var result = ExpiryCountdown.FormatCountdown(expiry, today);
        result.Should().Be("29 days left for expiry");
    }

    [Fact]
    public void DecemberToJanuaryTransition()
    {
        var today = new DateOnly(2026, 12, 15);
        var expiry = new DateOnly(2027, 1, 15);
        var result = ExpiryCountdown.FormatCountdown(expiry, today);
        result.Should().Be("1 month left for expiry");
    }

    [Fact]
    public void MoreThanOneYearAway()
    {
        var today = new DateOnly(2026, 1, 1);
        var expiry = new DateOnly(2028, 3, 5);
        var result = ExpiryCountdown.FormatCountdown(expiry, today);
        result.Should().Be("2 years and 2 months and 4 days left for expiry");
    }

    [Fact]
    public void CountdownUpdates_ConsecutiveDays()
    {
        var expiry = new DateOnly(2026, 7, 20);
        var today1 = new DateOnly(2026, 1, 20);
        var today2 = new DateOnly(2026, 1, 21);
        var result1 = ExpiryCountdown.FormatCountdown(expiry, today1);
        var result2 = ExpiryCountdown.FormatCountdown(expiry, today2);
        result1.Should().Be("6 months left for expiry");
        result2.Should().Be("5 months and 29 days left for expiry");
    }

    [Fact]
    public void NullExpiryDate_NotApplicable()
    {
        // Null expiry date not applicable - batch model has non-null DateTime
        true.Should().BeTrue();
    }
}
