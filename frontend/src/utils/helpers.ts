import { format, parseISO, differenceInDays, differenceInMonths } from 'date-fns';

export const formatDate = (date: string): string => {
  try {
    return format(parseISO(date), 'MMM dd, yyyy');
  } catch {
    return date;
  }
};

export const formatDateTime = (date: string): string => {
  try {
    return format(parseISO(date), 'MMM dd, yyyy HH:mm');
  } catch {
    return date;
  }
};

export const formatCurrency = (amount: number): string => {
  return `${amount.toLocaleString()} ETB`;
};

export const generateId = (): string => {
  return Math.random().toString(36).substr(2, 9);
};

export const generateSaleNumber = (count: number): string => {
  const year = new Date().getFullYear();
  const num = String(count + 1).padStart(5, '0');
  return `SAL-${year}-${num}`;
};

export const generatePurchaseNumber = (count: number): string => {
  const year = new Date().getFullYear();
  const num = String(count + 1).padStart(5, '0');
  return `PUR-${year}-${num}`;
};

export const getDaysUntilExpiry = (expiryDate: string): number => {
  try {
    return differenceInDays(parseISO(expiryDate), new Date());
  } catch {
    return 0;
  }
};

export const getMonthsUntilExpiry = (expiryDate: string): number => {
  try {
    return differenceInMonths(parseISO(expiryDate), new Date());
  } catch {
    return 0;
  }
};

export const getExpiryStatus = (expiryDate: string): 'expired' | 'critical' | 'warning' | 'ok' => {
  const days = getDaysUntilExpiry(expiryDate);
  if (days < 0) return 'expired';
  if (days <= 15) return 'critical';
  if (days < 180) return 'warning'; // 6 months = 180 days
  return 'ok';
};

export const getExpiryColor = (status: string): string => {
  switch (status) {
    case 'expired': return 'text-red-700 bg-red-100 border-red-200';
    case 'critical': return 'text-red-600 bg-red-50 border-red-200';
    case 'warning': return 'text-amber-600 bg-amber-50 border-amber-200';
    case 'ok': return 'text-green-600 bg-green-50 border-green-200';
    default: return 'text-gray-600 bg-gray-50 border-gray-200';
  }
};

// Check if medicine needs expiry notification (every 15 days after 6 months)
export const shouldSendExpiryNotification = (expiryDate: string): boolean => {
  const days = getDaysUntilExpiry(expiryDate);
  // Must be within 6 months (180 days) and not expired
  if (days <= 0 || days >= 180) return false;
  
  // Check if current days left aligns with 15-day intervals from 180
  // Notification days: 180, 165, 150, 135, 120, 105, 90, 75, 60, 45, 30, 15
  return days % 15 === 0 || days === 180;
};

// Get next notification date for expiry
export const getNextExpiryNotificationDays = (expiryDate: string): number => {
  const days = getDaysUntilExpiry(expiryDate);
  if (days <= 0 || days >= 180) return -1;
  
  // Find the next 15-day interval
  const intervals = [180, 165, 150, 135, 120, 105, 90, 75, 60, 45, 30, 15];
  const nextInterval = intervals.find(interval => interval <= days);
  return nextInterval || -1;
};

export const getStockStatus = (quantity: number, threshold: number): 'out_of_stock' | 'low' | 'ok' => {
  if (quantity === 0) return 'out_of_stock';
  if (quantity <= threshold) return 'low';
  return 'ok';
};

export const getStockColor = (status: string): string => {
  switch (status) {
    case 'out_of_stock': return 'text-red-700 bg-red-100';
    case 'low': return 'text-amber-700 bg-amber-100';
    case 'ok': return 'text-green-700 bg-green-100';
    default: return 'text-gray-700 bg-gray-100';
  }
};

// FEFO Algorithm: Sort batches by expiry date (earliest first)
export const fefoSort = <T extends { expiryDate: string }>(batches: T[]): T[] => {
  return [...batches].sort((a, b) => {
    return new Date(a.expiryDate).getTime() - new Date(b.expiryDate).getTime();
  });
};

// FEFO: Get available batches for a medicine, sorted by expiry
export const getFefoBatches = (medicine: { batches: { expiryDate: string; quantity: number; id: string; batchNumber: string; sellingPrice: number; purchasePrice: number; medicineId: string; createdAt: string }[] }) => {
  return fefoSort(medicine.batches.filter(b => b.quantity > 0));
};

export const getPaymentStatusColor = (status: string): string => {
  switch (status) {
    case 'paid': return 'text-green-700 bg-green-100';
    case 'pending': return 'text-red-700 bg-red-100';
    case 'partial': return 'text-amber-700 bg-amber-100';
    default: return 'text-gray-700 bg-gray-100';
  }
};
