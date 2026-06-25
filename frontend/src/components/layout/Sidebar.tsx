import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Pill,
  Package,
  ShoppingCart,
  Truck,
  Users,
  BarChart3,
  Bell,
  Settings,
  LogOut,
  ChevronLeft,
  Store,
  ClipboardList,
  AlertTriangle,
  Sun,
  Moon,
} from 'lucide-react';
import { useAppStore } from '../../store/appStore';
import { useThemeStore } from '../../store/themeStore';

interface NavItem {
  id: string;
  label: string;
  icon: React.ReactNode;
  path: string;
  adminOnly?: boolean;
}

const navItems: NavItem[] = [
  { id: 'dashboard', label: 'Dashboard', icon: <LayoutDashboard className="h-5 w-5" />, path: '/dashboard' },
  { id: 'pos', label: 'Point of Sale', icon: <ShoppingCart className="h-5 w-5" />, path: '/pos' },
  { id: 'medicines', label: 'Medicines', icon: <Pill className="h-5 w-5" />, path: '/medicines' },
  { id: 'inventory', label: 'Inventory', icon: <Package className="h-5 w-5" />, path: '/inventory' },
  { id: 'purchases', label: 'Purchases', icon: <ClipboardList className="h-5 w-5" />, path: '/purchases', adminOnly: true },
  { id: 'suppliers', label: 'Suppliers', icon: <Truck className="h-5 w-5" />, path: '/suppliers', adminOnly: true },
  { id: 'damages', label: 'Damage & Expiry', icon: <AlertTriangle className="h-5 w-5" />, path: '/damages' },
  { id: 'reports', label: 'Reports', icon: <BarChart3 className="h-5 w-5" />, path: '/reports' },
  { id: 'notifications', label: 'Notifications', icon: <Bell className="h-5 w-5" />, path: '/notifications' },
  { id: 'users', label: 'User Management', icon: <Users className="h-5 w-5" />, path: '/users', adminOnly: true },
  { id: 'settings', label: 'Settings', icon: <Settings className="h-5 w-5" />, path: '/settings', adminOnly: true },
];

