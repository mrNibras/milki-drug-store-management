import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5001';

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

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('current_user');
      window.location.href = '/login';
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
  role: string;
  userId: number;
  fullName: string;
}

export interface Category {
  categoryId: number;
  name: string;
}

export interface MedicineResponse {
  medicineId: number;
  medicineName: string;
  genericName: string;
  categoryId: number;
  categoryName: string;
  unitType: string;
  lowStockThreshold: number;
  isActive: boolean;
  createdAt: string;
  batches: BatchResponse[];
}

export interface BatchResponse {
  batchId: number;
  batchNumber: string;
  purchasePrice: number;
  sellingPrice: number;
  quantityReceived: number;
  quantityIssued: number;
  quantityDamaged: number;
  quantityExpired: number;
  balance: number;
  expiryDate: string;
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
  items: PurchaseItemResponse[];
}

export interface PurchaseItemResponse {
  purchaseItemId: number;
  medicineId: number;
  medicineName: string;
  batchNumber: string;
  quantity: number;
  purchasePrice: number;
  subTotal: number;
}

export interface CreatePurchaseRequest {
  supplierId: number;
  purchaseDate: string;
  items: {
    medicineId: number;
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
  userId: number;
  userName: string;
  items: SaleItemResponse[];
}

export interface SaleItemResponse {
  saleItemId: number;
  medicineId: number;
  medicineName: string;
  batchId?: number;
  batchNumber: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

export interface CreateSaleRequest {
  items: {
    medicineId: number;
    quantity: number;
  }[];
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

export interface DamageResponse {
  damageId: number;
  batchId: number;
  quantity: number;
  reason: string;
  recordedBy: number;
  recordedDate: string;
  batchNumber?: string;
  medicineName?: string;
}

export interface ExpiredResponse {
  expiredId: number;
  batchId: number;
  quantity: number;
  recordedDate: string;
  recordedBy: number;
  batchNumber?: string;
  medicineName?: string;
}
