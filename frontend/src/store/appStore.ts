import { create } from 'zustand';
import { User, Medicine, Supplier, Purchase, Sale, Notification, CartItem, PharmacySettings, AuditLog, Category } from '../types';
import { api, LoginRequest, LoginResponse, CreateSaleRequest, CreatePurchaseRequest, RecordDamageRequest, RecordExpiredRequest, ChangePasswordRequest, DamageResponse, ExpiredResponse, AuditLogResponse } from '../services/api';
import { getSettings, updateSettings } from '../services/settingsApi';
import { getDaysUntilExpiry, generateId } from '../utils/helpers';

interface AppState {
  currentUser: User | null;
  isAuthenticated: boolean;
  token: string | null;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
  register: (fullName: string, email: string, password: string) => Promise<{ ok: boolean; message?: string }>;

  users: User[];
  fetchUsers: () => Promise<void>;
  addUser: (user: User) => Promise<void>;
  updateUser: (id: string, updates: Partial<User>) => Promise<void>;
  deleteUser: (id: string) => Promise<void>;
  toggleUserActive: (id: string) => Promise<void>;

  medicines: Medicine[];
  fetchMedicines: () => Promise<void>;
  addMedicine: (medicine: Medicine) => Promise<void>;
  updateMedicine: (id: string, updates: Partial<Medicine>) => Promise<void>;
  deleteMedicine: (id: string) => Promise<void>;

  suppliers: Supplier[];
  fetchSuppliers: () => Promise<void>;
  addSupplier: (supplier: Supplier) => Promise<void>;
  updateSupplier: (id: string, updates: Partial<Supplier>) => Promise<void>;
  deleteSupplier: (id: string) => Promise<void>;

  purchases: Purchase[];
  fetchPurchases: () => Promise<void>;
  addPurchase: (purchase: Purchase) => Promise<void>;

  sales: Sale[];
  fetchSales: () => Promise<void>;
  addSale: (sale: Sale) => Promise<void>;

  cart: CartItem[];
  addToCart: (item: CartItem) => void;
  updateCartItemQuantity: (medicineId: string, batchId: string, quantity: number) => void;
  updateCartItemDiscount: (medicineId: string, batchId: string, discountAmount: number) => void;
  removeFromCart: (medicineId: string, batchId: string) => void;
  clearCart: () => void;
  cartDiscountReason: string;
  setCartDiscountReason: (reason: string) => void;

  notifications: Notification[];
  fetchNotifications: () => Promise<void>;
  markNotificationRead: (id: string) => Promise<void>;
  markAllNotificationsRead: () => Promise<void>;
  generateExpiryNotifications: () => void;

  categories: Category[];
  fetchCategories: () => Promise<void>;
  addCategory: (category: Category) => Promise<void>;

  auditLogs: AuditLog[];
  fetchAuditLogs: () => Promise<void>;
  addAuditLog: (log: AuditLog) => void;

  damages: DamageResponse[];
  fetchDamages: () => Promise<void>;
  recordDamage: (data: RecordDamageRequest) => Promise<void>;

  expiredRecords: ExpiredResponse[];
  fetchExpired: () => Promise<void>;
  recordExpired: (data: RecordExpiredRequest) => Promise<void>;

  settings: PharmacySettings;
  fetchSettings: () => Promise<void>;
  updateSettings: (settings: Partial<PharmacySettings>) => void;
  changePassword: (currentPassword: string, newPassword: string) => Promise<{ ok: boolean; message?: string }>;

  loading: boolean;
  error: string | null;

  sidebarOpen: boolean;
  toggleSidebar: () => void;
}

const toUser = (r: { userId: number; fullName: string; email: string; roleName: string; isActive: boolean; createdAt: string }): User => ({
  id: String(r.userId),
  fullName: r.fullName,
  email: r.email,
  role: r.roleName.toLowerCase() as 'admin' | 'pharmacist',
  isActive: r.isActive,
  createdAt: r.createdAt,
});

