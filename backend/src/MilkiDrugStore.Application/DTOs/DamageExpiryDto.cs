namespace MilkiDrugStore.Application.DTOs.Sale;

public class RecordDamageRequest
{
    /// <summary>Medicine batch to damage. Mutually exclusive with CosmeticBatchId.</summary>
    public int? BatchId { get; set; }

    /// <summary>Cosmetic batch to damage. Mutually exclusive with BatchId.</summary>
    public int? CosmeticBatchId { get; set; }

    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class DamageRecordResponse
{
    public int DamageId { get; set; }

    /// <summary>"medicine" or "cosmetic".</summary>
    public string ProductType { get; set; } = string.Empty;

    public int? ProductId { get; set; }
    public int? BatchId { get; set; }
    public int? CosmeticId { get; set; }
    public int? CosmeticBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Medicine brand name or cosmetic product name, depending on ProductType.</summary>
    public string BrandName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int RecordedBy { get; set; }
    public DateTime RecordedDate { get; set; }
    public int BranchId { get; set; }
}

public class ExpiredRecordResponse
{
    public int ExpiredId { get; set; }

    /// <summary>"medicine" or "cosmetic".</summary>
    public string ProductType { get; set; } = string.Empty;

    public int? ProductId { get; set; }
    public int? BatchId { get; set; }
    public int? CosmeticId { get; set; }
    public int? CosmeticBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Medicine brand name or cosmetic product name, depending on ProductType.</summary>
    public string BrandName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public DateTime RecordedDate { get; set; }
    public int RecordedBy { get; set; }
    public int BranchId { get; set; }
}

/// <summary>Result of an automatic expiry sweep.</summary>
public class ExpiredProcessingResult
{
    public int MedicineBatchesProcessed { get; set; }
    public int CosmeticBatchesProcessed { get; set; }
    public int TotalUnitsExpired { get; set; }
}
