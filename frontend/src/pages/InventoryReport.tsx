import React, { useMemo, useEffect } from 'react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency } from '../utils/helpers';

const InventoryReport: React.FC = () => {
  const { medicines, fetchMedicines, error, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    if (!medicines || !medicines.length) fetchMedicines();
  }, [fetchMedicines, medicines]);

  const inventoryData = useMemo(() => {
    return (medicines || []).map(m => {
      const totalQuantity = m.batches.reduce((s, b) => s + b.quantity, 0);
      const totalValue = m.batches.reduce((s, b) => s + (b.quantity * b.purchasePrice), 0);
      return {
        name: m.name,
        category: m.categoryName,
        quantity: totalQuantity,
        value: totalValue,
        status: totalQuantity === 0 ? 'Out of Stock' : totalQuantity <= m.lowStockThreshold ? 'Low Stock' : 'In Stock',
      };
    }).sort((a, b) => a.quantity - b.quantity);
  }, [medicines]);

  const totalInventoryValue = useMemo(() => inventoryData.reduce((s, i) => s + i.value, 0), [inventoryData]);
  const outOfStockItems = useMemo(() => inventoryData.filter(i => i.status === 'Out of Stock').length, [inventoryData]);
  const lowStockItems = useMemo(() => inventoryData.filter(i => i.status === 'Low Stock').length, [inventoryData]);

  if (loading.medicines) {
    return <div className="text-center py-10">Loading inventory data...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-500">{error}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Inventory Value</p>
          <p className={`text-2xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(totalInventoryValue)}</p>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Low Stock Items</p>
          <p className="text-2xl font-bold text-amber-600 mt-1">{lowStockItems}</p>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Out of Stock</p>
          <p className="text-2xl font-bold text-red-600 mt-1">{outOfStockItems}</p>
        </div>
      </div>
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <table className="w-full">
          <thead>
            <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
              <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Brand Name</th>
              <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Category</th>
              <th className={`text-right px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Stock</th>
              <th className={`text-right px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Value</th>
              <th className={`text-center px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
            </tr>
          </thead>
          <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
            {inventoryData.map((item, i) => (
              <tr key={i} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                <td className={`px-6 py-3 font-medium text-sm ${isDark ? 'text-white' : ''}`}>{item.name}</td>
                <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{item.category}</td>
                <td className={`px-6 py-3 text-sm font-medium text-right ${isDark ? 'text-white' : ''}`}>{item.quantity}</td>
                <td className={`px-6 py-3 text-sm text-right ${isDark ? 'text-gray-300' : ''}`}>{formatCurrency(item.value)}</td>
                <td className="px-6 py-3 text-center">
                  <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${item.status === 'Out of Stock' ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400' : item.status === 'Low Stock' ? 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400' : 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'}`}>{item.status}</span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default InventoryReport;