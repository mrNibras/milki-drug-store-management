import React, { useState, useMemo, useEffect } from 'react';
import { Plus, Search, Eye, ClipboardList, Truck, Package, X, AlertCircle, Check, ShoppingCart, FileSpreadsheet, Calendar } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { formatDate, formatCurrency, generateId, generatePurchaseNumber } from '../utils/helpers';
import { Medicine, MedicineBatch } from '../types';

export const PurchasesPage: React.FC = () => {
  const { purchases, suppliers, medicines, categories, fetchPurchases, fetchSuppliers, fetchMedicines, fetchCategories, addPurchase, addMedicine, updateMedicine, currentUser, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [search, setSearch] = useState('');
  const [showBulkModal, setShowBulkModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedPurchase, setSelectedPurchase] = useState<typeof purchases[0] | null>(null);
  const [selectedSupplier, setSelectedSupplier] = useState('');
  const [purchaseDate, setPurchaseDate] = useState(new Date().toISOString().split('T')[0]);
  const [items, setItems] = useState<BulkPurchaseItem[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [lastPurchaseNumber, setLastPurchaseNumber] = useState('');
  const [paymentStatus, setPaymentStatus] = useState<'paid' | 'partial' | 'unpaid'>('paid');

  useEffect(() => {
    fetchPurchases();
    fetchSuppliers();
    fetchMedicines();
    fetchCategories();
  }, [fetchPurchases, fetchSuppliers, fetchMedicines, fetchCategories]);
  const [paymentMethod, setPaymentMethod] = useState<'cash' | 'bank_transfer' | 'mobile_money' | 'credit'>('cash');
  const [amountPaid, setAmountPaid] = useState('');

  const filteredPurchases = useMemo(() => {
    return purchases.filter(p =>
      p.purchaseNumber.toLowerCase().includes(search.toLowerCase()) ||
      p.supplierName.toLowerCase().includes(search.toLowerCase()) ||
      p.items.some(i => 
        i.medicineName.toLowerCase().includes(search.toLowerCase()) ||
        i.batchNumber.toLowerCase().includes(search.toLowerCase())
      )
    ).sort((a, b) => new Date(b.purchaseDate).getTime() - new Date(a.purchaseDate).getTime());
  }, [purchases, search]);

  const totalAmount = items.reduce((sum, item) => {
    const qty = Number(item.quantity) || 0;
    const price = Number(item.purchasePrice) || 0;
    return sum + (qty * price);
  }, 0);

  const totalItems = items.filter(i => i.medicineName && i.quantity).length;

  const resetForm = () => {
    setSelectedSupplier('');
    setPurchaseDate(new Date().toISOString().split('T')[0]);
    setItems([]);
    setPaymentStatus('paid');
    setPaymentMethod('cash');
    setAmountPaid('');
  };

  const createEmptyItem = (): BulkPurchaseItem => ({
    id: generateId(),
    medicineName: '',
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

  const handleAddRow = () => {
    setItems([...items, createEmptyItem()]);
  };

  const handleRemoveRow = (id: string) => {
    setItems(items.filter(item => item.id !== id));
  };

  const handleItemChange = (id: string, field: keyof BulkPurchaseItem, value: string) => {
    setItems(items.map(item => {
      if (item.id !== id) return item;
      
      const updated = { ...item, [field]: value, errors: [] };
      
      // Auto-fill when medicine is selected from existing
      if (field === 'medicineName') {
        const existingMedicine = medicines.find(m => 
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
        const cat = categories.find(c => c.id === value);
        if (cat) {
          updated.categoryName = cat.name;
        }
      }
      
      return updated;
    }));
  };

  const validateItems = (): boolean => {
    let isValid = true;
    const validatedItems = items.map(item => {
      const errors: string[] = [];
      
      if (!item.medicineName.trim()) errors.push('Medicine name required');
      if (!item.batchNumber.trim()) errors.push('Batch number required');
      if (!item.quantity || Number(item.quantity) <= 0) errors.push('Invalid quantity');
      if (!item.purchasePrice || Number(item.purchasePrice) <= 0) errors.push('Invalid purchase price');
      if (!item.sellingPrice || Number(item.sellingPrice) <= 0) errors.push('Invalid selling price');
      if (!item.expiryDate) errors.push('Expiry date required');
      if (item.isNewMedicine && !item.categoryId) errors.push('Category required for new medicine');
      
      // Check duplicate batch numbers
      const duplicateBatch = items.find(i => 
        i.id !== item.id && 
        i.batchNumber.toLowerCase() === item.batchNumber.toLowerCase() &&
        i.medicineName.toLowerCase() === item.medicineName.toLowerCase()
      );
      if (duplicateBatch) errors.push('Duplicate batch number');
      
      if (errors.length > 0) isValid = false;
      return { ...item, errors };
    });
    
    setItems(validatedItems);
    return isValid;
  };

  const handleSave = async () => {
    if (!selectedSupplier) {
      alert('Please select a supplier');
      return;
    }
    
    if (items.length === 0) {
      alert('Please add at least one item');
      return;
    }
    
    if (!validateItems()) {
      return;
    }
    
    setIsProcessing(true);
    
    // Simulate processing delay
    await new Promise(resolve => setTimeout(resolve, 500));
    
    const supplier = suppliers.find(s => s.id === selectedSupplier);
    if (!supplier) {
      setIsProcessing(false);
      return;
    }
    
    const purchaseNumber = generatePurchaseNumber(purchases.length);
    
    // Process each item - create medicines if needed
    const processedItems = items.map(item => {
      let medicineId = item.existingMedicineId;
      
      // If new medicine, create it
      if (item.isNewMedicine || !medicineId) {
        const newMedicine: Medicine = {
          id: generateId(),
          name: item.medicineName,
          genericName: item.genericName || item.medicineName,
          categoryId: item.categoryId || categories[0]?.id || '',
          categoryName: item.categoryName || categories[0]?.name || 'General',
          unitType: item.unitType,
          lowStockThreshold: 10,
          createdAt: new Date().toISOString(),
          batches: [],
        };
        medicineId = newMedicine.id;
        addMedicine(newMedicine);
      } else {
        // Add batch to existing medicine
        const existingMedicine = medicines.find(m => m.id === medicineId);
        if (existingMedicine) {
          const newBatch: MedicineBatch = {
            id: generateId(),
            medicineId: medicineId,
            batchNumber: item.batchNumber,
            purchasePrice: Number(item.purchasePrice),
            sellingPrice: Number(item.sellingPrice),
            quantity: Number(item.quantity),
            expiryDate: item.expiryDate,
            createdAt: new Date().toISOString(),
          };
          updateMedicine(medicineId, {
            batches: [...existingMedicine.batches, newBatch],
          });
        }
      }
      
      return {
        id: generateId(),
        purchaseId: '',
        medicineId: medicineId!,
        medicineName: item.medicineName,
        batchNumber: item.batchNumber,
        quantity: Number(item.quantity),
        purchasePrice: Number(item.purchasePrice),
        sellingPrice: Number(item.sellingPrice),
        expiryDate: item.expiryDate,
      };
    });
    
    // Create purchase record
    // Calculate payment
    let finalAmountPaid = paymentStatus === 'paid' ? totalAmount : Number(amountPaid) || 0;
    let finalDebt = totalAmount - finalAmountPaid;
    
    // Auto-correct status
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

    const purchase = {
      id: generateId(),
      purchaseNumber,
      supplierId: selectedSupplier,
      supplierName: supplier.name,
      purchaseDate: new Date(purchaseDate).toISOString(),
      totalAmount,
      paymentStatus: finalStatus as 'paid' | 'partial' | 'unpaid',
      paymentMethod: paymentMethod,
      amountPaid: finalAmountPaid,
      remainingDebt: finalDebt,
      items: processedItems.map(item => ({ ...item, purchaseId: '' })),
    };
    purchase.items.forEach(item => item.purchaseId = purchase.id);
    
    addPurchase(purchase);
    
    setLastPurchaseNumber(purchaseNumber);
    setIsProcessing(false);
    setShowBulkModal(false);
    setShowSuccess(true);
    resetForm();
    
    setTimeout(() => setShowSuccess(false), 5000);
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
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{purchases.length}</p>
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
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(purchases.reduce((s, p) => s + p.totalAmount, 0))}</p>
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
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{new Set(purchases.map(p => p.supplierId)).size}</p>
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
            placeholder="Search by purchase number, supplier, medicine name, or batch number..."
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
              {filteredPurchases.map(purchase => (
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
        {filteredPurchases.length === 0 && (
          <div className="text-center py-12">
            <ClipboardList className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
            <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No purchases found</p>
          </div>
        )}
      </div>

      {/* ========== BULK PURCHASE MODAL ========== */}
      <Modal isOpen={showBulkModal} onClose={() => setShowBulkModal(false)} title="Bulk Purchase Entry" size="xl">
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
                  {suppliers.map(s => (
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
                Medicine Items ({items.length})
              </h4>
              <button
                onClick={handleAddRow}
                className="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-medium transition-colors flex items-center gap-1"
              >
                <Plus className="h-3 w-3" /> Add Row
              </button>
            </div>
            
            {items.length > 0 ? (
              <div className="overflow-x-auto max-h-[400px] overflow-y-auto">
                <table className="w-full">
                  <thead className={`sticky top-0 z-10 ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                    <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>#</th>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Medicine Name</th>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Category</th>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Batch #</th>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Qty</th>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Buy Price</th>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Sell Price</th>
                      <th className={`px-3 py-2 text-left text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Expiry</th>
                      <th className={`px-3 py-2 text-center text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Action</th>
                    </tr>
                  </thead>
                  <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                    {items.map((item, index) => (
                      <tr key={item.id} className={`${isDark ? 'hover:bg-gray-700/30' : 'hover:bg-gray-50'} ${item.errors.length > 0 ? isDark ? 'bg-red-900/10' : 'bg-red-50/50' : ''}`}>
                        <td className={`px-3 py-2 text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{index + 1}</td>
                        <td className="px-3 py-2">
                          <div className="relative">
                            <input
                              type="text"
                              value={item.medicineName}
                              onChange={(e) => handleItemChange(item.id, 'medicineName', e.target.value)}
                              placeholder="Medicine name"
                              className={`${smallInputClass} min-w-[140px] sm:min-w-[180px]`}
                              list={`medicines-${item.id}`}
                            />
                            <datalist id={`medicines-${item.id}`}>
                              {medicines.map(m => (
                                <option key={m.id} value={m.name} />
                              ))}
                            </datalist>
                            {item.isNewMedicine && item.medicineName && (
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
                              className={`mt-1 w-full px-2 py-1 border rounded text-xs ${isDark ? 'bg-gray-800 border-gray-600 text-gray-300 placeholder-gray-500' : 'bg-white border-gray-200 text-gray-600 placeholder-gray-400'}`}
                            />
                          )}
                        </td>
                        <td className="px-3 py-2">
                          <select
                            value={item.categoryId}
                            onChange={(e) => handleItemChange(item.id, 'categoryId', e.target.value)}
                            className={`${smallInputClass} ${!item.categoryId && item.isNewMedicine ? 'border-red-400' : ''}`}
                          >
                            <option value="">Category</option>
                            {categories.map(c => (
                              <option key={c.id} value={c.id}>{c.name}</option>
                            ))}
                          </select>
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="text"
                            value={item.batchNumber}
                            onChange={(e) => handleItemChange(item.id, 'batchNumber', e.target.value)}
                            placeholder="P001"
                            className={`${smallInputClass} font-mono ${!item.batchNumber ? 'border-red-400' : ''}`}
                          />
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="number"
                            value={item.quantity}
                            onChange={(e) => handleItemChange(item.id, 'quantity', e.target.value)}
                            placeholder="0"
                            className={`${smallInputClass} ${!item.quantity ? 'border-red-400' : ''}`}
                          />
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="number"
                            value={item.purchasePrice}
                            onChange={(e) => handleItemChange(item.id, 'purchasePrice', e.target.value)}
                            placeholder="0"
                            className={`${smallInputClass} ${!item.purchasePrice ? 'border-red-400' : ''}`}
                          />
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="number"
                            value={item.sellingPrice}
                            onChange={(e) => handleItemChange(item.id, 'sellingPrice', e.target.value)}
                            placeholder="0"
                            className={`${smallInputClass} ${!item.sellingPrice ? 'border-red-400' : ''}`}
                          />
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="date"
                            value={item.expiryDate}
                            onChange={(e) => handleItemChange(item.id, 'expiryDate', e.target.value)}
                            className={`${smallInputClass} ${!item.expiryDate ? 'border-red-400' : ''}`}
                          />
                        </td>
                        <td className="px-3 py-2 text-center">
                          <button
                            onClick={() => handleRemoveRow(item.id)}
                            className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-red-400' : 'hover:bg-red-100 text-red-500'}`}
                          >
                            <X className="h-4 w-4" />
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className={`text-center py-12 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                <Package className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="text-sm">No items added yet</p>
                <p className="text-xs mt-1">Click "Add Row" to start adding medicines</p>
              </div>
            )}
          </div>

          {/* Validation Errors */}
          {items.some(i => i.errors.length > 0) && (
            <div className={`p-4 rounded-xl border ${isDark ? 'bg-red-900/20 border-red-800' : 'bg-red-50 border-red-200'}`}>
              <div className="flex items-center gap-2 mb-2">
                <AlertCircle className="h-4 w-4 text-red-500" />
                <p className="text-sm font-medium text-red-500">Validation Errors</p>
              </div>
              <ul className="space-y-1">
                {items.filter(i => i.errors.length > 0).map((item) => (
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
              onClick={() => setShowBulkModal(false)}
              className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              Cancel
            </button>
            <button
              onClick={handleSave}
              disabled={isProcessing || items.length === 0 || !selectedSupplier}
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
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Medicine</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Batch</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Qty</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Price</th>
                  <th className={`text-left px-3 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Expiry</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {selectedPurchase.items.map(item => (
                  <tr key={item.id}>
                    <td className={`px-3 py-2 font-medium ${isDark ? 'text-white' : ''}`}>{item.medicineName}</td>
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
