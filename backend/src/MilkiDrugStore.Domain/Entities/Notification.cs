namespace MilkiDrugStore.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public int BranchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Branch? Branch { get; set; }
}
