namespace MilkiDrugStore.Domain.Entities;

public class SaleItem
{
    public int SaleItemId { get; set; }
    public int SaleId { get; set; }
    public int MedicineId { get; set; }
    public int? BatchId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Profit { get; set; }
    public decimal SubTotal { get; set; }

    public Sale? Sale { get; set; }
    public Medicine? Medicine { get; set; }
    public MedicineBatch? Batch { get; set; }
}
