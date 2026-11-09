import { useState } from 'react';
import { AuthProvider } from './context/AuthContext';
import { LoginPage } from './pages/LoginPage';
import { MisTurnos } from './features/turnos/MisTurnos';
import { Calendar, UserCheck } from 'lucide-react';

export default function App() {
  const [activeTab, setActiveTab] = useState<'mis-turnos' | 'login'>('mis-turnos');

  return (
    <AuthProvider>
      <div className="min-h-screen bg-white flex flex-col">
        {/* Barra superior de navegación */}
        <header className="border-b border-slate-200/80 bg-white sticky top-0 z-40 shadow-2xs">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
            {/* Logo y Nombre */}
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-xl bg-[#0d5c68] flex items-center justify-center text-white shadow-xs">
                <svg className="w-5 h-5 fill-current" viewBox="0 0 24 24">
                  <path d="M19 10.5h-5.5V5c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v5.5H5c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5h5.5V19c0 .83.67 1.5 1.5 1.5s1.5-.67 1.5-1.5v-5.5H19c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5z" />
                </svg>
              </div>
              <div>
                <span className="text-base font-extrabold text-slate-900 tracking-tight leading-none block">
                  Clínica Enza
                </span>
                <span className="text-[10px] font-bold tracking-wider text-slate-400 uppercase">
                  SISTEMA DE TURNOS
                </span>
              </div>
            </div>

            {/* Pestañas de Navegación */}
            <nav className="flex items-center gap-1.5 p-1 bg-slate-100 rounded-xl border border-slate-200/80">
              <button
                type="button"
                onClick={() => setActiveTab('mis-turnos')}
                className={`inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg text-xs font-bold transition-all ${
                  activeTab === 'mis-turnos'
                    ? 'bg-white text-[#0d5c68] shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                <Calendar className="w-3.5 h-3.5" />
                <span>Mis Turnos</span>
              </button>
              <button
                type="button"
                onClick={() => setActiveTab('login')}
                className={`inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg text-xs font-bold transition-all ${
                  activeTab === 'login'
                    ? 'bg-white text-[#0d5c68] shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                <UserCheck className="w-3.5 h-3.5" />
                <span>Acceso Profesional</span>
              </button>
            </nav>
          </div>
        </header>

        {/* Contenido principal */}
        <main className="flex-1">
          {activeTab === 'mis-turnos' ? <MisTurnos idPacienteInicial={1} /> : <LoginPage />}
        </main>
      </div>
    </AuthProvider>
  );
}

