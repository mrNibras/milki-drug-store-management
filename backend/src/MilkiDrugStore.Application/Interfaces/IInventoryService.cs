namespace MilkiDrugStore.Application.Interfaces;

public interface IInventoryService
{
    Task<int> GetCurrentStockAsync(int medicineId);
    Task RecordDamageAsync(int batchId, int quantity, string reason, int recordedBy);
    Task RecordExpiredAsync(int batchId, int quantity, int recordedBy);
}
