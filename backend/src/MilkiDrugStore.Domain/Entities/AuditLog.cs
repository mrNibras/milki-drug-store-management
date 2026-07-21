namespace MilkiDrugStore.Domain.Entities;

public class AuditLog
{
    public int AuditId { get; set; }
    public int UserId { get; set; }
    public int? BranchId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int? RecordId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User? User { get; set; }
    public Branch? Branch { get; set; }
}
