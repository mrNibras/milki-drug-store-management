import axios from 'axios';
import { config } from '../config';

const API_BASE_URL = config.apiBaseUrl;

export const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// --- Refresh-token handling ---------------------------------------------
let isRefreshing = false;
let pendingQueue: { resolve: (token: string) => void; reject: (err: unknown) => void }[] = [];

const processQueue = (error: unknown, token: string | null) => {
  pendingQueue.forEach((p) => {
    if (token) p.resolve(token);
    else p.reject(error);
  });
  pendingQueue = [];
};

const clearAuthAndRedirect = () => {
  localStorage.removeItem('auth_token');
  localStorage.removeItem('refresh_token');
  localStorage.removeItem('current_user');
  if (window.location.pathname !== '/login') {
    window.location.href = '/login';
  }
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    const status = error.response?.status;
    const url: string = originalRequest?.url || '';

    // 403 = authenticated but not allowed (role). Do NOT logout, just surface it.
    if (status === 403) {
      return Promise.reject(error);
    }

    const isAuthEndpoint = url.includes('/auth/login') || url.includes('/auth/refresh');

    if (status === 401 && originalRequest && !originalRequest._retry && !isAuthEndpoint) {
      const refreshToken = localStorage.getItem('refresh_token');
      if (!refreshToken) {
        clearAuthAndRedirect();
        return Promise.reject(error);
      }

      // A refresh is already in flight — queue this request until it resolves.
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          pendingQueue.push({
            resolve: (token: string) => {
              originalRequest.headers.Authorization = `Bearer ${token}`;
              resolve(api(originalRequest));
            },
            reject,
          });
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Use a bare axios call so this request does not loop through the interceptor.
        const res = await axios.post<LoginResponse>(
          `${API_BASE_URL}/api/auth/refresh`,
          { refreshToken },
          { headers: { 'Content-Type': 'application/json' } }
        );
        const newToken = res.data.token;
        const newRefresh = res.data.refreshToken;
        localStorage.setItem('auth_token', newToken);
        if (newRefresh) localStorage.setItem('refresh_token', newRefresh);
        api.defaults.headers.common.Authorization = `Bearer ${newToken}`;
        processQueue(null, newToken);
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        clearAuthAndRedirect();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    if (status === 401) {
      clearAuthAndRedirect();
    }
    return Promise.reject(error);
  }
);

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  role: string;
  userId: number;
  fullName: string;
  branchId: number;
  branchName: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface ForgotPasswordResponse {
  message: string;
}

export interface Category {
  categoryId: number;
  name: string;
  unitTypeId: number;
  unitTypeName: string;
  isActive: boolean;
  createdAt: string;
}

