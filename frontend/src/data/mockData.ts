import { Category, Medicine, Supplier, Purchase, Sale, Notification, AuditLog, User } from '../types';

export const mockCategories: Category[] = [
  { id: '1', name: 'Antibiotic' },
  { id: '2', name: 'Pain Killer' },
  { id: '3', name: 'Vitamin' },
  { id: '4', name: 'Respiratory' },
  { id: '5', name: 'Dermatology' },
  { id: '6', name: 'Cosmetics' },
  { id: '7', name: 'Baby Supplies' },
  { id: '8', name: 'Cardiovascular' },
  { id: '9', name: 'Gastrointestinal' },
  { id: '10', name: 'Anti-inflammatory' },
];

export const mockUsers: User[] = [
  { id: '1', fullName: 'Admin User', email: 'admin@milki.com', role: 'admin', isActive: true, createdAt: '2026-01-01T00:00:00' },
  { id: '2', fullName: 'Ahmed Hassan', email: 'ahmed@milki.com', role: 'pharmacist', isActive: true, createdAt: '2026-01-15T00:00:00' },
  { id: '3', fullName: 'Sara Mohammed', email: 'sara@milki.com', role: 'pharmacist', isActive: true, createdAt: '2026-02-01T00:00:00' },
  { id: '4', fullName: 'Dawit Tesfaye', email: 'dawit@milki.com', role: 'pharmacist', isActive: false, createdAt: '2026-03-01T00:00:00' },
];

