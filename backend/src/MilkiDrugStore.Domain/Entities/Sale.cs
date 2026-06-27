namespace MilkiDrugStore.Domain.Entities;

public class Sale
{
    public int SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public decimal TotalProfit { get; set; }
    public int UserId { get; set; }

    public User? User { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}
