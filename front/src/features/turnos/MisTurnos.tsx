import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Calendar,
  Clock,
  User,
  MapPin,
  AlertCircle,
  CheckCircle2,
  XCircle,
  RefreshCw,
  AlertTriangle,
  Stethoscope,
  ShieldCheck,
  LogOut,
  X,
  CreditCard,
  Mail,
  Activity,
  ArrowLeft,
} from 'lucide-react';
import { turnosService } from '../../api/turnosService';
import type { Turno } from '../../types/turnos';
import type { PacienteDto } from '../../types/medico';
import type { ApiError } from '../../api/client';

export const MisTurnos: React.FC = () => {
  const navigate = useNavigate();

  // Estado de sesión del paciente autenticado (DNI + Correo con JWT)
  const [pacienteActivo, setPacienteActivo] = useState<PacienteDto | null>(() => {
    const sesion = turnosService.obtenerSesionPaciente();
    return sesion?.paciente ?? null;
  });

  // Formulario de ingreso (Paso 1)
  const [dniInput, setDniInput] = useState<string>('');
  const [mailInput, setMailInput] = useState<string>('');
  const [passwordInput, setPasswordInput] = useState<string>('');
  const [nuevaPassword, setNuevaPassword] = useState<string>('');
  const [requiereCambioPassword, setRequiereCambioPassword] = useState<boolean>(() =>
    turnosService.obtenerSesionPaciente()?.requiereCambioPassword ?? false
  );
  const [cambiandoPassword, setCambiandoPassword] = useState<boolean>(false);
  const [autenticando, setAutenticando] = useState<boolean>(false);
  const [loginError, setLoginError] = useState<string | null>(null);

  // Filtro: true = Próximos / Pendientes, false = Todos los turnos
  const [soloPendientes, setSoloPendientes] = useState<boolean>(true);

  // Estado de turnos (Paso 2)
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Modal de confirmación de cancelación
  const [turnoParaCancelar, setTurnoParaCancelar] = useState<Turno | null>(null);
  const [cancelando, setCancelando] = useState<boolean>(false);

  // Salir / Cambiar DNI: elimina token JWT y datos del paciente y vuelve al Paso 1
  const handleSalirCambiarDni = useCallback(() => {
    turnosService.cerrarSesionPaciente();
    setPacienteActivo(null);
    setTurnos([]);
    setDniInput('');
    setMailInput('');
    setPasswordInput('');
    setNuevaPassword('');
    setRequiereCambioPassword(false);
    setLoginError(null);
    setError(null);
    setSuccessMessage(null);
    setTurnoParaCancelar(null);
    setSoloPendientes(true);
  }, []);

  // Cargar turnos del paciente autenticado
  const cargarTurnos = useCallback(
    async (pacienteId: number, filtrarSoloPendientes: boolean) => {
      setLoading(true);
      setError(null);
      try {
        const data = await turnosService.obtenerTurnosPaciente(
          pacienteId,
          filtrarSoloPendientes
        );
        setTurnos(data);
      } catch (err) {
        const apiErr = err as ApiError;
        if (apiErr.status === 401 || apiErr.status === 403) {
          handleSalirCambiarDni();
          setLoginError(
            apiErr.error || 'Tu sesión expiró. Por favor, volvé a ingresar tu DNI y correo.'
          );
          return;
        }
        setError(apiErr.error || 'No fue posible cargar los turnos del paciente.');
        setTurnos([]);
      } finally {
        setLoading(false);
      }
    },
    [handleSalirCambiarDni]
  );

  // Si ya había sesión activa en la pestaña actual, cargar sus turnos al montar
  useEffect(() => {
    if (pacienteActivo?.idPaciente) {
      cargarTurnos(pacienteActivo.idPaciente, true);
    }
  }, []);

  // Paso 1: Autenticación de Paciente por DNI exacto + Correo electrónico
  const handleAutenticarPaciente = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoginError(null);

    const dniLimpio = dniInput.trim().replace(/[.\-\s]/g, '');
    const mailLimpio = mailInput.trim();

    if (!dniLimpio) {
      setLoginError('Por favor, ingresá tu número de DNI.');
      return;
    }

    if (!/^\d{6,10}$/.test(dniLimpio)) {
      setLoginError('Ingresá un número de DNI válido (solo números, sin puntos).');
      return;
    }

    if (!mailLimpio) {
      setLoginError('Por favor, ingresá tu correo electrónico registrado.');
      return;
    }

    try {
      setAutenticando(true);
      const response = await turnosService.autenticarPaciente(dniLimpio, mailLimpio, passwordInput);
      const { paciente } = response;

      setPacienteActivo(paciente);
      setRequiereCambioPassword(response.requiereCambioPassword);
      setSoloPendientes(true);
      setSuccessMessage(null);
      setError(null);
      await cargarTurnos(paciente.idPaciente, true);
    } catch (err) {
      const apiErr = err as ApiError;
      setLoginError(
        apiErr.error || 'Los datos ingresados no coinciden con ningún paciente registrado.'
      );
    } finally {
      setAutenticando(false);
    }
  };

  const handleCambiarPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!pacienteActivo || nuevaPassword.length < 4) {
      setError('La nueva contraseña debe tener al menos 4 caracteres.');
      return;
    }
    try {
      setCambiandoPassword(true);
      await turnosService.cambiarPassword(pacienteActivo.idPaciente, nuevaPassword);
      setRequiereCambioPassword(false);
      setNuevaPassword('');
      setError(null);
      await cargarTurnos(pacienteActivo.idPaciente, true);
    } catch (err) {
      setError((err as ApiError).error || 'No fue posible cambiar la contraseña.');
    } finally {
      setCambiandoPassword(false);
    }
  };

  // Cambiar filtro entre "Próximos / Pendientes" y "Todos los turnos"
  const handleCambiarFiltro = async (nuevoSoloPendientes: boolean) => {
    if (!pacienteActivo) return;
    setSoloPendientes(nuevoSoloPendientes);
    setSuccessMessage(null);
    await cargarTurnos(pacienteActivo.idPaciente, nuevoSoloPendientes);
  };

  // Determinar el estado normalizado de un turno
  const obtenerEstadoNormalizado = (
    turno: Turno
  ): 'Pendiente' | 'Realizado' | 'Cancelado' => {
    const desc = turno.estadoDescripcion?.toLowerCase() || '';
    if (
      turno.estado === 1 ||
      turno.estado === 'Realizado' ||
      desc === 'realizado' ||
      desc === 'atendido'
    ) {
      return 'Realizado';
    }
    if (turno.estado === 2 || turno.estado === 'Cancelado' || desc === 'cancelado') {
      return 'Cancelado';
    }
    return 'Pendiente';
  };

  // Validar si un turno puede cancelarse (estado Pendiente y fecha futura)
  const esTurnoCancelable = (turno: Turno): boolean => {
    const estadoNorm = obtenerEstadoNormalizado(turno);
    if (estadoNorm !== 'Pendiente') return false;
    const fechaInicio = new Date(turno.horaInicio);
    return fechaInicio.getTime() > Date.now();
  };

  // Confirmar cancelación en el modal
  const handleConfirmarCancelacion = async () => {
    if (!turnoParaCancelar || !pacienteActivo) return;

    setCancelando(true);
    setError(null);
    setSuccessMessage(null);
    try {
      const response = await turnosService.cancelarTurnoPaciente(
        pacienteActivo.idPaciente,
        turnoParaCancelar.idTurno
      );
      setSuccessMessage(
        response.mensaje ||
          `El turno #${turnoParaCancelar.idTurno} fue cancelado exitosamente.`
      );
      setTurnoParaCancelar(null);
      await cargarTurnos(pacienteActivo.idPaciente, soloPendientes);
    } catch (err) {
      const apiErr = err as ApiError;
      setError(apiErr.error || 'No fue posible cancelar el turno seleccionado.');
      setTurnoParaCancelar(null);
    } finally {
      setCancelando(false);
    }
  };

  const formatearFecha = (isoString: string) => {
    try {
      const date = new Date(isoString);
      return date.toLocaleDateString('es-AR', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      });
    } catch {
      return isoString;
    }
  };

  const formatearHora = (isoString: string) => {
    if (!isoString) return '--:--';
    try {
      const date = new Date(isoString);
      return date.toLocaleTimeString('es-AR', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      });
    } catch {
      return isoString;
    }
  };

  // ============================================================================
  // PASO 1: PANTALLA DE INGRESO POR DNI + CORREO (Cuando no hay sesión activa)
  // ============================================================================
  if (!pacienteActivo) {
    return (
      <div className="relative min-h-screen w-full bg-gradient-to-br from-[#063b46] via-[#042e37] to-[#021f26] flex items-center justify-center p-4 sm:p-6 lg:p-8 overflow-hidden">
        {/* Círculos concéntricos decorativos de fondo */}
        <div className="absolute inset-0 pointer-events-none opacity-10 flex items-center justify-center">
          <svg className="w-full h-full max-w-5xl" viewBox="0 0 800 800" fill="none">
            <circle cx="400" cy="400" r="280" stroke="white" strokeWidth="1" strokeDasharray="4 4" />
            <circle cx="400" cy="400" r="420" stroke="white" strokeWidth="1" />
            <circle cx="400" cy="400" r="560" stroke="white" strokeWidth="1" strokeDasharray="6 6" />
          </svg>
        </div>

        {/* Tarjeta centrada de ingreso al Portal del Paciente */}
        <div className="relative z-10 w-full max-w-md bg-white rounded-3xl shadow-2xl p-6 sm:p-10 border border-white/20 animate-fadeIn my-auto">
          {/* Encabezado / Marca Institucional */}
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
                PORTAL DEL PACIENTE
              </p>
            </div>
          </header>

          {/* Título y Descripción */}
          <div className="mb-6">
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
              Consultar mis turnos
            </h2>
            <p className="text-xs sm:text-sm text-slate-500 mt-1.5 leading-relaxed">
              Ingresá tu DNI y el correo electrónico registrado en la clínica para acceder de forma segura a tu agenda de turnos.
            </p>
          </div>

          {/* Mensaje de error si las credenciales no coinciden (401) */}
          {loginError && (
            <div className="mb-5 p-3.5 rounded-xl bg-red-50 border border-red-200 flex items-start gap-2.5 text-red-700 text-xs animate-fadeIn">
              <AlertCircle className="w-4 h-4 shrink-0 mt-0.5 text-red-500" />
              <span className="font-medium leading-relaxed">{loginError}</span>
            </div>
          )}

          {/* Formulario de autenticación por DNI + Correo */}
          <form onSubmit={handleAutenticarPaciente} className="space-y-4">
            <div className="w-full space-y-1.5">
              <label
                htmlFor="dni-paciente"
                className="block text-xs font-semibold text-slate-700 tracking-wide"
              >
                DNI del Paciente
              </label>
              <div className="relative flex items-center">
                <span className="absolute left-3.5 text-slate-400 pointer-events-none flex items-center">
                  <CreditCard className="w-4 h-4" />
                </span>
                <input
                  id="dni-paciente"
                  type="text"
                  inputMode="numeric"
                  required
                  value={dniInput}
                  onChange={(e) => {
                    setDniInput(e.target.value);
                    if (loginError) setLoginError(null);
                  }}
                  placeholder="Ej: 30123456"
                  autoFocus
                  className="w-full rounded-xl border border-slate-200 bg-slate-50/60 pl-10 pr-10 py-3 text-sm text-slate-800 placeholder:text-slate-400 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/20 focus:border-[#0d5c68] focus:bg-white"
                />
                {dniInput && (
                  <button
                    type="button"
                    onClick={() => {
                      setDniInput('');
                      setLoginError(null);
                    }}
                    className="absolute right-3.5 text-slate-400 hover:text-slate-600 cursor-pointer"
                  >
                    <X className="w-4 h-4" />
                  </button>
                )}
              </div>
            </div>

            <div className="w-full space-y-1.5">
              <label
                htmlFor="mail-paciente"
                className="block text-xs font-semibold text-slate-700 tracking-wide"
              >
                Correo electrónico registrado
              </label>
              <div className="relative flex items-center">
                <span className="absolute left-3.5 text-slate-400 pointer-events-none flex items-center">
                  <Mail className="w-4 h-4" />
                </span>
                <input
                  id="mail-paciente"
                  type="email"
                  required
                  value={mailInput}
                  onChange={(e) => {
                    setMailInput(e.target.value);
                    if (loginError) setLoginError(null);
                  }}
                  placeholder="Ej: paciente@correo.com"
                  autoComplete="email"
                  className="w-full rounded-xl border border-slate-200 bg-slate-50/60 pl-10 pr-10 py-3 text-sm text-slate-800 placeholder:text-slate-400 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/20 focus:border-[#0d5c68] focus:bg-white"
                />
                {mailInput && (
                  <button
                    type="button"
                    onClick={() => {
                      setMailInput('');
                      setLoginError(null);
                    }}
                    className="absolute right-3.5 text-slate-400 hover:text-slate-600 cursor-pointer"
                  >
                    <X className="w-4 h-4" />
                  </button>
                )}
              </div>
            </div>

            <div className="w-full space-y-1.5">
              <label htmlFor="password-paciente" className="block text-xs font-semibold text-slate-700 tracking-wide">
                Contraseña
              </label>
              <input
                id="password-paciente"
                type="password"
                required
                value={passwordInput}
                onChange={(e) => setPasswordInput(e.target.value)}
                placeholder="Contraseña temporal: 1234"
                autoComplete="current-password"
                className="w-full rounded-xl border border-slate-200 bg-slate-50/60 px-3.5 py-3 text-sm text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/20 focus:border-[#0d5c68] focus:bg-white"
              />
            </div>

            <div className="pt-2">
              <button
                type="submit"
                disabled={autenticando}
                className="w-full py-3 px-4 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white font-semibold text-sm shadow-sm transition-all flex items-center justify-center gap-2 cursor-pointer disabled:opacity-60"
              >
                {autenticando ? (
                  <>
                    <RefreshCw className="w-4 h-4 animate-spin" />
                    <span>Verificando identidad...</span>
                  </>
                ) : (
                  <span>Consultar mis turnos</span>
                )}
              </button>
            </div>
          </form>

          {/* Volver al Login de Personal */}
          <div className="mt-6 pt-5 border-t border-slate-100">
            <button
              type="button"
              onClick={() => navigate('/login')}
              className="w-full flex items-center justify-center gap-2 py-2.5 px-4 rounded-xl border border-slate-200 hover:bg-slate-50 text-slate-600 hover:text-slate-900 text-xs font-semibold transition-colors cursor-pointer"
            >
              <ArrowLeft className="w-3.5 h-3.5" />
              <span>Volver al acceso del personal médico</span>
            </button>
          </div>

          <footer className="text-[11px] text-slate-400 text-center pt-5 mt-4 border-t border-slate-100">
            © 2026 Clínica Enza · Av. Rivadavia 4520, CABA
          </footer>
        </div>
      </div>
    );
  }

  // ============================================================================
  // PASO 2: VISTA DE TURNOS (Una vez autenticado el paciente)
  // ============================================================================
  const displayName =
    pacienteActivo.nombreCompleto ||
    `${pacienteActivo.apellido.toUpperCase()}, ${pacienteActivo.nombre}`;
  if (requiereCambioPassword) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
        <form onSubmit={handleCambiarPassword} className="w-full max-w-md bg-white rounded-2xl border border-slate-200 shadow-sm p-6 space-y-4">
          <h1 className="text-xl font-bold text-slate-900">Cambiar contraseña</h1>
          <p className="text-sm text-slate-600">Por seguridad, reemplazá la contraseña temporal antes de consultar tus turnos.</p>
          {error && <p className="text-sm text-red-600">{error}</p>}
          <input
            type="password"
            required
            minLength={4}
            value={nuevaPassword}
            onChange={(e) => setNuevaPassword(e.target.value)}
            placeholder="Nueva contraseña"
            className="w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/20 focus:border-[#0d5c68]"
          />
          <button type="submit" disabled={cambiandoPassword} className="w-full rounded-xl bg-[#0d5c68] py-3 text-white font-semibold disabled:opacity-60">
            {cambiandoPassword ? 'Guardando...' : 'Guardar contraseña'}
          </button>
        </form>
      </div>
    );
  }

  const displayObraSocial = pacienteActivo.obraSocial || 'Particular';
  const displayDni = pacienteActivo.dni;

  return (
    <div className="min-h-screen bg-slate-50 flex">
      {/* Sidebar Institucional del Portal Paciente */}
      <div className="hidden lg:block sticky top-0 h-screen">
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

            {/* Sección del Área */}
            <div className="px-6 pt-2 pb-3">
              <p className="text-[10px] font-bold tracking-widest text-teal-200/50 uppercase">
                PORTAL DEL PACIENTE
              </p>
            </div>

            {/* Navegación */}
            <nav className="px-3 space-y-1">
              <div className="flex items-center justify-between px-3.5 py-2.5 rounded-xl text-xs bg-[#0b3c46] text-white shadow-inner font-semibold">
                <div className="flex items-center gap-3">
                  <Calendar className="w-5 h-5 text-teal-300" />
                  <span>Mis Turnos Médicos</span>
                </div>
                <span className="w-1.5 h-1.5 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(34,211,238,0.8)]" />
              </div>
            </nav>

            {/* Tarjeta de perfil del paciente en el sidebar izquierdo */}
            <div className="mx-3 mt-6 p-4 rounded-xl bg-[#0b3c46]/60 border border-[#145361]/60 space-y-2.5">
              <div className="flex items-center gap-2 text-[10px] font-bold uppercase tracking-wider text-teal-300">
                <User className="w-3.5 h-3.5" />
                <span>Perfil del Paciente</span>
              </div>
              <div>
                <p className="text-xs font-bold text-white truncate">{displayName}</p>
                <p className="text-[11px] text-teal-100/70 mt-0.5">DNI: {displayDni}</p>
                {pacienteActivo.mail && (
                  <p className="text-[11px] text-teal-100/60 truncate mt-0.5">
                    {pacienteActivo.mail}
                  </p>
                )}
              </div>
              <div className="pt-1.5 border-t border-[#145361]/60 flex items-center justify-between text-[11px]">
                <span className="text-teal-200/70">Cobertura:</span>
                <span className="px-2 py-0.5 rounded bg-teal-400/20 text-teal-200 font-semibold text-[10px]">
                  {displayObraSocial}
                </span>
              </div>
            </div>
          </div>

          {/* Único botón de salida en la barra lateral */}
          <div className="p-4 border-t border-[#0d3c47]/60 space-y-3">
            <button
              type="button"
              onClick={handleSalirCambiarDni}
              className="w-full flex items-center justify-center gap-2 px-3.5 py-2.5 rounded-xl bg-[#0b3c46] hover:bg-[#0d4a56] text-teal-100 text-xs font-semibold transition-colors cursor-pointer"
            >
              <LogOut className="w-4 h-4 text-teal-300" />
              <span>Salir / Cambiar DNI</span>
            </button>

            <div className="space-y-1.5 text-[11px] text-teal-200/60 pt-1">
              <div className="flex items-center gap-2">
                <ShieldCheck className="w-3.5 h-3.5 text-emerald-400/90" />
                <span>Sesión protegida con JWT</span>
              </div>
            </div>
          </div>
        </aside>
      </div>

      {/* Contenedor Principal (Topbar limpia + Contenido) */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Topbar Institucional sin datos redundantes del paciente */}
        <header className="h-16 bg-white border-b border-slate-200/80 px-6 sm:px-8 flex items-center justify-between sticky top-0 z-20">
          <div className="lg:hidden flex items-center gap-2">
            <div className="w-7 h-7 rounded-lg bg-[#0d5c68] flex items-center justify-center text-white">
              <svg className="w-4 h-4 fill-current" viewBox="0 0 24 24">
                <path d="M19 10.5h-5.5V5c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v5.5H5c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5h5.5V19c0 .83.67 1.5 1.5 1.5s1.5-.67 1.5-1.5v-5.5H19c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5z" />
              </svg>
            </div>
            <span className="font-bold text-slate-800 text-sm">Clínica Enza</span>
          </div>

          <div className="hidden lg:block text-xs font-medium text-slate-400">
            Portal de Autogestión del Paciente · Consulta y Cancelación de Turnos
          </div>

          {/* Solo visible en móvil cuando el sidebar izquierdo está oculto */}
          <button
            type="button"
            onClick={handleSalirCambiarDni}
            className="lg:hidden inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 hover:bg-slate-50 text-slate-700 text-xs font-semibold transition-colors cursor-pointer"
          >
            <LogOut className="w-3.5 h-3.5 text-slate-500" />
            <span>Salir / Cambiar DNI</span>
          </button>
        </header>

        {/* Cuerpo Principal */}
        <main className="flex-1 p-6 sm:p-8 lg:p-10 max-w-6xl mx-auto w-full space-y-6 animate-fadeIn">
          {/* Saludo Principal y Botón de Recarga Manual */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <div className="flex items-center gap-2 text-xs font-bold text-[#0d5c68] uppercase tracking-wider">
                <Calendar className="w-4 h-4" />
                <span>Autogestión de Citas Médicas</span>
              </div>
              <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight mt-1">
                Hola, {displayName}
              </h1>
              <p className="text-xs sm:text-sm text-slate-500 mt-1">
                Consultá tus próximos turnos asignados, verificá tu consultorio y gestioná cancelaciones con antelación.
              </p>
            </div>

            <button
              type="button"
              onClick={() => cargarTurnos(pacienteActivo.idPaciente, soloPendientes)}
              disabled={loading}
              className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-semibold shadow-2xs transition-all cursor-pointer self-start sm:self-auto disabled:opacity-50"
            >
              <RefreshCw className={`w-4 h-4 text-[#0d5c68] ${loading ? 'animate-spin' : ''}`} />
              <span>Refrescar Turnos</span>
            </button>
          </div>

          {/* Barra de Filtro Rápido por Estado */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 bg-white p-4 rounded-2xl border border-slate-200/80 shadow-xs">
            <div className="flex items-center gap-2">
              <Activity className="w-4 h-4 text-[#0d5c68]" />
              <span className="text-xs font-bold text-slate-700">Filtrar mis turnos:</span>
            </div>

            <div className="inline-flex rounded-xl bg-slate-100 p-1 border border-slate-200/70 self-start sm:self-auto">
              <button
                type="button"
                onClick={() => handleCambiarFiltro(true)}
                className={`px-4 py-1.5 rounded-lg text-xs font-semibold transition-all cursor-pointer ${
                  soloPendientes
                    ? 'bg-[#0d5c68] text-white shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                Próximos / Pendientes
              </button>
              <button
                type="button"
                onClick={() => handleCambiarFiltro(false)}
                className={`px-4 py-1.5 rounded-lg text-xs font-semibold transition-all cursor-pointer ${
                  !soloPendientes
                    ? 'bg-[#0d5c68] text-white shadow-xs'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                Todos los turnos
              </button>
            </div>
          </div>

          {/* Alerta de Éxito Descartable */}
          {successMessage && (
            <div className="p-4 rounded-xl border border-emerald-200 bg-emerald-50 text-emerald-800 flex items-center justify-between gap-3 shadow-2xs animate-fadeIn">
              <div className="flex items-center gap-2.5">
                <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
                <span className="text-xs sm:text-sm font-medium">{successMessage}</span>
              </div>
              <button
                type="button"
                onClick={() => setSuccessMessage(null)}
                className="p-1 rounded-lg text-emerald-600 hover:text-emerald-900 hover:bg-emerald-100 transition-colors cursor-pointer"
                title="Cerrar notificación"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          )}

          {/* Alerta de Error Descartable */}
          {error && (
            <div className="p-4 rounded-xl border border-red-200 bg-red-50 text-red-800 flex items-center justify-between gap-3 shadow-2xs animate-fadeIn">
              <div className="flex items-center gap-2.5">
                <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
                <span className="text-xs sm:text-sm font-medium">{error}</span>
              </div>
              <button
                type="button"
                onClick={() => setError(null)}
                className="p-1 rounded-lg text-red-600 hover:text-red-900 hover:bg-red-100 transition-colors cursor-pointer"
                title="Cerrar alerta"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          )}

          {/* Estado: Cargando */}
          {loading ? (
            <div className="py-16 bg-white rounded-2xl border border-slate-200/80 flex flex-col items-center justify-center text-slate-400 gap-3">
              <RefreshCw className="w-8 h-8 animate-spin text-[#0d5c68]" />
              <p className="text-xs sm:text-sm font-semibold text-slate-600">
                Consultando turnos del paciente...
              </p>
            </div>
          ) : turnos.length === 0 ? (
            /* Estado Vacío Amigable */
            <div className="py-16 px-6 text-center border border-dashed border-slate-200 rounded-2xl bg-white space-y-3">
              <div className="w-14 h-14 rounded-2xl bg-teal-50/70 border border-teal-100 flex items-center justify-center mx-auto text-[#0d5c68]">
                <Calendar className="w-7 h-7" />
              </div>
              <div className="space-y-1">
                <h3 className="text-base font-bold text-slate-800">
                  {soloPendientes
                    ? 'No tenés turnos pendientes programados'
                    : 'No se encontraron turnos registrados'}
                </h3>
                <p className="text-xs text-slate-500 max-w-md mx-auto leading-relaxed">
                  {soloPendientes
                    ? `Actualmente no hay citas pendientes agendadas para ${displayName}. Podés cambiar al filtro «Todos los turnos» para consultar tu historial de atenciones y cancelaciones.`
                    : `El paciente ${displayName} aún no registra turnos en la clínica.`}
                </p>
              </div>
              {soloPendientes && (
                <div className="pt-2">
                  <button
                    type="button"
                    onClick={() => handleCambiarFiltro(false)}
                    className="px-4 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold transition-colors cursor-pointer"
                  >
                    Ver todos los turnos
                  </button>
                </div>
              )}
            </div>
          ) : (
            /* Listado de Tarjetas de Turnos */
            <div className="space-y-3.5">
              {turnos.map((turno) => {
                const fechaLegible = formatearFecha(turno.horaInicio);
                const horaInicioStr = formatearHora(turno.horaInicio);
                const horaFinStr = formatearHora(turno.horaFin);
                const estadoNorm = obtenerEstadoNormalizado(turno);
                const cancelable = esTurnoCancelable(turno);

                return (
                  <div
                    key={turno.idTurno}
                    className={`bg-white border rounded-2xl p-5 transition-all flex flex-col md:flex-row md:items-center justify-between gap-4 ${
                      estadoNorm === 'Realizado'
                        ? 'border-emerald-200/80 bg-emerald-50/15'
                        : estadoNorm === 'Cancelado'
                        ? 'border-rose-200/70 bg-rose-50/15 opacity-80'
                        : 'border-slate-200/90 hover:border-[#0d5c68]/40 hover:shadow-xs'
                    }`}
                  >
                    {/* Bloque Izquierdo: Franja Horaria y Datos del Turno */}
                    <div className="flex items-start sm:items-center gap-4">
                      {/* Píldora de Horario */}
                      <div className="flex flex-col items-center justify-center bg-sky-50 text-sky-900 rounded-xl px-3.5 py-2.5 shrink-0 border border-sky-100 min-w-20">
                        <Clock className="w-4 h-4 text-[#0d5c68] mb-1" />
                        <span className="text-xs font-bold leading-tight">{horaInicioStr}</span>
                        <span className="text-[10px] text-slate-500 mt-0.5">
                          a {horaFinStr} hs
                        </span>
                      </div>

                      {/* Datos de Fecha, Médico y Consultorio */}
                      <div className="space-y-1.5">
                        <div className="flex flex-wrap items-center gap-2">
                          <span className="text-xs font-bold text-[#0d5c68] capitalize">
                            {fechaLegible}
                          </span>
                          <span className="text-[11px] text-slate-400">
                            · Turno #{turno.idTurno}
                          </span>
                        </div>

                        <div className="flex items-center gap-2">
                          <Stethoscope className="w-4 h-4 text-slate-400 shrink-0" />
                          <h3 className="text-sm sm:text-base font-bold text-slate-900">
                            {turno.nombreMedico || `Médico #${turno.idMedico}`}
                          </h3>
                        </div>

                        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500">
                          <span className="flex items-center gap-1">
                            <MapPin className="w-3.5 h-3.5 text-slate-400" />
                            {turno.ubicacionConsultorio ||
                              (turno.numeroConsultorio
                                ? `Consultorio ${turno.numeroConsultorio}`
                                : `Consultorio #${turno.idConsultorio}`)}
                          </span>
                          {turno.obraSocialPaciente && (
                            <span className="px-2 py-0.5 rounded-md bg-slate-100 text-slate-700 text-[10px] font-semibold">
                              {turno.obraSocialPaciente}
                            </span>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Bloque Derecho: Badge de Estado y Botón de Cancelación */}
                    <div className="flex flex-wrap items-center justify-between md:justify-end gap-3 pt-3 md:pt-0 border-t md:border-t-0 border-slate-100 shrink-0">
                      <span
                        className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold border ${
                          estadoNorm === 'Realizado'
                            ? 'bg-emerald-50 text-emerald-700 border-emerald-200'
                            : estadoNorm === 'Cancelado'
                            ? 'bg-rose-50 text-rose-700 border-rose-200'
                            : 'bg-sky-50 text-sky-700 border-sky-200'
                        }`}
                      >
                        {estadoNorm === 'Realizado' ? (
                          <CheckCircle2 className="w-3.5 h-3.5" />
                        ) : estadoNorm === 'Cancelado' ? (
                          <XCircle className="w-3.5 h-3.5" />
                        ) : (
                          <Clock className="w-3.5 h-3.5" />
                        )}
                        <span>{estadoNorm}</span>
                      </span>

                      {cancelable && (
                        <button
                          type="button"
                          onClick={() => setTurnoParaCancelar(turno)}
                          className="inline-flex items-center justify-center gap-1.5 px-4 py-2 rounded-xl border border-rose-200 bg-rose-50 text-rose-700 hover:bg-rose-100 font-semibold text-xs transition-colors cursor-pointer active:scale-98"
                        >
                          <XCircle className="w-4 h-4 text-rose-600" />
                          <span>Cancelar Turno</span>
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}

          {/* Modal de Confirmación de Cancelación */}
          {turnoParaCancelar && (
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-xs animate-fadeIn">
              <div className="bg-white rounded-2xl border border-slate-200 shadow-xl max-w-md w-full p-6 space-y-4">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-start gap-3">
                    <div className="w-10 h-10 rounded-xl bg-rose-50 border border-rose-100 flex items-center justify-center text-rose-600 shrink-0">
                      <AlertTriangle className="w-5 h-5" />
                    </div>
                    <div>
                      <h3 className="text-base font-bold text-slate-900">
                        ¿Confirmás la cancelación del turno?
                      </h3>
                      <p className="text-xs text-slate-500 mt-0.5 leading-relaxed">
                        Esta acción liberará el horario seleccionado para otros pacientes y no se puede deshacer.
                      </p>
                    </div>
                  </div>
                  <button
                    type="button"
                    onClick={() => setTurnoParaCancelar(null)}
                    className="text-slate-400 hover:text-slate-600 cursor-pointer"
                  >
                    <X className="w-5 h-5" />
                  </button>
                </div>

                <div className="p-4 bg-slate-50 rounded-xl border border-slate-200/80 text-xs text-slate-600 space-y-2">
                  <div className="flex justify-between gap-2">
                    <span className="text-slate-400 font-medium">Profesional:</span>
                    <strong className="text-slate-800 text-right">
                      {turnoParaCancelar.nombreMedico || `Médico #${turnoParaCancelar.idMedico}`}
                    </strong>
                  </div>
                  <div className="flex justify-between gap-2">
                    <span className="text-slate-400 font-medium">Fecha:</span>
                    <strong className="text-slate-800 capitalize text-right">
                      {formatearFecha(turnoParaCancelar.horaInicio)}
                    </strong>
                  </div>
                  <div className="flex justify-between gap-2">
                    <span className="text-slate-400 font-medium">Horario:</span>
                    <strong className="text-slate-800 text-right">
                      {formatearHora(turnoParaCancelar.horaInicio)} a{' '}
                      {formatearHora(turnoParaCancelar.horaFin)} hs
                    </strong>
                  </div>
                  <div className="flex justify-between gap-2">
                    <span className="text-slate-400 font-medium">Consultorio:</span>
                    <strong className="text-slate-800 text-right">
                      {turnoParaCancelar.ubicacionConsultorio ||
                        (turnoParaCancelar.numeroConsultorio
                          ? `Consultorio ${turnoParaCancelar.numeroConsultorio}`
                          : `Consultorio #${turnoParaCancelar.idConsultorio}`)}
                    </strong>
                  </div>
                </div>

                <div className="flex items-center justify-end gap-2.5 pt-2">
                  <button
                    type="button"
                    disabled={cancelando}
                    onClick={() => setTurnoParaCancelar(null)}
                    className="px-4 py-2 rounded-xl border border-slate-200 bg-white text-slate-700 hover:bg-slate-50 text-xs font-semibold transition-colors cursor-pointer disabled:opacity-50"
                  >
                    Conservar turno
                  </button>
                  <button
                    type="button"
                    disabled={cancelando}
                    onClick={handleConfirmarCancelacion}
                    className="px-4 py-2 rounded-xl bg-rose-600 text-white hover:bg-rose-700 text-xs font-semibold transition-colors cursor-pointer disabled:opacity-50 flex items-center gap-1.5 shadow-xs"
                  >
                    <XCircle className="w-4 h-4" />
                    <span>{cancelando ? 'Cancelando...' : 'Sí, cancelar turno'}</span>
                  </button>
                </div>
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};
