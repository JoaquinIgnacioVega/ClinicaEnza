import React, { useState, useEffect, useCallback } from 'react';
import {
  Calendar,
  Clock,
  User,
  MapPin,
  CreditCard,
  CheckCircle2,
  XCircle,
  AlertCircle,
  RefreshCw,
  Stethoscope,
  Filter,
  ClipboardCheck,
  ShieldAlert,
  FileText,
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { agendaService } from '../../api/agendaService';
import type { ApiError } from '../../api/client';
import type {
  EstadoTurno,
  FiltroEstadoAgenda,
  TurnoAgendaMedico,
} from '../../types/turnos';

const getTodayLocalISO = (): string => {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const d = String(now.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
};

const normalizarEstado = (turno: TurnoAgendaMedico): EstadoTurno => {
  if (turno.estadoDescripcion === 'Realizado' || turno.estado === 1 || turno.estado === 'Realizado') {
    return 'Realizado';
  }
  if (turno.estadoDescripcion === 'Cancelado' || turno.estado === 2 || turno.estado === 'Cancelado') {
    return 'Cancelado';
  }
  return 'Pendiente';
};

export const AgendaMedica: React.FC = () => {
  const { user, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const idMedicoActual = user?.idMedico ?? null;

  const [fechaFiltro, setFechaFiltro] = useState<string>(getTodayLocalISO());
  const [estadoFiltro, setEstadoFiltro] = useState<FiltroEstadoAgenda>('Todos');

  const [turnos, setTurnos] = useState<TurnoAgendaMedico[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Modales de acciones médicas
  const [turnoParaAtender, setTurnoParaAtender] = useState<TurnoAgendaMedico | null>(null);
  const [medicamentoRecetado, setMedicamentoRecetado] = useState<string>('');
  const [turnoParaCancelar, setTurnoParaCancelar] = useState<TurnoAgendaMedico | null>(null);
  const [procesandoAccion, setProcesandoAccion] = useState<boolean>(false);

  const cargarAgenda = useCallback(async () => {
    if (!idMedicoActual) {
      setTurnos([]);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const data = await agendaService.obtenerAgendaMedico({
        idMedico: idMedicoActual,
        fecha: fechaFiltro || null,
        estado: estadoFiltro,
        idMedicoSolicitante: idMedicoActual,
      });

      const ordenados = [...data].sort(
        (a, b) => new Date(a.horaInicio).getTime() - new Date(b.horaInicio).getTime()
      );
      setTurnos(ordenados);
    } catch (err) {
      const apiErr = err as ApiError;
      setError(apiErr.error || 'No se pudo cargar la agenda médica.');
      setTurnos([]);
    } finally {
      setLoading(false);
    }
  }, [idMedicoActual, fechaFiltro, estadoFiltro]);

  useEffect(() => {
    if (isAuthenticated && idMedicoActual) {
      cargarAgenda();
    }
  }, [isAuthenticated, idMedicoActual, cargarAgenda]);

  const handleMarcarRealizado = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!turnoParaAtender) return;

    setProcesandoAccion(true);
    setError(null);
    setSuccessMessage(null);

    try {
      await agendaService.marcarTurnoRealizado(turnoParaAtender.idTurno, medicamentoRecetado);
      setSuccessMessage(
        `Turno de ${turnoParaAtender.nombreCompletoPaciente} marcado como realizado.`
      );
      setTurnoParaAtender(null);
      setMedicamentoRecetado('');
      await cargarAgenda();
    } catch (err) {
      const apiErr = err as ApiError;
      setError(apiErr.error || 'No se pudo marcar el turno como realizado.');
      setTurnoParaAtender(null);
    } finally {
      setProcesandoAccion(false);
    }
  };

  const handleCancelarTurno = async () => {
    if (!turnoParaCancelar) return;

    setProcesandoAccion(true);
    setError(null);
    setSuccessMessage(null);

    try {
      await agendaService.cancelarTurno(turnoParaCancelar.idTurno);
      setSuccessMessage(
        `Turno de ${turnoParaCancelar.nombreCompletoPaciente} cancelado correctamente.`
      );
      setTurnoParaCancelar(null);
      await cargarAgenda();
    } catch (err) {
      const apiErr = err as ApiError;
      setError(apiErr.error || 'No se pudo cancelar el turno.');
      setTurnoParaCancelar(null);
    } finally {
      setProcesandoAccion(false);
    }
  };

  const getBadgeEstado = (estado: EstadoTurno) => {
    switch (estado) {
      case 'Pendiente':
        return {
          classes: 'bg-amber-50 text-amber-800 border-amber-200',
          icon: <Clock className="w-3.5 h-3.5 text-amber-600" />,
        };
      case 'Realizado':
        return {
          classes: 'bg-emerald-50 text-emerald-800 border-emerald-200',
          icon: <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600" />,
        };
      case 'Cancelado':
        return {
          classes: 'bg-rose-50 text-rose-800 border-rose-200',
          icon: <XCircle className="w-3.5 h-3.5 text-rose-600" />,
        };
    }
  };

  const formatearHora = (iso: string) =>
    new Date(iso).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });

  const formatearFecha = (iso: string) =>
    new Date(iso).toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });

  if (!isAuthenticated || !user) {
    return (
      <div className="max-w-3xl mx-auto p-6 sm:p-10">
        <div className="p-8 rounded-2xl border border-slate-200 bg-slate-50 text-center space-y-3">
          <div className="w-12 h-12 rounded-xl bg-[#0d5c68]/10 text-[#0d5c68] flex items-center justify-center mx-auto">
            <ShieldAlert className="w-6 h-6" />
          </div>
          <h2 className="text-lg font-bold text-slate-900">Inicia sesión como profesional médico</h2>
          <p className="text-sm text-slate-500 max-w-md mx-auto">
            Para consultar tu agenda médica privada debes iniciar sesión con una cuenta de rol Médico desde la pestaña de Acceso.
          </p>
        </div>
      </div>
    );
  }

  if (!idMedicoActual) {
    return (
      <div className="max-w-3xl mx-auto p-6 sm:p-10">
        <div className="p-8 rounded-2xl border border-amber-200 bg-amber-50 text-center space-y-3">
          <div className="w-12 h-12 rounded-xl bg-amber-100 text-amber-700 flex items-center justify-center mx-auto">
            <ShieldAlert className="w-6 h-6" />
          </div>
          <h2 className="text-lg font-bold text-amber-950">Acceso exclusivo para médicos</h2>
          <p className="text-sm text-amber-800 max-w-md mx-auto">
            Tu sesión actual ({user.rolNombre}) no posee un perfil médico asociado para consultar una agenda profesional.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full max-w-6xl mx-auto animate-fadeIn">
      {/* Encabezado */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold text-[#0d5c68] uppercase tracking-wider mb-1">
            <Stethoscope className="w-4 h-4" />
            <span>MÓDULO DE ATENCIÓN MÉDICA</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
            Agenda Médica
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Profesional: <strong className="text-slate-700">{user.nombreCompleto}</strong> (ID #{idMedicoActual})
          </p>
        </div>

        <button
          type="button"
          onClick={cargarAgenda}
          disabled={loading}
          className="inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-semibold transition-colors shadow-2xs disabled:opacity-50"
        >
          <RefreshCw className={`w-4 h-4 text-[#0d5c68] ${loading ? 'animate-spin' : ''}`} />
          Actualizar Agenda
        </button>
      </div>

      {/* Controles de Filtrado */}
      <div className="bg-slate-50 border border-slate-200 rounded-2xl p-4 sm:p-5 mb-6">
        <div className="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-wider mb-3">
          <Filter className="w-3.5 h-3.5" />
          <span>Filtros de Agenda</span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-12 gap-4 items-end">
          {/* Selector de Fecha */}
          <div className="md:col-span-5">
            <label htmlFor="agenda-fecha" className="block text-xs font-semibold text-slate-700 mb-1.5">
              Fecha de atención
            </label>
            <div className="flex items-center gap-2">
              <input
                id="agenda-fecha"
                type="date"
                value={fechaFiltro}
                onChange={(e) => setFechaFiltro(e.target.value)}
                className="flex-1 px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-lg text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/30 focus:border-[#0d5c68]"
              />
              <button
                type="button"
                onClick={() => setFechaFiltro(getTodayLocalISO())}
                className={`px-3 py-2 text-xs font-semibold rounded-lg border transition-colors ${
                  fechaFiltro === getTodayLocalISO()
                    ? 'bg-[#0d5c68] text-white border-[#0d5c68]'
                    : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-100'
                }`}
              >
                Hoy
              </button>
              {fechaFiltro && (
                <button
                  type="button"
                  onClick={() => setFechaFiltro('')}
                  className="px-3 py-2 text-xs font-semibold rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 transition-colors"
                >
                  Todas
                </button>
              )}
            </div>
          </div>

          {/* Selector de Estado */}
          <div className="md:col-span-4">
            <label htmlFor="agenda-estado" className="block text-xs font-semibold text-slate-700 mb-1.5">
              Estado del turno
            </label>
            <select
              id="agenda-estado"
              value={estadoFiltro}
              onChange={(e) => setEstadoFiltro(e.target.value as FiltroEstadoAgenda)}
              className="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-lg text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/30 focus:border-[#0d5c68]"
            >
              <option value="Todos">Todos</option>
              <option value="Pendiente">Pendiente</option>
              <option value="Realizado">Realizado</option>
              <option value="Cancelado">Cancelado</option>
            </select>
          </div>

          {/* Contador rápido */}
          <div className="md:col-span-3 flex md:justify-end">
            <div className="w-full md:w-auto px-4 py-2 bg-white border border-slate-200 rounded-lg text-xs text-slate-600 flex items-center justify-between md:justify-end gap-2">
              <span>Turnos encontrados:</span>
              <strong className="text-sm text-[#0d5c68]">{turnos.length}</strong>
            </div>
          </div>
        </div>
      </div>

      {/* Alertas */}
      {successMessage && (
        <div className="mb-6 p-4 rounded-xl border border-emerald-200 bg-emerald-50 text-emerald-800 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2.5 text-sm font-medium">
            <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
            <span>{successMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => setSuccessMessage(null)}
            className="text-xs font-bold text-emerald-700 hover:text-emerald-900"
          >
            ✕
          </button>
        </div>
      )}

      {error && (
        <div className="mb-6 p-4 rounded-xl border border-red-200 bg-red-50 text-red-800 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2.5 text-sm font-medium">
            <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
            <span>{error}</span>
          </div>
          <button
            type="button"
            onClick={() => setError(null)}
            className="text-xs font-bold text-red-700 hover:text-red-900"
          >
            ✕
          </button>
        </div>
      )}

      {/* Contenido Principal */}
      {loading ? (
        <div className="py-16 flex flex-col items-center justify-center gap-3 text-slate-500">
          <RefreshCw className="w-8 h-8 animate-spin text-[#0d5c68]" />
          <span className="text-sm font-medium">Cargando agenda médica...</span>
        </div>
      ) : turnos.length === 0 ? (
        <div className="py-16 px-6 text-center border-2 border-dashed border-slate-200 rounded-2xl bg-white">
          <div className="w-14 h-14 rounded-full bg-slate-50 flex items-center justify-center mx-auto mb-3 text-slate-400">
            <Calendar className="w-7 h-7" />
          </div>
          <h3 className="text-base font-bold text-slate-800">
            No se encontraron turnos programados para la fecha o filtros seleccionados
          </h3>
          <p className="text-xs text-slate-500 mt-1 max-w-md mx-auto">
            Prueba cambiando la fecha seleccionada o cambiando el filtro de estado a &quot;Todos&quot;.
          </p>
          {(fechaFiltro !== '' || estadoFiltro !== 'Todos') && (
            <button
              type="button"
              onClick={() => {
                setFechaFiltro('');
                setEstadoFiltro('Todos');
              }}
              className="mt-4 inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-[#0d5c68]/10 text-[#0d5c68] hover:bg-[#0d5c68]/20 text-xs font-semibold transition-colors"
            >
              Ver toda mi agenda
            </button>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {turnos.map((turno) => {
            const estado = normalizarEstado(turno);
            const badge = getBadgeEstado(estado);

            return (
              <div
                key={turno.idTurno}
                className="bg-white border border-slate-200 rounded-2xl p-4 sm:p-5 shadow-2xs hover:shadow-md transition-shadow flex flex-col lg:flex-row lg:items-center justify-between gap-4"
              >
                {/* Bloque de horario y estado */}
                <div className="flex flex-wrap items-center gap-3 lg:w-52 shrink-0">
                  <div className="px-3 py-2 rounded-xl bg-[#0d5c68]/10 text-[#0d5c68] text-center min-w-[105px]">
                    <div className="text-sm font-extrabold leading-none">
                      {formatearHora(turno.horaInicio)} - {formatearHora(turno.horaFin)}
                    </div>
                    <div className="text-[11px] font-medium opacity-80 mt-1">
                      {formatearFecha(turno.horaInicio)}
                    </div>
                  </div>
                  <span
                    className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold border ${badge.classes}`}
                  >
                    {badge.icon}
                    {estado}
                  </span>
                </div>

                {/* Datos del paciente y consultorio */}
                <div className="flex-1 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 text-xs">
                  <div className="space-y-1">
                    <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
                      Paciente
                    </span>
                    <div className="flex items-center gap-1.5 font-bold text-sm text-slate-900">
                      <User className="w-4 h-4 text-[#0d5c68] shrink-0" />
                      <span>{turno.nombreCompletoPaciente}</span>
                    </div>
                    <div className="text-slate-500">
                      Edad: <strong className="text-slate-700">{turno.edadPaciente} años</strong>
                    </div>
                  </div>

                  <div className="space-y-1">
                    <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
                      Identificación y Cobertura
                    </span>
                    <div className="flex items-center gap-1.5 text-slate-700 font-medium">
                      <CreditCard className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                      <span>DNI: {turno.dniPaciente}</span>
                    </div>
                    <div className="text-slate-500">
                      Obra Social: <strong className="text-slate-700">{turno.obraSocialPaciente}</strong>
                    </div>
                  </div>

                  <div className="space-y-1">
                    <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
                      Consultorio
                    </span>
                    <div className="flex items-start gap-1.5 text-slate-700 font-medium">
                      <MapPin className="w-3.5 h-3.5 text-[#0d5c68] shrink-0 mt-0.5" />
                      <span>{turno.ubicacionConsultorio}</span>
                    </div>
                  </div>
                </div>

                {/* Acciones de atención médica */}
                <div className="flex flex-wrap items-center gap-2 pt-3 lg:pt-0 border-t lg:border-t-0 border-slate-100 shrink-0">
                  <button
                    type="button"
                    onClick={() => navigate(`/medico/historia-clinica?pacienteId=${turno.idPaciente}`)}
                    className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-[#0d5c68] text-xs font-semibold transition-colors"
                    title="Consultar Historia Clínica del Paciente"
                  >
                    <FileText className="w-4 h-4" />
                    Historia Clínica
                  </button>

                  {estado === 'Pendiente' ? (
                    <>
                      <button
                        type="button"
                        onClick={() => {
                          setTurnoParaAtender(turno);
                          setMedicamentoRecetado('');
                        }}
                        className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-[#0d5c68] hover:bg-[#09414a] text-white text-xs font-semibold transition-colors shadow-2xs"
                      >
                        <ClipboardCheck className="w-4 h-4" />
                        Marcar Realizado
                      </button>
                      <button
                        type="button"
                        onClick={() => setTurnoParaCancelar(turno)}
                        className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl border border-rose-200 bg-rose-50 hover:bg-rose-100 text-rose-700 text-xs font-semibold transition-colors"
                      >
                        <XCircle className="w-4 h-4" />
                        Cancelar
                      </button>
                    </>
                  ) : (
                    <span className="text-xs text-slate-400 italic px-2">Sin acciones pendientes</span>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Modal: Marcar Turno como Realizado */}
      {turnoParaAtender && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-xs">
          <form
            onSubmit={handleMarcarRealizado}
            className="bg-white rounded-2xl border border-slate-200 shadow-xl max-w-md w-full p-6 space-y-4"
          >
            <div className="flex items-start gap-3">
              <div className="w-10 h-10 rounded-xl bg-emerald-50 border border-emerald-200 flex items-center justify-center text-emerald-700 shrink-0">
                <ClipboardCheck className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-base font-bold text-slate-900">Finalizar Atención Médica</h3>
                <p className="text-xs text-slate-500 mt-0.5">
                  Paciente: <strong>{turnoParaAtender.nombreCompletoPaciente}</strong> ({formatearHora(turnoParaAtender.horaInicio)} hs)
                </p>
              </div>
            </div>

            <div>
              <label
                htmlFor="medicamento-recetado"
                className="block text-xs font-semibold text-slate-700 mb-1.5"
              >
                Prescripción médica / Medicamento recetado (opcional)
              </label>
              <textarea
                id="medicamento-recetado"
                rows={3}
                value={medicamentoRecetado}
                onChange={(e) => setMedicamentoRecetado(e.target.value)}
                placeholder="Ej: Ibuprofeno 400mg cada 8 horas por 3 días"
                className="w-full px-3.5 py-2 text-sm border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/30 focus:border-[#0d5c68]"
              />
            </div>

            <div className="flex items-center justify-end gap-3 pt-2">
              <button
                type="button"
                disabled={procesandoAccion}
                onClick={() => setTurnoParaAtender(null)}
                className="px-4 py-2 rounded-lg border border-slate-200 text-slate-700 hover:bg-slate-50 text-xs font-semibold"
              >
                Volver
              </button>
              <button
                type="submit"
                disabled={procesandoAccion}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-[#0d5c68] hover:bg-[#09414a] text-white text-xs font-semibold disabled:opacity-50"
              >
                {procesandoAccion ? 'Guardando...' : 'Confirmar Atención'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Modal: Cancelar Turno */}
      {turnoParaCancelar && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-xs">
          <div className="bg-white rounded-2xl border border-slate-200 shadow-xl max-w-md w-full p-6 space-y-4">
            <div className="flex items-start gap-3">
              <div className="w-10 h-10 rounded-xl bg-rose-50 border border-rose-200 flex items-center justify-center text-rose-600 shrink-0">
                <XCircle className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-base font-bold text-slate-900">¿Cancelar turno asignado?</h3>
                <p className="text-xs text-slate-500 mt-0.5">
                  Paciente: <strong>{turnoParaCancelar.nombreCompletoPaciente}</strong> —{' '}
                  {formatearFecha(turnoParaCancelar.horaInicio)} {formatearHora(turnoParaCancelar.horaInicio)} hs
                </p>
              </div>
            </div>

            <div className="flex items-center justify-end gap-3 pt-2">
              <button
                type="button"
                disabled={procesandoAccion}
                onClick={() => setTurnoParaCancelar(null)}
                className="px-4 py-2 rounded-lg border border-slate-200 text-slate-700 hover:bg-slate-50 text-xs font-semibold"
              >
                Volver
              </button>
              <button
                type="button"
                disabled={procesandoAccion}
                onClick={handleCancelarTurno}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold disabled:opacity-50"
              >
                {procesandoAccion ? 'Cancelando...' : 'Confirmar Cancelación'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

