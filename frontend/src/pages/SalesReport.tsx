import React, { useState, useMemo, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency, formatDate } from '../utils/helpers';
import { BarChart3 } from 'lucide-react';
import { ResponsiveTable } from '../components/ui/ResponsiveTable';

const SalesReport: React.FC = () => {
  const { sales, fetchSales, salesReport, salesReportBounds, fetchSalesReport, error, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [dateRange, setDateRange] = useState('weekly');

  useEffect(() => {
    if (!sales || !sales.length) fetchSales();
    fetchSalesReport(dateRange);
  }, [fetchSales, fetchSalesReport, dateRange]);

  const reportData = salesReport[dateRange] || [];

  const chartData = useMemo(() => reportData.map(r => ({
    label: r.label,
    sales: r.sales,
    profit: r.profit,
    transactions: r.transactionCount,
  })), [reportData]);

  const filteredTotalSales = useMemo(() => reportData.reduce((s, r) => s + r.sales, 0), [reportData]);
  const filteredTotalProfit = useMemo(() => reportData.reduce((s, r) => s + r.profit, 0), [reportData]);
  const filteredTotalTransactions = useMemo(() => reportData.reduce((s, r) => s + r.transactionCount, 0), [reportData]);

  // The detail table is filtered with the same business window the backend
  // charted (Ethiopian time), so the table and the chart can never disagree.
  // Falling back to the raw list is only for the brief moment before the
  // bounds request resolves.
  const bounds = salesReportBounds?.[dateRange];
  const filteredSales = useMemo(() => {
    if (!bounds) return sales || [];
    const start = new Date(bounds.startUtc).getTime();
    const end = new Date(bounds.endUtc).getTime();
    return (sales || [])
      .filter(sale => {
        const t = new Date(sale.saleDate).getTime();
        return t >= start && t <= end;
      })
      .sort((a, b) => new Date(b.saleDate).getTime() - new Date(a.saleDate).getTime());
  }, [sales, bounds]);

  const reportLoading = loading[`report_${dateRange}`] || false;

  if (reportLoading) {
    return <div className="text-center py-10">Loading report data...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-500">{error}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
            {dateRange.charAt(0).toUpperCase() + dateRange.slice(1)} Revenue
          </p>
          <p className={`text-2xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(filteredTotalSales)}</p>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
            {dateRange.charAt(0).toUpperCase() + dateRange.slice(1)} Profit
          </p>
          <p className="text-2xl font-bold text-emerald-600 mt-1">{formatCurrency(filteredTotalProfit)}</p>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Transactions</p>
          <p className={`text-2xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{filteredTotalTransactions}</p>
        </div>
      </div>

      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4 flex gap-2`}>
        {['daily', 'weekly', 'monthly', 'yearly'].map(range => (
          <button key={range} onClick={() => setDateRange(range)} className={`px-4 py-2 rounded-lg text-sm font-medium ${dateRange === range ? 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400' : isDark ? 'text-gray-300 hover:bg-gray-700' : 'text-gray-600 hover:bg-gray-100'}`}>
            {range.charAt(0).toUpperCase() + range.slice(1)}
          </button>
        ))}
        {bounds && (
          <span className={`ml-auto self-center text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
            Business period (EAT): {bounds.startLocal} → {bounds.endLocal}
          </span>
        )}
      </div>

      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
        <h3 className={`text-lg font-semibold mb-4 ${isDark ? 'text-white' : 'text-gray-900'}`}>{dateRange.charAt(0).toUpperCase() + dateRange.slice(1)} Sales Overview</h3>
        <ResponsiveContainer width="100%" height={350}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" stroke={isDark ? '#374151' : '#f0f0f0'} />
            <XAxis dataKey="label" stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} />
            <YAxis stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} tickFormatter={(value) => formatCurrency(Number(value))} />
            <Tooltip contentStyle={{ borderRadius: '12px', backgroundColor: isDark ? '#1f2937' : '#ffffff', border: `1px solid ${isDark ? '#374151' : '#e5e7eb'}` }} formatter={(value: any) => [formatCurrency(value), null]} />
            <Legend />
            <Bar dataKey="sales" fill="#10B981" radius={[4, 4, 0, 0]} name="Sales" />
            <Bar dataKey="profit" fill="#3B82F6" radius={[4, 4, 0, 0]} name="Profit" />
          </BarChart>
        </ResponsiveContainer>
      </div>

      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className={`px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
          <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Sales Detail - {dateRange.charAt(0).toUpperCase() + dateRange.slice(1)}</h3>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Showing {filteredSales.length} transaction{filteredSales.length !== 1 ? 's' : ''}</p>
        </div>
        <ResponsiveTable>
          <table className="w-full">
          <thead>
            <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
              <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Sale #</th>
              <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Date</th>
              <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Cashier</th>
              <th className={`text-left px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Items</th>
              <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total</th>
              <th className={`text-right px-3 sm:px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Profit</th>
            </tr>
          </thead>
          <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
            {filteredSales.map(sale => (
              <tr key={sale.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                <td className="px-3 sm:px-6 py-3 font-mono text-sm text-blue-500">{sale.saleNumber}</td>
                <td className={`px-3 sm:px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{formatDate(sale.saleDate)} <span className={`text-xs ml-2 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>{new Date(sale.saleDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span></td>
                <td className={`px-3 sm:px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{sale.userName}</td>
                <td className={`px-3 sm:px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}><span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-700'}`}>{sale.items.reduce((sum, item) => sum + item.quantity, 0)} items</span></td>
                <td className={`px-3 sm:px-6 py-3 text-sm font-medium text-right ${isDark ? 'text-white' : ''}`}>{formatCurrency(sale.totalAmount)}</td>
                <td className="px-3 sm:px-6 py-3 text-sm font-medium text-right text-emerald-500">{formatCurrency(sale.profit)}</td>
              </tr>
            ))}
          </tbody>
          {filteredSales.length > 0 && (
            <tfoot>
              <tr className={`border-t-2 ${isDark ? 'border-gray-600 bg-gray-700/50' : 'border-gray-200 bg-gray-50'}`}>
                <td colSpan={4} className={`px-3 sm:px-6 py-3 text-sm font-semibold text-left ${isDark ? 'text-white' : 'text-gray-900'}`}>Total ({filteredSales.length} transaction{filteredSales.length !== 1 ? 's' : ''})</td>
                <td className={`px-3 sm:px-6 py-3 text-sm font-bold text-right ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(filteredSales.reduce((s, sale) => s + sale.totalAmount, 0))}</td>
                <td className="px-3 sm:px-6 py-3 text-sm font-bold text-right text-emerald-500">{formatCurrency(filteredSales.reduce((s, sale) => s + sale.profit, 0))}</td>
              </tr>
            </tfoot>
          )}
          </table>
        </ResponsiveTable>
        {filteredSales.length === 0 && (
          <div className="text-center py-12">
            <BarChart3 className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
            <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No sales found for this period</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default SalesReport;
