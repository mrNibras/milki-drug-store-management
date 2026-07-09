namespace MilkiDrugStore.Application.DTOs.Sale;

public class CreateSaleRequest
{
    public List<SaleItemRequest> Items { get; set; } = new();
    public string PaymentMethod { get; set; } = "cash";
    public decimal AmountPaid { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class SaleItemRequest
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
}

public class SaleResponse
{
    public int SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalProfit { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string? ReferenceNumber { get; set; }
    public List<SaleItemResponse> Items { get; set; } = new();
}

public class SaleItemResponse
{
    public int SaleItemId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int? BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