export const mockMedicines: Medicine[] = [
  {
    id: '1', name: 'Paracetamol 500mg', genericName: 'Acetaminophen', categoryId: '2', categoryName: 'Pain Killer',
    unitType: 'Tablet', lowStockThreshold: 10, createdAt: '2026-01-10T00:00:00',
    batches: [
      { id: 'b1', medicineId: '1', batchNumber: 'B001', purchasePrice: 3, sellingPrice: 10, quantity: 120, expiryDate: '2027-06-15', createdAt: '2026-01-10' },
      { id: 'b2', medicineId: '1', batchNumber: 'B002', purchasePrice: 3, sellingPrice: 10, quantity: 80, expiryDate: '2026-12-01', createdAt: '2026-03-10' },
    ]
  },
  {
    id: '2', name: 'Amoxicillin 250mg', genericName: 'Amoxicillin', categoryId: '1', categoryName: 'Antibiotic',
    unitType: 'Capsule', lowStockThreshold: 10, createdAt: '2026-01-12T00:00:00',
    batches: [
      { id: 'b3', medicineId: '2', batchNumber: 'B003', purchasePrice: 15, sellingPrice: 50, quantity: 50, expiryDate: '2027-03-20', createdAt: '2026-01-12' },
      { id: 'b4', medicineId: '2', batchNumber: 'B004', purchasePrice: 15, sellingPrice: 50, quantity: 8, expiryDate: '2026-08-15', createdAt: '2026-02-12' },
    ]
  },
  {
    id: '3', name: 'Vitamin C 1000mg', genericName: 'Ascorbic Acid', categoryId: '3', categoryName: 'Vitamin',
    unitType: 'Tablet', lowStockThreshold: 10, createdAt: '2026-01-15T00:00:00',
    batches: [
      { id: 'b5', medicineId: '3', batchNumber: 'B005', purchasePrice: 5, sellingPrice: 15, quantity: 200, expiryDate: '2028-01-10', createdAt: '2026-01-15' },
    ]
  },
  {
    id: '4', name: 'Cetirizine 10mg', genericName: 'Cetirizine HCl', categoryId: '4', categoryName: 'Respiratory',
    unitType: 'Tablet', lowStockThreshold: 10, createdAt: '2026-01-20T00:00:00',
    batches: [
      { id: 'b6', medicineId: '4', batchNumber: 'B006', purchasePrice: 2, sellingPrice: 8, quantity: 150, expiryDate: '2027-09-30', createdAt: '2026-01-20' },
    ]
  },
  {
    id: '5', name: 'Ibuprofen 400mg', genericName: 'Ibuprofen', categoryId: '2', categoryName: 'Pain Killer',
    unitType: 'Tablet', lowStockThreshold: 10, createdAt: '2026-02-01T00:00:00',
    batches: [
      { id: 'b7', medicineId: '5', batchNumber: 'B007', purchasePrice: 4, sellingPrice: 12, quantity: 90, expiryDate: '2027-05-15', createdAt: '2026-02-01' },
      { id: 'b8', medicineId: '5', batchNumber: 'B008', purchasePrice: 4, sellingPrice: 12, quantity: 5, expiryDate: '2026-07-20', createdAt: '2026-04-01' },
    ]
  },
  {
    id: '6', name: 'Omeprazole 20mg', genericName: 'Omeprazole', categoryId: '9', categoryName: 'Gastrointestinal',
    unitType: 'Capsule', lowStockThreshold: 10, createdAt: '2026-02-05T00:00:00',
    batches: [
      { id: 'b9', medicineId: '6', batchNumber: 'B009', purchasePrice: 8, sellingPrice: 25, quantity: 60, expiryDate: '2027-11-30', createdAt: '2026-02-05' },
    ]
  },
  {
    id: '7', name: 'Metformin 500mg', genericName: 'Metformin HCl', categoryId: '8', categoryName: 'Cardiovascular',
    unitType: 'Tablet', lowStockThreshold: 10, createdAt: '2026-02-10T00:00:00',
    batches: [
      { id: 'b10', medicineId: '7', batchNumber: 'B010', purchasePrice: 6, sellingPrice: 18, quantity: 100, expiryDate: '2027-08-15', createdAt: '2026-02-10' },
    ]
  },
  {
    id: '8', name: 'Clotrimazole Cream', genericName: 'Clotrimazole', categoryId: '5', categoryName: 'Dermatology',
    unitType: 'Tube', lowStockThreshold: 10, createdAt: '2026-02-15T00:00:00',
    batches: [
      { id: 'b11', medicineId: '8', batchNumber: 'B011', purchasePrice: 12, sellingPrice: 35, quantity: 40, expiryDate: '2027-04-20', createdAt: '2026-02-15' },
    ]
  },
  {
    id: '9', name: 'Salbutamol Inhaler', genericName: 'Salbutamol', categoryId: '4', categoryName: 'Respiratory',
    unitType: 'Piece', lowStockThreshold: 5, createdAt: '2026-03-01T00:00:00',
    batches: [
      { id: 'b12', medicineId: '9', batchNumber: 'B012', purchasePrice: 45, sellingPrice: 120, quantity: 25, expiryDate: '2027-02-28', createdAt: '2026-03-01' },
    ]
  },
  {
    id: '10', name: 'Diclofenac Gel', genericName: 'Diclofenac Sodium', categoryId: '10', categoryName: 'Anti-inflammatory',
    unitType: 'Tube', lowStockThreshold: 10, createdAt: '2026-03-10T00:00:00',
    batches: [
      { id: 'b13', medicineId: '10', batchNumber: 'B013', purchasePrice: 18, sellingPrice: 55, quantity: 30, expiryDate: '2027-07-15', createdAt: '2026-03-10' },
    ]
  },
  {
    id: '11', name: 'Baby Lotion', genericName: 'Baby Care', categoryId: '7', categoryName: 'Baby Supplies',
    unitType: 'Bottle', lowStockThreshold: 10, createdAt: '2026-03-15T00:00:00',
    batches: [
      { id: 'b14', medicineId: '11', batchNumber: 'B014', purchasePrice: 30, sellingPrice: 85, quantity: 45, expiryDate: '2028-03-15', createdAt: '2026-03-15' },
    ]
  },
  {
    id: '12', name: 'Nivea Face Cream', genericName: 'Face Cream', categoryId: '6', categoryName: 'Cosmetics',
    unitType: 'Piece', lowStockThreshold: 10, createdAt: '2026-03-20T00:00:00',
    batches: [
      { id: 'b15', medicineId: '12', batchNumber: 'B015', purchasePrice: 50, sellingPrice: 150, quantity: 35, expiryDate: '2028-06-01', createdAt: '2026-03-20' },
    ]
  },
];

