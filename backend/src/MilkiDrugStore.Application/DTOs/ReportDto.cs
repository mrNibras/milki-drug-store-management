namespace MilkiDrugStore.Application.DTOs.Report;

public class DashboardSummaryResponse
{
    public int TotalMedicines { get; set; }
    public decimal InventoryValue { get; set; }
    public decimal TodaySales { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal MonthlyProfit { get; set; }
    public int LowStockCount { get; set; }
    public int ExpiringCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class SalesReportResponse
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public decimal Profit { get; set; }
    public int TransactionCount { get; set; }
}

public class InventoryReportResponse
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ProductType { get; set; } = "medicine";
    public string CategoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Value { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SupplierReportResponse
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int Purchases { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDebt { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class StaffReportResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Profit { get; set; }
}
