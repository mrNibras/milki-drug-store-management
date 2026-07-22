import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useEffect } from 'react';
import { MainLayout } from './layouts/MainLayout';
import { LoginPage } from './pages/LoginPage';
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
import { DashboardPage } from './pages/DashboardPage';
import { MedicinesPage } from './pages/MedicinesPage';
import { CategoriesPage } from './pages/CategoriesPage';
import { UnitTypesPage } from './pages/UnitTypesPage';
import { POSPage } from './pages/POSPage';
import { InventoryPage } from './pages/InventoryPage';
import { PurchasesPage } from './pages/PurchasesPage';
import { SuppliersPage } from './pages/SuppliersPage';
import { ReportsPage } from './pages/ReportsPage';
import { NotificationsPage } from './pages/NotificationsPage';
import { DamageExpiryPage } from './pages/DamageExpiryPage';
import { UsersPage } from './pages/UsersPage';
import { SettingsPage } from './pages/SettingsPage';
import { AuditLogsPage } from './pages/AuditLogsPage';
import { BranchesPage } from './pages/BranchesPage';
import { CosmeticsPage } from './pages/CosmeticsPage';
import { CosmeticCategoriesPage } from './pages/CosmeticCategoriesPage';
import { ProtectedRoute } from './components/ProtectedRoute';
import { useAppStore } from './store/appStore';

export default function App() {
  const isAuthenticated = useAppStore((state) => state.isAuthenticated);

  useEffect(() => {
    try {
      const storedToken = localStorage.getItem('auth_token');
      const storedUser = localStorage.getItem('current_user');
      const storedBranch = localStorage.getItem('current_branch');
      if (storedToken && storedUser) {
        useAppStore.setState({
          token: storedToken,
          currentUser: JSON.parse(storedUser),
          currentBranch: storedBranch ? JSON.parse(storedBranch) : null,
          isAuthenticated: true,
        });
      }
    } catch (e) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('current_user');
      localStorage.removeItem('current_branch');
    }
  }, []);

  // Proactive token refresh every 7 hours to keep the session alive.
  useEffect(() => {
    if (!isAuthenticated) return;
    const interval = setInterval(() => {
      useAppStore.getState().refreshToken();
    }, 7 * 60 * 60 * 1000);
    return () => clearInterval(interval);
  }, [isAuthenticated]);

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/" element={isAuthenticated ? <MainLayout /> : <Navigate to="/login" replace />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="medicines" element={<MedicinesPage />} />
          <Route path="categories" element={<CategoriesPage />} />
          <Route path="unit-types" element={<UnitTypesPage />} />
          <Route path="pos" element={<POSPage />} />
          <Route path="inventory" element={<InventoryPage />} />
          <Route path="damages" element={<DamageExpiryPage />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="notifications" element={<NotificationsPage />} />
          <Route path="purchases" element={<ProtectedRoute roles={['admin']}><PurchasesPage /></ProtectedRoute>} />
          <Route path="suppliers" element={<ProtectedRoute roles={['admin']}><SuppliersPage /></ProtectedRoute>} />
          <Route path="users" element={<ProtectedRoute roles={['admin']}><UsersPage /></ProtectedRoute>} />
          <Route path="settings" element={<ProtectedRoute roles={['admin']}><SettingsPage /></ProtectedRoute>} />
          <Route path="audit-logs" element={<ProtectedRoute roles={['admin']}><AuditLogsPage /></ProtectedRoute>} />
          <Route path="branches" element={<ProtectedRoute roles={['admin']}><BranchesPage /></ProtectedRoute>} />
          <Route path="cosmetics" element={<ProtectedRoute roles={['admin']}><CosmeticsPage /></ProtectedRoute>} />
          <Route path="cosmetic-categories" element={<ProtectedRoute roles={['admin']}><CosmeticCategoriesPage /></ProtectedRoute>} />
        </Route>
        <Route path="*" element={<Navigate to={isAuthenticated ? "/dashboard" : "/login"} replace />} />
      </Routes>
    </BrowserRouter>
  );
}
