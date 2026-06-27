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
