import React, { useState, useMemo, useEffect } from 'react';
import { BarChart3, TrendingUp, Package, Truck, Users, Download, Award } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, Legend } from 'recharts';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency, formatDate } from '../utils/helpers';
import { Button } from '../components/ui/Button';

export const ReportsPage: React.FC = () => {
  const { sales, medicines, suppliers, purchases, users, fetchSales, fetchMedicines, fetchSuppliers, fetchPurchases, fetchUsers, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    fetchSales();
    fetchMedicines();
    fetchSuppliers();
    fetchPurchases();
    fetchUsers();
  }, [fetchSales, fetchMedicines, fetchSuppliers, fetchPurchases, fetchUsers]);
  const [activeTab, setActiveTab] = useState<ReportTab>('sales');
  const [dateRange, setDateRange] = useState('weekly');

  const tabs = [
    { id: 'sales' as ReportTab, label: 'Sales Report', icon: <BarChart3 className="h-4 w-4" /> },
    { id: 'mostSelling' as ReportTab, label: 'Most Selling', icon: <Award className="h-4 w-4" /> },
    { id: 'inventory' as ReportTab, label: 'Inventory Report', icon: <Package className="h-4 w-4" /> },
    { id: 'profit' as ReportTab, label: 'Profit Report', icon: <TrendingUp className="h-4 w-4" /> },
    { id: 'suppliers' as ReportTab, label: 'Supplier Report', icon: <Truck className="h-4 w-4" /> },
    { id: 'staff' as ReportTab, label: 'Staff Report', icon: <Users className="h-4 w-4" /> },
  ];

  // Sales Report Data
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

  const dailySalesData = salesByDate;
  const weeklySalesData = salesByDate;
  const monthlySalesData = salesByDate;
  const yearlySalesData = salesByDate;

  const totalSales = (sales || []).reduce((s, sale) => s + sale.totalAmount, 0);
  const totalProfit = (sales || []).reduce((s, sale) => s + sale.profit, 0);

  // Most Selling Items by Month
  const mostSellingByMonth = useMemo(() => {
    const months: Record<string, Record<string, { name: string; quantity: number; revenue: number }>> = {};
    
    (sales || []).forEach(sale => {
      const month = sale.saleDate.slice(0, 7); // YYYY-MM
      if (!months[month]) months[month] = {};
      
      sale.items.forEach(item => {
        if (!months[month][item.medicineId]) {
          months[month][item.medicineId] = { name: item.medicineName, quantity: 0, revenue: 0 };
        }
        months[month][item.medicineId].quantity += item.quantity;
        months[month][item.medicineId].revenue += item.totalPrice;
      });
    });

    // Sort months and get top 5 for each
    const result: Record<string, { name: string; quantity: number; revenue: number }[]> = {};
    Object.entries(months)
      .sort(([a], [b]) => b.localeCompare(a))
      .forEach(([month, items]) => {
        result[month] = Object.values(items)
          .sort((a, b) => b.quantity - a.quantity)
          .slice(0, 5);
      });
    
    return result;
  }, [sales]);

  // Filtered sales based on date range
  const filteredSales = useMemo(() => {
    const now = new Date();
    const today = now.toISOString().split('T')[0];
    
    // Get start of week (Monday)
    const startOfWeek = new Date(now);
    const dayOfWeek = startOfWeek.getDay();
    const diff = dayOfWeek === 0 ? 6 : dayOfWeek - 1; // Adjust for Monday start
    startOfWeek.setDate(startOfWeek.getDate() - diff);
    startOfWeek.setHours(0, 0, 0, 0);
    
    // Get start of month
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    
    // Get start of year
    const startOfYear = new Date(now.getFullYear(), 0, 1);

    return (sales || []).filter(sale => {
      const saleDate = new Date(sale.saleDate);
      
      switch (dateRange) {
        case 'daily':
          return sale.saleDate.startsWith(today);
        case 'weekly':
          return saleDate >= startOfWeek;
        case 'monthly':
          return saleDate >= startOfMonth;
        case 'yearly':
          return saleDate >= startOfYear;
        default:
          return true;
      }
    }).sort((a, b) => new Date(b.saleDate).getTime() - new Date(a.saleDate).getTime());
  }, [sales, dateRange]);

  // Calculate stats for filtered period
  const filteredTotalSales = (filteredSales || []).reduce((s, sale) => s + sale.totalAmount, 0);
  const filteredTotalProfit = (filteredSales || []).reduce((s, sale) => s + sale.profit, 0);
  const filteredTotalTransactions = (filteredSales || []).length;

  // Overall most selling
  const overallMostSelling = useMemo(() => {
    const itemSales: Record<string, { name: string; quantity: number; revenue: number }> = {};
    (sales || []).forEach(sale => {
      sale.items.forEach(item => {
        if (!itemSales[item.medicineId]) {
          itemSales[item.medicineId] = { name: item.medicineName, quantity: 0, revenue: 0 };
        }
        itemSales[item.medicineId].quantity += item.quantity;
        itemSales[item.medicineId].revenue += item.totalPrice;
      });
    });
    return Object.values(itemSales).sort((a, b) => b.quantity - a.quantity).slice(0, 10);
  }, [sales]);

  // Inventory Report Data
  const inventoryData = useMemo(() => {
    return (medicines || []).map(m => {
      const totalQty = m.batches.reduce((s, b) => s + b.quantity, 0);
      const totalValue = m.batches.reduce((s, b) => s + (b.quantity * b.purchasePrice), 0);
      return {
        name: m.name,
        category: m.categoryName,
        quantity: totalQty,
        value: totalValue,
        status: totalQty === 0 ? 'Out of Stock' : totalQty <= m.lowStockThreshold ? 'Low Stock' : 'In Stock',
      };
    }).sort((a, b) => a.quantity - b.quantity);
  }, [medicines]);

  const totalInventoryValue = (inventoryData || []).reduce((s, i) => s + i.value, 0);
  const outOfStockItems = (inventoryData || []).filter(i => i.status === 'Out of Stock').length;
  const lowStockItems = (inventoryData || []).filter(i => i.status === 'Low Stock').length;

  // Supplier Report Data
  const supplierData = useMemo(() => {
    return (suppliers || []).map(s => {
      const supplierPurchases = (purchases || []).filter(p => p.supplierId === s.id);
      const totalAmount = supplierPurchases.reduce((sum, p) => sum + p.totalAmount, 0);
      const totalPaid = supplierPurchases.reduce((sum, p) => sum + p.amountPaid, 0);
      const totalDebt = supplierPurchases.reduce((sum, p) => sum + p.remainingDebt, 0);
      return {
        name: s.name,
        purchases: supplierPurchases.length,
        totalAmount,
        totalPaid,
        totalDebt,
        paymentStatus: totalDebt > 0 ? 'outstanding' : 'cleared',
      };
    }).sort((a, b) => b.totalAmount - a.totalAmount);
  }, [suppliers, purchases]);

  // Staff Report Data
  const staffData = useMemo(() => {
    return (users || []).filter(u => u.isActive).map(u => {
      const userSales = (sales || []).filter(s => s.userId === u.id);
      const totalAmount = userSales.reduce((sum, s) => sum + s.totalAmount, 0);
      const totalProfitAmount = userSales.reduce((sum, s) => sum + s.profit, 0);
      return {
        name: u.fullName,
        role: u.role,
        salesCount: userSales.length,
        totalAmount,
        profit: totalProfitAmount,
      };
    }).sort((a, b) => b.totalAmount - a.totalAmount);
  }, [users, sales]);

  const getMonthName = (monthStr: string) => {
    const [year, month] = monthStr.split('-');
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    return `${months[parseInt(month) - 1]} ${year}`;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Reports & Analytics</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>View detailed business reports</p>
        </div>
        <Button variant="secondary">
          <Download className="h-4 w-4" /> Export
        </Button>
      </div>

      {/* Tabs */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-1 flex gap-1 overflow-x-auto`}>
        {tabs.map(tab => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all whitespace-nowrap ${
              activeTab === tab.id
                ? 'bg-blue-600 text-white shadow-sm'
                : isDark ? 'text-gray-300 hover:bg-gray-700' : 'text-gray-600 hover:bg-gray-100'
            }`}
          >
            {tab.icon} {tab.label}
          </button>
        ))}
      </div>

      {/* Sales Report */}
      {activeTab === 'sales' && (
        <div className="space-y-6">
          {/* Stats */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                {dateRange === 'daily' ? "Today's" : dateRange === 'weekly' ? "This Week's" : dateRange === 'monthly' ? "This Month's" : "This Year's"} Revenue
              </p>
              <p className={`text-2xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(filteredTotalSales)}</p>
            </div>
            <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                {dateRange === 'daily' ? "Today's" : dateRange === 'weekly' ? "This Week's" : dateRange === 'monthly' ? "This Month's" : "This Year's"} Profit
              </p>
              <p className="text-2xl font-bold text-emerald-600 mt-1">{formatCurrency(filteredTotalProfit)}</p>
            </div>
            <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5`}>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Transactions</p>
              <p className={`text-2xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{filteredTotalTransactions}</p>
            </div>
          </div>

          {/* Date Range */}
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4 flex gap-2`}>
            {['daily', 'weekly', 'monthly', 'yearly'].map(range => (
              <button
                key={range}
                onClick={() => setDateRange(range)}
                className={`px-4 py-2 rounded-lg text-sm font-medium ${
                  dateRange === range
                    ? 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400'
                    : isDark ? 'text-gray-300 hover:bg-gray-700' : 'text-gray-600 hover:bg-gray-100'
                }`}
              >
                {range.charAt(0).toUpperCase() + range.slice(1)}
              </button>
            ))}
          </div>

          {/* Chart */}
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
            <h3 className={`text-lg font-semibold mb-4 ${isDark ? 'text-white' : 'text-gray-900'}`}>
              {dateRange === 'daily' ? 'Daily' : dateRange === 'weekly' ? 'Weekly' : dateRange === 'monthly' ? 'Monthly' : 'Yearly'} Sales Overview
            </h3>
            <ResponsiveContainer width="100%" height={350}>
              <BarChart data={dateRange === 'daily' ? dailySalesData : dateRange === 'weekly' ? weeklySalesData : dateRange === 'monthly' ? monthlySalesData : yearlySalesData}>
                <CartesianGrid strokeDasharray="3 3" stroke={isDark ? '#374151' : '#f0f0f0'} />
                <XAxis dataKey="date" stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} />
                <YAxis stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} />
                <Tooltip
                  contentStyle={{
                    borderRadius: '12px',
                    backgroundColor: isDark ? '#1f2937' : '#ffffff',
                    border: `1px solid ${isDark ? '#374151' : '#e5e7eb'}`,
                  }}
                  formatter={(value) => [`${Number(value).toLocaleString()} ETB`]}
                />
                <Legend />
                <Bar dataKey="sales" fill="#10B981" radius={[4, 4, 0, 0]} name="Sales" />
                <Bar dataKey="profit" fill="#3B82F6" radius={[4, 4, 0, 0]} name="Profit" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Sales Table */}
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
            <div className={`px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
              <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                Sales Detail - {dateRange === 'daily' ? 'Today' : dateRange === 'weekly' ? 'This Week' : dateRange === 'monthly' ? 'This Month' : 'This Year'}
              </h3>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>
                Showing {(filteredSales || []).length} transaction{(filteredSales || []).length !== 1 ? 's' : ''}
              </p>
            </div>
            <table className="w-full">
              <thead>
                <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Sale #</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Date</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Cashier</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Items</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Profit</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {(filteredSales || []).map(sale => (
                  <tr key={sale.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                    <td className="px-6 py-3 font-mono text-sm text-blue-500">{sale.saleNumber}</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>
                      {formatDate(sale.saleDate)}
                      <span className={`text-xs ml-2 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                        {new Date(sale.saleDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </span>
                    </td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{sale.userName}</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                        isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-700'
                      }`}>
                        {sale.items.reduce((sum, item) => sum + item.quantity, 0)} items
                      </span>
                    </td>
                    <td className={`px-6 py-3 text-sm font-medium ${isDark ? 'text-white' : ''}`}>{formatCurrency(sale.totalAmount)}</td>
                    <td className="px-6 py-3 text-sm font-medium text-emerald-500">{formatCurrency(sale.profit)}</td>
                  </tr>
                ))}
              </tbody>
              {(filteredSales || []).length > 0 && (
                <tfoot>
                  <tr className={`border-t-2 ${isDark ? 'border-gray-600 bg-gray-700/50' : 'border-gray-200 bg-gray-50'}`}>
                    <td colSpan={4} className={`px-6 py-3 text-sm font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                      Total ({(filteredSales || []).length} transaction{(filteredSales || []).length !== 1 ? 's' : ''})
                    </td>
                    <td className={`px-6 py-3 text-sm font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(filteredTotalSales)}</td>
                    <td className="px-6 py-3 text-sm font-bold text-emerald-500">{formatCurrency(filteredTotalProfit)}</td>
                  </tr>
                </tfoot>
              )}
            </table>
            {(filteredSales || []).length === 0 && (
              <div className="text-center py-12">
                <BarChart3 className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
                <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No sales found for this period</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Most Selling Report */}
      {activeTab === 'mostSelling' && (
        <div className="space-y-6">
          {/* Overall Top Sellers */}
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
            <div className={`px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
              <div className="flex items-center gap-2">
                <Award className="h-5 w-5 text-amber-500" />
                <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Overall Top Selling Medicines</h3>
              </div>
            </div>
            <table className="w-full">
              <thead>
                <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Rank</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Medicine</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Units Sold</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Revenue</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Performance</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {(overallMostSelling || []).map((item, index) => {
                  const maxQty = overallMostSelling[0]?.quantity || 1;
                  const percentage = (item.quantity / maxQty) * 100;
                  const medals = ['🥇', '🥈', '🥉'];
                  
                  return (
                    <tr key={item.name} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                      <td className="px-6 py-4">
                        <span className="text-lg">{medals[index] || `#${index + 1}`}</span>
                      </td>
                      <td className={`px-6 py-4 font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.name}</td>
                      <td className={`px-6 py-4 font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.quantity}</td>
                      <td className={`px-6 py-4 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{formatCurrency(item.revenue)}</td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2">
                          <div className={`flex-1 h-2 rounded-full ${isDark ? 'bg-gray-700' : 'bg-gray-200'}`}>
                            <div
                              className="h-2 rounded-full bg-gradient-to-r from-emerald-500 to-teal-500"
                              style={{ width: `${percentage}%` }}
                            />
                          </div>
                          <span className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{Math.round(percentage)}%</span>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Monthly Breakdown */}
          <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Monthly Breakdown</h3>
          {Object.entries(mostSellingByMonth).map(([month, items]) => (
            <div key={month} className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
              <div className={`px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'} ${isDark ? 'bg-gray-750' : 'bg-gray-50'}`}>
                <h4 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{getMonthName(month)}</h4>
              </div>
              <table className="w-full">
                <thead>
                  <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                    <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Rank</th>
                    <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Medicine</th>
                    <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Units Sold</th>
                    <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Revenue</th>
                  </tr>
                </thead>
                <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                  {(items || []).map((item, index) => {
                    const medals = ['🥇', '🥈', '🥉'];
                    return (
                      <tr key={item.name} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                        <td className="px-6 py-3">
                          <span className="text-lg">{medals[index] || `#${index + 1}`}</span>
                        </td>
                        <td className={`px-6 py-3 font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.name}</td>
                        <td className={`px-6 py-3 font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.quantity}</td>
                        <td className={`px-6 py-3 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{formatCurrency(item.revenue)}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          ))}
        </div>
      )}

      {/* Inventory Report */}
      {activeTab === 'inventory' && (
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
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Medicine</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Category</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Stock</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Value</th>
                  <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {(inventoryData || []).map((item, i) => (
                  <tr key={i} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                    <td className={`px-6 py-3 font-medium text-sm ${isDark ? 'text-white' : ''}`}>{item.name}</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{item.category}</td>
                    <td className={`px-6 py-3 text-sm font-medium ${isDark ? 'text-white' : ''}`}>{item.quantity}</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{formatCurrency(item.value)}</td>
                    <td className="px-6 py-3">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${
                        item.status === 'Out of Stock' ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400' :
                        item.status === 'Low Stock' ? 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400' :
                        'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                      }`}>{item.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Profit Report */}
      {activeTab === 'profit' && (
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
                <YAxis stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} />
                <Tooltip
                  contentStyle={{
                    borderRadius: '12px',
                    backgroundColor: isDark ? '#1f2937' : '#ffffff',
                    border: `1px solid ${isDark ? '#374151' : '#e5e7eb'}`,
                  }}
                  formatter={(value) => [`${Number(value).toLocaleString()} ETB`]}
                />
                <Legend />
                <Line type="monotone" dataKey="sales" stroke="#10B981" strokeWidth={2} name="Revenue" />
                <Line type="monotone" dataKey="profit" stroke="#3B82F6" strokeWidth={2} name="Profit" />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}

      {/* Supplier Report */}
      {activeTab === 'suppliers' && (
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
          <div className={`px-6 py-4 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
            <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
              Financial data calculated from purchase records
            </p>
          </div>
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Supplier</th>
                <th className={`text-center px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Purchases</th>
                <th className={`text-right px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Spent</th>
                <th className={`text-right px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Paid</th>
                <th className={`text-right px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Debt</th>
                <th className={`text-center px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {(supplierData || []).map((item, i) => (
                <tr key={i} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                  <td className={`px-6 py-3 font-medium text-sm ${isDark ? 'text-white' : ''}`}>{item.name}</td>
                  <td className={`px-6 py-3 text-sm text-center ${isDark ? 'text-gray-300' : ''}`}>{item.purchases}</td>
                  <td className={`px-6 py-3 text-sm font-medium text-right ${isDark ? 'text-white' : ''}`}>{formatCurrency(item.totalAmount)}</td>
                  <td className={`px-6 py-3 text-sm text-right ${isDark ? 'text-green-400' : 'text-green-600'}`}>{formatCurrency(item.totalPaid)}</td>
                  <td className={`px-6 py-3 text-sm text-right font-semibold ${item.totalDebt > 0 ? 'text-red-500' : isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                    {item.totalDebt > 0 ? formatCurrency(item.totalDebt) : '-'}
                  </td>
                  <td className="px-6 py-3 text-center">
                    <span className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      item.paymentStatus === 'cleared'
                        ? isDark ? 'bg-green-900/30 text-green-400' : 'bg-green-100 text-green-700'
                        : isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-700'
                    }`}>
                      {item.paymentStatus === 'cleared' ? '✅ Cleared' : '⚠️ Outstanding'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Staff Report */}
      {activeTab === 'staff' && (
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Staff</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Role</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Sales Count</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Revenue</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Profit</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {(staffData || []).map((item, i) => (
                <tr key={i} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                  <td className={`px-6 py-3 font-medium text-sm ${isDark ? 'text-white' : ''}`}>{item.name}</td>
                  <td className={`px-6 py-3 text-sm capitalize ${isDark ? 'text-gray-300' : ''}`}>{item.role}</td>
                  <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{item.salesCount}</td>
                  <td className={`px-6 py-3 text-sm font-medium ${isDark ? 'text-white' : ''}`}>{formatCurrency(item.totalAmount)}</td>
                  <td className="px-6 py-3 text-sm font-medium text-emerald-500">{formatCurrency(item.profit)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
