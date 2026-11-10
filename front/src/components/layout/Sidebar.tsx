import React from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutGrid, Calendar, Clock, FileText, Pill, Database, ShieldCheck } from 'lucide-react';

export const Sidebar: React.FC = () => {
  const navItems = [
    { to: '/medico', label: 'Inicio', icon: <LayoutGrid className="w-5 h-5" /> },
    { to: '/medico/turnos', label: 'Agenda de Turnos', icon: <Calendar className="w-5 h-5" /> },
    { to: '/medico/horarios', label: 'Horarios de Atención', icon: <Clock className="w-5 h-5" /> },
    { to: '/medico/historia-clinica', label: 'Historia Clínica', icon: <FileText className="w-5 h-5" /> },
    { to: '/medico/prescripciones', label: 'Prescripciones', icon: <Pill className="w-5 h-5" /> },
  ];

  return (
    <aside className="w-64 bg-[#05242c] text-white flex flex-col justify-between shrink-0 min-h-screen border-r border-[#0d3c47]/50 select-none">
      <div>
        {/* Cabecera / Marca */}
        <div className="p-6 flex items-center gap-3">
          <div className="w-8 h-8 rounded-lg bg-[#0d5c68] flex items-center justify-center text-white shadow-sm shrink-0">
            <svg className="w-4 h-4 fill-current" viewBox="0 0 24 24">
              <path d="M19 10.5h-5.5V5c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v5.5H5c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5h5.5V19c0 .83.67 1.5 1.5 1.5s1.5-.67 1.5-1.5v-5.5H19c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5z" />
            </svg>
          </div>
          <div>
            <h2 className="text-sm font-bold tracking-tight text-white leading-none">
              Clínica Enza
            </h2>
            <p className="text-[9px] font-semibold tracking-wider text-teal-300/70 uppercase mt-0.5">
              GESTIÓN DE TURNOS
            </p>
          </div>
        </div>

        {/* Sección / Título del Área */}
        <div className="px-6 pt-2 pb-3">
          <p className="text-[10px] font-bold tracking-widest text-teal-200/50 uppercase">
            PORTAL MÉDICO
          </p>
        </div>

        {/* Navegación Principal */}
        <nav className="px-3 space-y-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/medico'}
              className={({ isActive }) =>
                `flex items-center justify-between px-3.5 py-2.5 rounded-xl text-xs font-medium transition-all group ${
                  isActive
                    ? 'bg-[#0b3c46] text-white shadow-inner font-semibold'
                    : 'text-teal-100/70 hover:bg-[#0b3c46]/50 hover:text-white'
                }`
              }
            >
              {({ isActive }) => (
                <>
                  <div className="flex items-center gap-3">
                    <span className={`transition-colors ${isActive ? 'text-teal-300' : 'text-teal-200/60 group-hover:text-teal-200'}`}>
                      {item.icon}
                    </span>
                    <span>{item.label}</span>
                  </div>
                  {isActive && (
                    <span className="w-1.5 h-1.5 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(34,211,238,0.8)]"></span>
                  )}
                </>
              )}
            </NavLink>
          ))}
        </nav>
      </div>

      {/* Pie de Sidebar */}
      <div className="p-4 border-t border-[#0d3c47]/60 space-y-2 text-[11px] text-teal-200/60">
        <div className="flex items-center gap-2">
          <Database className="w-3.5 h-3.5 text-teal-400/80" />
          <span>Modo profesional · activo</span>
        </div>
        <div className="flex items-center gap-2">
          <ShieldCheck className="w-3.5 h-3.5 text-emerald-400/90" />
          <span>Sesión clínica segura</span>
        </div>
      </div>
    </aside>
  );
};
