namespace MilkiDrugStore.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int BranchId { get; set; }
    public bool IsApproved { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Role? Role { get; set; }
    public Branch? Branch { get; set; }
    public List<Sale> Sales { get; set; } = new();
    public List<AuditLog> AuditLogs { get; set; } = new();
    public List<InventoryTransaction> InventoryTransactions { get; set; } = new();
}
