namespace MilkiDrugStore.Application.DTOs.Purchase;

public class CreatePurchaseRequest
{
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public List<PurchaseItemRequest> Items { get; set; } = new();
}

public class PurchaseItemRequest
{
    public int MedicineId { get; set; }
    /// <summary>Provided when the medicine does not exist yet (auto-creation).</summary>
    public string? MedicineName { get; set; }
    public string? GenericName { get; set; }
    public int? CategoryId { get; set; }
    public string? UnitType { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class PurchaseResponse
{
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public List<PurchaseItemResponse> Items { get; set; } = new();
}

public class PurchaseItemResponse
{
    public int PurchaseItemId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SubTotal { get; set; }
}
