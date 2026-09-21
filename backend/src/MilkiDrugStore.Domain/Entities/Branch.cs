namespace MilkiDrugStore.Domain.Entities;

public class Branch
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<User> Users { get; set; } = new();
    public List<Sale> Sales { get; set; } = new();
    public List<Purchase> Purchases { get; set; } = new();
    public List<MedicineBatch> MedicineBatches { get; set; } = new();
    public List<DamageRecord> DamageRecords { get; set; } = new();
    public List<ExpiredRecord> ExpiredRecords { get; set; } = new();
    public List<AuditLog> AuditLogs { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
    public Settings? Settings { get; set; }
}
