namespace MilkiDrugStore.Domain.Common;

/// <summary>
/// Central business clock for the pharmacy. The business operates in Ethiopian
/// local time (UTC+03:00, no daylight saving). The backend runs on Linux, so the
/// IANA id "Africa/Addis_Ababa" is the primary lookup and the Windows id
/// "E. Africa Standard Time" is the cross-platform fallback.
/// </summary>
public static class BusinessClock
{
    public const string UnixTimeZoneId = "Africa/Addis_Ababa";
    public const string WindowsTimeZoneId = "E. Africa Standard Time";

    private static readonly TimeSpan FallbackOffset = TimeSpan.FromHours(3);

    private static TimeZoneInfo ResolveTimeZone()
    {
        return TryResolve(UnixTimeZoneId) ?? TryResolve(WindowsTimeZoneId) ?? TimeZoneInfo.CreateCustomTimeZone("Milki-Ethiopia", FallbackOffset, "Milki Ethiopia", "Milki Ethiopia");
    }

    private static TimeZoneInfo? TryResolve(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }
    }

    public static TimeZoneInfo TimeZone => ResolveTimeZone();

    /// <summary>Current time in the pharmacy's Ethiopian local time.</summary>
    public static DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone);

    /// <summary>Current Ethiopian calendar date (business date).</summary>
    public static DateTime LocalToday => LocalNow.Date;

    /// <summary>
    /// Ethiopian wall-clock "now", tagged as UTC so it can be used as a query
    /// parameter against PostgreSQL <c>timestamp with time zone</c> columns
    /// (Npgsql rejects DateTimeKind.Unspecified for those).
    ///
    /// Business dates are compared as Ethiopian wall-clock values: a batch whose
    /// ExpiryDate has passed in Addis Ababa is expired, regardless of the
    /// server's own offset.
    /// </summary>
    public static DateTime BusinessNowAsUtc => DateTime.SpecifyKind(LocalNow, DateTimeKind.Utc);
}
