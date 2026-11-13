import { useState } from 'react';
import { Calendar, LogOut, UserCheck } from 'lucide-react';
import { AuthProvider, useAuth } from './context/AuthContext';
import { LoginPage } from './pages/LoginPage';
import { AgendaMedica } from './features/turnos/AgendaMedica';

function AppShell() {
  const { user, isAuthenticated, logout } = useAuth();
  const [vista, setVista] = useState<'login' | 'agenda'>('login');

  const esMedico = isAuthenticated && user?.rolNombre.toLowerCase() === 'medico';

  return (
    <div className="min-h-screen bg-white flex flex-col">
      {isAuthenticated && user && (
        <header className="border-b border-slate-200/80 bg-white sticky top-0 z-40 shadow-2xs">
          <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between gap-4">
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
                  {user.nombreCompleto} · {user.rolNombre}
                </span>
              </div>
            </div>

            <div className="flex items-center gap-2">
              <nav className="flex items-center gap-1 p-1 bg-slate-100 rounded-xl border border-slate-200/80">
                <button
                  type="button"
                  onClick={() => setVista('login')}
                  className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
                    vista === 'login'
                      ? 'bg-white text-[#0d5c68] shadow-xs'
                      : 'text-slate-600 hover:text-slate-900'
                  }`}
                >
                  <UserCheck className="w-3.5 h-3.5" />
                  <span>Mi Panel</span>
                </button>
                {esMedico && (
                  <button
                    type="button"
                    onClick={() => setVista('agenda')}
                    className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
                      vista === 'agenda'
                        ? 'bg-white text-[#0d5c68] shadow-xs'
                        : 'text-slate-600 hover:text-slate-900'
                    }`}
                  >
                    <Calendar className="w-3.5 h-3.5" />
                    <span>Agenda Médica</span>
                  </button>
                )}
              </nav>

              <button
                type="button"
                onClick={() => {
                  logout();
                  setVista('login');
                }}
                className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-semibold transition-colors"
              >
                <LogOut className="w-3.5 h-3.5 text-red-500" />
                <span className="hidden sm:inline">Salir</span>
              </button>
            </div>
          </div>
        </header>
      )}

      <main className="flex-1">
        {vista === 'agenda' && isAuthenticated ? (
          <AgendaMedica />
        ) : (
          <LoginPage onNavigateToAgenda={() => setVista('agenda')} />
        )}
      </main>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppShell />
    </AuthProvider>
  );
}

