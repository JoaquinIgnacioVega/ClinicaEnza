import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { User } from '../types/auth';

export const getHomeRouteForUser = (user: User | null): string => {
  if (!user) return '/login';
  const role = user.rolNombre?.toLowerCase();
  if (role === 'medico' || user.idMedico != null || user.rol === 2) {
    return '/medico';
  }
  if (role === 'recepcionista' || user.rol === 1) {
    return '/recepcionista';
  }
  if (role === 'administrador' || user.rol === 0) {
    return '/administrador';
  }
  return '/login';
};

interface ProtectedRouteProps {
  allowedRoles?: string[];
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
  const { user, isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="w-8 h-8 border-3 border-[#0d5c68] border-t-transparent rounded-full animate-spin"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Si se especifican roles permitidos, verificar que el rol del usuario coincida
  if (allowedRoles && allowedRoles.length > 0) {
    const userRole = user?.rolNombre?.toLowerCase();
    const hasRole = allowedRoles.some((role) => {
      const lower = role.toLowerCase();
      if (lower === 'medico') {
        return userRole === 'medico' || user?.idMedico != null || user?.rol === 2;
      }
      if (lower === 'recepcionista') {
        return userRole === 'recepcionista' || user?.rol === 1;
      }
      if (lower === 'administrador') {
        return userRole === 'administrador' || user?.rol === 0;
      }
      return userRole === lower;
    });

    if (!hasRole) {
      // Redirigir directamente al portal que le corresponde según su rol
      const targetRoute = getHomeRouteForUser(user);
      return <Navigate to={targetRoute} replace />;
    }
  }

  return <Outlet />;
};
