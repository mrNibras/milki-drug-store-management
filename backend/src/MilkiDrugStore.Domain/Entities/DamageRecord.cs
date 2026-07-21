namespace MilkiDrugStore.Domain.Entities;

public class DamageRecord
{
    public int DamageId { get; set; }
    public int BranchId { get; set; }
    public int BatchId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int RecordedBy { get; set; }
    public DateTime RecordedDate { get; set; } = DateTime.Now;

    public MedicineBatch? Batch { get; set; }
    public Branch? Branch { get; set; }
}
