import React, { useMemo, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { useAppStore } from '../../store/appStore';
import { useThemeStore } from '../../store/themeStore';
import { formatCurrency } from '../../utils/helpers';

const ProfitReport: React.FC = () => {
  const { sales, fetchSales, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    if (!sales.length) fetchSales();
  }, [fetchSales, sales.length]);

  const salesByDate = useMemo(() => {
    const grouped: Record<string, { sales: number; profit: number; count: number }> = {};
    (sales || []).forEach(s => {
      const date = s.saleDate.split('T')[0];
      if (!grouped[date]) grouped[date] = { sales: 0, profit: 0, count: 0 };
      grouped[date].sales += s.totalAmount;
      grouped[date].profit += s.profit;
      grouped[date].count += 1;
    });
    return Object.entries(grouped)
      .map(([date, data]) => ({ date, ...data }))
      .sort((a, b) => a.date.localeCompare(b.date));
  }, [sales]);

  const totalSales = useMemo(() => (sales || []).reduce((s, sale) => s + sale.totalAmount, 0), [sales]);
  const totalProfit = useMemo(() => (sales || []).reduce((s, sale) => s + sale.profit, 0), [sales]);

  if (loading.sales) {
    return <div className="text-center py-10">Loading profit data...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Revenue</p>
          <p className={`text-2xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(totalSales)}</p>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Profit</p>
          <p className="text-2xl font-bold text-emerald-600 mt-1">{formatCurrency(totalProfit)}</p>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Profit Margin</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">{totalSales > 0 ? ((totalProfit / totalSales) * 100).toFixed(1) : 0}%</p>
        </div>
      </div>
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
        <h3 className={`text-lg font-semibold mb-4 ${isDark ? 'text-white' : 'text-gray-900'}`}>Profit Trend</h3>
        <ResponsiveContainer width="100%" height={350}>
          <LineChart data={salesByDate}>
            <CartesianGrid strokeDasharray="3 3" stroke={isDark ? '#374151' : '#f0f0f0'} />
            <XAxis dataKey="date" stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} />
            <YAxis stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} tickFormatter={(value) => formatCurrency(Number(value), 0)} />
            <Tooltip
              contentStyle={{ borderRadius: '12px', backgroundColor: isDark ? '#1f2937' : '#ffffff', border: `1px solid ${isDark ? '#374151' : '#e5e7eb'}` }}
              formatter={(value: number) => [formatCurrency(value), null]}
            />
            <Legend />
            <Line type="monotone" dataKey="sales" stroke="#10B981" strokeWidth={2} name="Revenue" />
            <Line type="monotone" dataKey="profit" stroke="#3B82F6" strokeWidth={2} name="Profit" />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};

export default ProfitReport;