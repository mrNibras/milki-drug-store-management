import React, { useState, useMemo, useEffect } from 'react';
import { Search, Package, AlertTriangle, CheckCircle, Layers, Tag } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Badge } from '../components/ui/Badge';
import { formatDate, getDaysUntilExpiry, getExpiryStatus, getStockStatus, getStockColor, formatCurrency } from '../utils/helpers';

export const InventoryPage: React.FC = () => {
  const { medicines, categories, fetchMedicines, fetchCategories, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');

  useEffect(() => {
    fetchMedicines();
    fetchCategories();
  }, [fetchMedicines, fetchCategories]);

  const filteredMedicines = useMemo(() => {
    return (medicines || []).filter(medicine => {
      const matchSearch = medicine.name?.toLowerCase().includes(search.toLowerCase()) ||
        medicine.genericName?.toLowerCase().includes(search.toLowerCase()) ||
        medicine.categoryName?.toLowerCase().includes(search.toLowerCase()) ||
        (medicine.batches || []).some(b => b.batchNumber?.toLowerCase().includes(search.toLowerCase()));
      
      const matchCategory = categoryFilter === 'all' || medicine.categoryId === categoryFilter;
      
      const totalQty = (medicine.batches || []).reduce((sum, b) => sum + (b.quantity || 0), 0);
      let matchStatus = true;
      if (statusFilter === 'expired') {
        matchStatus = (medicine.batches || []).some(b => getDaysUntilExpiry(b.expiryDate) < 0 && b.quantity > 0);
      } else if (statusFilter === 'expiring') {
        matchStatus = (medicine.batches || []).some(b => {
          const days = getDaysUntilExpiry(b.expiryDate);
          return days >= 0 && days <= 180 && b.quantity > 0;
        });
      } else if (statusFilter === 'low') {
        matchStatus = totalQty > 0 && totalQty <= medicine.lowStockThreshold;
      } else if (statusFilter === 'out_of_stock') {
        matchStatus = totalQty === 0;
      }

      return matchSearch && matchCategory && matchStatus;
    }).sort((a, b) => a.name.localeCompare(b.name));
  }, [medicines, search, categoryFilter, statusFilter]);

  const totalItems = (medicines || []).reduce((sum, m) => sum + (m.batches || []).reduce((b, r) => b + (r.quantity || 0), 0), 0);
  const totalInventoryValue = (medicines || []).reduce((sum, m) => sum + (m.batches || []).reduce((b, r) => b + ((r.quantity || 0) * (r.purchasePrice || 0)), 0), 0);
  const totalSellingValue = (medicines || []).reduce((sum, m) => sum + (m.batches || []).reduce((b, r) => b + ((r.quantity || 0) * (r.sellingPrice || 0)), 0), 0);
  const totalBatches = (medicines || []).reduce((sum, m) => sum + (m.batches || []).length, 0);
  const lowStockCount = (medicines || []).filter(m => {
    const qty = (m.batches || []).reduce((s, b) => s + (b.quantity || 0), 0);
    return qty > 0 && qty <= m.lowStockThreshold;
  }).length;

  const getMedicineStock = (m: typeof medicines[0]) => (m.batches || []).reduce((sum, b) => sum + (b.quantity || 0), 0);

  const thClass = `px-5 py-3 text-xs font-semibold uppercase whitespace-nowrap ${isDark ? 'text-gray-400' : 'text-gray-500'}`;

  return (
    <div className="space-y-6">
      <div>
        <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Inventory Management</h1>
        <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Medicines grouped by name with batch details</p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <Package className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Items</p>
              <p className={`text-lg font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{totalItems.toLocaleString()}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-violet-900/30 text-violet-400' : 'bg-violet-100 text-violet-600'}`}>
              <Layers className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Batches</p>
              <p className={`text-lg font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{totalBatches}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600'}`}>
              <CheckCircle className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Inventory Value</p>
              <p className={`text-lg font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(totalInventoryValue)}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-cyan-900/30 text-cyan-400' : 'bg-cyan-100 text-cyan-600'}`}>
              <Tag className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Selling Value</p>
              <p className={`text-lg font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(totalSellingValue)}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-600'}`}>
              <AlertTriangle className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Low Stock</p>
              <p className={`text-lg font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{lowStockCount}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
        <div className="flex flex-col md:flex-row gap-3">
          <div className="relative flex-1">
            <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
            <input
              type="text"
              placeholder="Search by brand name, generic name, batch number, or category..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className={`w-full pl-9 pr-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-sm ${
                isDark ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-400' : 'bg-white border-gray-300 text-gray-900'
              }`}
            />
          </div>
          <select value={categoryFilter} onChange={(e) => setCategoryFilter(e.target.value)}
            className={`px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm ${
              isDark ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-900'
            }`}>
            <option value="all">All Categories</option>
            {(categories || []).map(cat => <option key={cat.id} value={cat.id}>{cat.name}</option>)}
          </select>
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}
            className={`px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm ${
              isDark ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-900'
            }`}>
            <option value="all">All Status</option>
            <option value="low">Low Stock</option>
            <option value="out_of_stock">Out of Stock</option>
            <option value="expiring">Has Expiring</option>
            <option value="expired">Has Expired</option>
          </select>
        </div>
      </div>

      {/* ========== INVENTORY TABLE - ROWSPAN DESIGN ========== */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={`${thClass} text-left`}>Brand Name</th>
                <th className={`${thClass} text-left`}>Category</th>
                <th className={`${thClass} text-left`}>Batch #</th>
                <th className={`${thClass} text-left`}>Expiry Date</th>
                <th className={`${thClass} text-right`}>Purchase Price</th>
                <th className={`${thClass} text-right`}>Selling Price</th>
                <th className={`${thClass} text-center`}>Stock</th>
                <th className={`${thClass} text-right`}>Batch Value</th>
                <th className={`${thClass} text-center`}>Status</th>
              </tr>
            </thead>
            <tbody>
              {filteredMedicines.map(medicine => {
                const totalStock = getMedicineStock(medicine);
                const stockStatus = getStockStatus(totalStock, medicine.lowStockThreshold);
                const sortedBatches = [...medicine.batches].sort(
                  (a, b) => new Date(a.expiryDate).getTime() - new Date(b.expiryDate).getTime()
                );
                const batchCount = sortedBatches.length;

                return sortedBatches.map((batch, batchIndex) => {
                  const isFirstBatch = batchIndex === 0;
                  const isLastBatch = batchIndex === batchCount - 1;
                  const daysLeft = getDaysUntilExpiry(batch.expiryDate);
                  const expiryStatus = getExpiryStatus(batch.expiryDate);
                  const batchStockStatus = getStockStatus(batch.quantity, medicine.lowStockThreshold);
                  const batchValue = batch.quantity * batch.purchasePrice;
                  void stockStatus;

                  return (
                    <tr
                      key={`${medicine.id}-${batch.id}`}
                      className={`
                        ${isDark ? 'hover:bg-gray-700/30' : 'hover:bg-blue-50/30'}
                        ${isLastBatch 
                          ? `border-b-2 ${isDark ? 'border-b-gray-600' : 'border-b-gray-300'}` 
                          : `border-b ${isDark ? 'border-b-gray-700' : 'border-b-gray-100'}`
                        }
                      `}
                    >
                      {/* ===== MEDICINE NAME - ROWSPAN ===== */}
                      {isFirstBatch && (
                        <td
                          rowSpan={batchCount}
                          className={`px-5 py-4 border-r-2 align-top ${isDark ? 'border-r-gray-600 bg-gray-800' : 'border-r-gray-200 bg-white'}`}
                        >
                          <div className="flex items-center gap-3 min-w-[200px]">
                            <div className={`flex h-10 w-10 items-center justify-center rounded-lg flex-shrink-0 ${
                              isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'
                            }`}>
                              <Package className="h-5 w-5" />
                            </div>
                            <div>
                              <p className={`font-bold whitespace-nowrap ${isDark ? 'text-white' : 'text-gray-900'}`}>{medicine.name}</p>
                              <p className={`text-xs whitespace-nowrap ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{medicine.genericName}</p>
                              <p className={`text-xs whitespace-nowrap ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>{medicine.unitType}</p>
                            </div>
                          </div>
                        </td>
                      )}

                      {/* ===== CATEGORY - ROWSPAN ===== */}
                      {isFirstBatch && (
                        <td
                          rowSpan={batchCount}
                          className={`px-5 py-4 border-r align-top ${isDark ? 'border-r-gray-700 bg-gray-800/50' : 'border-r-gray-100 bg-gray-50/50'}`}
                        >
                          <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium whitespace-nowrap ${
                            isDark ? 'bg-violet-900/30 text-violet-400' : 'bg-violet-100 text-violet-600'
                          }`}>
                            <Tag className="h-3 w-3" /> {medicine.categoryName}
                          </span>
                        </td>
                      )}

                      {/* ===== BATCH NUMBER ===== */}
                      <td className="px-5 py-3">
                        <div className="flex items-center gap-2">
                          <div className={`h-2.5 w-2.5 rounded-full flex-shrink-0 ${
                            daysLeft < 0 ? 'bg-red-500' :
                            daysLeft <= 30 ? 'bg-red-400' :
                            daysLeft <= 90 ? 'bg-amber-400' :
                            'bg-green-400'
                          }`} />
                          <span className={`font-mono text-sm font-bold whitespace-nowrap ${isDark ? 'text-white' : 'text-gray-900'}`}>
                            {batch.batchNumber}
                          </span>
                        </div>
                      </td>

                      {/* ===== EXPIRY DATE ===== */}
                      <td className="px-5 py-3">
                        <p className={`text-sm whitespace-nowrap ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{formatDate(batch.expiryDate)}</p>
                        <span className={`text-xs whitespace-nowrap ${
                          daysLeft < 0 ? 'text-red-500' :
                          daysLeft <= 30 ? 'text-red-400' :
                          daysLeft <= 90 ? 'text-amber-400' :
                          'text-green-400'
                        }`}>
                          {daysLeft < 0 ? `Expired ${Math.abs(daysLeft)}d ago` : `${daysLeft} days left`}
                        </span>
                      </td>

                      {/* ===== PURCHASE PRICE ===== */}
                      <td className={`px-5 py-3 text-right whitespace-nowrap ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                        <span className="text-sm">{batch.purchasePrice.toLocaleString()}</span>
                        <span className={`text-xs ml-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>ETB</span>
                      </td>

                      {/* ===== SELLING PRICE ===== */}
                      <td className={`px-5 py-3 text-right whitespace-nowrap ${isDark ? 'text-emerald-400' : 'text-emerald-600'}`}>
                        <span className="text-sm font-semibold">{batch.sellingPrice.toLocaleString()}</span>
                        <span className={`text-xs ml-1 font-normal ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>ETB</span>
                      </td>

                      {/* ===== STOCK ===== */}
                      <td className="px-5 py-3 text-center">
                        <span className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-bold whitespace-nowrap ${getStockColor(batchStockStatus)}`}>
                          {batch.quantity} {medicine.unitType}{batch.quantity !== 1 ? 's' : ''}
                        </span>
                      </td>

                      {/* ===== BATCH VALUE ===== */}
                      <td className={`px-5 py-3 text-right whitespace-nowrap ${isDark ? 'text-white' : 'text-gray-900'}`}>
                        <span className="text-sm font-medium">{batchValue.toLocaleString()}</span>
                        <span className={`text-xs ml-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>ETB</span>
                      </td>

                      {/* ===== STATUS ===== */}
                      <td className="px-5 py-3 text-center">
                        {batch.quantity === 0 ? (
                          <Badge variant="danger">Empty</Badge>
                        ) : expiryStatus === 'expired' ? (
                          <Badge variant="danger">Expired</Badge>
                        ) : expiryStatus === 'critical' ? (
                          <Badge variant="danger">Critical</Badge>
                        ) : expiryStatus === 'warning' ? (
                          <Badge variant="warning">Expiring</Badge>
                        ) : batchStockStatus === 'low' ? (
                          <Badge variant="warning">Low</Badge>
                        ) : (
                          <Badge variant="success">OK</Badge>
                        )}
                      </td>
                    </tr>
                  );
                });
              })}
            </tbody>
          </table>
        </div>

        {filteredMedicines.length === 0 && (
          <div className="text-center py-16">
            <Package className={`h-16 w-16 mx-auto mb-4 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
            <p className={`text-lg font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>No medicines found</p>
            <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>Try adjusting your search or filters</p>
          </div>
        )}
      </div>
    </div>
  );
};
