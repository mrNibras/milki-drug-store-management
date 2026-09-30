import React, { useMemo, useEffect } from 'react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency } from '../utils/helpers';
import { ResponsiveTable } from '../components/ui/ResponsiveTable';

const StaffReport: React.FC = () => {
  const { users, sales, fetchUsers, fetchSales, loading, error } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    if (!users || !users.length) fetchUsers();
    if (!sales || !sales.length) fetchSales();
  }, [fetchUsers, fetchSales, users, sales]);

  const staffData = useMemo(() => {
    return (users || []).filter(u => u.isActive).map(u => {
      const userSales = (sales || []).filter(s => s.userId === u.id);
      const totalAmount = userSales.reduce((sum, s) => sum + s.totalAmount, 0);
      const totalProfitAmount = userSales.reduce((sum, s) => sum + s.profit, 0);
      return {
        id: u.id,
        name: u.fullName,
        role: u.role,
        salesCount: userSales.length,
        totalAmount,
        profit: totalProfitAmount,
      };
    }).sort((a, b) => b.totalAmount - a.totalAmount);
  }, [users, sales]);

  if (loading.users || loading.sales) {
    return <div className="text-center py-10">Loading staff data...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-500">{error}</div>;
  }

  return (
    <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden min-w-0`}>
      <ResponsiveTable>
        <table className="w-full">
        <thead>
          <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
            <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Staff</th>
            <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Role</th>
            <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Sales Count</th>
            <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Revenue</th>
            <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Profit</th>
          </tr>
        </thead>
        <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
          {staffData.map(item => (
            <tr key={item.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
              <td className={`px-3 sm:px-6 py-3 font-medium text-sm ${isDark ? 'text-white' : ''}`}>{item.name}</td>
              <td className={`px-3 sm:px-6 py-3 text-sm capitalize ${isDark ? 'text-gray-300' : ''}`}>{item.role}</td>
              <td className={`px-3 sm:px-6 py-3 text-sm text-right ${isDark ? 'text-gray-300' : ''}`}>{item.salesCount}</td>
              <td className={`px-3 sm:px-6 py-3 text-sm font-medium text-right ${isDark ? 'text-white' : ''}`}>{formatCurrency(item.totalAmount)}</td>
              <td className="px-3 sm:px-6 py-3 text-sm font-medium text-right text-emerald-500">{formatCurrency(item.profit)}</td>
            </tr>
          ))}
        </tbody>
        </table>
      </ResponsiveTable>
    </div>
  );
};

export default StaffReport;