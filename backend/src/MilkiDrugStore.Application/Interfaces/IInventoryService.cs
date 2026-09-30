using MilkiDrugStore.Application.DTOs.Sale;

namespace MilkiDrugStore.Application.Interfaces;

public interface IInventoryService
{
    Task<int> GetCurrentStockAsync(int productId, int? branchId = null);
    Task RecordDamageAsync(int? batchId, int? cosmeticBatchId, int quantity, string reason, int recordedBy);
    Task<ExpiredProcessingResult> ProcessExpiredInventoryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DamageRecordResponse>> GetDamagesAsync(int? branchId = null);
    Task<IEnumerable<ExpiredRecordResponse>> GetExpiredAsync(int? branchId = null);
}