const toCategory = (r: { categoryId: number; name: string }): Category => ({
  id: String(r.categoryId),
  name: r.name,
});

const toMedicine = (r: MedicineResponse): Medicine => ({
  id: String(r.medicineId),
  name: r.medicineName,
  genericName: r.genericName,
  categoryId: String(r.categoryId),
  categoryName: r.categoryName,
  unitType: r.unitType,
  lowStockThreshold: r.lowStockThreshold,
  createdAt: r.createdAt,
  batches: r.batches.map(b => ({
    id: String(b.batchId),
    medicineId: String(r.medicineId),
    batchNumber: b.batchNumber,
    purchasePrice: b.purchasePrice,
    sellingPrice: b.sellingPrice,
    quantity: b.balance,
    expiryDate: b.expiryDate,
    createdAt: r.createdAt,
  })),
});

const toSupplier = (r: SupplierResponse): Supplier => ({
  id: String(r.supplierId),
  name: r.supplierName,
  phone: r.phone,
  email: r.email,
  address: r.address,
  contactPerson: '',
  isActive: true,
  createdAt: r.createdAt,
});

const toPurchase = (r: PurchaseResponse): Purchase => ({
  id: String(r.purchaseId),
  purchaseNumber: r.purchaseNumber,
  supplierId: String(r.supplierId),
  supplierName: r.supplierName,
  purchaseDate: r.purchaseDate,
  totalAmount: r.totalAmount,
  paymentStatus: (r as any).paymentStatus || 'unpaid',
  paymentMethod: 'cash',
  amountPaid: 0,
  remainingDebt: r.totalAmount,
  items: r.items.map(i => ({
    id: String(i.purchaseItemId),
    purchaseId: String(r.purchaseId),
    medicineId: String(i.medicineId),
    medicineName: i.medicineName,
    batchNumber: i.batchNumber,
    quantity: i.quantity,
    purchasePrice: i.purchasePrice,
    expiryDate: '',
  })),
});

const toSale = (r: SaleResponse): Sale => ({
  id: String(r.saleId),
  saleNumber: r.saleNumber,
  saleDate: r.saleDate,
  totalAmount: r.totalAmount,
  totalDiscount: 0,
  discountReason: '',
  approvedBy: null,
  profit: r.totalProfit,
  userId: String(r.userId),
  userName: r.userName,
  items: r.items.map(i => ({
    id: String(i.saleItemId),
    saleId: String(r.saleId),
    medicineId: String(i.medicineId),
    medicineName: i.medicineName,
    batchId: i.batchId ? String(i.batchId) : '',
    quantity: i.quantity,
    unitPrice: i.unitPrice,
    standardUnitPrice: i.unitPrice,
    actualUnitPrice: i.unitPrice,
    discountAmount: 0,
    totalPrice: i.subTotal,
  })),
});

const toNotification = (r: NotificationResponse): Notification => ({
  id: String(r.notificationId),
  title: r.title,
  message: r.message,
  type: r.notificationType.toLowerCase() as any,
  isRead: r.isRead,
  createdAt: r.createdAt,
});

