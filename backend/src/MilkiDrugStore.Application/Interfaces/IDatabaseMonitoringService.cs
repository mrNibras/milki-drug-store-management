namespace MilkiDrugStore.Application.Interfaces;

public class DatabaseSizeInfo
{
    public long SizeBytes { get; set; }
    public string SizeReadable { get; set; } = string.Empty;
    public string Status { get; set; } = "ok";
    public long WarningThresholdBytes { get; set; }
    public long CriticalThresholdBytes { get; set; }
}

public interface IDatabaseMonitoringService
{
    Task<DatabaseSizeInfo> GetDatabaseSizeInfoAsync();
}
