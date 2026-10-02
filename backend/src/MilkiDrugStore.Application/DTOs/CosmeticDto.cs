namespace MilkiDrugStore.Application.DTOs.Cosmetic;

public class CosmeticResponse
{
    public int CosmeticId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int UnitTypeId { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int BranchId { get; set; }
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public List<BatchResponse> Batches { get; set; } = new();
}

public class BatchResponse
{
    public int BatchId { get; set; }
    public int CosmeticId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityReceived { get; set; }
    public int QuantityIssued { get; set; }
    public int QuantityDamaged { get; set; }
    public int QuantityExpired { get; set; }
    public int Balance { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime DateReceived { get; set; }
    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int LowStockThreshold { get; set; }
    public int BranchId { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? Remarks { get; set; }
}

public class CreateCosmeticRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int UnitTypeId { get; set; }

    public string? NewCategoryName { get; set; }
    public string? NewUnitTypeName { get; set; }

    public decimal Price { get; set; }
    public int? BranchId { get; set; }
    public int? SupplierId { get; set; }
}

public class UpdateCosmeticRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int UnitTypeId { get; set; }

    public string? NewCategoryName { get; set; }
    public string? NewUnitTypeName { get; set; }

    public decimal Price { get; set; }
    public bool? IsActive { get; set; }
}

public class AddBatchRequest
{
    public int CosmeticId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Remarks { get; set; }
    public int BranchId { get; set; }
    public int? SupplierId { get; set; }
    public int LowStockThreshold { get; set; } = 10;
}
