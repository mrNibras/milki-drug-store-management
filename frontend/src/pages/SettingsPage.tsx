import React, { useState, useEffect } from 'react';
import { Save, Building2, Bell, Database, Globe, Shield, Download, Upload, Key, Languages } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Button } from '../components/ui/Button';
import { backupDatabase, restoreDatabase } from '../services/api';
import { useTranslation } from '../i18n/LanguageContext';

export const SettingsPage: React.FC = () => {
  const { settings, updateSettings, fetchSettings, changePassword } = useAppStore();
  const { theme } = useThemeStore();
  const { language, setLanguage, t } = useTranslation();
  const isDark = theme === 'dark';
  const [formData, setFormData] = useState({ ...settings });
  const [showSuccess, setShowSuccess] = useState(false);
  const [passwordForm, setPasswordForm] = useState({ current: '', newPassword: '', confirm: '' });
  const [passwordMessage, setPasswordMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [dbMessage, setDbMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [isBackingUp, setIsBackingUp] = useState(false);
  const [isRestoring, setIsRestoring] = useState(false);
  const restoreFileInputRef = React.useRef<HTMLInputElement>(null);

  const handleBackup = async () => {
    setIsBackingUp(true);
    setDbMessage(null);
    try {
      const blob = await backupDatabase();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `MilkiDrugStore_Backup_${new Date().toISOString().split('T')[0]}.db`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);

      setDbMessage({ type: 'success', text: t.settings.databaseBackupDownloaded });
    } catch (e: any) {
      setDbMessage({ type: 'error', text: e.response?.data?.message || e.message || t.settings.failedToBackup });
    } finally {
      setIsBackingUp(false);
    }
  };

  const handleRestoreClick = () => {
    restoreFileInputRef.current?.click();
  };

  const handleRestore = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setIsRestoring(true);
    setDbMessage(null);
    try {
      await restoreDatabase(file);
      setDbMessage({ type: 'success', text: t.settings.databaseRestoredRefresh });
    } catch (e: any) {
      setDbMessage({ type: 'error', text: e.response?.data?.message || e.message || t.settings.failedToRestore });
    } finally {
      setIsRestoring(false);
      if (restoreFileInputRef.current) {
        restoreFileInputRef.current.value = '';
      }
    }
  };

  useEffect(() => {
    fetchSettings();
    setFormData({ ...settings });
  }, [fetchSettings, settings]);

  const handleSave = async () => {
    try {
      await updateSettings(formData);
      setShowSuccess(true);
      setTimeout(() => setShowSuccess(false), 3000);
    } catch (e: any) {
      console.error(t.settings.failedToSaveSettings, e);
    }
  };

  const handleChangePassword = async () => {
    setPasswordMessage(null);
    if (!passwordForm.current || !passwordForm.newPassword) {
      setPasswordMessage({ type: 'error', text: t.settings.pleaseFillAllPasswordFields });
      return;
    }
    if (passwordForm.newPassword !== passwordForm.confirm) {
      setPasswordMessage({ type: 'error', text: t.settings.passwordsDoNotMatch });
      return;
    }
    if (passwordForm.newPassword.length < 6) {
      setPasswordMessage({ type: 'error', text: t.settings.passwordMinLength });
      return;
    }

    const result = await changePassword(passwordForm.current, passwordForm.newPassword);
    if (result.ok) {
      setPasswordMessage({ type: 'success', text: t.settings.passwordChangedSuccessfully });
      setPasswordForm({ current: '', newPassword: '', confirm: '' });
    } else {
      setPasswordMessage({ type: 'error', text: result.message || t.settings.failedToChangePassword });
    }
  };

  const inputClass = `w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm ${
    isDark ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-400' : 'bg-white border-gray-300 text-gray-900'
  }`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{t.settings.title}</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>{t.common.appName}</p>
        </div>
          <Button onClick={handleSave}>
            <Save className="h-4 w-4" /> {t.common.save} {t.settings.title}
          </Button>
      </div>

      {showSuccess && (
        <div className={`rounded-xl p-4 flex items-center gap-3 ${isDark ? 'bg-emerald-900/20 border border-emerald-800' : 'bg-emerald-50 border border-emerald-200'}`}>
          <div className={`flex h-8 w-8 items-center justify-center rounded-full ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600'}`}>✓</div>
          <p className={`text-sm font-medium ${isDark ? 'text-emerald-400' : 'text-emerald-700'}`}>{t.settings.settingsSavedSuccessfully}</p>
        </div>
      )}

      {dbMessage && (
        <div className={`rounded-xl p-4 flex items-center gap-3 ${dbMessage.type === 'success' ? (isDark ? 'bg-emerald-900/20 border border-emerald-800' : 'bg-emerald-50 border border-emerald-200') : (isDark ? 'bg-red-900/20 border border-red-800' : 'bg-red-50 border border-red-200')}`}>
          <div className={`flex h-8 w-8 items-center justify-center rounded-full ${dbMessage.type === 'success' ? (isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600') : (isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-600')}`}>
            {dbMessage.type === 'success' ? '✓' : '✗'}
          </div>
          <p className={`text-sm font-medium ${dbMessage.type === 'success' ? (isDark ? 'text-emerald-400' : 'text-emerald-700') : (isDark ? 'text-red-400' : 'text-red-700')}`}>{dbMessage.text}</p>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Pharmacy Information */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center gap-3 mb-6">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <Building2 className="h-5 w-5" />
            </div>
            <div>
               <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{t.settings.pharmacyInformation}</h3>
               <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.basicPharmacyDetails}</p>
            </div>
          </div>
          <div className="space-y-4">
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.pharmacyName}</label>
              <input type="text" value={formData.pharmacyName} onChange={e => setFormData({ ...formData, pharmacyName: e.target.value })} className={inputClass} />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.address}</label>
              <input type="text" value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} className={inputClass} />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.phone}</label>
                <input type="text" value={formData.phone} onChange={e => setFormData({ ...formData, phone: e.target.value })} className={inputClass} />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.auth.email}</label>
                <input type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} className={inputClass} />
              </div>
            </div>
          </div>
        </div>

        {/* Inventory Settings */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center gap-3 mb-6">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600'}`}>
              <Bell className="h-5 w-5" />
            </div>
            <div>
               <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{t.settings.inventorySettings}</h3>
               <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.alertThresholdsAndPreferences}</p>
            </div>
          </div>
          <div className="space-y-4">
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.lowStockThreshold}</label>
              <input type="number" value={formData.lowStockThreshold} onChange={e => setFormData({ ...formData, lowStockThreshold: Number(e.target.value) })} className={inputClass} />
              <p className={`text-xs mt-1 ${isDark ? 'text-gray-500' : 'text-gray-500'}`}>{t.settings.alertWhenStockFallsBelow}</p>
            </div>
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.expiryAlertMonths}</label>
              <input type="number" value={formData.expiryAlertMonths} onChange={e => setFormData({ ...formData, expiryAlertMonths: Number(e.target.value) })} className={inputClass} />
              <p className={`text-xs mt-1 ${isDark ? 'text-gray-500' : 'text-gray-500'}`}>{t.settings.alertMonthsBeforeExpiry}</p>
            </div>
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.currency}</label>
              <select value={formData.currency} onChange={e => setFormData({ ...formData, currency: e.target.value })} className={inputClass}>
                <option value="ETB">ETB (Ethiopian Birr)</option>
                <option value="USD">USD (US Dollar)</option>
                <option value="EUR">EUR (Euro)</option>
              </select>
            </div>
          </div>
        </div>

        {/* Database Management */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center gap-3 mb-6">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-violet-900/30 text-violet-400' : 'bg-violet-100 text-violet-600'}`}>
              <Database className="h-5 w-5" />
            </div>
            <div>
               <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{t.settings.databaseManagement}</h3>
               <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.backupAndRestoreData}</p>
            </div>
          </div>
          <div className="space-y-3">
            <button
              onClick={handleBackup}
              disabled={isBackingUp}
              className={`w-full flex items-center gap-3 px-4 py-3 border rounded-lg transition-colors ${
                isDark ? 'border-gray-600 hover:bg-gray-700' : 'border-gray-200 hover:bg-gray-50'
              } ${isBackingUp ? 'opacity-50 cursor-not-allowed' : ''}`}
            >
              <Download className="h-5 w-5 text-blue-500" />
              <div className="text-left">
                 <p className={`text-sm font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{isBackingUp ? t.settings.backingUp : t.settings.backupDatabase}</p>
                 <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.downloadFullBackup}</p>
              </div>
            </button>
            <button
              onClick={handleRestoreClick}
              disabled={isRestoring}
              className={`w-full flex items-center gap-3 px-4 py-3 border rounded-lg transition-colors ${
                isDark ? 'border-gray-600 hover:bg-gray-700' : 'border-gray-200 hover:bg-gray-50'
              } ${isRestoring ? 'opacity-50 cursor-not-allowed' : ''}`}
            >
              <Upload className="h-5 w-5 text-emerald-500" />
              <div className="text-left">
                 <p className={`text-sm font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{isRestoring ? t.settings.restoring : t.settings.restoreDatabase}</p>
                 <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.restoreFromBackupFile}</p>
              </div>
            </button>
            <input
              ref={restoreFileInputRef}
              type="file"
              accept=".db"
              onChange={handleRestore}
              className="hidden"
            />
          </div>
        </div>

        {/* System Info */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center gap-3 mb-6">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-600'}`}>
              <Globe className="h-5 w-5" />
            </div>
            <div>
               <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{t.settings.systemInformation}</h3>
               <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.applicationDetails}</p>
            </div>
          </div>
          <div className="space-y-3">
            {[
              { label: t.settings.version, value: '1.0.0' },
              { label: t.settings.architecture, value: 'Clean Architecture' },
              { label: t.settings.inventoryMethod, value: 'FEFO' },
              { label: t.settings.authentication, value: 'JWT' },
              { label: t.settings.database, value: 'SQL Server' },
            ].map(item => (
              <div key={item.label} className={`flex items-center justify-between py-2 border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
                <span className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{item.label}</span>
                <span className={`text-sm font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{item.value}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Language & Localization */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6`}>
          <div className="flex items-center gap-3 mb-6">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-indigo-900/30 text-indigo-400' : 'bg-indigo-100 text-indigo-600'}`}>
              <Languages className="h-5 w-5" />
            </div>
            <div>
               <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{t.settings.language}</h3>
               <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.selectPreferredLanguage}</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <button
              onClick={async () => {
                setLanguage('en');
                await updateSettings({ language: 'en' });
              }}
              className={`flex-1 py-2.5 px-4 rounded-lg border text-sm font-medium transition-all ${
                language === 'en'
                  ? isDark
                    ? 'border-indigo-500 bg-indigo-900/30 text-indigo-300'
                    : 'border-indigo-500 bg-indigo-50 text-indigo-700'
                  : isDark
                    ? 'border-gray-600 text-gray-300 hover:bg-gray-700'
                    : 'border-gray-200 text-gray-700 hover:bg-gray-50'
              }`}
            >
              English
            </button>
            <button
              onClick={async () => {
                setLanguage('am');
                await updateSettings({ language: 'am' });
              }}
              className={`flex-1 py-2.5 px-4 rounded-lg border text-sm font-medium transition-all ${
                language === 'am'
                  ? isDark
                    ? 'border-indigo-500 bg-indigo-900/30 text-indigo-300'
                    : 'border-indigo-500 bg-indigo-50 text-indigo-700'
                  : isDark
                    ? 'border-gray-600 text-gray-300 hover:bg-gray-700'
                    : 'border-gray-200 text-gray-700 hover:bg-gray-50'
              }`}
            >
              አማርኛ
            </button>
          </div>
        </div>

        {/* Security */}
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-6 lg:col-span-2`}>
          <div className="flex items-center gap-3 mb-6">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-600'}`}>
              <Shield className="h-5 w-5" />
            </div>
            <div>
               <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{t.settings.securitySettings}</h3>
               <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{t.settings.passwordAndAuthenticationSettings}</p>
            </div>
          </div>

          {passwordMessage && (
            <div className={`mb-4 p-3 rounded-lg text-sm ${
              passwordMessage.type === 'success'
                ? isDark ? 'bg-emerald-900/20 border border-emerald-800 text-emerald-400' : 'bg-emerald-50 border border-emerald-200 text-emerald-700'
                : isDark ? 'bg-red-900/20 border border-red-800 text-red-400' : 'bg-red-50 border border-red-200 text-red-700'
            }`}>
              {passwordMessage.text}
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.currentPassword}</label>
              <input
                type="password"
                value={passwordForm.current}
                onChange={e => setPasswordForm({ ...passwordForm, current: e.target.value })}
                className={inputClass}
              />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.auth.newPassword}</label>
              <input
                type="password"
                value={passwordForm.newPassword}
                onChange={e => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                className={inputClass}
              />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>{t.settings.confirmNewPassword}</label>
              <input
                type="password"
                value={passwordForm.confirm}
                onChange={e => setPasswordForm({ ...passwordForm, confirm: e.target.value })}
                className={inputClass}
              />
            </div>
          </div>
          <div className="mt-4">
            <Button variant="secondary" onClick={handleChangePassword}>
              <Key className="h-4 w-4" /> Change Password
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};
