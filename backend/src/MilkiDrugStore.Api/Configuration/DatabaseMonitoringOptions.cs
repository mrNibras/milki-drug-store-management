namespace MilkiDrugStore.Api.Configuration;

public class DatabaseMonitoringOptions
{
    public int WarningThresholdGB { get; set; } = 5;
    public int CriticalThresholdGB { get; set; } = 10;
}

public class RetentionOptions
{
    public int AuditLogRetentionDays { get; set; } = 1095;
    public int NotificationRetentionDays { get; set; } = 180;
}
