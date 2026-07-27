import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Store, Mail, ArrowLeft, Moon, Sun } from 'lucide-react';
import { useThemeStore } from '../store/themeStore';
import { useTranslation } from '../i18n/LanguageContext';
import { api, ForgotPasswordRequest } from '../services/api';

export const ForgotPasswordPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { theme, toggleTheme } = useThemeStore();
  const { t } = useTranslation();
  const isDark = theme === 'dark';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('');
    setError('');
    setLoading(true);

    try {
      await api.post('/auth/forgot-password', { email } as ForgotPasswordRequest);
      setMessage(t.auth.passwordResetSent);
      setEmail('');
    } catch (err: any) {
      setError(err.response?.data?.message || t.common.somethingWentWrong);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={isDark ? 'min-h-screen flex bg-gray-900' : 'min-h-screen flex'}>
      <button onClick={toggleTheme} className={isDark ? 'fixed top-4 right-4 z-50 p-2 rounded-lg transition-colors bg-gray-800 text-yellow-400 hover:bg-gray-700' : 'fixed top-4 right-4 z-50 p-2 rounded-lg transition-colors bg-white text-gray-600 hover:bg-gray-100 shadow-md'}>
        {theme === 'light' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
      </button>

      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-emerald-600 via-teal-600 to-cyan-700 p-12 flex-col justify-between">
             <div>
               <div className="flex items-center gap-3 mb-8">
                 <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-white/20 backdrop-blur-sm shadow-lg">
                   <Store className="h-8 w-8 text-white" />
                 </div>
                 <div>
                   <h1 className="text-2xl font-bold text-white">{t.common.appName}</h1>
                    <p className="text-emerald-100 text-sm">{t.common.managementSystem}</p>
                 </div>
               </div>
             </div>
        <div className="space-y-6">
          <h2 className="text-4xl font-bold text-white leading-tight">
            {t.common.resetYourPassword}<br />
            <span className="text-emerald-200">{t.common.passwordSecurely}</span>
          </h2>
          <p className="text-emerald-100 text-lg max-w-md">
            {t.common.enterYourRegisteredEmail}
          </p>
        </div>
        <p className="text-emerald-200 text-sm">© 2026 Milki Drug Store. All rights reserved.</p>
      </div>

      <div className="w-full lg:w-1/2 flex items-center justify-center p-8">
        <div className="w-full max-w-md space-y-8">
          <div className="lg:hidden text-center">
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-600 shadow-lg mb-4">
              <Store className="h-8 w-8 text-white" />
            </div>
            <h1 className="text-xl font-bold text-white lg:text-gray-900">Milki Drug Store</h1>
          </div>

          <div>
            <button onClick={() => navigate('/login')} className="flex items-center gap-2 text-sm font-medium mb-4 text-gray-600 hover:text-gray-900">
              <ArrowLeft className="h-4 w-4" /> {t.common.backToLogin}
            </button>
            <h2 className="text-2xl font-bold text-gray-900">{t.auth.forgotPassword}</h2>
            <p className="mt-2 text-sm text-gray-500">{t.common.enterYourEmailAndResetLink}</p>
          </div>

          {message && (
            <div className="flex items-center gap-2 rounded-lg p-3 bg-emerald-50 border border-emerald-200">
              <p className="text-sm text-emerald-600">{message}</p>
            </div>
          )}

          {error && (
            <div className="flex items-center gap-2 rounded-lg p-3 bg-red-50 border border-red-200">
              <p className="text-sm text-red-500">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-sm font-medium mb-1.5 text-gray-700">{t.common.emailAddress}</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-all text-gray-900 placeholder-gray-400"
                  placeholder="admin@milki.com"
                  required
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3 px-4 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white rounded-xl font-medium transition-all shadow-lg disabled:opacity-50 flex items-center justify-center gap-2"
            >
               {loading ? (
                 <div className="h-5 w-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
               ) : t.auth.sendResetLink}
            </button>
          </form>

          <div className="text-center">
            <p className="text-sm text-gray-500">
              {t.common.rememberPassword} <Link to="/login" className="text-emerald-500 hover:text-emerald-400 font-medium">{t.common.signIn}</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
