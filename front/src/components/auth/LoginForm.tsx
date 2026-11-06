import React, { useState, useEffect } from 'react';
import { Mail, Lock, AlertCircle, CheckCircle2, Info, X } from 'lucide-react';
import { Input } from '../ui/Input';
import { Button } from '../ui/Button';
import { DemoAccounts } from './DemoAccounts';
import { useAuth } from '../../context/AuthContext';
import type { ApiError } from '../../api/client';

export const LoginForm: React.FC = () => {
  const { login } = useAuth();

  const [identificador, setIdentificador] = useState('');
  const [password, setPassword] = useState('');
  const [recordarme, setRecordarme] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [showForgotNotice, setShowForgotNotice] = useState(false);

  // Cargar identificador recordado si existe
  useEffect(() => {
    const saved = localStorage.getItem('clinica_enza_remembered_user');
    if (saved) {
      setIdentificador(saved);
      setRecordarme(true);
    }
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    const idTrimmed = identificador.trim();

    if (!idTrimmed) {
      setErrorMessage('Por favor, ingresá tu correo electrónico o legajo.');
      return;
    }

    if (!password) {
      setErrorMessage('Por favor, ingresá tu contraseña.');
      return;
    }

    try {
      setIsLoading(true);

      // Guardar o borrar preferencia de recordar
      if (recordarme) {
        localStorage.setItem('clinica_enza_remembered_user', idTrimmed);
      } else {
        localStorage.removeItem('clinica_enza_remembered_user');
      }

      const res = await login({ identificador: idTrimmed, password });
      setSuccessMessage(`¡Bienvenido/a, ${res.nombreCompleto}! Accediendo al panel de ${res.rolNombre}...`);
    } catch (err: unknown) {
      const apiErr = err as ApiError;
      setErrorMessage(apiErr.error || 'Credenciales inválidas. Verificá tu correo/legajo y contraseña.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSelectDemo = (demoId: string, demoPass: string) => {
    setIdentificador(demoId);
    setPassword(demoPass);
    setErrorMessage(null);
    setShowForgotNotice(false);
  };

  return (
    <div className="flex flex-col justify-between min-h-screen p-8 sm:p-12 lg:p-16 w-full max-w-xl mx-auto">
      {/* Encabezado / Logo */}
      <header className="flex items-center gap-3">
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
      </header>

      {/* Contenido principal / Formulario */}
      <div className="my-auto py-8">
        <div className="mb-7">
          <h2 className="text-3xl font-bold text-slate-900 tracking-tight">
            Iniciá sesión
          </h2>
          <p className="text-sm text-slate-500 mt-2 leading-relaxed">
            Accedé a tu agenda médica, turnos e historias clínicas de forma segura.
          </p>
        </div>

        {/* Modal / Banner informativo de recuperación de contraseña */}
        {showForgotNotice && (
          <div className="mb-5 p-4 rounded-xl bg-cyan-50 border border-cyan-200 text-cyan-900 text-xs animate-fadeIn relative">
            <div className="flex items-start gap-2.5 pr-6">
              <Info className="w-4 h-4 shrink-0 mt-0.5 text-[#0d5c68]" />
              <div>
                <p className="font-semibold text-slate-800 mb-1">Restablecimiento de credenciales</p>
                <p className="text-slate-600 leading-relaxed">
                  Por seguridad médica institucional, para recuperar tu clave o solicitar un nuevo usuario debes contactar al 
                  área de <strong>Sistemas / Administración</strong> (interno 104 o soporte@clinicaenza.com).
                </p>
              </div>
            </div>
            <button
              type="button"
              onClick={() => setShowForgotNotice(false)}
              className="absolute top-3 right-3 text-slate-400 hover:text-slate-700"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        )}

        {/* Alerta de Error */}
        {errorMessage && (
          <div className="mb-5 p-3.5 rounded-lg bg-red-50 border border-red-200 flex items-start gap-2.5 text-red-700 text-xs animate-fadeIn">
            <AlertCircle className="w-4 h-4 shrink-0 mt-0.5 text-red-500" />
            <span className="font-medium leading-relaxed">{errorMessage}</span>
          </div>
        )}

        {/* Alerta de Éxito */}
        {successMessage && (
          <div className="mb-5 p-3.5 rounded-lg bg-emerald-50 border border-emerald-200 flex items-start gap-2.5 text-emerald-800 text-xs animate-fadeIn">
            <CheckCircle2 className="w-4 h-4 shrink-0 mt-0.5 text-emerald-600" />
            <span className="font-medium leading-relaxed">{successMessage}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label="Correo electrónico o Legajo"
            type="text"
            placeholder="nombre@correo.com o legajo (ej. 1001)"
            value={identificador}
            onChange={(e) => setIdentificador(e.target.value)}
            icon={<Mail className="w-4 h-4" />}
            autoComplete="username"
          />

          <Input
            label="Contraseña"
            isPassword
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            icon={<Lock className="w-4 h-4" />}
            autoComplete="current-password"
            headerAction={
              <button
                type="button"
                onClick={() => setShowForgotNotice(true)}
                className="text-xs text-[#0d5c68] hover:text-[#094852] hover:underline font-medium cursor-pointer"
              >
                ¿Olvidaste tu contraseña?
              </button>
            }
          />

          {/* Recordarme */}
          <div className="flex items-center justify-between pt-1">
            <label className="flex items-center gap-2 cursor-pointer select-none">
              <input
                type="checkbox"
                checked={recordarme}
                onChange={(e) => setRecordarme(e.target.checked)}
                className="w-4 h-4 rounded text-[#0d5c68] focus:ring-[#0d5c68]/30 border-slate-300 accent-[#0d5c68]"
              />
              <span className="text-xs text-slate-600 font-medium">
                Recordar mis datos en este equipo
              </span>
            </label>
          </div>

          <div className="pt-2">
            <Button type="submit" isLoading={isLoading}>
              Iniciar sesión
            </Button>
          </div>
        </form>

        {/* Cuentas de Demostración */}
        <DemoAccounts onSelectAccount={handleSelectDemo} />
      </div>

      {/* Pie de página */}
      <footer className="text-xs text-slate-400 pt-4">
        © 2026 Clínica Enza · Av. Rivadavia 4520, CABA
      </footer>
    </div>
  );
};
