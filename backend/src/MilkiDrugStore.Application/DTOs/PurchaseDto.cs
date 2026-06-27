namespace MilkiDrugStore.Application.DTOs.Purchase;

public class CreatePurchaseRequest
{
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public List<PurchaseItemRequest> Items { get; set; } = new();
}

public class PurchaseItemRequest
{
    public int MedicineId { get; set; }
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
