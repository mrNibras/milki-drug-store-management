import { create } from 'zustand';
import { User, Medicine, Supplier, Purchase, Sale, Notification, CartItem, PharmacySettings, AuditLog, Category } from '../types';
import { mockUsers, mockMedicines, mockSuppliers, mockPurchases, mockSales, mockNotifications, mockAuditLogs, mockCategories } from '../data/mockData';
import { getDaysUntilExpiry, generateId } from '../utils/helpers';

interface AppState {
  // Auth
  currentUser: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => boolean;
  logout: () => void;

  // Users
  users: User[];
  addUser: (user: User) => void;
  updateUser: (id: string, updates: Partial<User>) => void;
  deleteUser: (id: string) => void;
  toggleUserActive: (id: string) => void;

  // Medicines
  medicines: Medicine[];
  addMedicine: (medicine: Medicine) => void;
  updateMedicine: (id: string, updates: Partial<Medicine>) => void;
  deleteMedicine: (id: string) => void;

  // Suppliers
  suppliers: Supplier[];
  addSupplier: (supplier: Supplier) => void;
  updateSupplier: (id: string, updates: Partial<Supplier>) => void;
  deleteSupplier: (id: string) => void;

  // Purchases
  purchases: Purchase[];
  addPurchase: (purchase: Purchase) => void;

  // Sales
  sales: Sale[];
  addSale: (sale: Sale) => void;

  // Cart
  cart: CartItem[];
  addToCart: (item: CartItem) => void;
  updateCartItemQuantity: (medicineId: string, batchId: string, quantity: number) => void;
  updateCartItemDiscount: (medicineId: string, batchId: string, discountAmount: number) => void;
  removeFromCart: (medicineId: string, batchId: string) => void;
  clearCart: () => void;
  cartDiscountReason: string;
  setCartDiscountReason: (reason: string) => void;

  // Notifications
  notifications: Notification[];
  markNotificationRead: (id: string) => void;
  markAllNotificationsRead: () => void;
  generateExpiryNotifications: () => void;

  // Categories
  categories: Category[];
  addCategory: (category: Category) => void;

  // Audit Logs
  auditLogs: AuditLog[];
  addAuditLog: (log: AuditLog) => void;

  // Settings
  settings: PharmacySettings;
  updateSettings: (settings: Partial<PharmacySettings>) => void;

  // Sidebar
  sidebarOpen: boolean;
  toggleSidebar: () => void;
}

