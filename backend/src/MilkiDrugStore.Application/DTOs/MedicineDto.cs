namespace MilkiDrugStore.Application.DTOs.Medicine;

public class CreateMedicineRequest
{
    public string? ProductCode { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int UnitTypeId { get; set; }

    /// <summary>
    /// Optional category name entered via the "Other" option. When provided,
    /// it is resolved against built-ins/customs and takes precedence over
    /// <see cref="CategoryId"/>.
    /// </summary>
    public string? NewCategoryName { get; set; }

    /// <summary>
    /// Optional unit type name entered via the "Other" option. When provided,
    /// it is resolved against built-ins/customs and takes precedence over
    /// <see cref="UnitTypeId"/>.
    /// </summary>
    public string? NewUnitTypeName { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int ReorderLevel { get; set; } = 10;
}

public class UpdateMedicineRequest
{
    public string? ProductCode { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int UnitTypeId { get; set; }

    /// <summary>
    /// Optional category name entered via the "Other" option. When provided,
    /// it is resolved against built-ins/customs and takes precedence over
    /// <see cref="CategoryId"/>.
    /// </summary>
    public string? NewCategoryName { get; set; }

    /// <summary>
    /// Optional unit type name entered via the "Other" option. When provided,
    /// it is resolved against built-ins/customs and takes precedence over
    /// <see cref="UnitTypeId"/>.
    /// </summary>
    public string? NewUnitTypeName { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public bool? IsActive { get; set; }
}

public class AddBatchRequest
{
    public int ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public int? SupplierId { get; set; }
    public string? Remarks { get; set; }
}

public class MedicineResponse
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int UnitTypeId { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int TotalStock { get; set; }
    public List<BatchResponse> Batches { get; set; } = new();
}

public class BatchResponse
{
    public int BatchId { get; set; }
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityIssued { get; set; }
    public int QuantityDamaged { get; set; }
    public int QuantityExpired { get; set; }
    public int RemainingQuantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime DateReceived { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
}

public class MedicineSearchResponse
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public string? Barcode { get; set; }
    public int TotalStock { get; set; }
    public decimal SellingPrice { get; set; }
}