export const useAppStore = create<AppState>((set, get) => ({
  currentUser: null,
  isAuthenticated: false,
  token: null,
  loading: false,
  error: null,

  login: async (email: string, password: string) => {
    set({ loading: true, error: null });
    try {
      const res = await api.post<LoginResponse>('/auth/login', { email, password });
      const token = res.data.token;
      const user: User = {
        id: String(res.data.userId),
        fullName: res.data.fullName,
        email,
        role: res.data.role.toLowerCase() as 'admin' | 'pharmacist',
        isActive: true,
        createdAt: new Date().toISOString(),
      };
      localStorage.setItem('auth_token', token);
      localStorage.setItem('current_user', JSON.stringify(user));
      set({ token, currentUser: user, isAuthenticated: true, loading: false });
      return true;
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Login failed', loading: false });
      return false;
    }
  },

  logout: () => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('current_user');
    set({ currentUser: null, isAuthenticated: false, token: null });
  },

  register: async (fullName: string, email: string, password: string) => {
    set({ loading: true, error: null });
    try {
      await api.post('/auth/register', { fullName, email, password });
      set({ loading: false });
      return { ok: true };
    } catch (e: any) {
      const msg = e.response?.data?.message || 'Registration failed';
      set({ error: msg, loading: false });
      return { ok: false, message: msg };
    }
  },

  fetchUsers: async () => {
    set({ loading: true, error: null });
    try {
      const res = await api.get<UserResponse[]>('/users');
      set({ users: res.data.map(toUser), loading: false });
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch users', loading: false });
    }
  },

  addUser: async (user) => {
    set({ loading: true, error: null });
    try {
      await api.post('/users', { fullName: user.fullName, email: user.email, roleId: user.role === 'admin' ? 1 : 2 });
      await get().fetchUsers();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add user', loading: false });
      throw e;
    }
  },

  updateUser: async (id, updates) => {
    try {
      await api.put(`/users/${id}`, updates);
      await get().fetchUsers();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to update user' });
      throw e;
    }
  },

  deleteUser: async (id) => {
    try {
      await api.delete(`/users/${id}`);
      set(state => ({ users: state.users.filter(u => u.id !== id) }));
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to delete user' });
      throw e;
    }
  },

  toggleUserActive: async (id) => {
    try {
      const user = get().users.find(u => u.id === id);
      if (!user) return;
      await api.put(`/users/${id}`, { ...user, isActive: !user.isActive });
      set(state => ({ users: state.users.map(u => u.id === id ? { ...u, isActive: !u.isActive } : u) }));
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to toggle user' });
    }
  },

  fetchMedicines: async () => {
    set({ loading: true, error: null });
    try {
      const res = await api.get<MedicineResponse[]>('/medicines');
      set({ medicines: res.data.map(toMedicine), loading: false });
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch medicines', loading: false });
    }
  },

  addMedicine: async (medicine) => {
    set({ loading: true, error: null });
    try {
      await api.post('/medicines', {
        medicineName: medicine.name,
        genericName: medicine.genericName,
        categoryId: Number(medicine.categoryId),
        unitType: medicine.unitType,
        lowStockThreshold: medicine.lowStockThreshold,
      });
      await get().fetchMedicines();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add medicine', loading: false });
      throw e;
    }
  },

  updateMedicine: async (id, updates) => {
    try {
      await api.put(`/medicines/${id}`, {
        medicineName: updates.name,
        genericName: updates.genericName,
        categoryId: Number(updates.categoryId),
        unitType: updates.unitType,
        lowStockThreshold: updates.lowStockThreshold,
        isActive: updates.isActive,
      });
      await get().fetchMedicines();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to update medicine' });
      throw e;
    }
  },

  deleteMedicine: async (id) => {
    try {
      await api.delete(`/medicines/${id}`);
      set(state => ({ medicines: state.medicines.filter(m => m.id !== id) }));
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to delete medicine' });
      throw e;
    }
  },

  fetchSuppliers: async () => {
    set({ loading: true, error: null });
    try {
      const res = await api.get<SupplierResponse[]>('/suppliers');
      set({ suppliers: res.data.map(toSupplier), loading: false });
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch suppliers', loading: false });
    }
  },

  addSupplier: async (supplier) => {
    set({ loading: true, error: null });
    try {
      await api.post('/suppliers', {
        supplierName: supplier.name,
        phone: supplier.phone,
        email: supplier.email,
        address: supplier.address,
        paymentStatus: 'Outstanding',
      });
      await get().fetchSuppliers();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add supplier', loading: false });
      throw e;
    }
  },

  updateSupplier: async (id, updates) => {
    try {
      await api.put(`/suppliers/${id}`, updates);
      await get().fetchSuppliers();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to update supplier' });
      throw e;
    }
  },

  deleteSupplier: async (id) => {
    try {
      await api.delete(`/suppliers/${id}`);
      set(state => ({ suppliers: state.suppliers.filter(s => s.id !== id) }));
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to delete supplier' });
      throw e;
    }
  },

  fetchPurchases: async () => {
    set({ loading: true, error: null });
    try {
      const res = await api.get<PurchaseResponse[]>('/purchases');
      set({ purchases: res.data.map(toPurchase), loading: false });
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch purchases', loading: false });
    }
  },

  addPurchase: async (purchase) => {
    set({ loading: true, error: null });
    try {
      const items = purchase.items.map(i => ({
        medicineId: Number(i.medicineId),
        batchNumber: i.batchNumber,
        quantity: i.quantity,
        purchasePrice: i.purchasePrice,
        sellingPrice: i.purchasePrice * 2,
        expiryDate: i.expiryDate || new Date().toISOString(),
      }));
      const res = await api.post<PurchaseResponse>('/purchases', {
        supplierId: Number(purchase.supplierId),
        purchaseDate: purchase.purchaseDate,
        items,
      });
      await get().fetchPurchases();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add purchase', loading: false });
      throw e;
    }
  },

  fetchSales: async () => {
    set({ loading: true, error: null });
    try {
      const res = await api.get<SaleResponse[]>('/sales');
      set({ sales: res.data.map(toSale), loading: false });
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch sales', loading: false });
    }
  },

  addSale: async (sale) => {
    set({ loading: true, error: null });
    try {
      const items = sale.items.map(i => ({
        medicineId: Number(i.medicineId),
        quantity: i.quantity,
      }));
      const res = await api.post<SaleResponse>('/sales', { items });
      await get().fetchSales();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add sale', loading: false });
      throw e;
    }
  },

  cart: [],
  addToCart: (item) => set((state) => {
    const existing = state.cart.find(c => c.medicineId === item.medicineId && c.batchId === item.batchId);
    if (existing) {
      return {
        cart: state.cart.map(c =>
          c.medicineId === item.medicineId && c.batchId === item.batchId
            ? { ...c, quantity: c.quantity + item.quantity }
            : c
        ),
      };
    }
    return { cart: [...state.cart, item] };
  }),
  updateCartItemQuantity: (medicineId, batchId, quantity) => set((state) => ({
    cart: state.cart.map(c =>
      c.medicineId === medicineId && c.batchId === batchId
        ? { ...c, quantity: Math.max(1, Math.min(quantity, c.availableQuantity)) }
        : c
    ),
  })),
  updateCartItemDiscount: (medicineId, batchId, discountAmount) => set((state) => ({
    cart: state.cart.map(c =>
      c.medicineId === medicineId && c.batchId === batchId
        ? { ...c, discountAmount: Math.max(0, Math.min(discountAmount, c.standardPrice)), sellingPrice: c.standardPrice - Math.max(0, Math.min(discountAmount, c.standardPrice)) }
        : c
    ),
  })),
  removeFromCart: (medicineId, batchId) => set((state) => ({
    cart: state.cart.filter(c => !(c.medicineId === medicineId && c.batchId === batchId)),
  })),
  clearCart: () => set({ cart: [], cartDiscountReason: '' }),
  cartDiscountReason: '',
  setCartDiscountReason: (reason) => set({ cartDiscountReason: reason }),

  notifications: [],
  fetchNotifications: async () => {
    try {
      const res = await api.get<NotificationResponse[]>('/notifications');
      set({ notifications: res.data.map(toNotification) });
    } catch (e) {
      console.error('Failed to fetch notifications', e);
    }
  },
  markNotificationRead: async (id) => {
    try {
      await api.put(`/notifications/${id}/read`);
      set((state) => ({
        notifications: state.notifications.map(n => n.id === id ? { ...n, isRead: true } : n),
      }));
    } catch (e) {
      console.error('Failed to mark notification read', e);
    }
  },
  markAllNotificationsRead: async () => {
    try {
      await api.put('/notifications/read-all');
      set((state) => ({
        notifications: state.notifications.map(n => ({ ...n, isRead: true })),
      }));
    } catch (e) {
      console.error('Failed to mark all notifications read', e);
    }
  },
  generateExpiryNotifications: () => {
    const state = get();
    if (state.notifications.length > 0 || state.medicines.length === 0) return;
    state.generateExpiryNotifications();
  },

  categories: [],
  fetchCategories: async () => {
    try {
      const res = await api.get<Category[]>('/categories');
      set({ categories: res.data.map(toCategory) });
    } catch (e) {
      console.error('Failed to fetch categories', e);
    }
  },
  addCategory: async (category) => {
    try {
      await api.post('/categories', { name: category.name });
      await get().fetchCategories();
    } catch (e) {
      console.error('Failed to add category', e);
      throw e;
    }
  },

  auditLogs: [],
  fetchAuditLogs: async () => {
    try {
      const res = await api.get<AuditLogResponse[]>('/audit-logs');
      set({
        auditLogs: res.data.map(l => ({
          id: String(l.auditId),
          userId: String(l.userId),
          userName: l.userName,
          action: l.action,
          tableName: l.tableName,
          recordId: String(l.recordId ?? 0),
          createdAt: l.createdAt,
        })),
      });
    } catch (e) {
      console.error('Failed to fetch audit logs', e);
    }
  },
  addAuditLog: (log) => set((state) => ({ auditLogs: [log, ...state.auditLogs] })),

  damages: [],
  fetchDamages: async () => {
    try {
      const res = await api.get<DamageResponse[]>('/damages');
      set({ damages: res.data });
    } catch (e) {
      console.error('Failed to fetch damages', e);
    }
  },
  recordDamage: async (data) => {
    try {
      await api.post('/damages', data);
      await get().fetchDamages();
      await get().fetchMedicines();
    } catch (e) {
      console.error('Failed to record damage', e);
      throw e;
    }
  },

  expiredRecords: [],
  fetchExpired: async () => {
    try {
      const res = await api.get<ExpiredResponse[]>('/expired');
      set({ expiredRecords: res.data });
    } catch (e) {
      console.error('Failed to fetch expired records', e);
    }
  },
  recordExpired: async (data) => {
    try {
      await api.post('/expired', data);
      await get().fetchExpired();
      await get().fetchMedicines();
    } catch (e) {
      console.error('Failed to record expired', e);
      throw e;
    }
  },

  settings: {
    pharmacyName: 'Milki Drug Store',
    lowStockThreshold: 10,
    expiryAlertMonths: 6,
    currency: 'ETB',
    address: 'Addis Ababa, Ethiopia',
    phone: '+251911223344',
    email: 'info@milki.com',
  },
  fetchSettings: async () => {
    try {
      const s = await getSettings();
      set({
        settings: {
          pharmacyName: s.pharmacyName,
          lowStockThreshold: s.lowStockThreshold,
          expiryAlertMonths: s.expiryAlertMonths,
          currency: s.currency,
          address: s.address,
          phone: s.phone,
          email: s.email,
        },
      });
    } catch (e) {
      console.error('Failed to fetch settings', e);
    }
  },
  updateSettings: async (updates) => {
    try {
      await updateSettings(updates);
      await get().fetchSettings();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to update settings' });
      throw e;
    }
  },
  changePassword: async (currentPassword, newPassword) => {
    try {
      await api.post('/auth/change-password', { currentPassword, newPassword } as ChangePasswordRequest);
      return { ok: true };
    } catch (e: any) {
      const msg = e.response?.data?.message || 'Failed to change password';
      return { ok: false, message: msg };
    }
  },

  sidebarOpen: true,
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
}));
