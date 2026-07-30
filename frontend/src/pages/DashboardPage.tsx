import React, { useMemo, useEffect } from 'react';
import {
  Pill, TrendingUp, DollarSign, AlertTriangle, Package, ShoppingCart,
  ArrowUpRight, ArrowDownRight, Clock, Users, Activity, Award
} from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { formatCurrency, formatDate, formatDateTime, getDaysUntilExpiry } from '../utils/helpers';

const COLORS = ['#10B981', '#3B82F6', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899', '#6B7280'];

export const DashboardPage: React.FC = () => {
  const { medicines, sales, notifications, auditLogs, fetchMedicines, fetchSales, fetchNotifications, fetchAuditLogs, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    fetchMedicines();
    fetchSales();
    fetchNotifications();
    fetchAuditLogs();
  }, [fetchMedicines, fetchSales, fetchNotifications, fetchAuditLogs]);

  // Calculate stats
  const totalMedicines = (medicines || []).length;
  const totalInventoryValue = (medicines || []).reduce((sum, m) => {
    return sum + (m.batches || []).reduce((bSum, b) => bSum + ((b.quantity || 0) * (b.purchasePrice || 0)), 0);
  }, 0);

  const today = new Date().toISOString().split('T')[0];
  const todaySales = (sales || [])
    .filter(s => s.saleDate?.startsWith(today))
    .reduce((sum, s) => sum + (s.totalAmount || 0), 0);

  const currentMonth = new Date().toISOString().slice(0, 7);
  const monthlySales = (sales || [])
    .filter(s => s.saleDate?.startsWith(currentMonth))
    .reduce((sum, s) => sum + (s.totalAmount || 0), 0);

  const monthlyProfit = (sales || [])
    .filter(s => s.saleDate?.startsWith(currentMonth))
    .reduce((sum, s) => sum + (s.profit || 0), 0);

  const lowStockCount = (medicines || []).filter(m => {
    const totalQty = (m.batches || []).reduce((sum, b) => sum + (b.quantity || 0), 0);
    return totalQty > 0 && totalQty <= (m.lowStockThreshold || 0);
  }).length;

  const expiringCount = (medicines || []).filter(m =>
    (m.batches || []).some(b => {
      const days = getDaysUntilExpiry(b.expiryDate);
      return days > 0 && days <= 180;
    })
  ).length;

  const outOfStockCount = (medicines || []).filter(m =>
    (m.batches || []).every(b => b.quantity === 0)
  ).length;

  const recentSales = [...(sales || [])].sort((a, b) => new Date(b.saleDate).getTime() - new Date(a.saleDate).getTime()).slice(0, 5);
  const unreadNotifications = (notifications || []).filter(n => !n.isRead);

  // Inventory by category
  const inventoryByCategory = (medicines || []).reduce((acc, m) => {
    const totalQty = (m.batches || []).reduce((sum, b) => sum + (b.quantity || 0), 0);
    const existing = acc.find(a => a.name === m.categoryName);
    if (existing) {
      existing.value += totalQty;
    } else {
      acc.push({ name: m.categoryName, value: totalQty });
    }
    return acc;
  }, [] as { name: string; value: number }[]);

  // Most selling items for current month
  const mostSellingItems = useMemo(() => {
    const currentMonth = new Date().toISOString().slice(0, 7);
    const monthlySales = (sales || []).filter(s => s.saleDate?.startsWith(currentMonth));

    const itemSales: Record<string, { name: string; quantity: number; revenue: number }> = {};
    monthlySales.forEach(sale => {
      (sale.items || []).forEach(item => {
        if (!itemSales[item.medicineId]) {
          itemSales[item.medicineId] = { name: item.brandName, quantity: 0, revenue: 0 };
        }
        itemSales[item.medicineId].quantity += item.quantity || 0;
        itemSales[item.medicineId].revenue += item.totalPrice || 0;
      });
    });

    return Object.values(itemSales)
      .sort((a, b) => b.quantity - a.quantity)
      .slice(0, 5);
  }, [sales]);

  const statsCards = [
    { label: 'Total Medicines', value: totalMedicines, icon: <Pill className="h-6 w-6" />, color: 'from-blue-500 to-blue-600', change: '+12%', up: true },
    { label: 'Inventory Value', value: formatCurrency(totalInventoryValue), icon: <Package className="h-6 w-6" />, color: 'from-emerald-500 to-emerald-600', change: '+8%', up: true },
    { label: "Today's Sales", value: formatCurrency(todaySales), icon: <ShoppingCart className="h-6 w-6" />, color: 'from-violet-500 to-violet-600', change: '+5%', up: true },
    { label: 'Monthly Sales', value: formatCurrency(monthlySales), icon: <DollarSign className="h-6 w-6" />, color: 'from-amber-500 to-orange-500', change: '+15%', up: true },
    { label: 'Monthly Profit', value: formatCurrency(monthlyProfit), icon: <TrendingUp className="h-6 w-6" />, color: 'from-pink-500 to-rose-500', change: '+10%', up: true },
    { label: 'Low Stock', value: lowStockCount, icon: <AlertTriangle className="h-6 w-6" />, color: 'from-red-500 to-red-600', change: '-2', up: false },
    { label: 'Expiring Soon', value: expiringCount, icon: <Clock className="h-6 w-6" />, color: 'from-orange-500 to-amber-500', change: '', up: true },
    { label: 'Out of Stock', value: outOfStockCount, icon: <Package className="h-6 w-6" />, color: 'from-gray-500 to-gray-600', change: '', up: false },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Dashboard</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Welcome back! Here's your pharmacy overview.</p>
        </div>
        <div className="text-right">
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Today</p>
          <p className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatDate(new Date().toISOString())}</p>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {statsCards.map((card, index) => (
          <div key={index} className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-5 hover:shadow-md transition-shadow`}>
            <div className="flex items-start justify-between">
              <div className={`flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br ${card.color} text-white shadow-lg`}>
                {card.icon}
              </div>
              {card.change && (
                <span className={`flex items-center gap-0.5 text-xs font-medium ${card.up ? 'text-emerald-500' : 'text-red-500'}`}>
                  {card.up ? <ArrowUpRight className="h-3 w-3" /> : <ArrowDownRight className="h-3 w-3" />}
                  {card.change}
                </span>
              )}
            </div>
            <div className="mt-4">
              <p className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{card.value}</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'} mt-0.5`}>{card.label}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Sales Chart */}
        <div className={`lg:col-span-2 ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Weekly Sales</h3>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Sales and profit overview</p>
            </div>
          </div>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={
              ['Mon','Tue','Wed','Thu','Fri','Sat','Sun'].map((day, i) => {
                const daySales = (sales || []).filter(s => {
                  const d = new Date(s.saleDate);
                  return d.getDay() === (i === 0 ? 1 : i === 1 ? 2 : i === 2 ? 3 : i === 3 ? 4 : i === 4 ? 5 : i === 5 ? 6 : 0);
                });
                return { date: day, sales: daySales.reduce((a, b) => a + (b.totalAmount || 0), 0), profit: daySales.reduce((a, b) => a + (b.profit || 0), 0) };
              })
            }>
              <CartesianGrid strokeDasharray="3 3" stroke={isDark ? '#374151' : '#f0f0f0'} />
              <XAxis dataKey="date" stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} />
              <YAxis stroke={isDark ? '#9ca3af' : '#9ca3af'} fontSize={12} />
              <Tooltip
                contentStyle={{
                  borderRadius: '12px',
                  border: '1px solid',
                  borderColor: isDark ? '#374151' : '#e5e7eb',
                  backgroundColor: isDark ? '#1f2937' : '#ffffff',
                  color: isDark ? '#ffffff' : '#000000',
                }}
                formatter={(value) => [`${Number(value).toLocaleString()} ETB`]}
              />
              <Bar dataKey="sales" fill="#10B981" radius={[4, 4, 0, 0]} name="Sales" />
              <Bar dataKey="profit" fill="#3B82F6" radius={[4, 4, 0, 0]} name="Profit" />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Category Distribution */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'} mb-2`}>Inventory by Category</h3>
          <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'} mb-4`}>Stock distribution</p>
          <ResponsiveContainer width="100%" height={220}>
            <PieChart>
              <Pie
                data={inventoryByCategory}
                cx="50%"
                cy="50%"
                innerRadius={50}
                outerRadius={80}
                paddingAngle={3}
                dataKey="value"
              >
                {inventoryByCategory.map((_, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip
                contentStyle={{
                  borderRadius: '12px',
                  backgroundColor: isDark ? '#1f2937' : '#ffffff',
                  border: `1px solid ${isDark ? '#374151' : '#e5e7eb'}`,
                }}
                formatter={(value) => [`${value} units`]}
              />
            </PieChart>
          </ResponsiveContainer>
          <div className="space-y-2 mt-2">
            {inventoryByCategory.slice(0, 5).map((cat, index) => (
              <div key={cat.name} className="flex items-center justify-between text-sm">
                <div className="flex items-center gap-2">
                  <div className="h-3 w-3 rounded-full" style={{ backgroundColor: COLORS[index % COLORS.length] }} />
                  <span className={isDark ? 'text-gray-300' : 'text-gray-600'}>{cat.name}</span>
                </div>
                <span className={`font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{cat.value}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Most Selling Items */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
        <div className="flex items-center justify-between mb-6">
          <div>
            <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Most Selling Items</h3>
            <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Top 5 medicines this month</p>
          </div>
          <Award className="h-5 w-5 text-amber-500" />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          {mostSellingItems.map((item, index) => {
            const colors = [
              { bg: 'from-amber-400 to-yellow-500', text: 'text-amber-500', light: 'bg-amber-50 dark:bg-amber-900/20' },
              { bg: 'from-gray-300 to-gray-400', text: 'text-gray-500', light: 'bg-gray-50 dark:bg-gray-700' },
              { bg: 'from-orange-400 to-amber-600', text: 'text-orange-500', light: 'bg-orange-50 dark:bg-orange-900/20' },
              { bg: 'from-blue-400 to-blue-500', text: 'text-blue-500', light: 'bg-blue-50 dark:bg-blue-900/20' },
              { bg: 'from-emerald-400 to-emerald-500', text: 'text-emerald-500', light: 'bg-emerald-50 dark:bg-emerald-900/20' },
            ];
            const color = colors[index] || colors[4];
            const medals = ['🥇', '🥈', '🥉', '4️⃣', '5️⃣'];
            
            return (
              <div key={item.name} className={`relative p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : color.light} border ${isDark ? 'border-gray-600' : 'border-gray-100'}`}>
                <div className="flex items-center gap-2 mb-3">
                  <span className="text-lg">{medals[index]}</span>
                  <span className={`text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>#{index + 1}</span>
                </div>
                <p className={`font-semibold text-sm mb-1 truncate ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.name}</p>
                <div className="flex items-center justify-between">
                  <div>
                    <p className={`text-2xl font-bold ${color.text}`}>{item.quantity}</p>
                    <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>units sold</p>
                  </div>
                  <div className="text-right">
                    <p className={`text-sm font-medium ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{formatCurrency(item.revenue)}</p>
                    <p className={`text-xs ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>revenue</p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Bottom Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Sales */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center justify-between mb-4">
            <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Recent Sales</h3>
            <Activity className={`h-5 w-5 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
          </div>
          <div className="space-y-3">
            {recentSales.map(sale => (
              <div key={sale.id} className={`flex items-center justify-between p-3 rounded-lg ${isDark ? 'bg-gray-700 hover:bg-gray-600' : 'bg-gray-50 hover:bg-gray-100'} transition-colors`}>
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400">
                    <ShoppingCart className="h-5 w-5" />
                  </div>
                  <div>
                    <p className={`text-sm font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{sale.saleNumber}</p>
                    <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{sale.userName} • {formatDateTime(sale.saleDate)}</p>
                  </div>
                </div>
                <span className={`text-sm font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(sale.totalAmount)}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Alerts & Notifications */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center justify-between mb-4">
            <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Active Alerts</h3>
            <AlertTriangle className="h-5 w-5 text-amber-500" />
          </div>
          <div className="space-y-3">
            {unreadNotifications.length > 0 ? unreadNotifications.map(notif => (
              <div key={notif.id} className={`flex items-start gap-3 p-3 rounded-lg border ${
                notif.type === 'low_stock' || notif.type === 'out_of_stock'
                  ? isDark ? 'bg-red-900/20 border-red-800' : 'bg-red-50 border-red-200'
                  : notif.type === 'expiry'
                  ? isDark ? 'bg-amber-900/20 border-amber-800' : 'bg-amber-50 border-amber-200'
                  : isDark ? 'bg-blue-900/20 border-blue-800' : 'bg-blue-50 border-blue-200'
              }`}>
                <AlertTriangle className={`h-5 w-5 mt-0.5 flex-shrink-0 ${
                  notif.type === 'low_stock' || notif.type === 'out_of_stock' ? 'text-red-500' :
                  notif.type === 'expiry' ? 'text-amber-500' :
                  'text-blue-500'
                }`} />
                <div>
                  <p className={`text-sm font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{notif.title}</p>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-600'} mt-0.5`}>{notif.message}</p>
                </div>
              </div>
            )) : (
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'} text-center py-4`}>No active alerts</p>
            )}
          </div>

          {/* Audit Log Preview */}
          <div className="mt-6">
            <h4 className={`text-sm font-semibold ${isDark ? 'text-gray-300' : 'text-gray-700'} mb-3 flex items-center gap-2`}>
              <Users className="h-4 w-4" /> Recent Activity
            </h4>
            <div className="space-y-2">
              {auditLogs.slice(0, 4).map(log => (
                <div key={log.id} className="flex items-center gap-2 text-xs">
                  <div className="h-2 w-2 rounded-full bg-emerald-400 flex-shrink-0" />
                  <span className={isDark ? 'text-gray-400' : 'text-gray-500'}>{formatDateTime(log.createdAt)}</span>
                  <span className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{log.userName}</span>
                  <span className={`${isDark ? 'text-gray-500' : 'text-gray-500'} truncate`}>{log.action}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