export const mockSuppliers: Supplier[] = [
  { id: '1', name: 'ABC Pharmaceuticals', phone: '+251912345678', email: 'abc@pharma.com', address: 'Addis Ababa, Ethiopia', contactPerson: 'Abebe Kebede', isActive: true, createdAt: '2026-01-01' },
  { id: '2', name: 'MedSupply Ethiopia', phone: '+251923456789', email: 'medsupply@email.com', address: 'Bole, Addis Ababa', contactPerson: 'Tigist Haile', isActive: true, createdAt: '2026-01-15' },
  { id: '3', name: 'HealthCorp Ltd', phone: '+251934567890', email: 'healthcorp@email.com', address: 'Piassa, Addis Ababa', contactPerson: 'Dawit Tesfaye', isActive: true, createdAt: '2026-02-01' },
  { id: '4', name: 'PharmaDirect', phone: '+251945678901', email: 'pharmadirect@email.com', address: 'Merkato, Addis Ababa', contactPerson: 'Sara Mohammed', isActive: true, createdAt: '2026-02-15' },
  { id: '5', name: 'Ethio Medical Supplies', phone: '+251956789012', email: 'ethiomedical@email.com', address: 'Kazanchis, Addis Ababa', contactPerson: 'Yonas Alemayehu', isActive: false, createdAt: '2026-03-01' },
];

export const mockPurchases: Purchase[] = [
  {
    id: '1', purchaseNumber: 'PUR-2026-00001', supplierId: '1', supplierName: 'ABC Pharmaceuticals',
    purchaseDate: '2026-01-10T10:00:00', totalAmount: 3000,
    paymentStatus: 'paid', paymentMethod: 'bank_transfer', amountPaid: 3000, remainingDebt: 0,
    items: [
      { id: '1', purchaseId: '1', medicineId: '1', medicineName: 'Paracetamol 500mg', batchNumber: 'B001', quantity: 200, purchasePrice: 3, expiryDate: '2027-06-15' },
      { id: '2', purchaseId: '1', medicineId: '2', medicineName: 'Amoxicillin 250mg', batchNumber: 'B003', quantity: 100, purchasePrice: 15, expiryDate: '2027-03-20' },
    ]
  },
  {
    id: '2', purchaseNumber: 'PUR-2026-00002', supplierId: '2', supplierName: 'MedSupply Ethiopia',
    purchaseDate: '2026-02-01T14:30:00', totalAmount: 2500,
    paymentStatus: 'partial', paymentMethod: 'cash', amountPaid: 1500, remainingDebt: 1000,
    items: [
      { id: '3', purchaseId: '2', medicineId: '3', medicineName: 'Vitamin C 1000mg', batchNumber: 'B005', quantity: 200, purchasePrice: 5, expiryDate: '2028-01-10' },
      { id: '4', purchaseId: '2', medicineId: '5', medicineName: 'Ibuprofen 400mg', batchNumber: 'B007', quantity: 100, purchasePrice: 4, expiryDate: '2027-05-15' },
    ]
  },
  {
    id: '3', purchaseNumber: 'PUR-2026-00003', supplierId: '3', supplierName: 'HealthCorp Ltd',
    purchaseDate: '2026-03-05T09:00:00', totalAmount: 4200,
    paymentStatus: 'unpaid', paymentMethod: 'credit', amountPaid: 0, remainingDebt: 4200,
    items: [
      { id: '5', purchaseId: '3', medicineId: '6', medicineName: 'Omeprazole 20mg', batchNumber: 'B009', quantity: 60, purchasePrice: 8, expiryDate: '2027-11-30' },
      { id: '6', purchaseId: '3', medicineId: '7', medicineName: 'Metformin 500mg', batchNumber: 'B010', quantity: 100, purchasePrice: 6, expiryDate: '2027-08-15' },
      { id: '7', purchaseId: '3', medicineId: '9', medicineName: 'Salbutamol Inhaler', batchNumber: 'B012', quantity: 25, purchasePrice: 45, expiryDate: '2027-02-28' },
    ]
  },
];

