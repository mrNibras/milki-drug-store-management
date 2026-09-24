import { create } from 'zustand';
import { User, Medicine, Supplier, Purchase, Sale, Notification, CartItem, PharmacySettings, AuditLog, Category, UnitType, Branch, CatalogOption, Cosmetic } from '../types';
import { api, LoginRequest, LoginResponse, CreateSaleRequest, CreatePurchaseRequest, RecordDamageRequest, RecordExpiredRequest, ChangePasswordRequest, DamageResponse, ExpiredResponse, AuditLogResponse, BranchResponse, CreateBranchRequest, UpdateBranchRequest, CatalogOptionDto, CosmeticResponse } from '../services/api';
import { getSettings, updateSettings } from '../services/settingsApi';
import { getDaysUntilExpiry, generateId } from '../utils/helpers';

interface AppState {
  currentUser: User | null;
  currentBranch: Branch | null;
  branches: Branch[];
  isAuthenticated: boolean;
  token: string | null;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
  refreshToken: () => Promise<boolean>;
  register: (fullName: string, email: string, password: string) => Promise<{ ok: boolean; message?: string }>;
  switchBranch: (branchId: number) => Promise<void>;
  fetchBranches: () => Promise<void>;
  addBranch: (branch: CreateBranchRequest) => Promise<void>;
  updateBranch: (id: string, updates: Partial<Branch>) => Promise<void>;
  deleteBranch: (id: string) => Promise<void>;

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

  cosmetics: Cosmetic[];
  fetchCosmetics: () => Promise<void>;

  suppliers: Supplier[];
  fetchSuppliers: () => Promise<void>;
  addSupplier: (supplier: Supplier) => Promise<void>;
  updateSupplier: (id: string, updates: Partial<Supplier>) => Promise<void>;
  deleteSupplier: (id: string) => Promise<void>;

  purchases: Purchase[];
  fetchPurchases: () => Promise<void>;
  addPurchase: (purchase: CreatePurchaseRequest) => Promise<Purchase>;

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

  categories: CatalogOption[];
  fetchCategories: () => Promise<void>;

  unitTypes: CatalogOption[];
  fetchUnitTypes: () => Promise<void>;

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

  loading: Record<string, boolean>;
  setLoading: (key: string, value: boolean) => void;
  error: string | null;

  sidebarOpen: boolean;
  toggleSidebar: () => void;
}

const toUser = (r: { userId: number; fullName: string; email: string; roleName: string; isActive: boolean; createdAt: string }): User => ({
  id: String(r.userId),
  fullName: r.fullName,
  email: r.email,
  role: (r.roleName?.toLowerCase() || 'pharmacist') as 'admin' | 'pharmacist',
  isActive: r.isActive,
  createdAt: r.createdAt,
});

const toMedicine = (r: MedicineResponse): Medicine => ({
  id: String(r.productId),
  name: r.brandName,
  genericName: r.genericName,
  categoryId: String(r.categoryId),
  categoryName: r.categoryName,
  unitType: r.unitTypeName,
  unitTypeId: r.unitTypeId,
  lowStockThreshold: r.reorderLevel,
  createdAt: r.createdDate,
  batches: r.batches.map(b => ({
    id: String(b.batchId),
    medicineId: String(b.productId),
    batchNumber: b.batchNumber,
    purchasePrice: b.purchasePrice,
    sellingPrice: b.sellingPrice,
    quantity: b.remainingQuantity,
    expiryDate: b.expiryDate,
    createdAt: b.dateReceived,
  })),
});

