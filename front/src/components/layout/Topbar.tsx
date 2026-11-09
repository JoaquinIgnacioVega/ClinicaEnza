import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { Bell, ChevronDown, LogOut, User as UserIcon } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export const Topbar: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [showMenu, setShowMenu] = useState(false);

  // Datos de usuario mostrados (del contexto o fallback para el prototipo visual)
  const displayName = user?.nombreCompleto || 'Sofía Ramírez';
  const displayRole = user?.rolNombre || 'Paciente';

  // Obtener iniciales
  const getInitials = (name: string) => {
    const parts = name.replace(/,/g, '').trim().split(' ');
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    }
    return name.slice(0, 2).toUpperCase();
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="h-16 bg-white border-b border-slate-200/80 px-6 sm:px-8 flex items-center justify-between sticky top-0 z-20">
      {/* Título de la clínica visible en móvil */}
      <div className="lg:hidden flex items-center gap-2">
        <div className="w-7 h-7 rounded-lg bg-[#0d5c68] flex items-center justify-center text-white">
          <svg className="w-4 h-4 fill-current" viewBox="0 0 24 24">
            <path d="M19 10.5h-5.5V5c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v5.5H5c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5h5.5V19c0 .83.67 1.5 1.5 1.5s1.5-.67 1.5-1.5v-5.5H19c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5z" />
          </svg>
        </div>
        <span className="font-bold text-slate-800 text-sm">Clínica Enza</span>
      </div>

      <div className="hidden lg:block text-xs font-medium text-slate-400">
        Portal de Autogestión de Salud
      </div>

      {/* Acciones de la derecha: Notificaciones + Perfil */}
      <div className="flex items-center gap-4 ml-auto">
        {/* Notificaciones */}
        <button
          type="button"
          className="relative p-2 rounded-full text-slate-500 hover:text-slate-700 hover:bg-slate-100 transition-colors cursor-pointer"
          aria-label="Notificaciones"
        >
          <Bell className="w-5 h-5" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-rose-500 ring-2 ring-white"></span>
        </button>

        {/* Separador */}
        <div className="h-6 w-px bg-slate-200"></div>

        {/* Píldora de Perfil de Usuario */}
        <div className="relative">
          <button
            type="button"
            onClick={() => setShowMenu(!showMenu)}
            className="flex items-center gap-3 p-1.5 pr-2.5 rounded-full hover:bg-slate-50 border border-transparent hover:border-slate-200 transition-all cursor-pointer group"
          >
            {/* Avatar circular con iniciales */}
            <div className="w-9 h-9 rounded-full bg-[#05242c] text-white flex items-center justify-center text-xs font-bold tracking-wider shrink-0 shadow-2xs">
              {getInitials(displayName)}
            </div>

            <div className="text-left hidden sm:block">
              <p className="text-xs font-bold text-slate-800 group-hover:text-[#0d5c68] transition-colors leading-tight">
                {displayName}
              </p>
              <p className="text-[11px] text-slate-400 font-medium leading-none mt-0.5">
                {displayRole}
              </p>
            </div>

            <ChevronDown className="w-4 h-4 text-slate-400 group-hover:text-slate-600 transition-transform duration-200" />
          </button>

          {/* Menú desplegable */}
          {showMenu && (
            <div className="absolute right-0 mt-2 w-52 bg-white rounded-xl shadow-lg border border-slate-100 py-1.5 z-30 animate-fadeIn">
              <div className="px-4 py-2 border-b border-slate-100 sm:hidden">
                <p className="text-xs font-bold text-slate-800">{displayName}</p>
                <p className="text-[11px] text-slate-400">{displayRole}</p>
              </div>

              <button
                type="button"
                onClick={() => {
                  setShowMenu(false);
                  navigate('/historia-clinica');
                }}
                className="w-full px-4 py-2 text-left text-xs font-medium text-slate-700 hover:bg-slate-50 flex items-center gap-2"
              >
                <UserIcon className="w-4 h-4 text-slate-400" />
                Mi Perfil / Historia
              </button>

              <button
                type="button"
                onClick={handleLogout}
                className="w-full px-4 py-2 text-left text-xs font-medium text-rose-600 hover:bg-rose-50 flex items-center gap-2 border-t border-slate-100"
              >
                <LogOut className="w-4 h-4 text-rose-500" />
                Cerrar sesión
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
};
