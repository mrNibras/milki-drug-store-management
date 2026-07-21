namespace MilkiDrugStore.Application.DTOs.Branch;

public class BranchResponse
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBranchRequest
{
    public string BranchName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class UpdateBranchRequest
{
    public string BranchName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}
