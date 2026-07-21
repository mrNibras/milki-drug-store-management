namespace MilkiDrugStore.Domain.Entities;

public class Settings
{
    public int SettingId { get; set; }
    public int BranchId { get; set; }
    public string PharmacyName { get; set; } = "Milki Drug Store";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Language { get; set; } = "English";
    public int LowStockThreshold { get; set; } = 10;
    public int ExpiryAlertMonths { get; set; } = 6;
    public string Currency { get; set; } = "ETB";

    public Branch? Branch { get; set; }
}