export interface UnitType {
  unitTypeId: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CatalogOptionDto {
  id: number;
  name: string;
  isSystem: boolean;
}

export interface MedicineResponse {
  productId: number;
  productCode: string;
  brandName: string;
  genericName: string;
  strength?: string;
  dosageForm?: string;
  barcode?: string;
  manufacturer?: string;
  description?: string;
  categoryId: number;
  categoryName: string;
  unitTypeId: number;
  unitTypeName: string;
  purchasePrice: number;
  sellingPrice: number;
  reorderLevel: number;
  isActive: boolean;
  createdDate: string;
  updatedDate?: string;
  totalStock: number;
  batches: BatchResponse[];
}

export interface BatchResponse {
  batchId: number;
  productId: number;
  branchId: number;
  batchNumber: string;
  purchasePrice: number;
  sellingPrice: number;
  quantityReceived: number;
  quantityIssued: number;
  quantityDamaged: number;
  quantityExpired: number;
  remainingQuantity: number;
  expiryDate: string;
  dateReceived: string;
  supplierId?: number;
  supplierName?: string;
}

export interface SupplierResponse {
  supplierId: number;
  supplierName: string;
  phone: string;
  email: string;
  address: string;
  paymentStatus?: string;
  createdAt: string;
}

export interface PurchaseResponse {
  purchaseId: number;
  purchaseNumber: string;
  supplierId: number;
  supplierName: string;
  purchaseDate: string;
  totalAmount: number;
  amountPaid: number;
  amountDue: number;
  paymentStatus: string;
  paymentMethod: string | null;
  items: PurchaseItemResponse[];
}

export interface PurchaseItemResponse {
  purchaseItemId: number;
  productId: number;
  productName?: string;
  productType?: string;
  brandName: string;
  batchNumber: string;
  quantity: number;
  purchasePrice: number;
  subTotal: number;
  cosmeticId?: number;
  cosmeticBatchId?: number;
}

export interface CreatePurchaseRequest {
  supplierId: number;
  purchaseDate: string;
  paymentMethod?: string;
  paymentStatus?: string;
  amountPaid: number;
  items: {
    productId?: number;
    productType?: string;
    brandName?: string;
    genericName?: string;
    categoryId?: number;
    categoryName?: string;
    unitType?: string;
    reorderLevel?: number;
    batchNumber: string;
    quantity: number;
    purchasePrice: number;
    sellingPrice: number;
    expiryDate?: string;
  }[];
}

export interface SaleResponse {
  saleId: number;
  saleNumber: string;
  saleDate: string;
  totalAmount: number;
  totalProfit: number;
  totalDiscount: number;
  discountReason: string | null;
  userId: number;
  userName: string;
  paymentMethod: string;
  paymentStatus: string;
  amountPaid: number;
  amountDue: number;
  referenceNumber: string | null;
  items: SaleItemResponse[];
}

export interface SaleItemResponse {
  saleItemId: number;
  medicineId: number;
  productType?: string;
  brandName: string;
  batchId?: number;
  cosmeticId?: number;
  cosmeticBatchId?: number;
  batchNumber: string;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  subTotal: number;
}

export interface CreateSaleRequest {
  items: {
    medicineId: number;
    productType?: string;
    quantity: number;
    discountAmount: number;
    cosmeticId?: number;
    cosmeticBatchId?: number;
    batchId?: number;
  }[];
  paymentMethod: string;
  amountPaid: number;
  referenceNumber?: string;
  discountReason?: string;
}

export interface DashboardSummaryResponse {
  totalMedicines: number;
  inventoryValue: number;
  todaySales: number;
  monthlySales: number;
  monthlyProfit: number;
  lowStockCount: number;
  expiringCount: number;
  outOfStockCount: number;
}

export interface NotificationResponse {
  notificationId: number;
  title: string;
  message: string;
  notificationType: string;
  isRead: boolean;
  createdAt: string;
}

export interface UserResponse {
  userId: number;
  fullName: string;
  email: string;
  roleId: number;
  roleName: string;
  isApproved: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface SettingsResponse {
  settingId: number;
  pharmacyName: string;
  address: string;
  phone: string;
  email: string;
  language: string;
  lowStockThreshold: number;
  expiryAlertMonths: number;
  currency: string;
}

export interface AuditLogResponse {
  auditId: number;
  userId: number;
  userName: string;
  action: string;
  tableName: string;
  recordId: number | null;
  createdAt: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface RecordDamageRequest {
  batchId: number;
  quantity: number;
  reason: string;
}

export interface RecordExpiredRequest {
  batchId: number;
  quantity: number;
}

export interface BranchResponse {
  branchId: number;
  branchName: string;
  location?: string;
  phone?: string;
  email?: string;
  address?: string;
  isActive: boolean;
}

export interface CreateBranchRequest {
  branchName: string;
  location?: string;
  phone?: string;
  email?: string;
  address?: string;
}

export interface UpdateBranchRequest {
  branchName: string;
  location?: string;
  phone?: string;
  email?: string;
  address?: string;
  isActive: boolean;
}

export interface DamageResponse {
  damageId: number;
  batchId: number;
  quantity: number;
  reason: string;
  recordedBy: number;
  recordedDate: string;
  batchNumber?: string;
  brandName?: string;
}

export interface ExpiredResponse {
  expiredId: number;
  batchId: number;
  quantity: number;
  recordedDate: string;
  recordedBy: number;
  batchNumber?: string;
  brandName?: string;
}

export interface CosmeticBatchResponse {
  batchId: number;
  cosmeticId: number;
  batchNumber: string;
  quantityReceived: number;
  quantityIssued: number;
  quantityDamaged: number;
  quantityExpired: number;
  balance: number;
  expiryDate?: string;
  dateReceived: string;
  buyingPrice: number;
  sellingPrice: number;
  lowStockThreshold: number;
  branchId: number;
  supplierId?: number;
  remarks?: string;
}

export interface CosmeticResponse {
  cosmeticId: number;
  productName: string;
  description: string;
  categoryId: number;
  categoryName: string;
  unitTypeId: number;
  unitTypeName: string;
  price: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  branchId: number;
  supplierId?: number;
  supplierName?: string;
  batches: CosmeticBatchResponse[];
}

// --- Settings / Backup / Restore -----------------------------------------
export const backupDatabase = async (): Promise<Blob> => {
  const response = await api.get('/settings/backup', { responseType: 'blob' });
  return response.data;
};

export const restoreDatabase = async (file: File): Promise<void> => {
  const formData = new FormData();
  formData.append('backupFile', file);
  await api.post('/settings/restore', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
};

// --- Cosmetics API ----------------------------------------------------------
export interface CreateCosmeticRequest {
  productName: string;
  description?: string;
  categoryId: number;
  unitTypeId: number;
  price: number;
  branchId?: number;
  supplierId?: number;
}

export interface AddCosmeticBatchRequest {
  cosmeticId: number;
  batchNumber: string;
  quantity: number;
  purchasePrice: number;
  sellingPrice: number;
  expiryDate?: string;
  lowStockThreshold?: number;
  branchId?: number;
  supplierId?: number;
  remarks?: string;
}

export const fetchCosmetics = async (): Promise<CosmeticResponse[]> => {
  const res = await api.get<CosmeticResponse[]>('/cosmetics');
  return res.data;
};

export const createCosmetic = async (req: CreateCosmeticRequest): Promise<CosmeticResponse> => {
  const res = await api.post<CosmeticResponse>('/cosmetics', req);
  return res.data;
};

export const addCosmeticBatch = async (req: AddCosmeticBatchRequest): Promise<CosmeticResponse> => {
  const res = await api.post<CosmeticResponse>('/cosmetics/batches', req);
  return res.data;
};
