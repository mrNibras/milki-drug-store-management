namespace MilkiDrugStore.Domain.Entities;

public class DamageRecord
{
    public int DamageId { get; set; }
    public int BranchId { get; set; }

    /// <summary>Medicine batch reference. Null for cosmetic damage.</summary>
    public int? BatchId { get; set; }

    /// <summary>Cosmetic product reference. Null for medicine damage.</summary>
    public int? CosmeticId { get; set; }

    /// <summary>Cosmetic batch reference. Null for medicine damage.</summary>
    public int? CosmeticBatchId { get; set; }

    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int RecordedBy { get; set; }
    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

    public MedicineBatch? Batch { get; set; }
    public Cosmetic? Cosmetic { get; set; }
    public CosmeticBatch? CosmeticBatch { get; set; }
    public Branch? Branch { get; set; }
}
