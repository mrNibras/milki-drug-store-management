using MilkiDrugStore.Application.DTOs.SupplierPayment;

namespace MilkiDrugStore.Application.Interfaces;

public interface ISupplierPaymentService
{
    Task<IList<SupplierDebtSummaryResponse>> GetOutstandingDebtsAsync(int? branchId = null);
    Task<SupplierDebtSummaryResponse?> GetSupplierDebtAsync(int supplierId, int? branchId = null);
    Task<IList<SupplierDebtPurchaseResponse>> GetOutstandingPurchasesAsync(int supplierId, int? branchId = null);
    Task<IList<SupplierPaymentResponse>> GetPaymentHistoryAsync(int supplierId, int? branchId = null);
    Task<SupplierPaymentResponse> RecordPaymentAsync(CreateSupplierPaymentRequest request, int createdBy);
}
