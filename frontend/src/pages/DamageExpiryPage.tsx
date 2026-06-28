import React, { useState, useMemo, useEffect } from 'react';
import { AlertTriangle, Plus, Package, Clock, Trash2, Zap, CheckCircle, Layers } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Badge } from '../components/ui/Badge';
import { Modal } from '../components/ui/Modal';
import { formatDate, getDaysUntilExpiry, generateId } from '../utils/helpers';

interface DamageRecord {
  id: string;
  medicineId: string;
  medicineName: string;
  batchNumber: string;
  quantity: number;
  reason: string;
  date: string;
}

export const DamageExpiryPage: React.FC = () => {
  const { medicines, fetchMedicines, addAuditLog, currentUser, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    fetchMedicines();
  }, [fetchMedicines]);
  const [activeTab, setActiveTab] = useState<'expiring' | 'expired' | 'damages'>('expiring');
  const [showDamageModal, setShowDamageModal] = useState(false);
  const [damages, setDamages] = useState<DamageRecord[]>([]);
  const [damageForm, setDamageForm] = useState({ medicineId: '', batchId: '', quantity: '', reason: '' });

  // Auto-detect expiring soon items (within 6 months / 180 days)
  const expiringMedicines = useMemo(() => {
    return medicines.flatMap(medicine =>
      medicine.batches
        .filter(b => {
          const days = getDaysUntilExpiry(b.expiryDate);
          return days > 0 && days <= 180 && b.quantity > 0;
        })
        .map(batch => ({
          ...batch,
          medicineName: medicine.name,
          genericName: medicine.genericName,
          categoryName: medicine.categoryName,
          unitType: medicine.unitType,
          daysLeft: getDaysUntilExpiry(batch.expiryDate),
        }))
    ).sort((a, b) => a.daysLeft - b.daysLeft);
  }, [medicines]);

  // Auto-detect expired items
  const expiredMedicines = useMemo(() => {
    return medicines.flatMap(medicine =>
      medicine.batches
        .filter(b => getDaysUntilExpiry(b.expiryDate) <= 0 && b.quantity > 0)
        .map(batch => ({
          ...batch,
          medicineName: medicine.name,
          genericName: medicine.genericName,
          daysOverdue: Math.abs(getDaysUntilExpiry(batch.expiryDate)),
        }))
    ).sort((a, b) => b.daysOverdue - a.daysOverdue);
  }, [medicines]);

  const handleRecordDamage = () => {
    if (!damageForm.medicineId || !damageForm.batchId || !damageForm.quantity) return;
    const medicine = medicines.find(m => m.id === damageForm.medicineId);
    const batch = medicine?.batches.find(b => b.id === damageForm.batchId);
    if (!medicine || !batch) return;

    const record: DamageRecord = {
      id: generateId(),
      medicineId: damageForm.medicineId,
      medicineName: medicine.name,
      batchNumber: batch.batchNumber,
      quantity: Number(damageForm.quantity),
      reason: damageForm.reason,
      date: new Date().toISOString(),
    };
    setDamages([record, ...damages]);
    addAuditLog({
      id: generateId(),
      userId: currentUser?.id || '',
      userName: currentUser?.fullName || '',
      action: `Recorded damage: ${record.quantity} ${medicine.name} - ${record.reason}`,
      tableName: 'Damage',
      recordId: record.id,
      createdAt: new Date().toISOString(),
    });
    setShowDamageModal(false);
    setDamageForm({ medicineId: '', batchId: '', quantity: '', reason: '' });
  };

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all ${
    isDark 
      ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500 focus:bg-gray-700' 
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400'
  }`;
  const labelClass = `flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`;
  const thClass = `text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`;

  return (
    <div className="space-y-6">
      <div>
        <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Damage & Expiry Management</h1>
        <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Auto-detected expiry tracking & manual damage recording</p>
      </div>

      {/* Info Banner */}
      <div className={`rounded-xl p-4 border ${isDark ? 'bg-emerald-900/20 border-emerald-800' : 'bg-emerald-50 border-emerald-200'}`}>
        <div className="flex items-start gap-3">
          <Zap className="h-5 w-5 text-emerald-500 mt-0.5 flex-shrink-0" />
          <div>
            <p className={`text-sm font-medium ${isDark ? 'text-emerald-300' : 'text-emerald-700'}`}>Auto-Detection Enabled</p>
            <p className={`text-xs mt-1 ${isDark ? 'text-emerald-400' : 'text-emerald-600'}`}>
              Expiring soon and expired items are <strong>automatically detected</strong> from batch expiry dates. 
              Only <strong>damages</strong> need manual recording.
            </p>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2">
        <button
          onClick={() => setActiveTab('expiring')}
          className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all ${
            activeTab === 'expiring' 
              ? 'bg-amber-600 text-white' 
              : isDark ? 'bg-gray-800 text-gray-300 border border-gray-700 hover:bg-gray-700' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
          }`}
        >
          <Clock className="h-4 w-4" /> 
          Expiring Soon 
          <span className={`ml-1 px-2 py-0.5 rounded-full text-xs ${activeTab === 'expiring' ? 'bg-white/20' : isDark ? 'bg-gray-700' : 'bg-gray-100'}`}>
            {expiringMedicines.length}
          </span>
        </button>
        <button
          onClick={() => setActiveTab('expired')}
          className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all ${
            activeTab === 'expired' 
              ? 'bg-red-600 text-white' 
              : isDark ? 'bg-gray-800 text-gray-300 border border-gray-700 hover:bg-gray-700' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
          }`}
        >
          <AlertTriangle className="h-4 w-4" /> 
          Expired 
          <span className={`ml-1 px-2 py-0.5 rounded-full text-xs ${activeTab === 'expired' ? 'bg-white/20' : isDark ? 'bg-gray-700' : 'bg-gray-100'}`}>
            {expiredMedicines.length}
          </span>
        </button>
        <button
          onClick={() => setActiveTab('damages')}
          className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all ${
            activeTab === 'damages' 
              ? 'bg-orange-600 text-white' 
              : isDark ? 'bg-gray-800 text-gray-300 border border-gray-700 hover:bg-gray-700' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
          }`}
        >
          <Trash2 className="h-4 w-4" /> 
          Damages 
          <span className={`ml-1 px-2 py-0.5 rounded-full text-xs ${activeTab === 'damages' ? 'bg-white/20' : isDark ? 'bg-gray-700' : 'bg-gray-100'}`}>
            {damages.length}
          </span>
        </button>
      </div>

      {/* ========== EXPIRING SOON TAB ========== */}
      {activeTab === 'expiring' && (
        <div className="space-y-4">
          {/* Alert Info */}
          <div className={`rounded-xl p-4 border ${isDark ? 'bg-amber-900/20 border-amber-800' : 'bg-amber-50 border-amber-200'}`}>
            <div className="flex items-start gap-3">
              <Clock className="h-5 w-5 text-amber-500 mt-0.5 flex-shrink-0" />
              <div>
                <p className={`text-sm font-medium ${isDark ? 'text-amber-300' : 'text-amber-700'}`}>
                  ⏰ Auto-Detected: Items Expiring Within 6 Months
                </p>
                <p className={`text-xs mt-1 ${isDark ? 'text-amber-400' : 'text-amber-600'}`}>
                  These items are automatically detected based on their batch expiry dates. Notifications are sent every 15 days.
                </p>
              </div>
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-3 gap-4">
            <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Critical (≤30 days)</p>
              <p className="text-2xl font-bold text-red-500 mt-1">
                {expiringMedicines.filter(i => i.daysLeft <= 30).length}
              </p>
            </div>
            <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Warning (31-90 days)</p>
              <p className="text-2xl font-bold text-amber-500 mt-1">
                {expiringMedicines.filter(i => i.daysLeft > 30 && i.daysLeft <= 90).length}
              </p>
            </div>
            <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Monitor (91-180 days)</p>
              <p className="text-2xl font-bold text-blue-500 mt-1">
                {expiringMedicines.filter(i => i.daysLeft > 90).length}
              </p>
            </div>
          </div>

          {/* Table */}
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
            <table className="w-full">
              <thead>
                <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                  <th className={thClass}>Medicine</th>
                  <th className={thClass}>Batch</th>
                  <th className={thClass}>Stock</th>
                  <th className={thClass}>Expiry Date</th>
                  <th className={thClass}>Days Left</th>
                  <th className={thClass}>Urgency</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {expiringMedicines.map(item => (
                  <tr key={item.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                    <td className="px-6 py-3">
                      <p className={`font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.medicineName}</p>
                      <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{item.genericName}</p>
                    </td>
                    <td className={`px-6 py-3 font-mono text-sm ${isDark ? 'text-gray-300' : ''}`}>{item.batchNumber}</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{item.quantity} {item.unitType}s</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{formatDate(item.expiryDate)}</td>
                    <td className="px-6 py-3">
                      <span className={`font-semibold text-sm ${
                        item.daysLeft <= 15 ? 'text-red-500' : 
                        item.daysLeft <= 30 ? 'text-red-400' : 
                        item.daysLeft <= 90 ? 'text-amber-400' : 
                        'text-blue-400'
                      }`}>
                        {item.daysLeft} days
                      </span>
                    </td>
                    <td className="px-6 py-3">
                      <Badge variant={
                        item.daysLeft <= 15 ? 'danger' : 
                        item.daysLeft <= 30 ? 'danger' : 
                        item.daysLeft <= 90 ? 'warning' : 
                        'info'
                      }>
                        {item.daysLeft <= 15 ? '🔴 Critical' : 
                         item.daysLeft <= 30 ? '🟠 Urgent' : 
                         item.daysLeft <= 90 ? '🟡 Warning' : 
                         '🔵 Monitor'}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {expiringMedicines.length === 0 && (
              <div className="text-center py-12">
                <CheckCircle className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-emerald-600' : 'text-emerald-400'}`} />
                <p className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>No items expiring soon</p>
                <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>All items have more than 6 months until expiry</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ========== EXPIRED TAB ========== */}
      {activeTab === 'expired' && (
        <div className="space-y-4">
          {/* Alert */}
          <div className={`rounded-xl p-4 border ${isDark ? 'bg-red-900/20 border-red-800' : 'bg-red-50 border-red-200'}`}>
            <div className="flex items-start gap-3">
              <AlertTriangle className="h-5 w-5 text-red-500 mt-0.5 flex-shrink-0" />
              <div>
                <p className={`text-sm font-medium ${isDark ? 'text-red-300' : 'text-red-700'}`}>
                  🚨 Auto-Detected: Expired Items
                </p>
                <p className={`text-xs mt-1 ${isDark ? 'text-red-400' : 'text-red-600'}`}>
                  These items have passed their expiry date and should be removed from inventory immediately.
                </p>
              </div>
            </div>
          </div>

          {/* Table */}
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
            <table className="w-full">
              <thead>
                <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                  <th className={thClass}>Medicine</th>
                  <th className={thClass}>Batch</th>
                  <th className={thClass}>Stock</th>
                  <th className={thClass}>Expiry Date</th>
                  <th className={thClass}>Days Overdue</th>
                  <th className={thClass}>Status</th>
                </tr>
              </thead>
              <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                {expiredMedicines.map(item => (
                  <tr key={item.id} className={isDark ? 'hover:bg-red-900/10' : 'hover:bg-red-50/50'}>
                    <td className="px-6 py-3">
                      <p className={`font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.medicineName}</p>
                      <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{item.genericName}</p>
                    </td>
                    <td className={`px-6 py-3 font-mono text-sm ${isDark ? 'text-gray-300' : ''}`}>{item.batchNumber}</td>
                    <td className="px-6 py-3 text-sm text-red-500 font-semibold">{item.quantity}</td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{formatDate(item.expiryDate)}</td>
                    <td className="px-6 py-3">
                      <span className="text-red-500 font-semibold">{item.daysOverdue} days</span>
                    </td>
                    <td className="px-6 py-3">
                      <Badge variant="danger">⛔ Expired</Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {expiredMedicines.length === 0 && (
              <div className="text-center py-12">
                <CheckCircle className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-emerald-600' : 'text-emerald-400'}`} />
                <p className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>No expired items</p>
                <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>All items are within their expiry dates</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ========== DAMAGES TAB ========== */}
      {activeTab === 'damages' && (
        <div className="space-y-4">
          {/* Info */}
          <div className={`rounded-xl p-4 border ${isDark ? 'bg-orange-900/20 border-orange-800' : 'bg-orange-50 border-orange-200'}`}>
            <div className="flex items-start gap-3 justify-between">
              <div className="flex items-start gap-3">
                <Trash2 className="h-5 w-5 text-orange-500 mt-0.5 flex-shrink-0" />
                <div>
                  <p className={`text-sm font-medium ${isDark ? 'text-orange-300' : 'text-orange-700'}`}>
                    📝 Manual Recording Required
                  </p>
                  <p className={`text-xs mt-1 ${isDark ? 'text-orange-400' : 'text-orange-600'}`}>
                    Damages must be recorded manually as they occur in the real world.
                  </p>
                </div>
              </div>
              <button
                onClick={() => setShowDamageModal(true)}
                className="px-4 py-2 bg-orange-600 hover:bg-orange-700 text-white rounded-xl text-sm font-medium transition-colors flex items-center gap-2"
              >
                <Plus className="h-4 w-4" /> Record Damage
              </button>
            </div>
          </div>

          {/* Table */}
          <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
            {damages.length > 0 ? (
              <table className="w-full">
                <thead>
                  <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                    <th className={thClass}>Medicine</th>
                    <th className={thClass}>Batch</th>
                    <th className={thClass}>Quantity</th>
                    <th className={thClass}>Reason</th>
                    <th className={thClass}>Date</th>
                  </tr>
                </thead>
                <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                  {damages.map(damage => (
                    <tr key={damage.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                      <td className={`px-6 py-3 font-medium text-sm ${isDark ? 'text-white' : ''}`}>{damage.medicineName}</td>
                      <td className={`px-6 py-3 font-mono text-sm ${isDark ? 'text-gray-300' : ''}`}>{damage.batchNumber}</td>
                      <td className="px-6 py-3 text-sm text-red-500 font-semibold">{damage.quantity}</td>
                      <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{damage.reason}</td>
                      <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : ''}`}>{formatDate(damage.date)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <div className="text-center py-12">
                <Trash2 className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
                <p className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>No damage records</p>
                <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>Click "Record Damage" to add a damage entry</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ========== DAMAGE MODAL ========== */}
      <Modal isOpen={showDamageModal} onClose={() => setShowDamageModal(false)} title="Record Damage">
        <div className="space-y-5">
          <div className="flex items-center gap-4 pb-2">
            <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-orange-900/30 text-orange-400' : 'bg-orange-100 text-orange-600'}`}>
              <Trash2 className="h-7 w-7" />
            </div>
            <div>
              <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>Record Damaged Stock</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Deduct damaged items from inventory</p>
            </div>
          </div>

          <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <div className="space-y-4">
              <div>
                <label className={labelClass}>
                  <Package className="h-4 w-4" /> Medicine <span className="text-red-500">*</span>
                </label>
                <select
                  value={damageForm.medicineId}
                  onChange={e => setDamageForm({ ...damageForm, medicineId: e.target.value, batchId: '' })}
                  className={inputClass}
                >
                  <option value="">Select Medicine</option>
                  {medicines.filter(m => m.batches.some(b => b.quantity > 0)).map(m => (
                    <option key={m.id} value={m.id}>{m.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className={labelClass}>
                  <Layers className="h-4 w-4" /> Batch <span className="text-red-500">*</span>
                </label>
                <select
                  value={damageForm.batchId}
                  onChange={e => setDamageForm({ ...damageForm, batchId: e.target.value })}
                  className={inputClass}
                >
                  <option value="">Select Batch</option>
                  {medicines.find(m => m.id === damageForm.medicineId)?.batches.filter(b => b.quantity > 0).map(b => (
                    <option key={b.id} value={b.id}>{b.batchNumber} (Qty: {b.quantity})</option>
                  ))}
                </select>
              </div>
              <div>
                <label className={labelClass}>
                  Quantity <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  value={damageForm.quantity}
                  onChange={e => setDamageForm({ ...damageForm, quantity: e.target.value })}
                  placeholder="Enter damaged quantity"
                  className={inputClass}
                />
              </div>
              <div>
                <label className={labelClass}>
                  Reason <span className="text-red-500">*</span>
                </label>
                <textarea
                  value={damageForm.reason}
                  onChange={e => setDamageForm({ ...damageForm, reason: e.target.value })}
                  placeholder="Describe the reason for damage..."
                  className={`${inputClass} min-h-[100px]`}
                />
              </div>
            </div>
          </div>

          <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <button
              onClick={() => setShowDamageModal(false)}
              className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
            >
              Cancel
            </button>
            <button
              onClick={handleRecordDamage}
              className="px-5 py-2.5 bg-gradient-to-r from-orange-600 to-red-600 hover:from-orange-700 hover:to-red-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2"
            >
              <Trash2 className="h-4 w-4" /> Record Damage
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
};


