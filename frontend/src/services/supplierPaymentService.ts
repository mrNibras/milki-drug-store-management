import { api } from './api';

export interface SupplierPaymentAllocationRequest {
  purchaseId: number;
  allocatedAmount: number;
}

export interface CreateSupplierPaymentRequest {
  supplierId: number;
  branchId: number;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber?: string;
  notes?: string;
  allocations: SupplierPaymentAllocationRequest[];
}

export interface SupplierPaymentAllocationResponse {
  supplierPaymentItemId: number;
  purchaseId: number;
  purchaseNumber: string;
  allocatedAmount: number;
}

export interface SupplierPaymentResponse {
  supplierPaymentId: number;
  supplierId: number;
  supplierName: string;
  branchId: number;
  branchName: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber?: string;
  notes?: string;
  createdBy: number;
  createdByName: string;
  createdAt: string;
  allocations: SupplierPaymentAllocationResponse[];
}

export interface SupplierDebtPurchaseResponse {
  purchaseId: number;
  purchaseNumber: string;
  purchaseDate: string;
  totalAmount: number;
  amountPaid: number;
  amountDue: number;
  paymentStatus: string;
  paymentMethod?: string;
}

export interface SupplierDebtSummaryResponse {
  supplierId: number;
  supplierName: string;
  phone: string;
  email: string;
  totalPurchases: number;
  totalPaid: number;
  outstandingDebt: number;
  outstandingPurchaseCount: number;
  outstandingPurchases: SupplierDebtPurchaseResponse[];
}

export const supplierPaymentService = {
  getOutstandingDebts: async (branchId?: number): Promise<SupplierDebtSummaryResponse[]> => {
    const response = await api.get('/supplierpayments/debts', { params: branchId ? { branchId } : {} });
    return response.data;
  },
  getSupplierDebt: async (supplierId: number, branchId?: number): Promise<SupplierDebtSummaryResponse | null> => {
    const response = await api.get(`/supplierpayments/debts/${supplierId}`, { params: branchId ? { branchId } : {} });
    return response.data;
  },
  getOutstandingPurchases: async (supplierId: number, branchId?: number): Promise<SupplierDebtPurchaseResponse[]> => {
    const response = await api.get(`/supplierpayments/outstanding/${supplierId}`, { params: branchId ? { branchId } : {} });
    return response.data;
  },
  getPaymentHistory: async (supplierId: number, branchId?: number): Promise<SupplierPaymentResponse[]> => {
    const response = await api.get(`/supplierpayments/history/${supplierId}`, { params: branchId ? { branchId } : {} });
    return response.data;
  },
  recordPayment: async (request: CreateSupplierPaymentRequest): Promise<SupplierPaymentResponse> => {
    const response = await api.post('/supplierpayments', request);
    return response.data;
  }
};
