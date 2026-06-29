import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useEffect } from 'react';
import { MainLayout } from './layouts/MainLayout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { MedicinesPage } from './pages/MedicinesPage';
import { POSPage } from './pages/POSPage';
import { InventoryPage } from './pages/InventoryPage';
import { PurchasesPage } from './pages/PurchasesPage';
import { SuppliersPage } from './pages/SuppliersPage';
import { ReportsPage } from './pages/ReportsPage';
import { NotificationsPage } from './pages/NotificationsPage';
import { DamageExpiryPage } from './pages/DamageExpiryPage';
import { UsersPage } from './pages/UsersPage';
import { SettingsPage } from './pages/SettingsPage';
import { useAppStore } from './store/appStore';

export default function App() {
  const isAuthenticated = useAppStore((state) => state.isAuthenticated);

  useEffect(() => {
    try {
      const storedToken = localStorage.getItem('auth_token');
      const storedUser = localStorage.getItem('current_user');
      if (storedToken && storedUser) {
        useAppStore.setState({ token: storedToken, currentUser: JSON.parse(storedUser), isAuthenticated: true });
      }
    } catch (e) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('current_user');
    }
  }, []);

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/" element={isAuthenticated ? <MainLayout /> : <Navigate to="/login" replace />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="medicines" element={<MedicinesPage />} />
          <Route path="pos" element={<POSPage />} />
          <Route path="inventory" element={<InventoryPage />} />
          <Route path="purchases" element={<PurchasesPage />} />
          <Route path="suppliers" element={<SuppliersPage />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="notifications" element={<NotificationsPage />} />
          <Route path="damages" element={<DamageExpiryPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>
        <Route path="*" element={<Navigate to={isAuthenticated ? "/dashboard" : "/login"} replace />} />
      </Routes>
    </BrowserRouter>
  );
}
