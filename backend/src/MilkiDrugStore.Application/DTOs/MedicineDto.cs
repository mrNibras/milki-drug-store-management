namespace MilkiDrugStore.Application.DTOs.Medicine;

public class CreateMedicineRequest
{
    public string MedicineName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string UnitType { get; set; } = "Tablet";
    public int LowStockThreshold { get; set; } = 10;
}

public class UpdateMedicineRequest
{
    public string MedicineName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string UnitType { get; set; } = "Tablet";
    public int LowStockThreshold { get; set; } = 10;
    public bool IsActive { get; set; }
}

public class AddBatchRequest
{
    public int MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public DateTime ExpiryDate { get; set; }
}

public class MedicineResponse
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public int LowStockThreshold { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BatchResponse> Batches { get; set; } = new();
}

public class BatchResponse
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityIssued { get; set; }
    public int QuantityDamaged { get; set; }
    public int QuantityExpired { get; set; }
    public int Balance { get; set; }
    public DateTime ExpiryDate { get; set; }
}
