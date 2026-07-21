namespace MilkiDrugStore.Domain.Entities;

public class ExpiredRecord
{
    public int ExpiredId { get; set; }
    public int BranchId { get; set; }
    public int BatchId { get; set; }
    public int Quantity { get; set; }
    public DateTime RecordedDate { get; set; } = DateTime.Now;
    public int RecordedBy { get; set; }

    public MedicineBatch? Batch { get; set; }
    public Branch? Branch { get; set; }
}
