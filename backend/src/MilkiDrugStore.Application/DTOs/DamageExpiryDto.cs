namespace MilkiDrugStore.Application.DTOs.Sale;

public class RecordDamageRequest
{
    public int BatchId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RecordExpiredRequest
{
    public int BatchId { get; set; }
    public int Quantity { get; set; }
}

public class DamageRecordResponse
{
    public int DamageId { get; set; }
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int RecordedBy { get; set; }
    public DateTime RecordedDate { get; set; }
    public int BranchId { get; set; }
}

public class ExpiredRecordResponse
{
    public int ExpiredId { get; set; }
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime RecordedDate { get; set; }
    public int RecordedBy { get; set; }
    public int BranchId { get; set; }
}
