import React from 'react';
import { LoginForm } from '../components/auth/LoginForm';
import { HeroSection } from '../components/auth/HeroSection';
import { useAuth } from '../context/AuthContext';
import { LogOut, Stethoscope, ClipboardList, ShieldCheck, Calendar, ArrowRight } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();

  const getRoleBadge = (rolNombre: string) => {
    switch (rolNombre.toLowerCase()) {
      case 'medico':
        return {
          icon: <Stethoscope className="w-6 h-6 text-emerald-600" />,
          bg: 'bg-emerald-50 text-emerald-800 border-emerald-200',
          title: 'Panel del Profesional Médico',
          description: 'Gestión de agenda, atención de turnos y prescripciones clínicas.',
          actions: ['Ver turnos de hoy', 'Consultar historias clínicas'],
        };
      case 'recepcionista':
        return {
          icon: <ClipboardList className="w-6 h-6 text-sky-600" />,
          bg: 'bg-sky-50 text-sky-800 border-sky-200',
          title: 'Panel de Recepción y Turnos',
          description: 'Asignación de turnos, recepción de pacientes y disponibilidad médica.',
          actions: ['Nuevo turno', 'Buscar paciente', 'Ver consultorios'],
        };
      default:
        return {
          icon: <ShieldCheck className="w-6 h-6 text-[#0d5c68]" />,
          bg: 'bg-teal-50 text-[#0d5c68] border-teal-200',
          title: 'Panel de Administración General',
          description: 'Gestión integral de usuarios, consultorios y configuración de la clínica.',
          actions: ['Gestión de médicos y horarios', 'Administración de usuarios'],
        };
    }
  };

  return (
    <div className="min-h-screen w-full bg-white flex flex-col lg:grid lg:grid-cols-2">
      {/* Panel Izquierdo: Formulario o Dashboard según autenticación */}
      <div className="flex flex-col justify-center items-center w-full bg-white overflow-y-auto">
        {isAuthenticated && user ? (
          <div className="p-8 sm:p-12 max-w-lg w-full animate-fadeIn">
            {/* Header de bienvenida */}
            <div className="flex items-center gap-3 mb-6">
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
                  GESTIÓN DE TURNOS
                </p>
              </div>
            </div>

            {/* Tarjeta del Rol */}
            {(() => {
              const roleInfo = getRoleBadge(user.rolNombre);
              return (
                <div className={`p-6 rounded-2xl border ${roleInfo.bg} shadow-sm mb-6`}>
                  <div className="flex items-center gap-4 mb-3">
                    <div className="w-12 h-12 rounded-xl bg-white shadow-sm flex items-center justify-center shrink-0">
                      {roleInfo.icon}
                    </div>
                    <div>
                      <span className="text-[10px] font-bold tracking-wider uppercase opacity-75">
                        SESIÓN ACTIVA
                      </span>
                      <h3 className="text-xl font-bold leading-tight">
                        {user.nombreCompleto}
                      </h3>
                      <p className="text-xs font-semibold opacity-90">
                        Rol: {user.rolNombre} {user.idMedico ? `(ID Médico: ${user.idMedico})` : ''}
                      </p>
                    </div>
                  </div>

                  <p className="text-xs opacity-80 mt-2 mb-4 leading-relaxed">
                    {roleInfo.description}
                  </p>

                  <div className="space-y-2 pt-2 border-t border-black/10">
                    <p className="text-[11px] font-bold uppercase tracking-wider opacity-70">
                      Accesos rápidos del módulo:
                    </p>
                    {roleInfo.actions.map((act) => (
                      <div
                        key={act}
                        className="flex items-center justify-between p-2.5 bg-white/80 rounded-lg text-xs font-medium text-slate-800 hover:bg-white transition-all cursor-pointer shadow-2xs"
                      >
                        <span className="flex items-center gap-2">
                          <Calendar className="w-3.5 h-3.5 text-[#0d5c68]" />
                          {act}
                        </span>
                        <ArrowRight className="w-3.5 h-3.5 text-slate-400" />
                      </div>
                    ))}
                  </div>
                </div>
              );
            })()}

            {/* Botón Cerrar Sesión */}
            <div className="flex justify-between items-center pt-2">
              <span className="text-xs text-slate-400">
                Token JWT almacenado y validado
              </span>
              <button
                type="button"
                onClick={logout}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium transition-colors shadow-2xs"
              >
                <LogOut className="w-3.5 h-3.5 text-red-500" />
                Cerrar sesión
              </button>
            </div>
          </div>
        ) : (
          <LoginForm />
        )}
      </div>

      {/* Panel Derecho: Hero visual médico */}
      <HeroSection />
    </div>
  );
};
