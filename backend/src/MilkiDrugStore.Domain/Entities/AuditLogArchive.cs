namespace MilkiDrugStore.Domain.Entities;

public class AuditLogArchive
{
    public int ArchiveId { get; set; }
    public int OriginalAuditId { get; set; }
    public int UserId { get; set; }
    public int? BranchId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int? RecordId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
    public int ArchivedBy { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }
}
