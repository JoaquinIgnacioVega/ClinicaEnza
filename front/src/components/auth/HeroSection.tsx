import React from 'react';
import { ShieldCheck, Calendar, FileHeart } from 'lucide-react';

export const HeroSection: React.FC = () => {
  return (
    <div className="relative hidden lg:flex flex-col justify-between p-12 xl:p-16 bg-gradient-to-br from-[#063b46] via-[#042e37] to-[#021f26] text-white overflow-hidden min-h-screen">
      {/* Círculos decorativos de fondo con SVG */}
      <div className="absolute inset-0 pointer-events-none opacity-10">
        <svg className="w-full h-full" viewBox="0 0 800 800" fill="none">
          <circle cx="650" cy="200" r="280" stroke="white" strokeWidth="1" strokeDasharray="4 4" />
          <circle cx="650" cy="200" r="420" stroke="white" strokeWidth="1" />
          <circle cx="650" cy="200" r="560" stroke="white" strokeWidth="1" strokeDasharray="6 6" />
        </svg>
      </div>

      {/* Insignia superior */}
      <div className="relative z-10 flex items-center gap-2 text-xs font-medium text-teal-100/90 tracking-wide">
        <ShieldCheck className="w-4 h-4 text-teal-300" />
        <span>Tus datos clínicos, protegidos.</span>
      </div>

      {/* Tarjetas Flotantes del Centro */}
      <div className="relative z-10 my-auto py-12 flex flex-col items-center justify-center space-y-4 max-w-md mx-auto w-full">
        {/* Tarjeta 1: Próximo turno */}
        <div className="w-full bg-white text-slate-800 rounded-2xl p-4 shadow-2xl border border-white/20 transform hover:-translate-y-1 transition-transform duration-300">
          <div className="flex items-center justify-between mb-3">
            <span className="text-[10px] font-bold tracking-wider text-slate-400 uppercase">
              PRÓXIMO TURNO
            </span>
            <span className="px-2.5 py-0.5 rounded-full text-[10px] font-semibold bg-sky-100 text-sky-700">
              Pendiente
            </span>
          </div>
          <div className="flex items-center gap-3.5">
            <div className="flex flex-col items-center justify-center bg-sky-50 text-sky-700 rounded-xl px-3 py-1.5 shrink-0 border border-sky-100">
              <span className="text-[9px] font-bold tracking-wide uppercase">NOV</span>
              <span className="text-lg font-black leading-tight">12</span>
            </div>
            <div>
              <h4 className="text-sm font-bold text-slate-800 leading-tight">
                Dra. Laura Méndez
              </h4>
              <p className="text-xs text-slate-500 mt-0.5">
                Clínica médica · 10:30 h
              </p>
            </div>
          </div>
        </div>

        {/* Tarjeta 2: Turno Confirmado */}
        <div className="w-[90%] self-end bg-white/95 backdrop-blur-md text-slate-800 rounded-2xl p-3.5 shadow-xl border border-white/30 flex items-center gap-3.5 transform hover:-translate-y-1 transition-transform duration-300">
          <div className="w-10 h-10 rounded-xl bg-emerald-100 text-emerald-600 flex items-center justify-center shrink-0">
            <Calendar className="w-5 h-5" />
          </div>
          <div>
            <h4 className="text-xs font-bold text-slate-800 leading-tight">
              Turno confirmado
            </h4>
            <p className="text-[11px] text-slate-500 mt-0.5">
              Te esperamos en el Consultorio 1
            </p>
          </div>
        </div>

        {/* Tarjeta 3: Historia clínica */}
        <div className="w-[85%] self-start bg-white/95 backdrop-blur-md text-slate-800 rounded-2xl p-3.5 shadow-xl border border-white/30 flex items-center gap-3.5 transform hover:-translate-y-1 transition-transform duration-300">
          <div className="w-10 h-10 rounded-xl bg-cyan-100 text-cyan-700 flex items-center justify-center shrink-0">
            <FileHeart className="w-5 h-5" />
          </div>
          <div>
            <h4 className="text-xs font-bold text-slate-800 leading-tight">
              Historia clínica
            </h4>
            <p className="text-[11px] text-slate-500 mt-0.5">
              Actualizada hace 2 semanas
            </p>
          </div>
        </div>
      </div>

      {/* Contenido Inferior */}
      <div className="relative z-10 max-w-lg">
        <div className="w-10 h-10 rounded-xl bg-white/10 backdrop-blur-md border border-white/20 flex items-center justify-center text-white mb-5 shadow-sm">
          <svg className="w-5 h-5 fill-current" viewBox="0 0 24 24">
            <path d="M19 10.5h-5.5V5c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v5.5H5c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5h5.5V19c0 .83.67 1.5 1.5 1.5s1.5-.67 1.5-1.5v-5.5H19c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5z" />
          </svg>
        </div>

        <h3 className="text-2xl xl:text-3xl font-bold tracking-tight text-white leading-tight mb-3">
          Atención cercana, organizada y a tu tiempo.
        </h3>
        <p className="text-xs xl:text-sm text-teal-100/80 leading-relaxed max-w-md">
          Reservá turnos, consultá tu agenda y accedé a tu información clínica desde cualquier dispositivo.
        </p>
      </div>
    </div>
  );
};
