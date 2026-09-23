namespace MilkiDrugStore.Domain.Entities;

public class PurchaseItem
{
    public int PurchaseItemId { get; set; }
    public int PurchaseId { get; set; }
    public int ProductId { get; set; }
    public int? BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SubTotal { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int? CosmeticId { get; set; }
    public int? CosmeticBatchId { get; set; }

    public Purchase? Purchase { get; set; }
    public Medicine? Medicine { get; set; }
    public MedicineBatch? Batch { get; set; }
    public Cosmetic? Cosmetic { get; set; }
    public CosmeticBatch? CosmeticBatch { get; set; }
}