export const Sidebar: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { sidebarOpen, toggleSidebar, currentUser, logout, notifications, settings } = useAppStore();
  const { theme, toggleTheme } = useThemeStore();
  const unreadCount = notifications.filter(n => !n.isRead).length;

  const filteredItems = navItems.filter(item => {
    if (item.adminOnly && currentUser?.role !== 'admin') return false;
    return true;
  });

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <>
      {/* Sidebar */}
      <aside
        className={`fixed left-0 top-0 z-40 h-screen bg-gradient-to-b from-slate-900 to-slate-800 dark:from-slate-950 dark:to-slate-900 text-white transition-all duration-300 ease-in-out ${
          sidebarOpen ? 'w-64' : 'w-[72px]'
        }`}
      >
        {/* Header */}
        <div className={`flex h-16 items-center border-b border-slate-700 dark:border-slate-800 ${sidebarOpen ? 'justify-between px-4' : 'justify-center px-2'}`}>
          {sidebarOpen ? (
            <>
              <div className="flex items-center gap-3 overflow-hidden">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-emerald-400 to-teal-500 shadow-lg flex-shrink-0">
                  <Store className="h-5 w-5 text-white" />
                </div>
                <div className="overflow-hidden">
                  <h1 className="text-sm font-bold text-white truncate">{settings.pharmacyName}</h1>
                  <p className="text-xs text-slate-400">Management System</p>
                </div>
              </div>
              <button
                onClick={toggleSidebar}
                className="rounded-lg p-1.5 hover:bg-slate-700 transition-colors flex-shrink-0"
                title="Collapse sidebar"
              >
                <ChevronLeft className="h-5 w-5 text-slate-400" />
              </button>
            </>
          ) : (
            <button
              onClick={toggleSidebar}
              className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-emerald-400 to-teal-500 shadow-lg hover:from-emerald-500 hover:to-teal-600 transition-all"
              title="Expand sidebar"
            >
              <Store className="h-5 w-5 text-white" />
            </button>
          )}
        </div>

        {/* Navigation */}
        <nav className={`mt-4 space-y-1 overflow-y-auto h-[calc(100vh-16rem)] ${sidebarOpen ? 'px-3' : 'px-2'}`}>
          {filteredItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <button
                key={item.id}
                onClick={() => navigate(item.path)}
                className={`relative flex w-full items-center gap-3 rounded-lg transition-all group ${
                  sidebarOpen ? 'px-3 py-2.5' : 'px-0 py-2.5 justify-center'
                } ${
                  isActive
                    ? 'bg-gradient-to-r from-emerald-500/20 to-teal-500/20 text-emerald-400 shadow-sm'
                    : 'text-slate-300 hover:bg-slate-700/50 hover:text-white'
                }`}
                title={!sidebarOpen ? item.label : undefined}
              >
                <span className={`flex-shrink-0 ${isActive ? 'text-emerald-400' : ''}`}>{item.icon}</span>
                {sidebarOpen && <span className="truncate text-sm font-medium">{item.label}</span>}
                {item.id === 'notifications' && unreadCount > 0 && (
                  <span className={`${
                    sidebarOpen ? 'ml-auto' : 'absolute -top-1 -right-1'
                  } flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-xs text-white font-medium`}>
                    {unreadCount}
                  </span>
                )}
                {isActive && (
                  <div className="absolute left-0 top-1/2 h-6 w-1 -translate-y-1/2 rounded-r-full bg-emerald-400" />
                )}
                {/* Tooltip for collapsed state */}
                {!sidebarOpen && (
                  <div className="absolute left-full ml-2 px-3 py-2 bg-gray-900 text-white text-sm rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 whitespace-nowrap z-50 shadow-xl">
                    {item.label}
                    <div className="absolute top-1/2 -left-1 -translate-y-1/2 w-2 h-2 bg-gray-900 rotate-45"></div>
                  </div>
                )}
              </button>
            );
          })}
        </nav>

        {/* Bottom Section */}
        <div className={`absolute bottom-0 left-0 right-0 border-t border-slate-700 dark:border-slate-800 ${sidebarOpen ? 'p-3' : 'p-2'}`}>
          {/* Theme Toggle */}
          <button
            onClick={toggleTheme}
            className={`w-full flex items-center gap-3 rounded-lg text-sm font-medium text-slate-300 hover:bg-slate-700/50 hover:text-white transition-all group relative ${
              sidebarOpen ? 'px-3 py-2.5' : 'px-0 py-2.5 justify-center'
            }`}
            title={theme === 'light' ? 'Switch to Dark Mode' : 'Switch to Light Mode'}
          >
            {theme === 'light' ? (
              <>
                <Moon className="h-5 w-5 flex-shrink-0" />
                {sidebarOpen && <span>Dark Mode</span>}
              </>
            ) : (
              <>
                <Sun className="h-5 w-5 flex-shrink-0" />
                {sidebarOpen && <span>Light Mode</span>}
              </>
            )}
            {!sidebarOpen && (
              <div className="absolute left-full ml-2 px-3 py-2 bg-gray-900 text-white text-sm rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 whitespace-nowrap z-50 shadow-xl">
                {theme === 'light' ? 'Dark Mode' : 'Light Mode'}
                <div className="absolute top-1/2 -left-1 -translate-y-1/2 w-2 h-2 bg-gray-900 rotate-45"></div>
              </div>
            )}
          </button>

          {/* User Info */}
          <div className={`flex items-center gap-3 mt-2 ${sidebarOpen ? '' : 'justify-center'}`}>
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-blue-400 to-indigo-500 text-sm font-bold flex-shrink-0">
              {currentUser?.fullName.charAt(0)}
            </div>
            {sidebarOpen && (
              <>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-white truncate">{currentUser?.fullName}</p>
                  <p className="text-xs text-slate-400 capitalize">{currentUser?.role}</p>
                </div>
                <button
                  onClick={handleLogout}
                  className="rounded-lg p-1.5 hover:bg-slate-700 transition-colors"
                  title="Logout"
                >
                  <LogOut className="h-4 w-4 text-slate-400" />
                </button>
              </>
            )}
          </div>
        </div>
      </aside>
    </>
  );
};
