import React, { useState, useMemo, useEffect } from 'react';
import { Plus, Search, Eye, ClipboardList, Truck, Package, X, AlertCircle, Check, ShoppingCart, FileSpreadsheet, Calendar, Sparkles, Tag, Minus, Edit2, Trash2, Loader2, DollarSign, ChevronDown, RotateCcw, Save } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { formatDate, formatCurrency } from '../utils/helpers';
import { BulkPurchaseItem, CosmeticPurchaseItem, COSMETIC_CATEGORIES, CosmeticCategoryValue } from '../types';
import { CreatePurchaseRequest } from '../services/api';

// Cosmetic category mapping: frontend index -> backend catalog ID (CosmeticCatalog)
const COSMETIC_CATEGORY_IDS: Record<number, number> = {
  0: -100,  // Hair Care
  1: -101,  // Skin Care
  2: -102,  // Bath & Body
  3: -103,  // Oral Care
  4: -104,  // Baby Care
  5: -105,  // Makeup
  6: -106,  // Fragrance
  7: -107,  // Feminine Care
  8: -108,  // Other
};

export const PurchasesPage: React.FC = () => {
  const { purchases, suppliers, medicines, categories, fetchPurchases, fetchSuppliers, fetchMedicines, fetchCategories, addPurchase } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [search, setSearch] = useState('');
  const [showBulkModal, setShowBulkModal] = useState(false);
  const [showCosmeticModal, setShowCosmeticModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedPurchase, setSelectedPurchase] = useState<typeof purchases[0] | null>(null);
  const [selectedSupplier, setSelectedSupplier] = useState('');
  const [purchaseDate, setPurchaseDate] = useState(new Date().toISOString().split('T')[0]);
  const [items, setItems] = useState<BulkPurchaseItem[]>([]);
  const [cosmeticItems, setCosmeticItems] = useState<CosmeticPurchaseItem[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [lastPurchaseNumber, setLastPurchaseNumber] = useState('');
  const [paymentStatus, setPaymentStatus] = useState<'paid' | 'partial' | 'unpaid'>('paid');

  // DIAGNOSTIC: Page mount/unmount
  useEffect(() => {
    console.log('[PURCHASES PAGE] MOUNTED');
    return () => {
      console.log('[PURCHASES PAGE] UNMOUNTED');
    };
  }, []);

  // DIAGNOSTIC: Track cosmeticItems changes
  useEffect(() => {
    console.log('[COSMETIC] STATE CHANGED', cosmeticItems);
  }, [cosmeticItems]);

  // DIAGNOSTIC: Track showCosmeticModal changes
  useEffect(() => {
    console.log('[COSMETIC MODAL] showCosmeticModal changed', showCosmeticModal);
  }, [showCosmeticModal]);

  useEffect(() => {
    fetchPurchases();
    fetchSuppliers();
    fetchMedicines();
    fetchCategories();
  }, [fetchPurchases, fetchSuppliers, fetchMedicines, fetchCategories]);
  const [paymentMethod, setPaymentMethod] = useState<'cash' | 'bank_transfer' | 'mobile_money' | 'credit'>('cash');
  const [amountPaid, setAmountPaid] = useState('');

  const filteredPurchases = useMemo(() => {
    return (purchases || []).filter(p =>
      p.purchaseNumber.toLowerCase().includes(search.toLowerCase()) ||
      p.supplierName.toLowerCase().includes(search.toLowerCase()) ||
      p.items.some(i => 
        i.brandName.toLowerCase().includes(search.toLowerCase()) ||
        i.batchNumber.toLowerCase().includes(search.toLowerCase())
      )
    ).sort((a, b) => new Date(b.purchaseDate).getTime() - new Date(a.purchaseDate).getTime());
  }, [purchases, search]);

  const totalAmount = (items || []).reduce((sum, item) => {
    const qty = Number(item.quantity) || 0;
    const price = Number(item.purchasePrice) || 0;
    return sum + (qty * price);
  }, 0) + (cosmeticItems || []).reduce((sum, item) => {
    const qty = Number(item.quantity) || 0;
    const price = Number(item.buyingPrice) || 0;
    return sum + (qty * price);
  }, 0);

  const totalItems = (items || []).filter(i => i.brandName && i.quantity).length + (cosmeticItems || []).filter(i => i.brandName && i.quantity).length;

  const resetForm = () => {
    console.log('[PURCHASES] resetForm called');
    setSelectedSupplier('');
    setPurchaseDate(new Date().toISOString().split('T')[0]);
    setItems([]);
    setCosmeticItems([]);
    setPaymentStatus('paid');
    setPaymentMethod('cash');
    setAmountPaid('');
  };

  const createEmptyItem = (): BulkPurchaseItem => ({
    id: `temp_${new Date().getTime()}_${Math.random()}`,
    brandName: '',
    genericName: '',
    categoryId: '',
    categoryName: '',
    batchNumber: '',
    quantity: '',
    purchasePrice: '',
    sellingPrice: '',
    expiryDate: '',
    unitType: 'Tablet',
    isNewMedicine: false,
    errors: [],
  });

  const createEmptyCosmeticItem = (): CosmeticPurchaseItem => ({
    id: `temp_cosmetic_${new Date().getTime()}_${Math.random()}`,
    productType: 'cosmetic',
    categoryId: '',
    brandName: '',
    quantity: '',
    buyingPrice: '',
    sellingPrice: '',
    lowStock: '',
    expiryDate: '',
    errors: [],
  });

  const handleAddRow = () => {
    setItems([...items, createEmptyItem()]);
  };

  const handleRemoveRow = (id: string) => {
    setItems((items || []).filter(item => item.id !== id));
  };

  const handleItemChange = (id: string, field: keyof BulkPurchaseItem, value: string) => {
    setItems((items || []).map(item => {
      if (item.id !== id) return item;
      
      const updated = { ...item, [field]: value, errors: [] };
      
      // Auto-fill when medicine is selected from existing
      if (field === 'brandName') {
        const existingMedicine = (medicines || []).find(m => 
          m.name.toLowerCase() === value.toLowerCase()
        );
        if (existingMedicine) {
          updated.existingMedicineId = existingMedicine.id;
          updated.isNewMedicine = false;
          updated.genericName = existingMedicine.genericName;
          updated.categoryId = existingMedicine.categoryId;
          updated.categoryName = existingMedicine.categoryName;
          updated.unitType = existingMedicine.unitType;
          if (existingMedicine.batches[0]) {
            updated.purchasePrice = String(existingMedicine.batches[0].purchasePrice);
            updated.sellingPrice = String(existingMedicine.batches[0].sellingPrice);
          }
        } else if (value.length > 0) {
          updated.isNewMedicine = true;
          updated.existingMedicineId = undefined;
        }
      }
      
      if (field === 'categoryId') {
        const cat = (categories || []).find(c => c.id === value);
        if (cat) {
          updated.categoryName = cat.name;
        }
      }
      
      return updated;
    }));
  };

  const handleCosmeticItemChange = (id: string, field: keyof CosmeticPurchaseItem, value: string) => {
    console.log('[COSMETIC] handleCosmeticItemChange called', { id, field, value, currentItems: cosmeticItems });
    if (field === 'categoryId') {
      console.log('[COSMETIC CATEGORY] CHANGE START', { id, value });
    }
    setCosmeticItems((prevItems) => {
      console.log('[COSMETIC] setCosmeticItems prev', prevItems);
      const next = prevItems.map(item => {
        if (item.id !== id) return item;
        const updated = { ...item, [field]: value, errors: [] };
        if (field === 'categoryId') {
          console.log('[COSMETIC CATEGORY] ITEM UPDATED', { id, oldCategory: item.categoryId, newCategory: value });
        }
        return updated;
      });
      console.log('[COSMETIC] setCosmeticItems next', next);
      if (field === 'categoryId') {
        console.log('[COSMETIC CATEGORY] STATE UPDATED');
      }
      return next;
    });
  };

  const handleAddCosmeticRow = () => {
    console.log('[COSMETIC] handleAddCosmeticRow called');
    setCosmeticItems((prev) => {
      const next = [...prev, createEmptyCosmeticItem()];
      console.log('[COSMETIC] handleAddCosmeticRow next', next);
      return next;
    });
  };

  const handleRemoveCosmeticRow = (id: string) => {
    console.log('[COSMETIC] handleRemoveCosmeticRow called', { id });
    setCosmeticItems((prev) => {
      const next = prev.filter(item => item.id !== id);
      console.log('[COSMETIC] handleRemoveCosmeticRow next', next);
      return next;
    });
  };

  const validateCosmeticItems = (): boolean => {
    console.log('[COSMETIC] validateCosmeticItems called', cosmeticItems);
    let isValid = true;
    const validatedItems = cosmeticItems.map(item => {
      const errors: string[] = [];
      if (!item.categoryId) errors.push('Category required');
      if (!item.brandName.trim()) errors.push('Brand name required');
      if (!item.quantity || Number(item.quantity) <= 0) errors.push('Invalid quantity');
      if (!item.buyingPrice || Number(item.buyingPrice) < 0) errors.push('Invalid buying price');
      if (!item.sellingPrice || Number(item.sellingPrice) < 0) errors.push('Invalid selling price');
      if (Number(item.buyingPrice) > Number(item.sellingPrice)) errors.push('Selling price must not exceed buying price');
      if (!item.lowStock || Number(item.lowStock) < 0) errors.push('Invalid low stock threshold');
      const duplicate = cosmeticItems.find(i => 
        i.id !== item.id &&
        i.brandName.toLowerCase() === item.brandName.toLowerCase() &&
        i.expiryDate === item.expiryDate
      );
      if (duplicate) errors.push('Duplicate batch');
      if (errors.length > 0) isValid = false;
      return { ...item, errors };
    });
    console.log('[COSMETIC] validateCosmeticItems validatedItems', validatedItems);
    setCosmeticItems(validatedItems);
    return isValid;
  };

  const validateItems = (): boolean => {
    let isValid = true;
    const validatedItems = (items || []).map(item => {
      const errors: string[] = [];
      
      if (!item.brandName.trim()) errors.push('Brand name required');
      if (!item.batchNumber.trim()) errors.push('Batch number required');
      if (!item.quantity || Number(item.quantity) <= 0) errors.push('Invalid quantity');
      if (!item.purchasePrice || Number(item.purchasePrice) <= 0) errors.push('Invalid purchase price');
      if (!item.sellingPrice || Number(item.sellingPrice) <= 0) errors.push('Invalid selling price');
      if (!item.expiryDate) errors.push('Expiry date required');
      if (item.isNewMedicine && !item.categoryId) errors.push('Category required for new medicine');
      
      // Check duplicate batch numbers
      const duplicateBatch = (items || []).find(i => 
        i.id !== item.id && 
        i.batchNumber.toLowerCase() === item.batchNumber.toLowerCase() &&
        i.brandName.toLowerCase() === item.brandName.toLowerCase()
      );
      if (duplicateBatch) errors.push('Duplicate batch number');
      
      if (errors.length > 0) isValid = false;
      return { ...item, errors };
    });
    
    setItems(validatedItems);
    return isValid;
  };

  const handleSave = async () => {
    console.log('[PURCHASES] handleSave called', { items: items.length, cosmeticItems: cosmeticItems.length, selectedSupplier });
    if (!selectedSupplier) { alert('Please select a supplier'); return; }
    if ((items || []).length === 0 && (cosmeticItems || []).length === 0) { alert('Please add at least one item'); return; }
    if ((items || []).length > 0 && !validateItems()) { return; }
    if ((cosmeticItems || []).length > 0 && !validateCosmeticItems()) { return; }

    setIsProcessing(true);

    let finalAmountPaid = paymentStatus === 'paid' ? totalAmount : Number(amountPaid) || 0;
    let finalDebt = totalAmount - finalAmountPaid;
    let finalStatus = paymentStatus;

    if (finalDebt <= 0) {
      finalStatus = 'paid';
      finalAmountPaid = totalAmount;
      finalDebt = 0;
    } else if (finalAmountPaid === 0) {
      finalStatus = 'unpaid';
    } else {
      finalStatus = 'partial';
    }

    if (finalAmountPaid > totalAmount) {
      alert('Amount paid cannot exceed the purchase total.');
      return;
    }

    const purchaseRequest: CreatePurchaseRequest = {
        supplierId: Number(selectedSupplier),
      purchaseDate: new Date(purchaseDate).toISOString(),
      paymentStatus: finalStatus,
      paymentMethod: paymentMethod,
      amountPaid: finalAmountPaid,
      items: [
        ...items.map(item => ({
          productId: item.existingMedicineId ? Number(item.existingMedicineId) : undefined,
          productType: 'medicine',
          brandName: item.isNewMedicine ? item.brandName : undefined,
          genericName: item.isNewMedicine ? item.genericName : undefined,
          categoryId: item.isNewMedicine ? (item.categoryId ? Number(item.categoryId) : undefined) : undefined,
          categoryName: item.isNewMedicine ? item.categoryName : undefined,
          unitType: item.isNewMedicine ? item.unitType : undefined,
          reorderLevel: item.isNewMedicine ? (item.lowStockThreshold || 10) : undefined,
          batchNumber: item.batchNumber,
          quantity: Number(item.quantity),
          purchasePrice: Number(item.purchasePrice),
          sellingPrice: Number(item.sellingPrice),
          expiryDate: item.expiryDate ? new Date(item.expiryDate).toISOString() : undefined,
        })),
        ...cosmeticItems.map(item => {
          const frontendCategoryIndex = item.categoryId ? Number(item.categoryId) : undefined;
          const backendCategoryId = frontendCategoryIndex !== undefined ? COSMETIC_CATEGORY_IDS[frontendCategoryIndex] : undefined;
          return ({
            productId: undefined,
            productType: 'cosmetic' as const,
            brandName: item.brandName,
            genericName: item.brandName,
            categoryId: backendCategoryId,
            categoryName: undefined,
            reorderLevel: item.lowStock ? Number(item.lowStock) : 10,
            batchNumber: '',
            quantity: Number(item.quantity),
            purchasePrice: Number(item.buyingPrice),
            sellingPrice: Number(item.sellingPrice),
            expiryDate: item.expiryDate ? new Date(item.expiryDate).toISOString() : undefined,
          });
        }),
      ],
    };

    try {
      const newPurchase = await addPurchase(purchaseRequest);
      setLastPurchaseNumber(newPurchase.purchaseNumber);
      setShowBulkModal(false);
      setShowSuccess(true);
      resetForm();
      setTimeout(() => setShowSuccess(false), 5000);
    } catch (error) {
      const apiError = error as { response?: { status?: number; data?: { message?: string; title?: string; errors?: string[] | Record<string, string[]>; correlationId?: string } } };
      const status = apiError.response?.status;
      const data = apiError.response?.data;
      const validationErrors = data?.errors;
      const detail = validationErrors
        ? Object.values(validationErrors).flat().join('\n')
        : data?.message ?? data?.title ?? error.message ?? 'Unknown error';
      const correlationId = data?.correlationId ?? apiError.response?.headers?.['x-correlation-id'];
      console.error('Failed to save purchase', { status, detail, correlationId, error });

      let alertMsg = `Unable to save purchase:\n\n${detail}`;
      if (correlationId) {
        alertMsg += `\n\nPlease reference correlation ID: ${correlationId} when contacting support.`;
      }
      alert(alertMsg);
    } finally {
      setIsProcessing(false);
    }
  };

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all text-base ${
    isDark 
      ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500 focus:bg-gray-700' 
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400'
  }`;
  
  const smallInputClass = `w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm transition-all ${
    isDark 
      ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500' 
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400'
  }`;

  const itemInputClass = `w-full min-w-0 px-4 py-3 border rounded-lg text-base leading-5 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all ${
    isDark 
      ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500' 
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400'
  }`;
  
  const thClass = `text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Purchase Management</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Bulk medicine registration & purchase entry</p>
        </div>
        <div className="flex gap-3">
          <button
            onClick={() => { resetForm(); setShowBulkModal(true); }}
            className="px-4 py-2.5 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg shadow-emerald-200 dark:shadow-emerald-900/30 flex items-center gap-2"
          >
            <ShoppingCart className="h-4 w-4" /> Bulk Purchase Entry
          </button>
          <button
            onClick={() => { console.log('[COSMETIC MODAL] Open clicked'); setShowCosmeticModal(true); }}
            className="px-4 py-2.5 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg shadow-purple-200 dark:shadow-purple-900/30 flex items-center gap-2"
          >
            <Sparkles className="h-4 w-4" /> Add Cosmetics
          </button>
        </div>
      </div>

      {/* Info Banner */}
      <div className={`rounded-xl p-4 border ${isDark ? 'bg-blue-900/20 border-blue-800' : 'bg-blue-50 border-blue-200'}`}>
        <div className="flex items-start gap-3">
          <FileSpreadsheet className="h-5 w-5 text-blue-500 mt-0.5 flex-shrink-0" />
          <div>
            <p className={`text-sm font-medium ${isDark ? 'text-blue-300' : 'text-blue-700'}`}>Bulk Purchase Entry</p>
            <p className={`text-xs mt-1 ${isDark ? 'text-blue-400' : 'text-blue-600'}`}>
              Register multiple medicines and stock in a single transaction. New medicines will be created automatically if they don't exist.
            </p>
          </div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <ClipboardList className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Purchases</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{(purchases || []).length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600'}`}>
              <Package className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Spent</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency((purchases || []).reduce((s, p) => s + p.totalAmount, 0))}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-violet-900/30 text-violet-400' : 'bg-violet-100 text-violet-600'}`}>
              <Truck className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Suppliers</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{new Set((purchases || []).map(p => p.supplierId)).size}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Search */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
        <div className="relative">
          <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
          <input
            type="text"
            placeholder="Search by purchase number, supplier, brand name, or batch number..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className={inputClass}
          />
        </div>
      </div>

      {/* Purchase History Table */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={thClass}>Purchase #</th>
                <th className={thClass}>Supplier</th>
                <th className={thClass}>Date</th>
                <th className={thClass}>Items</th>
                <th className={thClass}>Total</th>
                <th className={thClass}>Payment</th>
                <th className={`${thClass} text-right`}>Actions</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {(filteredPurchases || []).map(purchase => (
                <tr key={purchase.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                  <td className="px-6 py-4">
                    <span className="font-mono text-sm font-medium text-blue-500">{purchase.purchaseNumber}</span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <Truck className={`h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
                      <span className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-900'}`}>{purchase.supplierName}</span>
                    </div>
                  </td>
                  <td className={`px-6 py-4 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{formatDate(purchase.purchaseDate)}</td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${
                      isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-700'
                    }`}>
                      {purchase.items.length} items
                    </span>
                  </td>
                  <td className={`px-6 py-4 text-sm font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(purchase.totalAmount)}</td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium whitespace-nowrap ${
                      purchase.paymentStatus === 'paid' ? isDark ? 'bg-green-900/30 text-green-400' : 'bg-green-100 text-green-700' :
                      purchase.paymentStatus === 'partial' ? isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-700' :
                      isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-700'
                    }`}>
                      {purchase.paymentStatus === 'paid' ? '✅ Paid' :
                       purchase.paymentStatus === 'partial' ? `🔶 ${formatCurrency(purchase.amountPaid)}` :
                       '❌ Unpaid'}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <button
                      onClick={() => { setSelectedPurchase(purchase); setShowDetailModal(true); }}
                      className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-blue-900/30 text-gray-400 hover:text-blue-400' : 'hover:bg-blue-50 text-gray-400 hover:text-blue-600'}`}
                    >
                      <Eye className="h-4 w-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {(filteredPurchases || []).length === 0 && (
          <div className="text-center py-12">
            <ClipboardList className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
            <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No purchases found</p>
          </div>
        )}
      </div>

      {/* ========== BULK PURCHASE MODAL ========== */}
      <Modal isOpen={showBulkModal} onClose={() => setShowBulkModal(false)} title="Bulk Purchase Entry" size="2xl">
        <div className="space-y-5">
          {/* Header Info */}
          <div className="flex items-center gap-4 pb-2">
            <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600'}`}>
              <ShoppingCart className="h-7 w-7" />
            </div>
            <div>
              <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>New Bulk Purchase</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Add multiple medicines in one transaction</p>
            </div>
          </div>

          {/* Supplier & Date */}
          <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <Truck className="h-4 w-4" /> Supplier <span className="text-red-500">*</span>
                </label>
                <select
                  value={selectedSupplier}
                  onChange={(e) => setSelectedSupplier(e.target.value)}
                  className={inputClass}
                >
                  <option value="">Select Supplier</option>
                  {(suppliers || []).map(s => (
                    <option key={s.id} value={s.id}>{s.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <Calendar className="h-4 w-4" /> Purchase Date
                </label>
                <input
                  type="date"
                  value={purchaseDate}
                  onChange={(e) => setPurchaseDate(e.target.value)}
                  className={inputClass}
                />
              </div>
            </div>
          </div>

          {/* Items Table */}
          <div className={`rounded-xl border overflow-hidden ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <div className={`px-4 py-3 flex items-center justify-between ${isDark ? 'bg-gray-700' : 'bg-gray-100'}`}>
              <h4 className={`text-sm font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                Medicine Items ({(items || []).length})
              </h4>
              <button
                type="button"
                onClick={handleAddRow}
                className="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-medium transition-colors flex items-center gap-1"
              >
                <Plus className="h-3 w-3" /> Add Item
              </button>
            </div>
            
            {(items || []).length > 0 ? (
              <div className="p-3 space-y-3 max-h-[440px] overflow-y-auto">
                <div className="hidden xl:grid xl:grid-cols-[minmax(10rem,1.5fr)_minmax(7rem,1fr)_minmax(8.5rem,1.2fr)_minmax(5.5rem,.7fr)_minmax(7.5rem,.9fr)_minmax(7.5rem,.9fr)_minmax(7.5rem,1fr)_2rem] gap-2 px-2 pb-1 text-xs font-semibold uppercase tracking-wide">
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Medicine Name</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Category</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Batch #</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Qty</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Buy Price</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Sell Price</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Expiry</div>
                  <div />
                </div>

                {(items || []).map((item, index) => (
                  <div
                    key={item.id}
                    className={`rounded-lg border p-3 transition-colors ${
                      isDark ? 'bg-gray-800/50 border-gray-700 hover:border-gray-600' : 'bg-white border-gray-200 hover:border-gray-300'
                    } ${item.errors.length > 0 ? isDark ? 'border-red-500/50 bg-red-900/10' : 'border-red-400 bg-red-50/50' : ''}`}
                  >
                    <div className="flex items-center justify-between mb-3 xl:hidden">
                      <span className={`text-sm font-medium ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                        Item #{index + 1}
                      </span>
                      <button
                        type="button"
                        onClick={() => handleRemoveRow(item.id)}
                        className={`text-xs font-medium px-2 py-1 rounded-lg transition-colors ${
                          isDark ? 'text-red-400 hover:bg-red-900/30' : 'text-red-500 hover:bg-red-100'
                        }`}
                      >
                        Remove
                      </button>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-[minmax(10rem,1.5fr)_minmax(7rem,1fr)_minmax(8.5rem,1.2fr)_minmax(5.5rem,.7fr)_minmax(7.5rem,.9fr)_minmax(7.5rem,.9fr)_minmax(7.5rem,1fr)_2rem] gap-3 xl:gap-2 items-end">
                      <div className="sm:col-span-2 xl:col-span-1">
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Medicine Name {item.isNewMedicine && <span className="text-emerald-500">(New)</span>}
                        </label>
                        <div className="relative">
                          <input
                            type="text"
                            value={item.brandName}
                            onChange={(e) => handleItemChange(item.id, 'brandName', e.target.value)}
                            placeholder="Brand name"
                            className={`${itemInputClass} ${!item.brandName ? 'border-red-400' : ''}`}
                            list={`medicines-${item.id}`}
                          />
                          <datalist id={`medicines-${item.id}`}>
                            {(medicines || []).map(m => (
                              <option key={m.id} value={m.name} />
                            ))}
                          </datalist>
                          {item.isNewMedicine && item.brandName && (
                            <span className={`absolute right-2 top-1/2 -translate-y-1/2 text-xs px-1.5 py-0.5 rounded ${
                              isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-700'
                            }`}>NEW</span>
                          )}
                        </div>
                        {item.isNewMedicine && (
                          <input
                            type="text"
                            value={item.genericName}
                            onChange={(e) => handleItemChange(item.id, 'genericName', e.target.value)}
                            placeholder="Generic name"
                            className={`mt-1 w-full px-3 py-1.5 border rounded-lg text-sm ${isDark ? 'bg-gray-800 border-gray-600 text-gray-300 placeholder-gray-500' : 'bg-white border-gray-200 text-gray-600 placeholder-gray-400'}`}
                          />
                        )}
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Category {item.isNewMedicine && <span className="text-red-500">*</span>}
                        </label>
                        <select
                          value={item.categoryId}
                          onChange={(e) => handleItemChange(item.id, 'categoryId', e.target.value)}
                          className={`${itemInputClass} ${!item.categoryId && item.isNewMedicine ? 'border-red-400' : ''}`}
                        >
                          <option value="">Select</option>
                          {(categories || []).map(c => (
                            <option key={c.id} value={c.id}>{c.name}</option>
                          ))}
                        </select>
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Batch # <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          value={item.batchNumber}
                          onChange={(e) => handleItemChange(item.id, 'batchNumber', e.target.value)}
                          placeholder="e.g. P001"
                          className={`${itemInputClass} font-mono ${!item.batchNumber ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Qty <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          value={item.quantity}
                          onChange={(e) => handleItemChange(item.id, 'quantity', e.target.value)}
                          placeholder="0"
                          min="0"
                          className={`${itemInputClass} ${!item.quantity ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Buy Price <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          value={item.purchasePrice}
                          onChange={(e) => handleItemChange(item.id, 'purchasePrice', e.target.value)}
                          placeholder="0.00"
                          min="0"
                          step="0.01"
                          className={`${itemInputClass} ${!item.purchasePrice ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Sell Price <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          value={item.sellingPrice}
                          onChange={(e) => handleItemChange(item.id, 'sellingPrice', e.target.value)}
                          placeholder="0.00"
                          min="0"
                          step="0.01"
                          className={`${itemInputClass} ${!item.sellingPrice ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Expiry <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="date"
                          value={item.expiryDate}
                          onChange={(e) => handleItemChange(item.id, 'expiryDate', e.target.value)}
                          className={`${itemInputClass} ${!item.expiryDate ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div className="hidden xl:flex justify-center">
                        <button
                          type="button"
                          onClick={() => handleRemoveRow(item.id)}
                          className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-red-400' : 'hover:bg-red-100 text-red-500'}`}
                        >
                          <X className="h-4 w-4" />
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className={`text-center py-12 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                <Package className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="text-sm">No items added yet</p>
                <p className="text-xs mt-1">Click "Add Item" to start adding medicines</p>
              </div>
            )}
          </div>

          {/* Validation Errors */}
          {(items || []).some(i => i.errors.length > 0) && (
            <div className={`p-4 rounded-xl border ${isDark ? 'bg-red-900/20 border-red-800' : 'bg-red-50 border-red-200'}`}>
              <div className="flex items-center gap-2 mb-2">
                <AlertCircle className="h-4 w-4 text-red-500" />
                <p className="text-sm font-medium text-red-500">Validation Errors</p>
              </div>
              <ul className="space-y-1">
                {(items || []).filter(i => i.errors.length > 0).map((item) => (
                  <li key={item.id} className="text-xs text-red-400">
                    Row {items.indexOf(item) + 1}: {item.errors.join(', ')}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Payment Information */}
          <div className={`p-5 rounded-xl border ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}>
            <h4 className={`text-sm font-semibold mb-4 flex items-center gap-2 ${isDark ? 'text-white' : 'text-gray-900'}`}>
              💳 Payment Information
            </h4>
            
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
              {/* Payment Method */}
              <div>
                <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Payment Method</label>
                <select
                  value={paymentMethod}
                  onChange={(e) => setPaymentMethod(e.target.value as any)}
                  className={`w-full px-3 py-2.5 border rounded-lg text-sm ${
                    isDark ? 'bg-gray-800 border-gray-600 text-white' : 'bg-white border-gray-300'
                  }`}
                >
                  <option value="cash">💵 Cash</option>
                  <option value="bank_transfer">�� Bank Transfer</option>
                  <option value="mobile_money">📱 Mobile Money</option>
                  <option value="credit">📝 Credit</option>
                </select>
              </div>

              {/* Payment Status */}
              <div>
                <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Payment Status</label>
                <div className="grid grid-cols-3 gap-2">
                  {(['paid', 'partial', 'unpaid'] as const).map((status) => (
                    <button
                      key={status}
                      type="button"
                      onClick={() => {
                        setPaymentStatus(status);
                        if (status === 'paid') setAmountPaid(String(totalAmount));
                        if (status === 'unpaid') setAmountPaid('0');
                      }}
                      className={`px-3 py-2.5 rounded-lg text-xs font-medium transition-all ${
                        paymentStatus === status
                          ? status === 'paid' ? 'bg-emerald-600 text-white shadow-md' : status === 'partial' ? 'bg-amber-600 text-white shadow-md' : 'bg-red-600 text-white shadow-md'
                          : isDark ? 'bg-gray-800 text-gray-300 border border-gray-600' : 'bg-white text-gray-600 border border-gray-300'
                      }`}
                    >
                      {status === 'paid' ? '✅ Paid' : status === 'partial' ? '🔶 Partial' : '❌ Unpaid'}
                    </button>
                 ))}
                </div>
              </div>
            </div>

            {/* Amount Paid (for partial) */}
            {paymentStatus === 'partial' && (
              <div className={`p-4 rounded-lg border ${isDark ? 'bg-amber-900/20 border-amber-800' : 'bg-amber-50 border-amber-200'}`}>
                <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-amber-300' : 'text-amber-700'}`}>
                  💰 How much are you paying now? <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  value={amountPaid}
                  onChange={(e) => setAmountPaid(e.target.value)}
                  placeholder="Enter amount paid"
                  className={`w-full px-3 py-2.5 border rounded-lg text-sm ${
                    isDark ? 'bg-gray-800 border-amber-700 text-white placeholder-gray-500' : 'bg-white border-amber-300'
                  }`}
                />
                {Number(amountPaid) > 0 && Number(amountPaid) < totalAmount && (
                  <div className={`mt-2 p-2 rounded text-xs ${isDark ? 'bg-red-900/20 text-red-400' : 'bg-red-50 text-red-600'}`}>
                    Remaining debt: <strong>{formatCurrency(totalAmount - Number(amountPaid))}</strong>
                  </div>
                )}
                {Number(amountPaid) >= totalAmount && (
                  <div className={`mt-2 p-2 rounded text-xs ${isDark ? 'bg-green-900/20 text-green-400' : 'bg-green-50 text-green-600'}`}>
                    ✅ Full payment will be recorded
                  </div>
                )}
              </div>
            )}

            {/* Payment Summary */}
            <div className={`mt-4 p-4 rounded-lg ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 text-center">
                <div>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Amount</p>
                  <p className={`text-lg font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(totalAmount)}</p>
                </div>
                <div>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Amount Paid</p>
                  <p className="text-lg font-bold text-green-500">
                    {formatCurrency(paymentStatus === 'paid' ? totalAmount : Number(amountPaid) || 0)}
                  </p>
                </div>
                <div>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Remaining Debt</p>
                  <p className={`text-lg font-bold ${
                    (paymentStatus === 'paid' ? 0 : totalAmount - (Number(amountPaid) || 0)) > 0 ? 'text-red-500' : 'text-green-500'
                  }`}>
                    {formatCurrency(paymentStatus === 'paid' ? 0 : Math.max(0, totalAmount - (Number(amountPaid) || 0)))}
                  </p>
                </div>
              </div>
              <div className={`mt-3 pt-3 border-t text-center ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                  Status: <span className={`font-semibold ${
                    paymentStatus === 'paid' ? 'text-green-500' :
                    paymentStatus === 'partial' ? 'text-amber-500' :
                    'text-red-500'
                  }`}>
                    {paymentStatus === 'paid' ? '✅ Paid' :
                     paymentStatus === 'partial' ? '🔶 Partial' :
                     '❌ Unpaid'}
                  </span>
                  {paymentStatus !== 'unpaid' && (
                    <span className={`ml-2 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                      via {paymentMethod === 'cash' ? '💵 Cash' :
                           paymentMethod === 'bank_transfer' ? '🏦 Bank' :
                           paymentMethod === 'mobile_money' ? '📱 Mobile' :
                           '📝 Credit'}
                    </span>
                  )}
                </p>
              </div>
            </div>
          </div>

          {/* Summary */}
          <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <div className="flex items-center justify-between">
              <div>
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Items</p>
                <p className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{totalItems}</p>
              </div>
              <div className="text-right">
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Amount</p>
                <p className="text-2xl font-bold text-emerald-500">{formatCurrency(totalAmount)}</p>
              </div>
            </div>
          </div>

          {/* Actions */}
          <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <button
              type="button"
              onClick={() => { setShowBulkModal(false); setItems([]); }}
              className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleSave}
              disabled={isProcessing || (items || []).length === 0 || !selectedSupplier}
              className="px-5 py-2.5 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
            >
              {isProcessing ? (
                <>
                  <div className="h-4 w-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  Processing...
                </>
              ) : (
                <>
                  <ShoppingCart className="h-4 w-4" />
                  Save Purchase ({totalItems} items)
                </>
              )}
            </button>
          </div>
        </div>
      </Modal>

      {/* Detail Modal */}
      <Modal isOpen={showDetailModal} onClose={() => setShowDetailModal(false)} title="Purchase Details" size="lg">
        {selectedPurchase && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Purchase Number</p>
                <p className="font-mono font-semibold text-blue-500">{selectedPurchase.purchaseNumber}</p>
              </div>
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Supplier</p>
                <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedPurchase.supplierName}</p>
              </div>
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Date</p>
                <p className={isDark ? 'text-gray-300' : 'text-gray-900'}>{formatDate(selectedPurchase.purchaseDate)}</p>
              </div>
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Amount</p>
                <p className="font-semibold text-emerald-500">{formatCurrency(selectedPurchase.totalAmount)}</p>
              </div>
            </div>

            {/* Payment Info */}
            <div className={`p-4 rounded-xl border ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}>
              <h4 className={`text-sm font-semibold mb-3 ${isDark ? 'text-white' : 'text-gray-900'}`}>💳 Payment Information</h4>
              <div className="grid grid-cols-4 gap-3">
                <div>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</p>
                  <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${
                    selectedPurchase.paymentStatus === 'paid' ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' :
                    selectedPurchase.paymentStatus === 'partial' ? 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400' :
                    'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'
                  }`}>
                    {selectedPurchase.paymentStatus === 'paid' ? '✅ Paid' :
                     selectedPurchase.paymentStatus === 'partial' ? '🔶 Partial' :
                     '❌ Unpaid'}
                  </span>
                </div>
                <div>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Amount Paid</p>
                  <p className={`font-semibold text-sm ${isDark ? 'text-green-400' : 'text-green-600'}`}>{formatCurrency(selectedPurchase.amountPaid)}</p>
                </div>
                <div>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Debt</p>
                  <p className={`font-semibold text-sm ${selectedPurchase.remainingDebt > 0 ? 'text-red-500' : 'text-green-500'}`}>
                    {selectedPurchase.remainingDebt > 0 ? formatCurrency(selectedPurchase.remainingDebt) : '0'}
                  </p>
                </div>
                <div>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Method</p>
                  <p className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    {selectedPurchase.paymentMethod === 'cash' ? '💵 Cash' :
                     selectedPurchase.paymentMethod === 'bank_transfer' ? '🏦 Bank' :
                     selectedPurchase.paymentMethod === 'mobile_money' ? '📱 Mobile' :
                     '📝 Credit'}
                  </p>
                </div>
              </div>
            </div>

            <table className="w-full text-sm">
              <thead>
                <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Brand Name</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Batch</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Qty</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Price</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Expiry</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {selectedPurchase.items.map(item => (
                  <tr key={item.id}>
                    <td className={`px-3 py-2 font-medium ${isDark ? 'text-white' : ''}`}>{item.brandName}</td>
                    <td className={`px-3 py-2 font-mono ${isDark ? 'text-gray-300' : ''}`}>{item.batchNumber}</td>
                    <td className={`px-3 py-2 ${isDark ? 'text-gray-300' : ''}`}>{item.quantity}</td>
                    <td className={`px-3 py-2 ${isDark ? 'text-gray-300' : ''}`}>{item.purchasePrice} ETB</td>
                    <td className={`px-3 py-2 ${isDark ? 'text-gray-300' : ''}`}>{formatDate(item.expiryDate)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Modal>

      {/* ========== COSMETIC MODAL ========== */}
      <Modal isOpen={showCosmeticModal} onClose={() => { console.log('[COSMETIC MODAL] onClose (backdrop/x)'); setShowCosmeticModal(false); }} title="Add Cosmetics" size="2xl">
        <div className="space-y-5">
          <div className="flex items-center gap-4 pb-2">
            <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-purple-900/30 text-purple-400' : 'bg-purple-100 text-purple-600'}`}>
              <Sparkles className="h-7 w-7" />
            </div>
            <div>
              <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>New Cosmetic Purchase</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Add cosmetic products to this purchase</p>
            </div>
          </div>

          {/* Cosmetic Items Table */}
          <div className={`rounded-xl border overflow-hidden ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <div className={`px-4 py-3 flex items-center justify-between ${isDark ? 'bg-gray-700' : 'bg-gray-100'}`}>
              <h4 className={`text-sm font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                Cosmetic Items ({cosmeticItems.length})
              </h4>
              <button
                type="button"
                onClick={handleAddCosmeticRow}
                className="px-3 py-1.5 bg-purple-600 hover:bg-purple-700 text-white rounded-lg text-xs font-medium transition-colors flex items-center gap-1"
              >
                <Plus className="h-3 w-3" /> Add Item
              </button>
            </div>

            {cosmeticItems.length > 0 ? (
              <div className="p-3 space-y-3 max-h-[440px] overflow-y-auto">
                <div className="hidden xl:grid xl:grid-cols-[minmax(8rem,1fr)_minmax(8rem,1fr)_minmax(7rem,1fr)_minmax(8rem,1fr)_minmax(8rem,1fr)_minmax(7rem,1fr)_2rem] gap-2 px-2 pb-1 text-xs font-semibold uppercase tracking-wide">
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Category</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Brand Name</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Qty</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Buying Price</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Selling Price</div>
                  <div className={isDark ? 'text-gray-400' : 'text-gray-500'}>Expire Date</div>
                  <div />
                </div>

                {cosmeticItems.map((item) => (
                  <div
                    key={item.id}
                    className={`rounded-lg border p-3 transition-colors ${
                      isDark ? 'bg-gray-800/50 border-gray-700 hover:border-gray-600' : 'bg-white border-gray-200 hover:border-gray-300'
                    } ${item.errors.length > 0 ? isDark ? 'border-red-500/50 bg-red-900/10' : 'border-red-400 bg-red-50/50' : ''}`}
                  >
                    <div className="flex items-center justify-between mb-3 xl:hidden">
                      <span className={`text-sm font-medium ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                        Item #{cosmeticItems.findIndex(i => i.id === item.id) + 1}
                      </span>
                      <button
                        type="button"
                        onClick={() => handleRemoveCosmeticRow(item.id)}
                        className={`text-xs font-medium px-2 py-1 rounded-lg transition-colors ${
                          isDark ? 'text-red-400 hover:bg-red-900/30' : 'text-red-500 hover:bg-red-100'
                        }`}
                      >
                        Remove
                      </button>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-[minmax(8rem,1fr)_minmax(8rem,1fr)_minmax(7rem,1fr)_minmax(8rem,1fr)_minmax(8rem,1fr)_minmax(7rem,1fr)_2rem] gap-3 xl:gap-2 items-end">
                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Category <span className={item.errors.some(e => e.includes('Category')) ? 'text-red-500' : ''}>*</span>
                        </label>
                        <select
                          value={item.categoryId}
                          onChange={(e) => handleCosmeticItemChange(item.id, 'categoryId', e.target.value)}
                          className={`${itemInputClass} ${item.errors.some(e => e.includes('Category')) ? 'border-red-400' : ''}`}
                        >
                          <option value="">Select Category</option>
                          {COSMETIC_CATEGORIES.map((cat, catIdx) => (
                            <option key={cat} value={String(catIdx)}>{cat}</option>
                          ))}
                        </select>
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Brand Name <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          value={item.brandName}
                          onChange={(e) => handleCosmeticItemChange(item.id, 'brandName', e.target.value)}
                          placeholder="Brand name"
                          className={`${itemInputClass} ${item.errors.some(e => e.includes('Brand')) ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Qty <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          value={item.quantity}
                          onChange={(e) => handleCosmeticItemChange(item.id, 'quantity', e.target.value)}
                          placeholder="0"
                          min="0"
                          className={`${itemInputClass} ${item.errors.some(e => e.includes('quantity')) ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Buying Price <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          value={item.buyingPrice}
                          onChange={(e) => handleCosmeticItemChange(item.id, 'buyingPrice', e.target.value)}
                          placeholder="0.00"
                          min="0"
                          step="0.01"
                          className={`${itemInputClass} ${item.errors.some(e => e.includes('buying price')) ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Selling Price <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          value={item.sellingPrice}
                          onChange={(e) => handleCosmeticItemChange(item.id, 'sellingPrice', e.target.value)}
                          placeholder="0.00"
                          min="0"
                          step="0.01"
                          className={`${itemInputClass} ${item.errors.some(e => e.includes('selling price')) ? 'border-red-400' : ''}`}
                        />
                      </div>

                      <div>
                        <label className={`text-xs font-medium mb-1 xl:hidden block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                          Low Stock
                        </label>
                        <input
                          type="number"
                          value={item.lowStock}
                          onChange={(e) => handleCosmeticItemChange(item.id, 'lowStock', e.target.value)}
                          placeholder="10"
                          min="0"
                          className={itemInputClass}
                        />
                      </div>

                      <div className="hidden xl:flex justify-center">
                        <button
                          type="button"
                          onClick={() => handleRemoveCosmeticRow(item.id)}
                          className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-red-400' : 'hover:bg-red-100 text-red-500'}`}
                        >
                          <X className="h-4 w-4" />
                        </button>
                      </div>
                    </div>

                    {/* Expiry Date (optional) */}
                    <div className="mt-3">
                      <label className={`text-xs font-medium mb-1 block ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                        Expire Date <span className={isDark ? 'text-gray-500' : 'text-gray-400'}>(optional)</span>
                      </label>
                      <input
                        type="date"
                        value={item.expiryDate}
                        onChange={(e) => handleCosmeticItemChange(item.id, 'expiryDate', e.target.value)}
                        className={itemInputClass}
                      />
                    </div>

                    {/* Validation errors */}
                    {item.errors.length > 0 && (
                      <div className={`mt-2 p-2 rounded text-xs ${isDark ? 'bg-red-900/20 text-red-400' : 'bg-red-50 text-red-600'}`}>
                        {item.errors.map((err, i) => <div key={i}>• {err}</div>)}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <div className={`text-center py-12 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                <Sparkles className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="text-sm">No cosmetic items added yet</p>
                <p className="text-xs mt-1">Click "Add Item" to start adding cosmetics</p>
              </div>
            )}
          </div>

          {/* Cosmetic Validation Errors Summary */}
          {cosmeticItems.some(i => i.errors.length > 0) && (
            <div className={`p-4 rounded-xl border ${isDark ? 'bg-red-900/20 border-red-800' : 'bg-red-50 border-red-200'}`}>
              <div className="flex items-center gap-2 mb-2">
                <AlertCircle className="h-4 w-4 text-red-500" />
                <p className="text-sm font-medium text-red-500">Validation Errors</p>
              </div>
              <ul className="space-y-1">
                {cosmeticItems.filter(i => i.errors.length > 0).map((item, itemIdx) => (
                  <li key={item.id} className="text-xs text-red-400">
                    Item #{cosmeticItems.findIndex(i => i.id === item.id) + 1}: {item.errors.join(', ')}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Actions */}
          <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <button
              type="button"
              onClick={() => { console.log('[COSMETIC MODAL] Cancel clicked'); setShowCosmeticModal(false); setCosmeticItems([]); }}
              className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={() => { console.log('[COSMETIC MODAL] Save Purchase clicked'); handleSave(); }}
              disabled={isProcessing || ((items || []).length === 0 && cosmeticItems.length === 0) || !selectedSupplier}
              className="px-5 py-2.5 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
            >
              {isProcessing ? (
                <>
                  <div className="h-4 w-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  Processing...
                </>
              ) : (
                <>
                  <ShoppingCart className="h-4 w-4" />
                  Save Purchase ({totalItems} items)
                </>
              )}
            </button>
          </div>
        </div>
      </Modal>

      {/* Success Toast */}
      {showSuccess && (
        <div className="fixed top-4 right-4 z-50 bg-emerald-600 text-white px-6 py-4 rounded-xl shadow-2xl flex items-center gap-3 animate-in slide-in-from-right">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/20">
            <Check className="h-5 w-5" />
          </div>
          <div>
            <p className="font-medium">Bulk Purchase Completed!</p>
            <p className="text-sm text-emerald-100">{lastPurchaseNumber} - {formatCurrency(totalAmount)}</p>
          </div>
        </div>
      )}
    </div>
  );
};

// Calendar is imported at the top
