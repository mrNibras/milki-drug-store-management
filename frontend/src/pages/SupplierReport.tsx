import React, { useMemo, useEffect } from 'react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency } from '../utils/helpers';
import { ResponsiveTable } from '../components/ui/ResponsiveTable';

const SupplierReport: React.FC = () => {
  const { suppliers, purchases, fetchSuppliers, fetchPurchases, error, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    if (!suppliers || !suppliers.length) fetchSuppliers();
    if (!purchases || !purchases.length) fetchPurchases();
  }, [fetchSuppliers, fetchPurchases, suppliers, purchases]);

  const supplierData = useMemo(() => {
    return (suppliers || []).map(s => {
      const supplierPurchases = (purchases || []).filter(p => p.supplierId === s.id);
      const totalAmount = supplierPurchases.reduce((sum, p) => sum + p.totalAmount, 0);
      const totalPaid = supplierPurchases.reduce((sum, p) => sum + p.amountPaid, 0);
      const totalDebt = supplierPurchases.reduce((sum, p) => sum + p.remainingDebt, 0);
      return {
        id: s.id,
        name: s.name,
        purchases: supplierPurchases.length,
        totalAmount,
        totalPaid,
        totalDebt,
        paymentStatus: totalDebt > 0 ? 'outstanding' : 'cleared',
      };
    }).sort((a, b) => b.totalAmount - a.totalAmount);
  }, [suppliers, purchases]);

  if (loading.suppliers || loading.purchases) {
    return <div className="text-center py-10">Loading supplier data...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-500">{error}</div>;
  }

  return (
    <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden min-w-0`}>
      <div className={`px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
        <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Financial data calculated from purchase records</p>
      </div>
      <ResponsiveTable>
        <table className="w-full">
        <thead>
          <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
            <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Supplier</th>
            <th className={`text-center px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Purchases</th>
            <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Spent</th>
            <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Paid</th>
            <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Debt</th>
            <th className={`text-center px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
          </tr>
        </thead>
        <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
          {supplierData.map(item => (
            <tr key={item.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
              <td className={`px-3 sm:px-6 py-3 font-medium text-sm ${isDark ? 'text-white' : ''}`}>{item.name}</td>
              <td className={`px-3 sm:px-6 py-3 text-sm text-center ${isDark ? 'text-gray-300' : ''}`}>{item.purchases}</td>
              <td className={`px-3 sm:px-6 py-3 text-sm font-medium text-right ${isDark ? 'text-white' : ''}`}>{formatCurrency(item.totalAmount)}</td>
              <td className={`px-3 sm:px-6 py-3 text-sm text-right ${isDark ? 'text-green-400' : 'text-green-600'}`}>{formatCurrency(item.totalPaid)}</td>
              <td className={`px-3 sm:px-6 py-3 text-sm text-right font-semibold ${item.totalDebt > 0 ? 'text-red-500' : isDark ? 'text-gray-400' : 'text-gray-500'}`}>{item.totalDebt > 0 ? formatCurrency(item.totalDebt) : '-'}</td>
              <td className="px-3 sm:px-6 py-3 text-center"><span className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${item.paymentStatus === 'cleared' ? (isDark ? 'bg-green-900/30 text-green-400' : 'bg-green-100 text-green-700') : (isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-700')}`}>{item.paymentStatus === 'cleared' ? '✅ Cleared' : '⚠️ Outstanding'}</span></td>
            </tr>
          ))}
        </tbody>
        </table>
      </ResponsiveTable>
    </div>
  );
};

export default SupplierReport;