import React from 'react';
import { NavLink } from 'react-router-dom';
import { CalendarDays, ClipboardList, LayoutGrid, Stethoscope, Users, Building2, ShieldCheck } from 'lucide-react';

const items = [
  { to: '/recepcionista', label: 'Panel general', icon: LayoutGrid, end: true },
  { to: '/recepcionista/turnos', label: 'Turnos', icon: CalendarDays },
  { to: '/recepcionista/pacientes', label: 'Pacientes', icon: Users },
  { to: '/recepcionista/medicos', label: 'Médicos y horarios', icon: Stethoscope },
  { to: '/recepcionista/consultorios', label: 'Consultorios', icon: Building2 },
];

export const RecepcionistaSidebar: React.FC = () => (
  <aside className="w-64 bg-[#05242c] text-white flex flex-col justify-between shrink-0 min-h-screen border-r border-[#0d3c47]/50">
    <div>
      <div className="p-6 flex items-center gap-3"><div className="w-8 h-8 rounded-lg bg-[#55bdc7] flex items-center justify-center text-[#05242c]"><ClipboardList className="w-4 h-4" /></div><div><h2 className="text-sm font-bold tracking-tight leading-none">Clínica Enza</h2><p className="text-[9px] font-semibold tracking-wider text-teal-300/70 uppercase mt-0.5">GESTIÓN DE TURNOS</p></div></div>
      <div className="px-6 pt-2 pb-3"><p className="text-[10px] font-bold tracking-widest text-teal-200/50 uppercase">PORTAL DE RECEPCIÓN</p></div>
      <nav className="px-3 space-y-1">{items.map(({ to, label, icon: Icon, end }) => <NavLink key={to} to={to} end={end} className={({ isActive }) => `flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs font-medium transition-all ${isActive ? 'bg-[#0b3c46] text-white shadow-inner font-semibold' : 'text-teal-100/70 hover:bg-[#0b3c46]/50 hover:text-white'}`}><Icon className="w-5 h-5" /><span>{label}</span></NavLink>)}</nav>
    </div>
    <div className="p-4 border-t border-[#0d3c47]/60 text-[11px] text-teal-200/60"><div className="flex items-center gap-2"><ShieldCheck className="w-3.5 h-3.5 text-emerald-400/90" /><span>Área protegida</span></div><p className="mt-2 leading-relaxed">Información disponible solo para personal autorizado.</p></div>
  </aside>
);
