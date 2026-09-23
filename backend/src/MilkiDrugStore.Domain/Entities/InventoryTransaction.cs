namespace MilkiDrugStore.Domain.Entities;

public class InventoryTransaction
{
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public int? BatchId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CosmeticId { get; set; }
    public int? CosmeticBatchId { get; set; }

    public Medicine? Medicine { get; set; }
    public MedicineBatch? Batch { get; set; }
    public Cosmetic? Cosmetic { get; set; }
    public CosmeticBatch? CosmeticBatch { get; set; }
}
