import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Shield, Users, Building2, Activity, ArrowRight, LogOut, CheckCircle2 } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { Topbar } from '../../components/layout/Topbar';

export const AdministradorDashboardPage: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const nombreUsuario = user?.nombreCompleto || 'Carlos González';

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col">
      <Topbar />

      <main className="flex-1 p-6 sm:p-8 lg:p-10 max-w-6xl mx-auto w-full space-y-8 animate-fadeIn">
        {/* Encabezado Principal */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <div className="flex items-center gap-2 text-xs font-bold text-[#0d5c68] uppercase tracking-wider">
              <Shield className="w-4 h-4" />
              <span>Portal de Administración</span>
              <span className="ml-2 px-2.5 py-0.5 rounded-full text-[10px] font-semibold bg-indigo-100 text-indigo-800 border border-indigo-200">
                Sesión Activa
              </span>
            </div>
            <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight mt-1">
              Bienvenido, {nombreUsuario}
            </h1>
            <p className="text-xs sm:text-sm text-slate-500 mt-1">
              Módulo de gestión integral, configuración de infraestructura y usuarios del sistema.
            </p>
          </div>

          <button
            type="button"
            onClick={handleLogout}
            className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl border border-slate-200 hover:bg-white hover:border-slate-300 text-slate-700 text-xs sm:text-sm font-semibold shadow-xs transition-all cursor-pointer self-start sm:self-auto"
          >
            <LogOut className="w-4 h-4 text-slate-400" />
            <span>Cerrar sesión</span>
          </button>
        </div>

        {/* Banner Informativo de Asignación de Módulo */}
        <div className="bg-white rounded-2xl border border-slate-200/90 p-6 sm:p-7 shadow-xs">
          <div className="flex items-start gap-4">
            <div className="w-12 h-12 rounded-2xl bg-[#0d5c68]/10 text-[#0d5c68] flex items-center justify-center shrink-0 border border-[#0d5c68]/20">
              <Shield className="w-6 h-6" />
            </div>
            <div className="space-y-2 flex-1">
              <h2 className="text-base sm:text-lg font-bold text-slate-800">
                Módulo asignado a tus compañeros de equipo
              </h2>
              <p className="text-xs sm:text-sm text-slate-600 leading-relaxed max-w-3xl">
                Has iniciado sesión correctamente como <strong>Administrador</strong>. La navegación a este espacio está correctamente protegida y separada del portal médico. La interfaz y flujos operativos específicos de administración y auditoría están reservados para la implementación de tus compañeros.
              </p>
              <div className="pt-2 flex flex-wrap items-center gap-3 text-xs text-slate-500 font-medium">
                <span className="flex items-center gap-1 text-emerald-700">
                  <CheckCircle2 className="w-4 h-4 text-emerald-600" /> Autenticación JWT activa
                </span>
                <span>•</span>
                <span>Rol: Administrador (ID Rol: 0)</span>
                <span>•</span>
                <span>Acceso exclusivo a endpoints de administración</span>
              </div>
            </div>
          </div>
        </div>

        {/* Tarjetas de Funcionalidades Previstas para Administración */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-xs flex flex-col justify-between">
            <div>
              <div className="w-10 h-10 rounded-xl bg-indigo-50 text-[#0d5c68] flex items-center justify-center mb-4">
                <Users className="w-5 h-5" />
              </div>
              <h3 className="font-bold text-slate-800 text-sm">Gestión de Usuarios</h3>
              <p className="text-xs text-slate-500 mt-1 leading-relaxed">
                Alta y modificación de profesionales médicos, recepcionistas y credenciales de acceso.
              </p>
            </div>
            <div className="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
              <span>En desarrollo</span>
            </div>
          </div>

          <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-xs flex flex-col justify-between">
            <div>
              <div className="w-10 h-10 rounded-xl bg-teal-50 text-[#0d5c68] flex items-center justify-center mb-4">
                <Building2 className="w-5 h-5" />
              </div>
              <h3 className="font-bold text-slate-800 text-sm">Consultorios y Especialidades</h3>
              <p className="text-xs text-slate-500 mt-1 leading-relaxed">
                Infraestructura física, habilitación de consultorios, pisos y sectores de atención.
              </p>
            </div>
            <div className="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
              <span>En desarrollo</span>
            </div>
          </div>

          <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-xs flex flex-col justify-between">
            <div>
              <div className="w-10 h-10 rounded-xl bg-amber-50 text-amber-700 flex items-center justify-center mb-4">
                <Activity className="w-5 h-5" />
              </div>
              <h3 className="font-bold text-slate-800 text-sm">Auditoría y Métricas</h3>
              <p className="text-xs text-slate-500 mt-1 leading-relaxed">
                Monitoreo de actividad, reportes de turnos atendidos, cancelaciones y ocupación.
              </p>
            </div>
            <div className="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
              <span>En desarrollo</span>
            </div>
          </div>
        </div>

        {/* Acceso Rápido al Módulo de Pacientes */}
        <div className="p-4 rounded-xl bg-slate-100/70 border border-slate-200 flex flex-col sm:flex-row items-center justify-between gap-3 text-xs text-slate-600">
          <span>¿Querés probar la vista de turnos de pacientes desarrollada por el equipo?</span>
          <button
            type="button"
            onClick={() => navigate('/mis-turnos')}
            className="text-[#0d5c68] hover:text-[#094852] font-semibold flex items-center gap-1 cursor-pointer hover:underline"
          >
            <span>Ver Mis Turnos (Paciente)</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </button>
        </div>
      </main>
    </div>
  );
};
