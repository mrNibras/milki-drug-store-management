namespace MilkiDrugStore.Application.DTOs.Supplier;

public class CreateSupplierRequest
{
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
}

public class UpdateSupplierRequest
{
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
}

public class SupplierResponse
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
