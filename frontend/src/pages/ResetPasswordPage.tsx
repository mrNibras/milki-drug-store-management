import React, { useState, useEffect } from 'react';
import { useNavigate, Link, useSearchParams } from 'react-router-dom';
import { Store, Lock, Eye, EyeOff, AlertCircle, Moon, Sun } from 'lucide-react';
import { useThemeStore } from '../store/themeStore';
import { useTranslation } from '../i18n/LanguageContext';
import { api, ResetPasswordRequest } from '../services/api';

export const ResetPasswordPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();
  const { theme, toggleTheme } = useThemeStore();
  const { t } = useTranslation();
  const isDark = theme === 'dark';

  useEffect(() => {
    if (!token) {
      setError(t.auth.invalidToken);
    }
  }, [token]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (password.length < 6) {
      setError(t.common.passwordMinLength);
      return;
    }
    if (password !== confirmPassword) {
      setError(t.common.passwordsDoNotMatch);
      return;
    }

    setLoading(true);
    try {
      await api.post('/auth/reset-password', { token, newPassword: password } as ResetPasswordRequest);
      setSuccess(t.common.passwordResetSuccessRedirecting);
      setTimeout(() => navigate('/login'), 2000);
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
            {t.common.createANewPasswordLine1}<br />
            <span className="text-emerald-200">{t.common.createANewPasswordLine2}</span>
          </h2>
          <p className="text-emerald-100 text-lg max-w-md">
            {t.common.makeItStrongAndEasy}
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
              <h1 className="text-xl font-bold text-white lg:text-gray-900">{t.common.appName}</h1>
          </div>

          <div>
            <h2 className="text-2xl font-bold text-gray-900">{t.auth.resetPassword}</h2>
            <p className="mt-2 text-sm text-gray-500">{t.common.enterYourNewPassword}</p>
          </div>

          {success && (
            <div className="flex items-center gap-2 rounded-lg p-3 bg-emerald-50 border border-emerald-200">
              <p className="text-sm text-emerald-600">{success}</p>
            </div>
          )}

          {error && (
            <div className="flex items-center gap-2 rounded-lg p-3 bg-red-50 border border-red-200">
              <AlertCircle className="h-5 w-5 text-red-500 flex-shrink-0" />
              <p className="text-sm text-red-500">{error}</p>
            </div>
          )}

          {!success && (
            <form onSubmit={handleSubmit} className="space-y-5">
              <div>
                <label className="block text-sm font-medium mb-1.5 text-gray-700">{t.auth.newPassword}</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                  <input
                    type={showPassword ? 'text' : 'password'}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="w-full pl-10 pr-12 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-all text-gray-900 placeholder-gray-400"
                    placeholder="••••••••"
                    required
                    minLength={6}
                  />
                  <button type="button" onClick={() => setShowPassword(!showPassword)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                    {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                  </button>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1.5 text-gray-700">{t.auth.confirmPassword}</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                  <input
                    type={showPassword ? 'text' : 'password'}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-all text-gray-900 placeholder-gray-400"
                    placeholder="••••••••"
                    required
                    minLength={6}
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={loading || !token}
                className="w-full py-3 px-4 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white rounded-xl font-medium transition-all shadow-lg disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {loading ? (
                  <div className="h-5 w-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : t.auth.resetPassword}
              </button>
            </form>
          )}

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
