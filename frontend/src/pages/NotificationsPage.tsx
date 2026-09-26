import React, { useEffect, useMemo } from 'react';
import { Bell, Check, CheckCheck, AlertTriangle, Info, Package, Clock } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Button } from '../components/ui/Button';
import { formatDateTime } from '../utils/helpers';

export const NotificationsPage: React.FC = () => {
  const { notifications, fetchNotifications, markNotificationRead, markAllNotificationsRead, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    fetchNotifications();
  }, [fetchNotifications]);
   const unreadCount = (notifications || []).filter(n => !n.isRead).length;

   const sortedNotifications = useMemo(() => {
     return [...(notifications || [])].sort((a, b) => {
       if (a.isRead !== b.isRead) return a.isRead ? 1 : -1;
       return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
     });
   }, [notifications]);

  const getIcon = (type: string) => {
    switch (type) {
      case 'low_stock': return <AlertTriangle className="h-5 w-5 text-amber-500" />;
      case 'out_of_stock': return <Package className="h-5 w-5 text-red-500" />;
      case 'expiry': return <Clock className="h-5 w-5 text-orange-500" />;
      default: return <Info className="h-5 w-5 text-blue-500" />;
    }
  };

  const getBgColor = (type: string, isRead: boolean) => {
    if (isDark) {
      if (isRead) return 'bg-gray-800';
      switch (type) {
        case 'low_stock': return 'bg-amber-900/20';
        case 'out_of_stock': return 'bg-red-900/20';
        case 'expiry': return 'bg-orange-900/20';
        default: return 'bg-blue-900/20';
      }
    }
    if (isRead) return 'bg-white';
    switch (type) {
      case 'low_stock': return 'bg-amber-50';
      case 'out_of_stock': return 'bg-red-50';
      case 'expiry': return 'bg-orange-50';
      default: return 'bg-blue-50';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Notifications</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>
            {unreadCount > 0 ? `You have ${unreadCount} unread notification${unreadCount > 1 ? 's' : ''}` : 'All caught up!'}
          </p>
        </div>
        {unreadCount > 0 && (
          <Button variant="secondary" onClick={markAllNotificationsRead}>
            <CheckCheck className="h-4 w-4" /> Mark All Read
          </Button>
        )}
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <Bell className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total</p>
               <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{sortedNotifications.length}</p>
             </div>
           </div>
         </div>
         <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
           <div className="flex items-center gap-3">
             <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-600'}`}>
               <AlertTriangle className="h-5 w-5" />
             </div>
             <div>
               <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Low Stock</p>
               <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{sortedNotifications.filter(n => n.type === 'low_stock').length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-orange-900/30 text-orange-400' : 'bg-orange-100 text-orange-600'}`}>
              <Clock className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Expiry</p>
               <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{sortedNotifications.filter(n => n.type === 'expiry').length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-600'}`}>
              <Package className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Out of Stock</p>
               <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{sortedNotifications.filter(n => n.type === 'out_of_stock').length}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Notifications List */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
           {sortedNotifications.length === 0 ? (
             <div className="text-center py-16">
               <Bell className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
               <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No notifications</p>
             </div>
           ) : (
             sortedNotifications.map(notification => (
              <div
                key={notification.id}
                className={`flex items-start gap-4 p-5 transition-colors ${getBgColor(notification.type, notification.isRead)} ${isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}`}
              >
                <div className="flex-shrink-0 mt-0.5">
                  {getIcon(notification.type)}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className={`text-sm ${notification.isRead ? isDark ? 'text-gray-400' : 'text-gray-600' : `font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}`}>
                        {notification.title}
                      </p>
                      <p className={`text-sm mt-1 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{notification.message}</p>
                      <p className={`text-xs mt-2 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>{formatDateTime(notification.createdAt)}</p>
                    </div>
                    {!notification.isRead && (
                      <button
                        onClick={() => markNotificationRead(notification.id)}
                        className={`flex-shrink-0 p-2 rounded-lg transition-colors ${
                          isDark ? 'hover:bg-gray-600 text-gray-400 hover:text-emerald-400' : 'hover:bg-white text-gray-400 hover:text-emerald-600'
                        }`}
                        title="Mark as read"
                      >
                        <Check className="h-4 w-4" />
                      </button>
                    )}
                  </div>
                </div>
                {!notification.isRead && (
                  <div className="flex-shrink-0">
                    <div className="h-2.5 w-2.5 rounded-full bg-blue-500" />
                  </div>
                )}
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};
