import React, { useState, Suspense, lazy } from 'react';
import { BarChart3, TrendingUp, Package, Truck, Users, Download, Award } from 'lucide-react';
import { useThemeStore } from '../store/themeStore';
import { Button } from '../components/ui/Button';

// Define ReportTab type
type ReportTab = 'sales' | 'mostSelling' | 'inventory' | 'profit' | 'suppliers' | 'staff';

// Lazy load the report components
const SalesReport = lazy(() => import('./SalesReport'));
const MostSellingReport = lazy(() => import('./MostSellingReport'));
const InventoryReport = lazy(() => import('./InventoryReport'));
const ProfitReport = lazy(() => import('./ProfitReport'));
const SupplierReport = lazy(() => import('./SupplierReport'));
const StaffReport = lazy(() => import('./StaffReport'));

const reportComponents: Record<ReportTab, React.LazyExoticComponent<React.FC<{}>>> = {
  sales: SalesReport,
  mostSelling: MostSellingReport,
  inventory: InventoryReport,
  profit: ProfitReport,
  suppliers: SupplierReport,
  staff: StaffReport,
};

export const ReportsPage: React.FC = () => {
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [activeTab, setActiveTab] = useState<ReportTab>('sales');

  const tabs = [
    { id: 'sales' as ReportTab, label: 'Sales Report', icon: <BarChart3 className="h-4 w-4" /> },
    { id: 'mostSelling' as ReportTab, label: 'Most Selling', icon: <Award className="h-4 w-4" /> },
    { id: 'inventory' as ReportTab, label: 'Inventory Report', icon: <Package className="h-4 w-4" /> },
    { id: 'profit' as ReportTab, label: 'Profit Report', icon: <TrendingUp className="h-4 w-4" /> },
    { id: 'suppliers' as ReportTab, label: 'Supplier Report', icon: <Truck className="h-4 w-4" /> },
    { id: 'staff' as ReportTab, label: 'Staff Report', icon: <Users className="h-4 w-4" /> },
  ];

  const ActiveReportComponent = reportComponents[activeTab];

  const LoadingSpinner: React.FC = () => (
    <div className="flex justify-center items-center py-20">
      <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-blue-600"></div>
    </div>
  );

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
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

      {/* Report Content */}
      <Suspense fallback={<LoadingSpinner />}>
        <ActiveReportComponent />
      </Suspense>
    </div>
  );
};
