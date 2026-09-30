namespace MilkiDrugStore.Domain.Entities;

public class ExpiredRecord
{
    public int ExpiredId { get; set; }
    public int BranchId { get; set; }

    /// <summary>Medicine batch reference. Null for cosmetic expiry.</summary>
    public int? BatchId { get; set; }

    /// <summary>Cosmetic product reference. Null for medicine expiry.</summary>
    public int? CosmeticId { get; set; }

    /// <summary>Cosmetic batch reference. Null for medicine expiry.</summary>
    public int? CosmeticBatchId { get; set; }

    public int Quantity { get; set; }
    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
    public int RecordedBy { get; set; }

    public MedicineBatch? Batch { get; set; }
    public Cosmetic? Cosmetic { get; set; }
    public CosmeticBatch? CosmeticBatch { get; set; }
    public Branch? Branch { get; set; }
}
