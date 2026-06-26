import React, { useState, useMemo } from 'react';
import { Search, ShoppingCart, Trash2, Plus, Minus, CreditCard, X, Check, Package, Tag, AlertCircle } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency, getFefoBatches, generateId, generateSaleNumber } from '../utils/helpers';
import { Medicine } from '../types';

const DISCOUNT_REASONS = [
  'Family Assistance',
  'Employee Discount',
  'Charity Case',
  'Emergency Assistance',
  'Promotional Discount',
  'Senior Citizen',
  'Bulk Purchase',
  'Other',
];

const MAX_PHARMACIST_DISCOUNT_PERCENT = 5;
const MAX_ADMIN_DISCOUNT_PERCENT = 100;

export const POSPage: React.FC = () => {
  const { 
    medicines, cart, addToCart, removeFromCart, updateCartItemQuantity, 
    updateCartItemDiscount, clearCart, addSale, sales, currentUser, addAuditLog,
    cartDiscountReason, setCartDiscountReason
  } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [search, setSearch] = useState('');
  const [showCheckout, setShowCheckout] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [lastSaleNumber, setLastSaleNumber] = useState('');
  const [showDiscountInput, setShowDiscountInput] = useState<string | null>(null);
  const [tempDiscount, setTempDiscount] = useState('');
  const [showDiscountReason, setShowDiscountReason] = useState(false);

  const isAdmin = currentUser?.role === 'admin';
  const maxDiscountPercent = isAdmin ? MAX_ADMIN_DISCOUNT_PERCENT : MAX_PHARMACIST_DISCOUNT_PERCENT;

  const searchResults = useMemo(() => {
    if (!search.trim()) return [];
    return medicines.filter(m => {
      const totalQty = m.batches.reduce((sum, b) => sum + b.quantity, 0);
      const matchesBatch = m.batches.some(b => b.batchNumber.toLowerCase().includes(search.toLowerCase()));
      return totalQty > 0 && (
        m.name.toLowerCase().includes(search.toLowerCase()) ||
        m.genericName.toLowerCase().includes(search.toLowerCase()) ||
        m.categoryName.toLowerCase().includes(search.toLowerCase()) ||
        matchesBatch
      );
    }).slice(0, 8);
  }, [medicines, search]);

  const cartSubtotal = cart.reduce((sum, item) => sum + (item.standardPrice * item.quantity), 0);
  const cartTotalDiscount = cart.reduce((sum, item) => sum + (item.discountAmount * item.quantity), 0);
  const cartTotal = cart.reduce((sum, item) => sum + (item.sellingPrice * item.quantity), 0);
  const cartProfit = cart.reduce((sum, item) => {
    const medicine = medicines.find(m => m.id === item.medicineId);
    const batch = medicine?.batches.find(b => b.id === item.batchId);
    return sum + ((item.sellingPrice - (batch?.purchasePrice || 0)) * item.quantity);
  }, 0);

  const handleAddToCart = (medicine: Medicine) => {
    const availableBatches = getFefoBatches(medicine);
    if (availableBatches.length === 0) return;
    
    const earliestBatch = availableBatches[0];
    addToCart({
      medicineId: medicine.id,
      medicineName: medicine.name,
      batchId: earliestBatch.id,
      batchNumber: earliestBatch.batchNumber,
      quantity: 1,
      unitPrice: earliestBatch.sellingPrice,
      sellingPrice: earliestBatch.sellingPrice,
      standardPrice: earliestBatch.sellingPrice,
      discountAmount: 0,
      expiryDate: earliestBatch.expiryDate,
      availableQuantity: earliestBatch.quantity,
    });
    setSearch('');
  };

  const handleApplyDiscount = (medicineId: string, batchId: string) => {
    const discount = Number(tempDiscount) || 0;
    const item = cart.find(c => c.medicineId === medicineId && c.batchId === batchId);
    if (!item) return;

    const maxDiscount = (item.standardPrice * maxDiscountPercent) / 100;
    const clampedDiscount = Math.min(discount, maxDiscount);
    
    updateCartItemDiscount(medicineId, batchId, clampedDiscount);
    setShowDiscountInput(null);
    setTempDiscount('');

    if (clampedDiscount > 0 && !cartDiscountReason) {
      setShowDiscountReason(true);
    }
  };

  const handleCheckout = () => {
    if (cart.length === 0) return;

    // Check if discount requires reason
    if (cartTotalDiscount > 0 && !cartDiscountReason) {
      setShowDiscountReason(true);
      return;
    }

    const saleNumber = generateSaleNumber(sales.length);
    const sale = {
      id: generateId(),
      saleNumber,
      saleDate: new Date().toISOString(),
      totalAmount: cartTotal,
      totalDiscount: cartTotalDiscount,
      discountReason: cartDiscountReason,
      approvedBy: null,
      profit: cartProfit,
      userId: currentUser?.id || '',
      userName: currentUser?.fullName || '',
      items: cart.map(item => ({
        id: generateId(),
        saleId: '',
        medicineId: item.medicineId,
        medicineName: item.medicineName,
        batchId: item.batchId,
        quantity: item.quantity,
        unitPrice: item.sellingPrice,
        standardUnitPrice: item.standardPrice,
        actualUnitPrice: item.sellingPrice,
        discountAmount: item.discountAmount,
        totalPrice: item.sellingPrice * item.quantity,
      })),
    };
    sale.items.forEach(item => item.saleId = sale.id);

    addSale(sale);
    addAuditLog({
      id: generateId(),
      userId: currentUser?.id || '',
      userName: currentUser?.fullName || '',
      action: `Created Sale ${saleNumber} - Total: ${formatCurrency(cartTotal)}${cartTotalDiscount > 0 ? ` - Discount: ${formatCurrency(cartTotalDiscount)} (${cartDiscountReason})` : ''}`,
      tableName: 'Sales',
      recordId: sale.id,
      createdAt: new Date().toISOString(),
    });

    setLastSaleNumber(saleNumber);
    clearCart();
    setShowCheckout(false);
    setShowDiscountReason(false);
    setShowSuccess(true);
    setTimeout(() => setShowSuccess(false), 3000);
  };

  return (
    <div className="flex flex-col lg:flex-row gap-4 lg:gap-6 h-auto lg:h-[calc(100vh-7rem)]">
      {/* Left Side - Product Search & Results */}
      <div className="flex-1 flex flex-col min-h-0">
        <div className="mb-4">
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Point of Sale</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} text-sm`}>Search and add medicines to cart</p>
        </div>

        {/* Search Bar */}
        <div className="relative mb-4">
          <Search className={`absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
          <input
            type="text"
            placeholder="Search by medicine name, generic name, batch number, or category..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className={`w-full pl-12 pr-4 py-4 border-2 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 text-base transition-all ${
              isDark ? 'bg-gray-800 border-gray-700 text-white placeholder-gray-500' : 'bg-white border-gray-200 text-gray-900 placeholder-gray-400'
            }`}
            autoFocus
          />
        </div>

        {/* Search Results */}
        {searchResults.length > 0 && (
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'} rounded-xl border shadow-lg mb-4 max-h-64 overflow-y-auto`}>
            {searchResults.map(medicine => {
              const totalQty = medicine.batches.reduce((sum, b) => sum + b.quantity, 0);
              const fefoBatch = getFefoBatches(medicine)[0];
              return (
                <button
                  key={medicine.id}
                  onClick={() => handleAddToCart(medicine)}
                  className={`w-full flex items-center justify-between px-4 py-3 transition-colors border-b last:border-0 ${
                    isDark ? 'hover:bg-emerald-900/20 border-gray-700' : 'hover:bg-emerald-50 border-gray-100'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                      <Package className="h-5 w-5" />
                    </div>
                    <div className="text-left">
                      <p className={`font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{medicine.name}</p>
                      <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{medicine.genericName} • {medicine.categoryName}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold text-emerald-500">{fefoBatch?.sellingPrice.toLocaleString()} ETB</p>
                    <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{totalQty} in stock</p>
                  </div>
                </button>
              );
            })}
          </div>
        )}

        {/* Quick Access Grid */}
        {!search && (
          <div className="flex-1 overflow-y-auto">
            <h3 className={`text-sm font-semibold mb-3 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Quick Access - Available Medicines</h3>
            <div className="grid grid-cols-2 lg:grid-cols-3 gap-3">
              {medicines.filter(m => m.batches.some(b => b.quantity > 0)).slice(0, 12).map(medicine => {
                const totalQty = medicine.batches.reduce((sum, b) => sum + b.quantity, 0);
                const fefoBatch = getFefoBatches(medicine)[0];
                return (
                  <button
                    key={medicine.id}
                    onClick={() => handleAddToCart(medicine)}
                    className={`rounded-xl border p-4 hover:border-emerald-300 hover:shadow-md transition-all text-left ${
                      isDark ? 'bg-gray-800 border-gray-700 hover:bg-gray-700' : 'bg-white border-gray-200'
                    }`}
                  >
                    <div className="flex items-center gap-2 mb-2">
                      <div className={`flex h-8 w-8 items-center justify-center rounded-lg ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600'}`}>
                        <Package className="h-4 w-4" />
                      </div>
                      <span className={`text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{medicine.categoryName}</span>
                    </div>
                    <p className={`font-medium text-sm truncate ${isDark ? 'text-white' : 'text-gray-900'}`}>{medicine.name}</p>
                    <p className={`text-xs mb-2 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{medicine.genericName}</p>
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-bold text-emerald-500">{fefoBatch?.sellingPrice.toLocaleString()} ETB</span>
                      <span className={`text-xs ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>{totalQty} left</span>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>
        )}
      </div>

      {/* Right Side - Cart */}
      <div className={`w-full lg:w-96 flex flex-col rounded-xl border shadow-sm ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
        {/* Cart Header */}
        <div className={`px-5 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <ShoppingCart className="h-5 w-5 text-emerald-500" />
              <h2 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Cart</h2>
            </div>
            <span className={`text-xs font-medium px-2 py-1 rounded-full ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-700'}`}>
              {cart.length} items
            </span>
          </div>
        </div>

        {/* Cart Items */}
        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {cart.length === 0 ? (
            <div className="text-center py-12">
              <ShoppingCart className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
              <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>Cart is empty</p>
              <p className={`text-xs mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>Search and add medicines</p>
            </div>
          ) : (
            cart.map((item) => (
              <div key={`${item.medicineId}-${item.batchId}`} className={`rounded-lg p-3 ${isDark ? 'bg-gray-700' : 'bg-gray-50'}`}>
                <div className="flex items-start justify-between mb-2">
                  <div>
                    <p className={`font-medium text-sm ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.medicineName}</p>
                    <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Batch: {item.batchNumber}</p>
                  </div>
                  <button
                    onClick={() => removeFromCart(item.medicineId, item.batchId)}
                    className={`p-1 rounded transition-colors ${isDark ? 'hover:bg-red-900/30 text-gray-400 hover:text-red-400' : 'hover:bg-red-100 text-gray-400 hover:text-red-500'}`}
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
                
                {/* Price Display */}
                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => updateCartItemQuantity(item.medicineId, item.batchId, item.quantity - 1)}
                      className={`h-7 w-7 flex items-center justify-center rounded-md transition-colors ${isDark ? 'bg-gray-600 hover:bg-gray-500 text-white' : 'bg-white border border-gray-300 hover:bg-gray-100'}`}
                    >
                      <Minus className="h-3 w-3" />
                    </button>
                    <span className={`w-8 text-center text-sm font-medium ${isDark ? 'text-white' : ''}`}>{item.quantity}</span>
                    <button
                      onClick={() => updateCartItemQuantity(item.medicineId, item.batchId, item.quantity + 1)}
                      className={`h-7 w-7 flex items-center justify-center rounded-md transition-colors ${isDark ? 'bg-gray-600 hover:bg-gray-500 text-white' : 'bg-white border border-gray-300 hover:bg-gray-100'}`}
                    >
                      <Plus className="h-3 w-3" />
                    </button>
                  </div>
                  <div className="text-right">
                    {item.discountAmount > 0 ? (
                      <>
                        <p className={`text-xs line-through ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>{item.standardPrice.toLocaleString()} ETB</p>
                        <p className={`font-semibold text-emerald-500`}>{item.sellingPrice.toLocaleString()} ETB</p>
                      </>
                    ) : (
                      <span className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.sellingPrice.toLocaleString()} ETB</span>
                    )}
                  </div>
                </div>

                {/* Discount Section */}
                {showDiscountInput === `${item.medicineId}-${item.batchId}` ? (
                  <div className={`flex items-center gap-2 p-2 rounded-lg ${isDark ? 'bg-gray-600' : 'bg-white'}`}>
                    <input
                      type="number"
                      value={tempDiscount}
                      onChange={(e) => setTempDiscount(e.target.value)}
                      placeholder="Discount amount"
                      className={`flex-1 px-2 py-1 border rounded text-sm ${isDark ? 'bg-gray-700 border-gray-500 text-white' : 'bg-white border-gray-300'}`}
                      autoFocus
                    />
                    <button
                      onClick={() => handleApplyDiscount(item.medicineId, item.batchId)}
                      className="px-2 py-1 bg-emerald-600 text-white rounded text-xs"
                    >
                      Apply
                    </button>
                    <button
                      onClick={() => { setShowDiscountInput(null); setTempDiscount(''); }}
                      className={`px-2 py-1 rounded text-xs ${isDark ? 'bg-gray-500 text-white' : 'bg-gray-200'}`}
                    >
                      Cancel
                    </button>
                  </div>
                ) : (
                  <button
                    onClick={() => {
                      setShowDiscountInput(`${item.medicineId}-${item.batchId}`);
                      setTempDiscount(String(item.discountAmount || ''));
                    }}
                    className={`w-full flex items-center justify-center gap-1 py-1.5 rounded text-xs transition-colors ${
                      item.discountAmount > 0
                        ? isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-50 text-emerald-600'
                        : isDark ? 'bg-gray-600 text-gray-300 hover:bg-gray-500' : 'bg-white text-gray-500 hover:bg-gray-100'
                    }`}
                  >
                    <Tag className="h-3 w-3" />
                    {item.discountAmount > 0 ? `Discount: -${item.discountAmount.toLocaleString()} ETB` : 'Add Discount'}
                  </button>
                )}
              </div>
            ))
          )}
        </div>

        {/* Cart Summary */}
        <div className={`border-t p-5 space-y-2 ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
          {cartTotalDiscount > 0 && (
            <>
              <div className="flex items-center justify-between text-sm">
                <span className={isDark ? 'text-gray-400' : 'text-gray-500'}>Subtotal</span>
                <span className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{formatCurrency(cartSubtotal)}</span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="text-red-500 flex items-center gap-1">
                  <Tag className="h-3 w-3" /> Discount
                </span>
                <span className="font-medium text-red-500">-{formatCurrency(cartTotalDiscount)}</span>
              </div>
              {cartDiscountReason && (
                <div className={`text-xs px-2 py-1 rounded ${isDark ? 'bg-gray-700 text-gray-400' : 'bg-gray-100 text-gray-500'}`}>
                  Reason: {cartDiscountReason}
                </div>
              )}
            </>
          )}
          <div className="flex items-center justify-between text-sm">
            <span className={isDark ? 'text-gray-400' : 'text-gray-500'}>Estimated Profit</span>
            <span className="font-medium text-emerald-500">{formatCurrency(cartProfit)}</span>
          </div>
          <div className={`flex items-center justify-between pt-2 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <span className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Total</span>
            <span className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(cartTotal)}</span>
          </div>
          
          {cartTotalDiscount > 0 && !cartDiscountReason && (
            <button
              onClick={() => setShowDiscountReason(true)}
              className="w-full py-2 bg-amber-600 hover:bg-amber-700 text-white rounded-xl text-sm font-medium transition-colors flex items-center justify-center gap-2"
            >
              <AlertCircle className="h-4 w-4" /> Add Discount Reason
            </button>
          )}
          
          <button
            onClick={() => setShowCheckout(true)}
            disabled={cart.length === 0}
            className="w-full py-3 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white rounded-xl font-medium transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            <CreditCard className="h-5 w-5" /> Complete Sale
          </button>
          
          {cart.length > 0 && (
            <button onClick={clearCart} className={`w-full py-2 text-sm rounded-lg transition-colors ${isDark ? 'text-red-400 hover:bg-red-900/20' : 'text-red-600 hover:bg-red-50'}`}>
              Clear Cart
            </button>
          )}
        </div>
      </div>

      {/* Discount Reason Modal */}
      {showDiscountReason && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className={`rounded-2xl shadow-2xl w-full max-w-md p-6 ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
            <div className="flex items-center justify-between mb-4">
              <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Discount Reason</h3>
              <button onClick={() => setShowDiscountReason(false)} className={`p-1 rounded-lg ${isDark ? 'hover:bg-gray-700' : 'hover:bg-gray-100'}`}>
                <X className={`h-5 w-5 ${isDark ? 'text-gray-400' : 'text-gray-500'}`} />
              </button>
            </div>
            
            <div className={`p-4 rounded-xl mb-4 ${isDark ? 'bg-amber-900/20' : 'bg-amber-50'}`}>
              <p className={`text-sm ${isDark ? 'text-amber-300' : 'text-amber-700'}`}>
                ⚠️ A discount of <strong>{formatCurrency(cartTotalDiscount)}</strong> has been applied. Please select a reason.
              </p>
            </div>

            <div className="space-y-2 mb-4">
              {DISCOUNT_REASONS.map(reason => (
                <button
                  key={reason}
                  onClick={() => {
                    setCartDiscountReason(reason);
                    setShowDiscountReason(false);
                  }}
                  className={`w-full text-left px-4 py-3 rounded-xl text-sm transition-all ${
                    cartDiscountReason === reason
                      ? 'bg-emerald-600 text-white'
                      : isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-50 text-gray-700 hover:bg-gray-100'
                  }`}
                >
                  {reason}
                </button>
              ))}
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setShowDiscountReason(false)}
                className={`flex-1 py-3 rounded-xl font-medium transition-colors ${isDark ? 'border border-gray-600 text-gray-300 hover:bg-gray-700' : 'border border-gray-300 text-gray-700 hover:bg-gray-50'}`}
              >
                Cancel
              </button>
              {cartDiscountReason && (
                <button
                  onClick={() => setShowCheckout(true)}
                  className="flex-1 py-3 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-medium transition-all flex items-center justify-center gap-2"
                >
                  <Check className="h-5 w-5" /> Confirm
                </button>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Checkout Modal */}
      {showCheckout && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className={`rounded-2xl shadow-2xl w-full max-w-md p-6 ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
            <div className="flex items-center justify-between mb-6">
              <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Confirm Sale</h3>
              <button onClick={() => setShowCheckout(false)} className={`p-1 rounded-lg ${isDark ? 'hover:bg-gray-700' : 'hover:bg-gray-100'}`}>
                <X className={`h-5 w-5 ${isDark ? 'text-gray-400' : 'text-gray-500'}`} />
              </button>
            </div>
            <div className="space-y-3 mb-6">
              {cart.map(item => (
                <div key={`${item.medicineId}-${item.batchId}`} className={`flex items-center justify-between text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>
                  <span>{item.medicineName} × {item.quantity}</span>
                  <div className="text-right">
                    {item.discountAmount > 0 && (
                      <span className={`text-xs line-through mr-2 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                        {(item.standardPrice * item.quantity).toLocaleString()} ETB
                      </span>
                    )}
                    <span className="font-medium">{(item.sellingPrice * item.quantity).toLocaleString()} ETB</span>
                  </div>
                </div>
              ))}
              {cartTotalDiscount > 0 && (
                <div className={`flex items-center justify-between text-sm pt-2 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
                  <span className="text-red-500">Discount</span>
                  <span className="font-medium text-red-500">-{formatCurrency(cartTotalDiscount)}</span>
                </div>
              )}
              {cartDiscountReason && (
                <div className={`text-xs px-3 py-2 rounded-lg ${isDark ? 'bg-gray-700 text-gray-400' : 'bg-gray-100 text-gray-500'}`}>
                  Reason: {cartDiscountReason}
                </div>
              )}
              <div className={`border-t pt-3 flex items-center justify-between ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
                <span className={`font-semibold ${isDark ? 'text-white' : ''}`}>Total Amount</span>
                <span className="text-xl font-bold text-emerald-500">{formatCurrency(cartTotal)}</span>
              </div>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => setShowCheckout(false)}
                className={`flex-1 py-3 rounded-xl font-medium transition-colors ${isDark ? 'border border-gray-600 text-gray-300 hover:bg-gray-700' : 'border border-gray-300 text-gray-700 hover:bg-gray-50'}`}
              >
                Cancel
              </button>
              <button
                onClick={handleCheckout}
                className="flex-1 py-3 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white rounded-xl font-medium transition-all shadow-lg flex items-center justify-center gap-2"
              >
                <Check className="h-5 w-5" /> Confirm
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Success Toast */}
      {showSuccess && (
        <div className="fixed top-4 right-4 z-50 bg-emerald-600 text-white px-6 py-4 rounded-xl shadow-2xl flex items-center gap-3 animate-in slide-in-from-right">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/20">
            <Check className="h-5 w-5" />
          </div>
          <div>
            <p className="font-medium">Sale Completed!</p>
            <p className="text-sm text-emerald-100">{lastSaleNumber} - {formatCurrency(cartTotal)}</p>
          </div>
        </div>
      )}
    </div>
  );
};
