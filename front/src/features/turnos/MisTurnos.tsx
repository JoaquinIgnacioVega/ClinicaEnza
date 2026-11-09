import React, { useState, useEffect, useCallback } from 'react';
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
} from 'lucide-react';
import { turnosService } from '../../api/turnosService';
import type { Turno } from '../../types/turnos';
import type { ApiError } from '../../api/client';

interface MisTurnosProps {
  idPacienteInicial?: number;
}

export const MisTurnos: React.FC<MisTurnosProps> = ({ idPacienteInicial = 1 }) => {
  const [idPaciente, setIdPaciente] = useState<number>(idPacienteInicial);
  const [inputPacienteId, setInputPacienteId] = useState<string>(idPacienteInicial.toString());
  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Modal de confirmación de cancelación
  const [turnoParaCancelar, setTurnoParaCancelar] = useState<Turno | null>(null);
  const [cancelando, setCancelando] = useState<boolean>(false);

  const cargarTurnos = useCallback(async (pacienteId: number) => {
    if (pacienteId <= 0) {
      setError('Por favor, ingresa un ID de paciente válido.');
      setTurnos([]);
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const data = await turnosService.obtenerTurnosPorPaciente(pacienteId, true);
      setTurnos(data);
    } catch (err) {
      const apiErr = err as ApiError;
      setError(apiErr.error || 'No fue posible cargar los turnos.');
      setTurnos([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargarTurnos(idPaciente);
  }, [idPaciente, cargarTurnos]);

  const handleBuscarPaciente = (e: React.FormEvent) => {
    e.preventDefault();
    const parsed = parseInt(inputPacienteId, 10);
    if (isNaN(parsed) || parsed <= 0) {
      setError('El ID del paciente debe ser un número mayor a cero.');
      return;
    }
    setSuccessMessage(null);
    setIdPaciente(parsed);
  };

  const esTurnoCancelable = (turno: Turno): boolean => {
    const isPendiente =
      turno.estado === 'Pendiente' ||
      turno.estado === 0 ||
      turno.estadoDescripcion?.toLowerCase() === 'pendiente';
    const fechaInicio = new Date(turno.horaInicio);
    const esFuturo = fechaInicio.getTime() > Date.now();
    return isPendiente && esFuturo;
  };

  const handleConfirmarCancelacion = async () => {
    if (!turnoParaCancelar) return;

    setCancelando(true);
    setError(null);
    try {
      const response = await turnosService.cancelarTurnoPaciente(idPaciente, turnoParaCancelar.idTurno);
      setSuccessMessage(response.mensaje || 'Turno cancelado exitosamente.');
      
      // Actualizar la interfaz en tiempo real removiendo el turno cancelado de pendientes
      setTurnos((prev) =>
        prev.filter((t) => t.idTurno !== turnoParaCancelar.idTurno)
      );
      setTurnoParaCancelar(null);
    } catch (err) {
      const apiErr = err as ApiError;
      setError(apiErr.error || 'Error al intentar cancelar el turno.');
      setTurnoParaCancelar(null);
    } finally {
      setCancelando(false);
    }
  };

  const formatearFechaHora = (isoString: string) => {
    try {
      const date = new Date(isoString);
      return {
        fecha: date.toLocaleDateString('es-AR', {
          weekday: 'long',
          year: 'numeric',
          month: 'long',
          day: 'numeric',
        }),
        hora: date.toLocaleTimeString('es-AR', {
          hour: '2-digit',
          minute: '2-digit',
        }),
      };
    } catch {
      return { fecha: isoString, hora: '' };
    }
  };

  return (
    <div className="w-full max-w-4xl mx-auto p-4 sm:p-6 lg:p-8 animate-fadeIn">
      {/* Encabezado */}
      <div className="mb-8">
        <div className="flex items-center gap-2.5 text-xs font-bold text-[#0d5c68] uppercase tracking-wider mb-1">
          <Calendar className="w-4 h-4" />
          <span>PORTAL DEL PACIENTE</span>
        </div>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
          Mis Turnos Pendientes
        </h1>
        <p className="text-sm text-slate-500 mt-1">
          Consulta tu agenda de citas médicas pendientes y gestiona tus cancelaciones con antelación.
        </p>
      </div>

      {/* Selector / Búsqueda de Paciente */}
      <div className="bg-slate-50 border border-slate-200 rounded-2xl p-4 sm:p-5 mb-6">
        <form onSubmit={handleBuscarPaciente} className="flex flex-col sm:flex-row items-center gap-3">
          <div className="w-full sm:w-auto flex-1">
            <label htmlFor="pacienteIdInput" className="block text-xs font-semibold text-slate-700 mb-1">
              Identificador del Paciente (ID):
            </label>
            <input
              id="pacienteIdInput"
              type="number"
              min="1"
              value={inputPacienteId}
              onChange={(e) => setInputPacienteId(e.target.value)}
              placeholder="Ej: 1"
              className="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-lg text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/30 focus:border-[#0d5c68]"
            />
          </div>
          <div className="w-full sm:w-auto self-end flex gap-2">
            <button
              type="submit"
              className="w-full sm:w-auto px-5 py-2 text-sm font-semibold text-white bg-[#0d5c68] hover:bg-[#09414a] rounded-lg transition-colors shadow-sm"
            >
              Consultar Turnos
            </button>
            <button
              type="button"
              onClick={() => cargarTurnos(idPaciente)}
              title="Recargar lista"
              className="p-2 text-slate-600 bg-white border border-slate-200 hover:bg-slate-100 rounded-lg transition-colors"
            >
              <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            </button>
          </div>
        </form>
      </div>

      {/* Alerta de Éxito */}
      {successMessage && (
        <div className="mb-6 p-4 rounded-xl border border-emerald-200 bg-emerald-50/90 text-emerald-800 flex items-start justify-between gap-3 shadow-xs">
          <div className="flex items-center gap-2.5">
            <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
            <span className="text-sm font-medium">{successMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => setSuccessMessage(null)}
            className="text-emerald-600 hover:text-emerald-800 text-xs font-semibold"
          >
            ✕
          </button>
        </div>
      )}

      {/* Alerta de Error */}
      {error && (
        <div className="mb-6 p-4 rounded-xl border border-red-200 bg-red-50 text-red-800 flex items-start justify-between gap-3 shadow-xs">
          <div className="flex items-center gap-2.5">
            <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
            <span className="text-sm font-medium">{error}</span>
          </div>
          <button
            type="button"
            onClick={() => setError(null)}
            className="text-red-600 hover:text-red-800 text-xs font-semibold"
          >
            ✕
          </button>
        </div>
      )}

      {/* Estado: Cargando */}
      {loading ? (
        <div className="py-16 flex flex-col items-center justify-center text-slate-400 gap-3">
          <RefreshCw className="w-8 h-8 animate-spin text-[#0d5c68]" />
          <p className="text-sm font-medium text-slate-600">Cargando turnos pendientes...</p>
        </div>
      ) : turnos.length === 0 ? (
        /* Estado: Sin turnos */
        <div className="py-16 px-4 text-center border-2 border-dashed border-slate-200 rounded-2xl bg-white">
          <div className="w-14 h-14 rounded-full bg-slate-50 flex items-center justify-center mx-auto mb-3 text-slate-400">
            <Calendar className="w-7 h-7" />
          </div>
          <h3 className="text-base font-bold text-slate-800">No tienes turnos pendientes</h3>
          <p className="text-xs text-slate-400 mt-1 max-w-sm mx-auto">
            Actualmente no posees citas pendientes agendadas para el paciente ID {idPaciente}.
          </p>
        </div>
      ) : (
        /* Listado de Turnos */
        <div className="space-y-4">
          {turnos.map((turno) => {
            const { fecha, hora } = formatearFechaHora(turno.horaInicio);
            const cancelable = esTurnoCancelable(turno);

            return (
              <div
                key={turno.idTurno}
                className="bg-white border border-slate-200 rounded-2xl p-5 shadow-xs hover:shadow-md transition-shadow flex flex-col md:flex-row md:items-center justify-between gap-4"
              >
                {/* Información principal */}
                <div className="flex-1 space-y-2">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                      <Clock className="w-3.5 h-3.5" />
                      {turno.estadoDescripcion || 'Pendiente'}
                    </span>
                    <span className="text-xs font-semibold text-slate-500 capitalize">
                      {fecha} - {hora} hs
                    </span>
                  </div>

                  <div className="flex items-start gap-3 pt-1">
                    <div className="w-10 h-10 rounded-xl bg-[#0d5c68]/10 text-[#0d5c68] flex items-center justify-center shrink-0">
                      <Stethoscope className="w-5 h-5" />
                    </div>
                    <div>
                      <h4 className="text-base font-bold text-slate-900 leading-snug">
                        {turno.nombreMedico || `Médico ID: ${turno.idMedico}`}
                      </h4>
                      <p className="text-xs text-slate-500 flex items-center gap-1 mt-0.5">
                        <MapPin className="w-3.5 h-3.5 text-slate-400" />
                        {turno.ubicacionConsultorio || `Consultorio ${turno.numeroConsultorio || turno.idConsultorio}`}
                      </p>
                    </div>
                  </div>

                  <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-400 pt-1">
                    <span className="flex items-center gap-1">
                      <User className="w-3 h-3" />
                      Paciente: <strong className="text-slate-700">{turno.nombrePaciente || `ID ${turno.idPaciente}`}</strong>
                    </span>
                    {turno.obraSocialPaciente && (
                      <span>
                        Cobertura: <strong className="text-slate-700">{turno.obraSocialPaciente}</strong>
                      </span>
                    )}
                  </div>
                </div>

                {/* Acciones */}
                <div className="shrink-0 flex items-center gap-2 pt-2 md:pt-0 border-t md:border-t-0 border-slate-100">
                  {cancelable ? (
                    <button
                      type="button"
                      onClick={() => setTurnoParaCancelar(turno)}
                      className="w-full md:w-auto inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl border border-red-200 bg-red-50 text-red-700 hover:bg-red-100 font-semibold text-xs transition-colors shadow-2xs cursor-pointer active:scale-98"
                    >
                      <XCircle className="w-4 h-4 text-red-500" />
                      Cancelar Turno
                    </button>
                  ) : (
                    <span className="text-xs text-slate-400 italic">
                      No cancelable (fecha vencida o estado no pendiente)
                    </span>
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
            <div className="flex items-start gap-3">
              <div className="w-10 h-10 rounded-xl bg-red-50 border border-red-100 flex items-center justify-center text-red-600 shrink-0">
                <AlertTriangle className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-base font-bold text-slate-900">
                  ¿Confirmas la cancelación del turno?
                </h3>
                <p className="text-xs text-slate-500 mt-0.5">
                  Esta acción liberará el horario para otros pacientes y no se puede deshacer.
                </p>
              </div>
            </div>

            <div className="p-3 bg-slate-50 rounded-xl border border-slate-200/80 text-xs text-slate-600 space-y-1">
              <div>
                Médico: <strong className="text-slate-800">{turnoParaCancelar.nombreMedico || `ID ${turnoParaCancelar.idMedico}`}</strong>
              </div>
              <div>
                Fecha y Hora: <strong className="text-slate-800">{new Date(turnoParaCancelar.horaInicio).toLocaleString('es-AR')}</strong>
              </div>
              <div>
                Consultorio: <strong className="text-slate-800">{turnoParaCancelar.ubicacionConsultorio || turnoParaCancelar.numeroConsultorio}</strong>
              </div>
            </div>

            <div className="flex items-center justify-end gap-3 pt-2">
              <button
                type="button"
                disabled={cancelando}
                onClick={() => setTurnoParaCancelar(null)}
                className="px-4 py-2 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-semibold transition-colors"
              >
                Volver
              </button>
              <button
                type="button"
                disabled={cancelando}
                onClick={handleConfirmarCancelacion}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-red-600 hover:bg-red-700 text-white text-xs font-semibold transition-colors shadow-sm disabled:opacity-50"
              >
                {cancelando ? (
                  <>
                    <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                    Cancelando...
                  </>
                ) : (
                  'Confirmar Cancelación'
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