const toCosmetic = (r: CosmeticResponse): Cosmetic => ({
  cosmeticId: r.cosmeticId,
  productName: r.productName,
  description: r.description,
  categoryId: r.categoryId,
  categoryName: r.categoryName,
  unitTypeId: r.unitTypeId,
  unitTypeName: r.unitTypeName,
  price: r.price,
  isActive: r.isActive,
  createdAt: r.createdAt,
  updatedAt: r.updatedAt,
  branchId: r.branchId,
  supplierId: r.supplierId,
  supplierName: r.supplierName,
  batches: r.batches.map(b => ({
    batchId: b.batchId,
    cosmeticId: b.cosmeticId,
    batchNumber: b.batchNumber,
    quantityReceived: b.quantityReceived,
    quantityIssued: b.quantityIssued,
    quantityDamaged: b.quantityDamaged,
    quantityExpired: b.quantityExpired,
    balance: b.balance,
    expiryDate: b.expiryDate,
    dateReceived: b.dateReceived,
    buyingPrice: b.buyingPrice,
    sellingPrice: b.sellingPrice,
    lowStockThreshold: b.lowStockThreshold,
    branchId: b.branchId,
    supplierId: b.supplierId,
    remarks: b.remarks,
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
  paymentMethod: ((r as any).paymentMethod as any) || 'cash',
  amountPaid: (r as any).amountPaid || 0,
  remainingDebt: (r as any).amountDue || 0,
  items: r.items.map(i => ({
    id: String(i.purchaseItemId),
    purchaseId: String(r.purchaseId),
    medicineId: String(i.productId),
    brandName: i.brandName,
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
  totalDiscount: r.totalDiscount ?? 0,
  discountReason: r.discountReason ?? '',
  approvedBy: null,
  profit: r.totalProfit,
  userId: String(r.userId),
  userName: r.userName,
  paymentMethod: r.paymentMethod,
  paymentStatus: r.paymentStatus,
  amountPaid: r.amountPaid,
  amountDue: r.amountDue,
  referenceNumber: r.referenceNumber,
  items: r.items.map(i => ({
    id: String(i.saleItemId),
    saleId: String(r.saleId),
    medicineId: String(i.medicineId),
    brandName: i.brandName,
    batchId: i.batchId ? String(i.batchId) : '',
    quantity: i.quantity,
    unitPrice: i.unitPrice - (i.discountAmount ?? 0),
    standardUnitPrice: i.unitPrice,
    actualUnitPrice: i.unitPrice - (i.discountAmount ?? 0),
    discountAmount: i.discountAmount ?? 0,
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
  currentBranch: null,
  branches: [],
  users: [],
  medicines: [],
  cosmetics: [],
  suppliers: [],
  purchases: [],
  sales: [],
  categories: [],
  unitTypes: [],
  isAuthenticated: false,
  token: null,
  loading: {},
  error: null,
  setLoading: (key: string, value: boolean) => {
    set(state => ({ loading: { ...state.loading, [key]: value } }));
  },

  login: async (email: string, password: string) => {
    get().setLoading('auth', true); set({ error: null });
    try {
      const normalizedEmail = email.trim().toLowerCase();
      console.log('[login] attempting login for', normalizedEmail, 'to', api.defaults.baseURL);
      const res = await api.post<LoginResponse>('/auth/login', { email: normalizedEmail, password });
      console.log('[login] login response', res.status, res.data);
      const token = res.data.token;
      const user: User = {
        id: String(res.data.userId),
        fullName: res.data.fullName,
        email: normalizedEmail,
        role: res.data.role.toLowerCase() as 'admin' | 'pharmacist',
        branchId: res.data.branchId,
        branchName: res.data.branchName,
        isActive: true,
        createdAt: new Date().toISOString(),
      };
      const branch: Branch = {
        id: String(res.data.branchId),
        name: res.data.branchName,
        isActive: true,
      };
      localStorage.setItem('auth_token', token);
      if (res.data.refreshToken) localStorage.setItem('refresh_token', res.data.refreshToken);
      localStorage.setItem('current_user', JSON.stringify(user));
      localStorage.setItem('current_branch', JSON.stringify(branch));
      set({ token, currentUser: user, currentBranch: branch, isAuthenticated: true });
      get().setLoading('auth', false);
      return true;
    } catch (e: any) {
      const status = e?.response?.status;
      const message = e?.response?.data?.message;
      console.warn('[login] login failed', { status, message, url: e?.config?.url, baseURL: api.defaults.baseURL });
      get().setLoading('auth', false);
      if (status === 401) {
        set({ error: message || 'Invalid email or password.' });
      } else if (status === 403) {
        set({ error: 'Your account does not have a valid role. Contact the administrator.' });
      } else if (status >= 500) {
        set({ error: 'Unable to log in due to a server error. Please try again later.' });
      } else {
        set({ error: message || 'Login failed. Please try again.' });
      }
      return false;
    }
  },

  logout: () => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('current_user');
    localStorage.removeItem('current_branch');
    set({ currentUser: null, currentBranch: null, branches: [], isAuthenticated: false, token: null });
  },

  switchBranch: async (branchId: number) => {
    const branch = get().branches.find(b => Number(b.id) === branchId) || null;
    if (branch) {
      localStorage.setItem('current_branch', JSON.stringify(branch));
      set({ currentBranch: branch });
    }
  },

   fetchBranches: async () => {
    get().setLoading('branches', true); set({ error: null });
    try {
      const res = await api.get<BranchResponse[]>('/branches');
      set({ branches: (Array.isArray(res.data) ? res.data : []).map(b => ({
        id: String(b.branchId),
        name: b.branchName,
        location: b.location,
        phone: b.phone,
        email: b.email,
        address: b.address,
        isActive: b.isActive,
      })) });
      get().setLoading('branches', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch branches' });
      get().setLoading('branches', false);
    }
  },

  addBranch: async (branch) => {
    get().setLoading('branches', true); set({ error: null });
    try {
      await api.post('/branches', branch);
      await get().fetchBranches();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add branch' });
      get().setLoading('branches', false);
      throw e;
    }
  },

  updateBranch: async (id, updates) => {
    try {
      await api.put(`/branches/${id}`, updates);
      await get().fetchBranches();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to update branch' });
      throw e;
    }
  },

  deleteBranch: async (id) => {
    try {
      await api.delete(`/branches/${id}`);
      set(state => ({ branches: state.branches.filter(b => b.id !== id) }));
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to delete branch' });
      throw e;
    }
  },

  refreshToken: async () => {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) return false;
    try {
      const res = await api.post<LoginResponse>('/auth/refresh', { refreshToken });
      const newToken = res.data.token;
      const newRefresh = res.data.refreshToken;
      localStorage.setItem('auth_token', newToken);
      if (newRefresh) localStorage.setItem('refresh_token', newRefresh);
      const user: User = {
        id: String(res.data.userId),
        fullName: res.data.fullName,
        email: res.data.email,
        role: (res.data.role?.toLowerCase() || 'pharmacist') as 'admin' | 'pharmacist',
        branchId: res.data.branchId,
        branchName: res.data.branchName,
        isActive: true,
        createdAt: new Date().toISOString(),
      };
      const branch: Branch = {
        id: String(res.data.branchId),
        name: res.data.branchName,
        isActive: true,
      };
      localStorage.setItem('current_user', JSON.stringify(user));
      localStorage.setItem('current_branch', JSON.stringify(branch));
      set({ token: newToken, currentUser: user, currentBranch: branch });
      return true;
    } catch (e) {
      logout();
      return false;
    }
  },

  register: async (fullName: string, email: string, password: string) => {
    get().setLoading('auth', true); set({ error: null });
    try {
      await api.post('/auth/register', { fullName: fullName.trim(), email: email.trim().toLowerCase(), password });
      get().setLoading('auth', false);
      return { ok: true };
    } catch (e: any) {
      const msg = e.response?.data?.message || 'Registration failed';
      set({ error: msg });
      get().setLoading('auth', false);
      return { ok: false, message: msg };
    }
  },

  fetchUsers: async () => {
    get().setLoading('users', true); set({ error: null });
    try {
      const res = await api.get<UserResponse[]>('/users');
      set({ users: (Array.isArray(res.data) ? res.data : []).map(toUser) });
      get().setLoading('users', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch users' });
      get().setLoading('users', false);
    }
  },

  addUser: async (user) => {
    get().setLoading('users', true); set({ error: null });
    try {
      await api.post('/users', {
        fullName: user.fullName.trim(),
        email: user.email.trim().toLowerCase(),
        password: user.password,
        roleId: user.role === 'admin' ? 1 : 2,
      });
      await get().fetchUsers();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add user' });
      get().setLoading('users', false);
      throw e;
    }
  },

  updateUser: async (id, updates) => {
    try {
      const existing = get().users.find(u => u.id === id);
      const merged = { ...existing, ...updates } as User;
      await api.put(`/users/${id}`, {
        fullName: (merged.fullName || '').trim(),
        email: (merged.email || '').trim().toLowerCase(),
        roleId: merged.role === 'admin' ? 1 : 2,
        isActive: merged.isActive,
      });
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
      const newActive = !user.isActive;
      await api.put(`/users/${id}`, {
        fullName: (user.fullName || '').trim(),
        email: (user.email || '').trim().toLowerCase(),
        roleId: user.role === 'admin' ? 1 : 2,
        isActive: newActive,
      });
      set(state => ({ users: state.users.map(u => u.id === id ? { ...u, isActive: newActive } : u) }));
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to toggle user' });
      throw e;
    }
  },

  fetchCategories: async () => {
    try {
      const res = await api.get<CatalogOptionDto[]>('/lookups/categories');
      set({ categories: (Array.isArray(res.data) ? res.data : []).map(c => ({
        id: String(c.id),
        name: c.name,
        isSystem: c.isSystem,
      })) });
    } catch (e) {
      console.error('Failed to fetch categories', e);
    }
  },

  fetchUnitTypes: async () => {
    get().setLoading('unitTypes', true); set({ error: null });
    try {
      const res = await api.get<CatalogOptionDto[]>('/lookups/unit-types');
      set({ unitTypes: (Array.isArray(res.data) ? res.data : []).map(c => ({
        id: String(c.id),
        name: c.name,
        isSystem: c.isSystem,
      })) });
      get().setLoading('unitTypes', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch unit types' });
      get().setLoading('unitTypes', false);
    }
  },

  addMedicine: async (medicine) => {
    get().setLoading('medicines', true); set({ error: null });
    try {
      await api.post('/medicines', {
        brandName: medicine.name,
        genericName: medicine.genericName,
        categoryId: medicine.categoryId,
        newCategoryName: medicine.newCategoryName,
        unitTypeId: medicine.unitTypeId,
        newUnitTypeName: medicine.newUnitTypeName,
        reorderLevel: medicine.lowStockThreshold,
      });
      await get().fetchMedicines();
      await get().fetchCategories();
      await get().fetchUnitTypes();
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add medicine' });
      get().setLoading('medicines', false);
      throw e;
    }
  },

  updateMedicine: async (id, updates) => {
    try {
      await api.put(`/medicines/${id}`, {
        brandName: updates.name,
        genericName: updates.genericName,
        categoryId: Number(updates.categoryId),
        newCategoryName: updates.newCategoryName,
        unitTypeId: updates.unitTypeId,
        newUnitTypeName: updates.newUnitTypeName,
        reorderLevel: updates.reorderLevel,
        isActive: updates.isActive,
      });
      await get().fetchMedicines();
      await get().fetchCategories();
      await get().fetchUnitTypes();
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

  fetchMedicines: async () => {
    get().setLoading('medicines', true); set({ error: null });
    try {
      const res = await api.get<MedicineResponse[]>('/medicines');
      set({ medicines: (Array.isArray(res.data) ? res.data : []).map(toMedicine) });
      get().setLoading('medicines', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch medicines' });
      get().setLoading('medicines', false);
    }
  },

  fetchSuppliers: async () => {
    get().setLoading('suppliers', true); set({ error: null });
    try {
      const res = await api.get<SupplierResponse[]>('/suppliers');
      set({ suppliers: (Array.isArray(res.data) ? res.data : []).map(toSupplier) });
      get().setLoading('suppliers', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch suppliers' });
      get().setLoading('suppliers', false);
    }
  },

  addSupplier: async (supplier) => {
    get().setLoading('suppliers', true); set({ error: null });
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
      set({ error: e.response?.data?.message || 'Failed to add supplier' });
      get().setLoading('suppliers', false);
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
    get().setLoading('purchases', true); set({ error: null });
    try {
      const res = await api.get<PurchaseResponse[]>('/purchases');
      set({ purchases: (Array.isArray(res.data) ? res.data : []).map(toPurchase) });
      get().setLoading('purchases', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch purchases' });
      get().setLoading('purchases', false);
    }
  },

  addPurchase: async (purchase) => {
    get().setLoading('purchases', true); set({ error: null });
    try {
      const res = await api.post<PurchaseResponse>('/purchases', purchase);
      const createdPurchase = toPurchase(res.data);
      await get().fetchPurchases();
      await get().fetchMedicines();
      await get().fetchCosmetics();
      return createdPurchase;
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to add purchase' });
      get().setLoading('purchases', false);
      throw e;
    }
  },

  fetchCosmetics: async () => {
    get().setLoading('cosmetics', true); set({ error: null });
    try {
      const res = await api.get<CosmeticResponse[]>('/cosmetics');
      const data = Array.isArray(res.data) ? res.data : [];
      set({ cosmetics: data.map(toCosmetic) });
      get().setLoading('cosmetics', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch cosmetics' });
      get().setLoading('cosmetics', false);
    }
  },

  fetchSales: async () => {
    get().setLoading('sales', true); set({ error: null });
    try {
      const res = await api.get<SaleResponse[]>('/sales');
      set({ sales: (Array.isArray(res.data) ? res.data : []).map(toSale) });
      get().setLoading('sales', false);
    } catch (e: any) {
      set({ error: e.response?.data?.message || 'Failed to fetch sales' });
      get().setLoading('sales', false);
    }
  },

  addSale: async (sale) => {
    get().setLoading('sales', true); set({ error: null });
    try {
      const items = sale.items.map(i => ({
        productId: i.medicineId ? Number(i.medicineId) : undefined,
        cosmeticId: i.cosmeticId ? Number(i.cosmeticId) : undefined,
        productType: i.productType || 'medicine',
        quantity: i.quantity,
        discountAmount: i.discountAmount || 0,
        batchId: i.batchId ? Number(i.batchId) : undefined,
        cosmeticBatchId: i.cosmeticBatchId ? Number(i.cosmeticBatchId) : undefined,
      }));
      const payload = {
        items,
        paymentMethod: sale.paymentMethod || 'cash',
        amountPaid: sale.amountPaid || 0,
        referenceNumber: sale.referenceNumber || null,
        discountReason: sale.discountReason || null,
      };
      console.log('SALE REQUEST:', JSON.stringify(payload, null, 2));
      const res = await api.post<SaleResponse>('/sales', payload);
      console.log('SALE RESPONSE:', res.data);
      await get().fetchSales();
      return res.data;
    } catch (e: any) {
      console.error('SALE ERROR:', e.response?.data || e.message);
      set({ error: e.response?.data?.message || 'Failed to add sale' });
      get().setLoading('sales', false);
      throw e;
    }
  },

  cart: [],
  addToCart: (item) => set((state) => {
    const existing = state.cart.find(c => 
      (c.productType === item.productType) &&
      ((item.productType === 'medicine' && c.medicineId === item.medicineId && c.batchId === item.batchId) ||
       (item.productType === 'cosmetic' && c.cosmeticId === item.cosmeticId && c.cosmeticBatchId === item.cosmeticBatchId))
    );
    if (existing) {
      return {
        cart: state.cart.map(c =>
          (c.productType === item.productType) &&
          ((item.productType === 'medicine' && c.medicineId === item.medicineId && c.batchId === item.batchId) ||
           (item.productType === 'cosmetic' && c.cosmeticId === item.cosmeticId && c.cosmeticBatchId === item.cosmeticBatchId))
            ? { ...c, quantity: c.quantity + item.quantity }
            : c
        ),
      };
    }
    return { cart: [...state.cart, item] };
  }),
  updateCartItemQuantity: (itemId, batchId, quantity, productType = 'medicine') => set((state) => ({
    cart: state.cart.map(c =>
      c.productType === productType &&
      ((productType === 'medicine' && c.medicineId === itemId && c.batchId === batchId) ||
       (productType === 'cosmetic' && c.cosmeticId === itemId && c.cosmeticBatchId === batchId))
        ? { ...c, quantity: Math.max(1, Math.min(quantity, c.availableQuantity)) }
        : c
    ),
  })),
  updateCartItemDiscount: (itemId, batchId, discountAmount, productType = 'medicine') => set((state) => ({
    cart: state.cart.map(c =>
      c.productType === productType &&
      ((productType === 'medicine' && c.medicineId === itemId && c.batchId === batchId) ||
       (productType === 'cosmetic' && c.cosmeticId === itemId && c.cosmeticBatchId === batchId))
        ? { ...c, discountAmount: Math.max(0, Math.min(discountAmount, c.standardPrice)), sellingPrice: c.standardPrice - Math.max(0, Math.min(discountAmount, c.standardPrice)) }
        : c
    ),
  })),
  removeFromCart: (itemId, batchId, productType = 'medicine') => set((state) => ({
    cart: state.cart.filter(c => !(c.productType === productType && ((productType === 'medicine' && c.medicineId === itemId && c.batchId === batchId) || (productType === 'cosmetic' && c.cosmeticId === itemId && c.cosmeticBatchId === batchId)))),
  })),
  clearCart: () => set({ cart: [], cartDiscountReason: '' }),
  cartDiscountReason: '',
  setCartDiscountReason: (reason) => set({ cartDiscountReason: reason }),

  notifications: [],
  fetchNotifications: async () => {
    try {
      const res = await api.get<NotificationResponse[]>('/notifications');
      const data = Array.isArray(res.data) ? res.data : [];
      set({ notifications: data.map(toNotification) });
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
    const newNotifications: Notification[] = [];
    state.medicines.forEach(medicine => {
      medicine.batches.forEach(batch => {
        const days = getDaysUntilExpiry(batch.expiryDate);
        if (days > 0 && days <= state.settings.expiryAlertMonths * 30) {
          newNotifications.push({
            id: generateId(),
            title: 'Medicine Expiring Soon',
            message: `${medicine.name} (Batch: ${batch.batchNumber}) expires in ${days} days`,
            type: 'expiry',
            isRead: false,
            createdAt: new Date().toISOString(),
          });
        }
      });
    });
    if (newNotifications.length > 0) {
      set({ notifications: [...state.notifications, ...newNotifications] });
    }
  },

  auditLogs: [],
  fetchAuditLogs: async () => {
    try {
      const res = await api.get<AuditLogResponse[]>('/audit-logs');
      const data = Array.isArray(res.data) ? res.data : [];
      set({
        auditLogs: data.map(l => ({
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
          language: s.language,
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
