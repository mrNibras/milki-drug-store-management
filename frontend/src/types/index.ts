export interface User {
  id: string;
  fullName: string;
  email: string;
  role: 'admin' | 'pharmacist';
  branchId?: number;
  branchName?: string;
  isActive: boolean;
  createdAt: string;
  password?: string;
}

export interface Category {
  id: string;
  name: string;
  unitTypeId: number;
  unitTypeName?: string;
  isActive: boolean;
  createdAt: string;
}

export interface UnitType {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CatalogOption {
  id: string;
  name: string;
  isSystem: boolean;
}

export interface Medicine {
  id: string;
  name: string;
  genericName: string;
  categoryId: string;
  categoryName: string;
  unitType: string;
  unitTypeId: number;
  lowStockThreshold: number;
  createdAt: string;
  batches: MedicineBatch[];
}

export interface MedicineBatch {
  id: string;
  medicineId: string;
  batchNumber: string;
  purchasePrice: number;
  sellingPrice: number;
  quantity: number;
  expiryDate: string;
  createdAt: string;
}

export interface Supplier {
  id: string;
  name: string;
  phone: string;
  email: string;
  address: string;
  contactPerson: string;
  isActive: boolean;
  createdAt: string;
}

export interface SupplierFinancialSummary {
  supplierId: string;
  totalPurchases: number;
  totalPaid: number;
  totalDebt: number;
  purchaseCount: number;
  status: 'cleared' | 'outstanding';
}

export interface Purchase {
  id: string;
  purchaseNumber: string;
  supplierId: string;
  supplierName: string;
  purchaseDate: string;
  totalAmount: number;
  paymentStatus: 'paid' | 'partial' | 'unpaid';
  paymentMethod: 'cash' | 'bank_transfer' | 'mobile_money' | 'credit';
  amountPaid: number;
  remainingDebt: number;
  items: PurchaseItem[];
}

export interface PurchaseItem {
  id: string;
  purchaseId: string;
  medicineId: string;
  brandName: string;
  batchNumber: string;
  quantity: number;
  purchasePrice: number;
  expiryDate: string;
}

export interface BulkPurchaseItem {
  id: string;
  brandName: string;
  genericName: string;
  categoryId: string;
  categoryName: string;
  batchNumber: string;
  quantity: string;
  purchasePrice: string;
  sellingPrice: string;
  expiryDate: string;
  unitType: string;
  isNewMedicine: boolean;
  existingMedicineId?: string;
  errors: string[];
  lowStockThreshold?: number;
}

export interface Sale {
  id: string;
  saleNumber: string;
  saleDate: string;
  totalAmount: number;
  totalDiscount: number;
  discountReason: string;
  approvedBy: string | null;
  profit: number;
  userId: string;
  userName: string;
  paymentMethod: string;
  paymentStatus: string;
  amountPaid: number;
  amountDue: number;
  referenceNumber: string | null;
  items: SaleItem[];
}

export interface SaleItem {
  id: string;
  saleId: string;
  medicineId: string;
  brandName: string;
  batchId: string;
  quantity: number;
  unitPrice: number;
  standardUnitPrice: number;
  actualUnitPrice: number;
  discountAmount: number;
  totalPrice: number;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: 'low_stock' | 'out_of_stock' | 'expiry' | 'info';
  isRead: boolean;
  createdAt: string;
}

export interface AuditLog {
  id: string;
  userId: string;
  userName: string;
  action: string;
  tableName: string;
  recordId: string;
  createdAt: string;
}

export interface CartItem {
  medicineId: string;
  brandName: string;
  batchId: string;
  batchNumber: string;
  quantity: number;
  unitPrice: number;
  sellingPrice: number;
  standardPrice: number;
  discountAmount: number;
  expiryDate: string;
  availableQuantity: number;
}

export interface DashboardStats {
  totalMedicines: number;
  totalInventoryValue: number;
  todaySales: number;
  monthlySales: number;
  lowStockCount: number;
  expiringMedicinesCount: number;
  totalProfit: number;
  totalSuppliers: number;
}

export interface SalesReportData {
  date: string;
  sales: number;
  profit: number;
  transactions: number;
}

export interface PharmacySettings {
  pharmacyName: string;
  lowStockThreshold: number;
  expiryAlertMonths: number;
  currency: string;
  address: string;
  phone: string;
  email: string;
  language: string;
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

export interface Branch {
  id: string;
  name: string;
  location?: string;
  phone?: string;
  email?: string;
  address?: string;
  isActive: boolean;
}
