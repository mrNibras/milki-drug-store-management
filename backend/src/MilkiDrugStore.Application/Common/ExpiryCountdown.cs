using System.Globalization;
using MilkiDrugStore.Domain.Common;

namespace MilkiDrugStore.Application.Common;

public static class ExpiryCountdown
{
    public static DateOnly GetToday(DateTime? nowUtc = null)
    {
        var utcNow = nowUtc ?? DateTime.UtcNow;
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcNow, BusinessClock.TimeZone);
        return DateOnly.FromDateTime(local.Date);
    }

    public static string FormatCountdown(DateOnly expiryDate, DateOnly today)
    {
        if (expiryDate < today)
        {
            var diff = today.DayNumber - expiryDate.DayNumber;
            if (diff == 1)
                return "Expired 1 day ago";
            if (diff == 0)
                return "Expires today";
            return $"Expired {diff} days ago";
        }

        if (expiryDate == today)
            return "Expires today";

        var remaining = GetCalendarDiff(today, expiryDate);
        var years = remaining.Years;
        var months = remaining.Months;
        var days = remaining.Days;

        if (years == 0 && months == 0)
        {
            if (days == 1)
                return "1 day left for expiry";
            return $"{days} days left for expiry";
        }

        var parts = new List<string>();
        if (years > 0)
            parts.Add(years == 1 ? "1 year" : $"{years} years");
        if (months > 0)
            parts.Add(months == 1 ? "1 month" : $"{months} months");
        if (days > 0)
            parts.Add(days == 1 ? "1 day" : $"{days} days");

        var result = string.Join(" and ", parts);
        return $"{result} left for expiry";
    }

    public static (int Years, int Months, int Days) GetCalendarDiff(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            var diff = GetCalendarDiff(to, from);
            return (-diff.Years, -diff.Months, -diff.Days);
        }

        var years = to.Year - from.Year;
        var months = to.Month - from.Month;
        var days = to.Day - from.Day;

        if (days < 0)
        {
            months--;
            var previousMonth = to.AddMonths(-1);
            days += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        }

        if (months < 0)
        {
            years--;
            months += 12;
        }

        return (years, months, days);
    }
}
