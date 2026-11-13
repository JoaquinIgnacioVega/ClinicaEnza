import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ShieldAlert, LogOut, Stethoscope, ArrowRight, User as UserIcon } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { Button } from '../components/ui/Button';

export const NoAutorizadoPage: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogoutAndLogin = () => {
    logout();
    navigate('/login', { replace: true });
  };

  const handleGoToPatient = () => {
    navigate('/mis-turnos');
  };

  const roleName = user?.rolNombre || 'Usuario';
  const userName = user?.nombreCompleto || 'Usuario identificado';

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

      {/* Tarjeta de Acceso Restringido */}
      <div className="relative z-10 w-full max-w-lg bg-white rounded-3xl shadow-2xl p-6 sm:p-10 border border-white/20 animate-fadeIn my-auto">
        {/* Cabecera / Marca */}
        <header className="flex items-center gap-3 mb-6">
          <div className="w-9 h-9 rounded-lg bg-[#0d5c68] flex items-center justify-center text-white shadow-sm shrink-0">
            <svg className="w-5 h-5 fill-current" viewBox="0 0 24 24">
              <path d="M19 10.5h-5.5V5c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v5.5H5c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5h5.5V19c0 .83.67 1.5 1.5 1.5s1.5-.67 1.5-1.5v-5.5H19c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5z" />
            </svg>
          </div>
          <div>
            <h1 className="text-base font-bold text-slate-900 tracking-tight leading-none">
              Clínica Enza
            </h1>
            <p className="text-[10px] font-semibold tracking-wider text-slate-400 uppercase mt-0.5">
              CONTROL DE ACCESO Y SEGURIDAD
            </p>
          </div>
        </header>

        {/* Ícono de Alerta y Título */}
        <div className="flex items-start gap-4 mb-6">
          <div className="w-12 h-12 rounded-2xl bg-amber-50 border border-amber-200/80 text-amber-600 flex items-center justify-center shrink-0 shadow-xs">
            <ShieldAlert className="w-6 h-6" />
          </div>
          <div>
            <h2 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">
              Acceso Restringido
            </h2>
            <p className="text-xs sm:text-sm text-slate-500 mt-1">
              Portal reservado exclusivamente para profesionales médicos.
            </p>
          </div>
        </div>

        {/* Ficha de la sesión activa */}
        <div className="mb-6 p-4 rounded-2xl bg-slate-50 border border-slate-200/80 space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wider">
              Sesión Iniciada
            </span>
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-100 text-amber-800 border border-amber-200">
              Rol: {roleName}
            </span>
          </div>

          <div className="flex items-center gap-3 pt-1">
            <div className="w-9 h-9 rounded-full bg-slate-200 text-slate-700 flex items-center justify-center font-bold text-xs shrink-0">
              <UserIcon className="w-4 h-4" />
            </div>
            <div>
              <p className="text-sm font-bold text-slate-800 leading-snug">{userName}</p>
              <p className="text-xs text-slate-500">Credenciales validadas en el sistema</p>
            </div>
          </div>

          <div className="pt-2 border-t border-slate-200/70 text-xs text-slate-600 leading-relaxed">
            Has iniciado sesión con el rol de <strong>{roleName}</strong>. El módulo <strong>/medico</strong> (agenda de turnos, configuración de horarios y prescripciones) está restringido a médicos con matrícula habilitada.
          </div>
        </div>

        {/* Acciones */}
        <div className="space-y-3">
          <Button
            type="button"
            onClick={handleLogoutAndLogin}
            className="w-full flex items-center justify-center gap-2"
          >
            <Stethoscope className="w-4 h-4" />
            <span>Iniciar sesión con cuenta de Médico</span>
          </Button>

          <button
            type="button"
            onClick={handleLogoutAndLogin}
            className="w-full py-2.5 px-4 rounded-xl border border-slate-200 hover:border-slate-300 hover:bg-slate-50 text-slate-700 text-xs sm:text-sm font-semibold flex items-center justify-center gap-2 transition-all cursor-pointer"
          >
            <LogOut className="w-4 h-4 text-slate-400" />
            <span>Cerrar sesión actual</span>
          </button>

          <button
            type="button"
            onClick={handleGoToPatient}
            className="w-full py-2 text-center text-xs text-[#0d5c68] hover:text-[#094852] hover:underline font-medium cursor-pointer flex items-center justify-center gap-1.5"
          >
            <span>Ir al Módulo de Turnos de Pacientes</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </button>
        </div>

        {/* Pie de página */}
        <footer className="text-[11px] text-slate-400 text-center pt-5 mt-6 border-t border-slate-100">
          © 2026 Clínica Enza · Seguridad y Políticas de Acceso
        </footer>
      </div>
    </div>
  );
};
