import React from 'react';
import { Stethoscope, ClipboardList, Shield } from 'lucide-react';

interface DemoAccount {
  label: string;
  identificador: string;
  rolNombre: string;
  icon: React.ReactNode;
}

interface DemoAccountsProps {
  onSelectAccount: (identificador: string, password: string) => void;
}

export const DemoAccounts: React.FC<DemoAccountsProps> = ({ onSelectAccount }) => {
  const passwordDemo = 'Password123!';

  const accounts: DemoAccount[] = [
    {
      label: 'Médico',
      identificador: 'dr.perez@clinicaenza.com',
      rolNombre: 'Dr. Juan Pérez (Clínica Médica)',
      icon: <Stethoscope className="w-4 h-4 text-[#0d5c68]" />,
    },
    {
      label: 'Recepcionista',
      identificador: 'ana.recepcion@clinicaenza.com',
      rolNombre: 'Ana Gómez',
      icon: <ClipboardList className="w-4 h-4 text-[#0d5c68]" />,
    },
    {
      label: 'Administrador',
      identificador: 'carlos.admin@clinicaenza.com',
      rolNombre: 'Carlos González',
      icon: <Shield className="w-4 h-4 text-[#0d5c68]" />,
    },
  ];

  return (
    <div className="mt-8 p-4 rounded-xl border border-slate-200/90 bg-slate-50/50">
      <div className="text-[11px] font-bold text-slate-400 tracking-wider uppercase mb-0.5">
        CUENTAS DE DEMOSTRACIÓN (DATOS REALES)
      </div>
      <p className="text-xs text-slate-500 mb-3">
        Contraseña demo: <span className="font-semibold text-slate-700">{passwordDemo}</span>
      </p>

      <div className="grid grid-cols-3 gap-2.5">
        {accounts.map((acc) => (
          <button
            key={acc.label}
            type="button"
            onClick={() => onSelectAccount(acc.identificador, passwordDemo)}
            className="flex flex-col items-center justify-center p-2.5 rounded-lg border border-slate-200 bg-white hover:border-[#0d5c68]/40 hover:bg-slate-50/80 active:scale-98 transition-all text-xs font-medium text-slate-700 gap-1.5 shadow-sm group"
          >
            <span className="p-1.5 rounded-full bg-slate-50 group-hover:bg-[#0d5c68]/10 transition-colors">
              {acc.icon}
            </span>
            <span className="font-semibold">{acc.label}</span>
          </button>
        ))}
      </div>
    </div>
  );
};