export const useAppStore = create<AppState>((set) => ({
  // Auth
  currentUser: null,
  isAuthenticated: false,
  login: (email: string, _password: string) => {
    const user = mockUsers.find(u => u.email === email && u.isActive);
    if (user) {
      set({ currentUser: user, isAuthenticated: true });
      return true;
    }
    return false;
  },
  logout: () => set({ currentUser: null, isAuthenticated: false }),

  // Users
  users: mockUsers,
  addUser: (user) => set(state => ({ users: [...state.users, user] })),
  updateUser: (id, updates) => set(state => ({
    users: state.users.map(u => u.id === id ? { ...u, ...updates } : u)
  })),
  deleteUser: (id) => set(state => ({
    users: state.users.filter(u => u.id !== id)
  })),
  toggleUserActive: (id) => set(state => ({
    users: state.users.map(u => u.id === id ? { ...u, isActive: !u.isActive } : u)
  })),

  // Medicines
  medicines: mockMedicines,
  addMedicine: (medicine) => set(state => ({ medicines: [...state.medicines, medicine] })),
  updateMedicine: (id, updates) => set(state => ({
    medicines: state.medicines.map(m => m.id === id ? { ...m, ...updates } : m)
  })),
  deleteMedicine: (id) => set(state => ({
    medicines: state.medicines.filter(m => m.id !== id)
  })),

  // Suppliers
  suppliers: mockSuppliers,
  addSupplier: (supplier) => set(state => ({ suppliers: [...state.suppliers, supplier] })),
  updateSupplier: (id, updates) => set(state => ({
    suppliers: state.suppliers.map(s => s.id === id ? { ...s, ...updates } : s)
  })),
  deleteSupplier: (id) => set(state => ({
    suppliers: state.suppliers.filter(s => s.id !== id)
  })),

  // Purchases
  purchases: mockPurchases,
  addPurchase: (purchase) => set(state => ({ purchases: [...state.purchases, purchase] })),

  // Sales
  sales: mockSales,
  addSale: (sale) => set(state => ({ sales: [sale, ...state.sales] })),

  // Cart
  cart: [],
  addToCart: (item) => set(state => {
    const existing = state.cart.find(c => c.medicineId === item.medicineId && c.batchId === item.batchId);
    if (existing) {
      return {
        cart: state.cart.map(c =>
          c.medicineId === item.medicineId && c.batchId === item.batchId
            ? { ...c, quantity: c.quantity + item.quantity }
            : c
        )
      };
    }
    return { cart: [...state.cart, item] };
  }),
  updateCartItemQuantity: (medicineId, batchId, quantity) => set(state => ({
    cart: state.cart.map(c =>
      c.medicineId === medicineId && c.batchId === batchId
        ? { ...c, quantity: Math.max(1, Math.min(quantity, c.availableQuantity)) }
        : c
    )
  })),
  updateCartItemDiscount: (medicineId, batchId, discountAmount) => set(state => ({
    cart: state.cart.map(c =>
      c.medicineId === medicineId && c.batchId === batchId
        ? { ...c, discountAmount: Math.max(0, Math.min(discountAmount, c.standardPrice)), sellingPrice: c.standardPrice - Math.max(0, Math.min(discountAmount, c.standardPrice)) }
        : c
    )
  })),
  removeFromCart: (medicineId, batchId) => set(state => ({
    cart: state.cart.filter(c => !(c.medicineId === medicineId && c.batchId === batchId))
  })),
  clearCart: () => set({ cart: [], cartDiscountReason: '' }),
  cartDiscountReason: '',
  setCartDiscountReason: (reason) => set({ cartDiscountReason: reason }),

  // Notifications
  notifications: mockNotifications,
  markNotificationRead: (id) => set(state => ({
    notifications: state.notifications.map(n => n.id === id ? { ...n, isRead: true } : n)
  })),
  markAllNotificationsRead: () => set(state => ({
    notifications: state.notifications.map(n => ({ ...n, isRead: true }))
  })),
  generateExpiryNotifications: () => set(state => {
    const newNotifications: Notification[] = [];
    const existingMessages = new Set(state.notifications.map(n => n.message));
    
    state.medicines.forEach(medicine => {
      medicine.batches.forEach(batch => {
        if (batch.quantity <= 0) return;
        
        const daysLeft = getDaysUntilExpiry(batch.expiryDate);
        
        // Only generate notifications for items within 6 months (180 days) and not expired
        if (daysLeft <= 0 || daysLeft > 180) return;
        
        // Check if days left aligns with 15-day intervals
        // Notification at: 180, 165, 150, 135, 120, 105, 90, 75, 60, 45, 30, 15
        const isNotificationDay = daysLeft % 15 === 0 || daysLeft === 180;
        if (!isNotificationDay) return;
        
        const message = daysLeft === 180
          ? `${medicine.name} (Batch ${batch.batchNumber}) expires on ${batch.expiryDate}. 6 months remaining. Expiry monitoring started.`
          : daysLeft <= 15
            ? `${medicine.name} (Batch ${batch.batchNumber}) expires on ${batch.expiryDate}. Only ${daysLeft} days remaining. Immediate action required!`
            : daysLeft <= 30
              ? `${medicine.name} (Batch ${batch.batchNumber}) expires on ${batch.expiryDate}. ${daysLeft} days remaining. Urgent: Consider discounting.`
              : `${medicine.name} (Batch ${batch.batchNumber}) expires on ${batch.expiryDate}. ${daysLeft} days remaining.`;
        
        // Don't create duplicate notifications
        if (existingMessages.has(message)) return;
        
        const urgency = daysLeft <= 15 ? '🔴' : daysLeft <= 30 ? '🟠' : daysLeft <= 90 ? '🟡' : '⚠️';
        
        newNotifications.push({
          id: generateId(),
          title: `${urgency} Expiry Alert (${daysLeft} days left)`,
          message,
          type: 'expiry',
          isRead: false,
          createdAt: new Date().toISOString(),
        });
      });
    });
    
    // Also check for low stock
    state.medicines.forEach(medicine => {
      const totalQty = medicine.batches.reduce((sum, b) => sum + b.quantity, 0);
      const threshold = medicine.lowStockThreshold || 10;
      
      if (totalQty === 0) {
        const message = `${medicine.name} is out of stock!`;
        if (!existingMessages.has(message)) {
          newNotifications.push({
            id: generateId(),
            title: '🔴 Out of Stock',
            message,
            type: 'out_of_stock',
            isRead: false,
            createdAt: new Date().toISOString(),
          });
        }
      } else if (totalQty <= threshold) {
        const message = `${medicine.name} stock is below ${threshold}. Current: ${totalQty}`;
        if (!existingMessages.has(message)) {
          newNotifications.push({
            id: generateId(),
            title: '⚠️ Low Stock Alert',
            message,
            type: 'low_stock',
            isRead: false,
            createdAt: new Date().toISOString(),
          });
        }
      }
    });
    
    if (newNotifications.length === 0) return state;
    
    return {
      notifications: [...newNotifications, ...state.notifications]
    };
  }),

  // Categories
  categories: mockCategories,
  addCategory: (category) => set(state => ({ categories: [...state.categories, category] })),

  // Audit Logs
  auditLogs: mockAuditLogs,
  addAuditLog: (log) => set(state => ({ auditLogs: [log, ...state.auditLogs] })),

  // Settings
  settings: {
    pharmacyName: 'Milki Drug Store',
    lowStockThreshold: 10,
    expiryAlertMonths: 6,
    currency: 'ETB',
    address: 'Addis Ababa, Ethiopia',
    phone: '+251911223344',
    email: 'info@milki.com',
  },
  updateSettings: (updates) => set(state => ({
    settings: { ...state.settings, ...updates }
  })),

  // Sidebar
  sidebarOpen: true,
  toggleSidebar: () => set(state => ({ sidebarOpen: !state.sidebarOpen })),
}));