export const mockSales: Sale[] = [
  {
    id: '1', saleNumber: 'SAL-2026-00001', saleDate: '2026-06-19T08:30:00', totalAmount: 170, totalDiscount: 0, discountReason: '', approvedBy: null, profit: 50, userId: '2', userName: 'Ahmed Hassan',
    items: [
      { id: '1', saleId: '1', medicineId: '1', medicineName: 'Paracetamol 500mg', batchId: 'b2', quantity: 10, unitPrice: 10, standardUnitPrice: 10, actualUnitPrice: 10, discountAmount: 0, totalPrice: 100 },
      { id: '2', saleId: '1', medicineId: '2', medicineName: 'Amoxicillin 250mg', batchId: 'b4', quantity: 2, unitPrice: 50, standardUnitPrice: 50, actualUnitPrice: 50, discountAmount: 0, totalPrice: 100 },
    ]
  },
  {
    id: '2', saleNumber: 'SAL-2026-00002', saleDate: '2026-06-19T10:15:00', totalAmount: 45, totalDiscount: 0, discountReason: '', approvedBy: null, profit: 12, userId: '3', userName: 'Sara Mohammed',
    items: [
      { id: '3', saleId: '2', medicineId: '3', medicineName: 'Vitamin C 1000mg', batchId: 'b5', quantity: 3, unitPrice: 15, standardUnitPrice: 15, actualUnitPrice: 15, discountAmount: 0, totalPrice: 45 },
    ]
  },
  {
    id: '3', saleNumber: 'SAL-2026-00003', saleDate: '2026-06-18T14:00:00', totalAmount: 300, totalDiscount: 20, discountReason: 'Family Assistance', approvedBy: null, profit: 75, userId: '2', userName: 'Ahmed Hassan',
    items: [
      { id: '4', saleId: '3', medicineId: '9', medicineName: 'Salbutamol Inhaler', batchId: 'b12', quantity: 2, unitPrice: 120, standardUnitPrice: 120, actualUnitPrice: 110, discountAmount: 20, totalPrice: 220 },
      { id: '5', saleId: '3', medicineId: '8', medicineName: 'Clotrimazole Cream', batchId: 'b11', quantity: 1, unitPrice: 35, standardUnitPrice: 35, actualUnitPrice: 35, discountAmount: 0, totalPrice: 35 },
      { id: '6', saleId: '3', medicineId: '4', medicineName: 'Cetirizine 10mg', batchId: 'b6', quantity: 5, unitPrice: 8, standardUnitPrice: 8, actualUnitPrice: 8, discountAmount: 0, totalPrice: 40 },
    ]
  },
  {
    id: '4', saleNumber: 'SAL-2026-00004', saleDate: '2026-06-17T09:45:00', totalAmount: 550, totalDiscount: 0, discountReason: '', approvedBy: null, profit: 170, userId: '3', userName: 'Sara Mohammed',
    items: [
      { id: '7', saleId: '4', medicineId: '12', medicineName: 'Nivea Face Cream', batchId: 'b15', quantity: 2, unitPrice: 150, standardUnitPrice: 150, actualUnitPrice: 150, discountAmount: 0, totalPrice: 300 },
      { id: '8', saleId: '4', medicineId: '7', medicineName: 'Metformin 500mg', batchId: 'b10', quantity: 5, unitPrice: 18, standardUnitPrice: 18, actualUnitPrice: 18, discountAmount: 0, totalPrice: 90 },
      { id: '9', saleId: '4', medicineId: '10', medicineName: 'Diclofenac Gel', batchId: 'b13', quantity: 2, unitPrice: 55, standardUnitPrice: 55, actualUnitPrice: 55, discountAmount: 0, totalPrice: 110 },
    ]
  },
  {
    id: '5', saleNumber: 'SAL-2026-00005', saleDate: '2026-06-16T11:20:00', totalAmount: 285, totalDiscount: 0, discountReason: '', approvedBy: null, profit: 80, userId: '2', userName: 'Ahmed Hassan',
    items: [
      { id: '10', saleId: '5', medicineId: '11', medicineName: 'Baby Lotion', batchId: 'b14', quantity: 2, unitPrice: 85, standardUnitPrice: 85, actualUnitPrice: 85, discountAmount: 0, totalPrice: 170 },
      { id: '11', saleId: '5', medicineId: '5', medicineName: 'Ibuprofen 400mg', batchId: 'b7', quantity: 5, unitPrice: 12, standardUnitPrice: 12, actualUnitPrice: 12, discountAmount: 0, totalPrice: 60 },
      { id: '12', saleId: '5', medicineId: '1', medicineName: 'Paracetamol 500mg', batchId: 'b1', quantity: 5, unitPrice: 10, standardUnitPrice: 10, actualUnitPrice: 10, discountAmount: 0, totalPrice: 50 },
    ]
  },
];

