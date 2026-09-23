namespace MilkiDrugStore.Domain.Entities;

public class SaleItem
{
    public int SaleItemId { get; set; }
    public int SaleId { get; set; }
    public int? ProductId { get; set; }
    public int? BatchId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Profit { get; set; }
    public decimal SubTotal { get; set; }

    public int? CosmeticId { get; set; }
    public int? CosmeticBatchId { get; set; }

    public Sale? Sale { get; set; }
    public Medicine? Medicine { get; set; }
    public MedicineBatch? Batch { get; set; }
    public Cosmetic? Cosmetic { get; set; }
    public CosmeticBatch? CosmeticBatch { get; set; }
}
