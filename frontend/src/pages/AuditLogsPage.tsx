import React, { useState, useEffect } from 'react';
import { Activity, Filter, Search, RefreshCw } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Button } from '../components/ui/Button';

export const AuditLogsPage: React.FC = () => {
  const { auditLogs, fetchAuditLogs, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [filterTable, setFilterTable] = useState('');
  const [filterUser, setFilterUser] = useState('');

  useEffect(() => {
    fetchAuditLogs();
  }, [fetchAuditLogs]);

  const filteredLogs = auditLogs.filter(log => {
    if (filterTable && !log.tableName.toLowerCase().includes(filterTable.toLowerCase())) return false;
    if (filterUser && !log.userName.toLowerCase().includes(filterUser.toLowerCase())) return false;
    return true;
  });

  const inputClass = `w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm ${
    isDark ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-400' : 'bg-white border-gray-300 text-gray-900'
  }`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Audit Logs</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>System activity and change history</p>
        </div>
        <Button variant="secondary" onClick={fetchAuditLogs}>
          <RefreshCw className="h-4 w-4" /> Refresh
        </Button>
      </div>

      <div className={`p-4 rounded-xl border ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'}`}>
        <div className="flex items-center gap-2 mb-3">
          <Filter className={`h-4 w-4 ${isDark ? 'text-gray-400' : 'text-gray-500'}`} />
          <span className={`text-sm font-medium ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Filters</span>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className={`block text-xs font-medium mb-1 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Table / Module</label>
            <div className="relative">
              <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
              <input
                type="text"
                placeholder="e.g. Medicines, Users, Sales..."
                value={filterTable}
                onChange={e => setFilterTable(e.target.value)}
                className={`${inputClass} pl-9`}
              />
            </div>
          </div>
          <div>
            <label className={`block text-xs font-medium mb-1 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>User Name</label>
            <div className="relative">
              <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
              <input
                type="text"
                placeholder="Search by user name..."
                value={filterUser}
                onChange={e => setFilterUser(e.target.value)}
                className={`${inputClass} pl-9`}
              />
            </div>
          </div>
        </div>
      </div>

      <div className={`rounded-xl border overflow-hidden ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'}`}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Time</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>User</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Action</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Table</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Record ID</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {filteredLogs.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-12 text-center">
                    <Activity className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
                    <p className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>No audit logs found</p>
                    <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                      {filterTable || filterUser ? 'Try adjusting your filters' : 'Activity will appear here as actions are performed'}
                    </p>
                  </td>
                </tr>
              ) : (
                filteredLogs.map(log => (
                  <tr key={log.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                    <td className={`px-6 py-3 text-sm whitespace-nowrap ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>
                      {new Date(log.createdAt).toLocaleString()}
                    </td>
                    <td className={`px-6 py-3 text-sm font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{log.userName}</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{log.action}</td>
                    <td className="px-6 py-3">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${
                        isDark ? 'bg-gray-700 text-gray-300' : 'bg-gray-100 text-gray-700'
                      }`}>
                        {log.tableName}
                      </span>
                    </td>
                    <td className={`px-6 py-3 text-sm font-mono ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                      #{log.recordId}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
