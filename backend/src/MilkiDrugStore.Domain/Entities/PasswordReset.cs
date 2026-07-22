namespace MilkiDrugStore.Domain.Entities;

public class PasswordReset
{
    public int PasswordResetId { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
