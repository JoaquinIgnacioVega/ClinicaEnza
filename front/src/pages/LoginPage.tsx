import React from 'react';
import { Navigate } from 'react-router-dom';
import { LoginForm } from '../components/auth/LoginForm';
import { useAuth } from '../context/AuthContext';
import { getHomeRouteForUser } from '../routes/ProtectedRoute';

export const LoginPage: React.FC = () => {
  const { user, isAuthenticated } = useAuth();

  // Si ya está autenticado, redirigir automáticamente al portal que le corresponde
  if (isAuthenticated) {
    const targetRoute = getHomeRouteForUser(user);
    return <Navigate to={targetRoute} replace />;
  }

  return (
    <div className="relative min-h-screen w-full bg-gradient-to-br from-[#063b46] via-[#042e37] to-[#021f26] flex items-center justify-center p-4 sm:p-6 lg:p-8 overflow-hidden">
      {/* Círculos concéntricos decorativos de fondo con SVG */}
      <div className="absolute inset-0 pointer-events-none opacity-10 flex items-center justify-center">
        <svg className="w-full h-full max-w-5xl" viewBox="0 0 800 800" fill="none">
          <circle cx="400" cy="400" r="280" stroke="white" strokeWidth="1" strokeDasharray="4 4" />
          <circle cx="400" cy="400" r="420" stroke="white" strokeWidth="1" />
          <circle cx="400" cy="400" r="560" stroke="white" strokeWidth="1" strokeDasharray="6 6" />
        </svg>
      </div>

      {/* Tarjeta del Formulario de Inicio de Sesión Centrada */}
      <div className="relative z-10 w-full max-w-lg bg-white rounded-3xl shadow-2xl p-6 sm:p-10 border border-white/20 animate-fadeIn my-auto">
        <LoginForm />
      </div>
    </div>
  );
};

