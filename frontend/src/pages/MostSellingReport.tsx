import React, { useMemo, useEffect } from 'react';
import { Award } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency } from '../utils/helpers';
import { SaleItem } from '../types';
import { ResponsiveTable } from '../components/ui/ResponsiveTable';

const getItemKey = (item: SaleItem): string => {
  if (item.productType === 'cosmetic') {
    return `cosmetic-${item.cosmeticId || item.id}`;
  }
  return `medicine-${item.medicineId || item.id}`;
};

const MostSellingReport: React.FC = () => {
  const { sales, fetchSales, loading, error } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    if (!sales || !sales.length) fetchSales();
  }, [fetchSales, sales]);

  const mostSellingByMonth = useMemo(() => {
    const months: Record<string, Record<string, { name: string; quantity: number; revenue: number }>> = {};
    (sales || []).forEach(sale => {
      const month = sale.saleDate.slice(0, 7);
      if (!months[month]) months[month] = {};
      sale.items.forEach(item => {
        const key = getItemKey(item);
        if (!months[month][key]) {
          months[month][key] = { name: item.brandName, quantity: 0, revenue: 0 };
        }
        months[month][key].quantity += item.quantity;
        months[month][key].revenue += item.totalPrice;
      });
    });
    const result: Record<string, { name: string; quantity: number; revenue: number }[]> = {};
    Object.entries(months).sort(([a], [b]) => b.localeCompare(a)).forEach(([month, items]) => {
      result[month] = Object.values(items).sort((a, b) => b.quantity - a.quantity).slice(0, 5);
    });
    return result;
  }, [sales]);

  const overallMostSelling = useMemo(() => {
    const itemSales: Record<string, { name: string; quantity: number; revenue: number }> = {};
    (sales || []).forEach(sale => {
      sale.items.forEach(item => {
        const key = getItemKey(item);
        if (!itemSales[key]) {
          itemSales[key] = { name: item.brandName, quantity: 0, revenue: 0 };
        }
        itemSales[key].quantity += item.quantity;
        itemSales[key].revenue += item.totalPrice;
      });
    });
    return Object.values(itemSales).sort((a, b) => b.quantity - a.quantity).slice(0, 10);
  }, [sales]);

  const getMonthName = (monthStr: string) => {
    const [year, month] = monthStr.split('-');
    return new Date(Number(year), Number(month) - 1).toLocaleString('default', { month: 'long', year: 'numeric' });
  };

  if (loading.sales) {
    return <div className="text-center py-10">Loading sales data...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-500">{error}</div>;
  }

  return (
    <div className="space-y-6 min-w-0">
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className={`px-3 sm:px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
          <div className="flex items-center gap-2"><Award className="h-5 w-5 text-amber-500" /><h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Overall Top Selling Medicines</h3></div>
        </div>
        <ResponsiveTable>
          <table className="w-full">
          <thead>
            <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
              <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Rank</th>
              <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Brand Name</th>
              <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Units Sold</th>
              <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Revenue</th>
              <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Performance</th>
            </tr>
          </thead>
          <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
            {overallMostSelling.map((item, index) => {
              const maxQty = overallMostSelling[0]?.quantity || 1;
              const percentage = (item.quantity / maxQty) * 100;
              const medals = ['🥇', '🥈', '🥉'];
              return (
                <tr key={item.name} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                  <td className="px-3 sm:px-6 py-4"><span className="text-lg">{medals[index] || `#${index + 1}`}</span></td>
                  <td className={`px-3 sm:px-6 py-4 font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.name}</td>
                  <td className={`px-3 sm:px-6 py-4 font-semibold text-right ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.quantity}</td>
                  <td className={`px-3 sm:px-6 py-4 text-right ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{formatCurrency(item.revenue)}</td>
                  <td className="px-3 sm:px-6 py-4">
                    <div className="flex items-center gap-2">
                      <div className={`flex-1 h-2 rounded-full ${isDark ? 'bg-gray-700' : 'bg-gray-200'}`}><div className="h-2 rounded-full bg-gradient-to-r from-emerald-500 to-teal-500" style={{ width: `${percentage}%` }} /></div>
                      <span className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{Math.round(percentage)}%</span>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
          </table>
        </ResponsiveTable>
      </div>

      <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Monthly Breakdown</h3>
      {Object.entries(mostSellingByMonth).map(([month, items]) => (
        <div key={month} className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
          <div className={`px-3 sm:px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'} ${isDark ? 'bg-gray-750' : 'bg-gray-50'}`}><h4 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{getMonthName(month)}</h4></div>
          <ResponsiveTable>
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Rank</th>
                <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Brand Name</th>
                <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Units Sold</th>
                <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Revenue</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {items.map((item, index) => {
                const medals = ['🥇', '🥈', '🥉'];
                return (
                  <tr key={item.name} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                    <td className="px-3 sm:px-6 py-3"><span className="text-lg">{medals[index] || `#${index + 1}`}</span></td>
                    <td className={`px-3 sm:px-6 py-3 font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.name}</td>
                    <td className={`px-3 sm:px-6 py-3 font-semibold text-right ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.quantity}</td>
                    <td className={`px-3 sm:px-6 py-3 text-right ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{formatCurrency(item.revenue)}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
          </ResponsiveTable>
        </div>
      ))}
    </div>
  );
};

export default MostSellingReport;