export const mockNotifications: Notification[] = [
  // Low Stock Alerts
  { id: '1', title: 'Low Stock Alert', message: 'Amoxicillin 250mg (Batch B004) stock is below 10. Current: 8', type: 'low_stock', isRead: false, createdAt: '2026-06-19T08:00:00' },
  { id: '2', title: 'Low Stock Alert', message: 'Ibuprofen 400mg (Batch B008) stock is below 10. Current: 5', type: 'low_stock', isRead: false, createdAt: '2026-06-19T07:30:00' },
  
  // Expiry Notifications - 15 day intervals after 6 months
  { id: '3', title: '⚠️ Expiry Alert (15 days left)', message: 'Ibuprofen 400mg (Batch B008) expires on 2026-07-20. Only 15 days remaining. Immediate action required!', type: 'expiry', isRead: false, createdAt: '2026-07-05T08:00:00' },
  { id: '4', title: '⚠️ Expiry Alert (30 days left)', message: 'Ibuprofen 400mg (Batch B008) expires on 2026-07-20. 30 days remaining. Consider discounting.', type: 'expiry', isRead: false, createdAt: '2026-06-20T08:00:00' },
  { id: '5', title: '⚠️ Expiry Alert (60 days left)', message: 'Ibuprofen 400mg (Batch B008) expires on 2026-07-20. 60 days remaining.', type: 'expiry', isRead: true, createdAt: '2026-05-21T08:00:00' },
  { id: '6', title: '⚠️ Expiry Alert (90 days left)', message: 'Ibuprofen 400mg (Batch B008) expires on 2026-07-20. 90 days remaining.', type: 'expiry', isRead: true, createdAt: '2026-04-21T08:00:00' },
  
  { id: '7', title: '⚠️ Expiry Alert (45 days left)', message: 'Amoxicillin 250mg (Batch B004) expires on 2026-08-15. 45 days remaining.', type: 'expiry', isRead: false, createdAt: '2026-07-01T08:00:00' },
  { id: '8', title: '⚠️ Expiry Alert (75 days left)', message: 'Amoxicillin 250mg (Batch B004) expires on 2026-08-15. 75 days remaining.', type: 'expiry', isRead: true, createdAt: '2026-06-01T08:00:00' },
  { id: '9', title: '⚠️ Expiry Alert (105 days left)', message: 'Amoxicillin 250mg (Batch B004) expires on 2026-08-15. 105 days remaining.', type: 'expiry', isRead: true, createdAt: '2026-05-02T08:00:00' },
  { id: '10', title: '⚠️ Expiry Alert (135 days left)', message: 'Amoxicillin 250mg (Batch B004) expires on 2026-08-15. 135 days remaining.', type: 'expiry', isRead: true, createdAt: '2026-04-02T08:00:00' },
  { id: '11', title: '⚠️ Expiry Alert (180 days - 6 months)', message: 'Amoxicillin 250mg (Batch B004) expires on 2026-08-15. 6 months remaining. Monitoring started.', type: 'expiry', isRead: true, createdAt: '2026-02-16T08:00:00' },
  
  // Info notifications
  { id: '12', title: 'Purchase Completed', message: 'Purchase PUR-2026-00003 from HealthCorp Ltd completed. Total: 4,200 ETB', type: 'info', isRead: true, createdAt: '2026-03-05T09:00:00' },
];

