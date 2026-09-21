namespace MilkiDrugStore.Domain.Entities;

public class NotificationArchive
{
    public int ArchiveId { get; set; }
    public int OriginalNotificationId { get; set; }
    public int BranchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
    public int ArchivedBy { get; set; }

    public Branch? Branch { get; set; }
}
