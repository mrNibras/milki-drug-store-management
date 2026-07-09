namespace MilkiDrugStore.Application.DTOs;

public class AuditLogDto
{
    public int AuditId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int? RecordId { get; set; }
    public DateTime CreatedAt { get; set; }
}