export const mockAuditLogs: AuditLog[] = [
  { id: '1', userId: '2', userName: 'Ahmed Hassan', action: 'Created Sale SAL-2026-00001', tableName: 'Sales', recordId: '1', createdAt: '2026-06-19T08:30:00' },
  { id: '2', userId: '3', userName: 'Sara Mohammed', action: 'Created Sale SAL-2026-00002', tableName: 'Sales', recordId: '2', createdAt: '2026-06-19T10:15:00' },
  { id: '3', userId: '1', userName: 'Admin User', action: 'Added Medicine: Diclofenac Gel', tableName: 'Medicines', recordId: '10', createdAt: '2026-03-10T14:00:00' },
  { id: '4', userId: '1', userName: 'Admin User', action: 'Created Purchase PUR-2026-00003', tableName: 'Purchases', recordId: '3', createdAt: '2026-03-05T09:00:00' },
  { id: '5', userId: '2', userName: 'Ahmed Hassan', action: 'Created Sale SAL-2026-00003', tableName: 'Sales', recordId: '3', createdAt: '2026-06-18T14:00:00' },
  { id: '6', userId: '3', userName: 'Sara Mohammed', action: 'Created Sale SAL-2026-00004', tableName: 'Sales', recordId: '4', createdAt: '2026-06-17T09:45:00' },
  { id: '7', userId: '1', userName: 'Admin User', action: 'Approved User: Sara Mohammed', tableName: 'Users', recordId: '3', createdAt: '2026-02-01T08:00:00' },
  { id: '8', userId: '2', userName: 'Ahmed Hassan', action: 'Logged in', tableName: 'Users', recordId: '2', createdAt: '2026-06-19T08:00:00' },
];

export const dailySalesData = [
  { date: '8-10 AM', sales: 1130, profit: 315 },
  { date: '10-12 PM', sales: 2070, profit: 610 },
  { date: '1-3 PM', sales: 1540, profit: 445 },
  { date: '3-5 PM', sales: 2230, profit: 655 },
  { date: '5-7 PM', sales: 1850, profit: 540 },
  { date: '7-9 PM', sales: 1420, profit: 415 },
  { date: '9-11 PM', sales: 980, profit: 285 },
];

export const weeklySalesData = [
  { date: 'Mon', sales: 1200, profit: 350 },
  { date: 'Tue', sales: 1800, profit: 520 },
  { date: 'Wed', sales: 1400, profit: 400 },
  { date: 'Thu', sales: 2200, profit: 650 },
  { date: 'Fri', sales: 2800, profit: 820 },
  { date: 'Sat', sales: 3200, profit: 950 },
  { date: 'Sun', sales: 1600, profit: 460 },
];

export const monthlySalesData = [
  { date: 'Week 1', sales: 8500, profit: 2400 },
  { date: 'Week 2', sales: 12000, profit: 3500 },
  { date: 'Week 3', sales: 9800, profit: 2800 },
  { date: 'Week 4', sales: 15000, profit: 4300 },
];

export const yearlySalesData = [
  { date: 'Jan', sales: 45000, profit: 12500 },
  { date: 'Feb', sales: 52000, profit: 14800 },
  { date: 'Mar', sales: 48000, profit: 13200 },
  { date: 'Apr', sales: 61000, profit: 17500 },
  { date: 'May', sales: 55000, profit: 15600 },
  { date: 'Jun', sales: 67000, profit: 19200 },
  { date: 'Jul', sales: 72000, profit: 20800 },
  { date: 'Aug', sales: 58000, profit: 16400 },
  { date: 'Sep', sales: 63000, profit: 18000 },
  { date: 'Oct', sales: 70000, profit: 20000 },
  { date: 'Nov', sales: 75000, profit: 21500 },
  { date: 'Dec', sales: 82000, profit: 23600 },
];

export const categoryDistribution = [
  { name: 'Antibiotic', value: 25 },
  { name: 'Pain Killer', value: 20 },
  { name: 'Vitamin', value: 15 },
  { name: 'Respiratory', value: 12 },
  { name: 'Dermatology', value: 10 },
  { name: 'Cosmetics', value: 8 },
  { name: 'Others', value: 10 },
];
