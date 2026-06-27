namespace MilkiDrugStore.Domain.Events;

public record SaleCreatedEvent(int SaleId, decimal TotalAmount, int UserId) : IDomainEvent;
public record StockLowEvent(int MedicineId, int CurrentStock) : IDomainEvent;
public record MedicineExpiredEvent(int BatchId, int MedicineId) : IDomainEvent;
public record PurchaseCreatedEvent(int PurchaseId, decimal TotalAmount) : IDomainEvent;

public interface IDomainEvent;
