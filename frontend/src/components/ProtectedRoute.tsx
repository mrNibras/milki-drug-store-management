import { Navigate } from 'react-router-dom';
import { useAppStore } from '../store/appStore';
import type { ReactNode } from 'react';

type ProtectedRouteProps = {
  children: ReactNode;
  roles?: Array<'admin' | 'pharmacist'>;
};

export function ProtectedRoute({ children, roles }: ProtectedRouteProps) {
  const isAuthenticated = useAppStore((state) => state.isAuthenticated);
  const userRole = useAppStore((state) => state.currentUser?.role);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && roles.length > 0 && userRole && !roles.includes(userRole)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}
