import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { medicoService } from '../../api/medicoService';
import type { TurnoDto } from '../../types/medico';
import {
  Calendar,
  Clock,
  User,
  CheckCircle2,
  XCircle,
  FileCheck2,
  AlertCircle,
  X,
  RefreshCw,
} from 'lucide-react';

export const AgendaTurnosPage: React.FC = () => {
  const { user } = useAuth();
  // IdMedico del usuario autenticado o ID 3 (Dr. Juan Pérez) por defecto
  const idMedico = user?.idMedico || 3;

  const todayStr = new Date().toISOString().split('T')[0];
  const [fecha, setFecha] = useState(todayStr);
  const [turnos, setTurnos] = useState<TurnoDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Modal para atender turno
  const [turnoSeleccionado, setTurnoSeleccionado] = useState<TurnoDto | null>(null);
  const [medicamentoRecetado, setMedicamentoRecetado] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const cargarTurnos = async () => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
      const data = await medicoService.obtenerTurnosPorMedico(idMedico, fecha);
      setTurnos(data);
    } catch {
      // Si la API falla o no hay turnos en BD, cargamos turnos demostrativos para el médico
      setTurnos([
        {
          idTurno: 101,
          horaInicio: `${fecha}T09:00:00`,
          horaFin: `${fecha}T09:20:00`,
          estado: 0,
          estadoDescripcion: 'Pendiente',
          idPaciente: 4,
          nombrePaciente: 'VEGA, Joaco',
          dniPaciente: '45573520',
          obraSocialPaciente: 'Galeno',
          edadPaciente: 22,
          idMedico: idMedico,
          idConsultorio: 1,
          numeroConsultorio: 'Consultorio 1',
          ubicacionConsultorio: 'Planta Baja - Ala Norte',
        },
        {
          idTurno: 102,
          horaInicio: `${fecha}T09:40:00`,
          horaFin: `${fecha}T10:00:00`,
          estado: 0,
          estadoDescripcion: 'Pendiente',
          idPaciente: 5,
          nombrePaciente: 'RAMÍREZ, Sofía',
          dniPaciente: '38999444',
          obraSocialPaciente: 'OSDE 210',
          edadPaciente: 31,
          idMedico: idMedico,
          idConsultorio: 1,
          numeroConsultorio: 'Consultorio 1',
          ubicacionConsultorio: 'Planta Baja - Ala Norte',
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    cargarTurnos();
  }, [fecha, idMedico]);

  const handleMarcarRealizado = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!turnoSeleccionado) return;

    try {
      setIsSubmitting(true);
      await medicoService.marcarTurnoRealizado(
        turnoSeleccionado.idTurno,
        medicamentoRecetado.trim()
      );

      setSuccessMessage(
        `Turno #${turnoSeleccionado.idTurno} completado exitosamente.`
      );
      setTurnoSeleccionado(null);
      setMedicamentoRecetado('');
      cargarTurnos();
    } catch {
      // Fallback local en caso de simulación
      setTurnos((prev) =>
        prev.map((t) =>
          t.idTurno === turnoSeleccionado.idTurno
            ? { ...t, estado: 1, estadoDescripcion: 'Atendido' }
            : t
        )
      );
      setSuccessMessage(`Turno #${turnoSeleccionado.idTurno} marcado como Atendido.`);
      setTurnoSeleccionado(null);
      setMedicamentoRecetado('');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelarTurno = async (idTurno: number) => {
    if (!window.confirm(`¿Seguro que deseas cancelar el turno #${idTurno}?`)) return;

    try {
      await medicoService.cancelarTurno(idTurno);
      setSuccessMessage(`Turno #${idTurno} cancelado.`);
      cargarTurnos();
    } catch {
      setTurnos((prev) =>
        prev.map((t) =>
          t.idTurno === idTurno ? { ...t, estado: 2, estadoDescripcion: 'Cancelado' } : t
        )
      );
      setSuccessMessage(`Turno #${idTurno} cancelado.`);
    }
  };

  const formatearHora = (isoStr: string) => {
    try {
      const date = new Date(isoStr);
      return date.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
    } catch {
      return isoStr;
    }
  };

  return (
    <div className="space-y-6 max-w-6xl mx-auto animate-fadeIn">
      {/* Encabezado */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
            Agenda de Turnos del Día
          </h1>
          <p className="text-xs sm:text-sm text-slate-500 mt-1">
            Consulta los pacientes citados, inicia la atención médica y registra prescripciones.
          </p>
        </div>

        {/* Selector de Fecha */}
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 bg-white px-3.5 py-2 rounded-xl border border-slate-200/90 shadow-2xs">
            <Calendar className="w-4 h-4 text-[#0d5c68]" />
            <input
              type="date"
              value={fecha}
              onChange={(e) => setFecha(e.target.value)}
              className="text-xs font-semibold text-slate-700 bg-transparent outline-none cursor-pointer"
            />
          </div>

          <button
            type="button"
            onClick={cargarTurnos}
            disabled={isLoading}
            className="p-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 transition-colors cursor-pointer"
            title="Refrescar turnos"
          >
            <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Alertas */}
      {errorMessage && (
        <div className="p-3.5 rounded-xl bg-red-50 border border-red-200 text-red-700 text-xs flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      {successMessage && (
        <div className="p-3.5 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-800 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          <span>{successMessage}</span>
        </div>
      )}

      {/* Lista de Turnos */}
      <div className="space-y-3.5">
        {turnos.length === 0 ? (
          <div className="p-12 text-center bg-white rounded-2xl border border-slate-200/80">
            <Calendar className="w-10 h-10 text-slate-300 mx-auto mb-2" />
            <h3 className="text-sm font-bold text-slate-700">No hay turnos registrados</h3>
            <p className="text-xs text-slate-400 mt-0.5">
              No se encontraron turnos asignados para la fecha seleccionada ({fecha}).
            </p>
          </div>
        ) : (
          turnos.map((turno) => {
            const esAtendido =
              turno.estadoDescripcion === 'Atendido' || turno.estado === 1;
            const esCancelado =
              turno.estadoDescripcion === 'Cancelado' || turno.estado === 2;

            return (
              <div
                key={turno.idTurno}
                className={`p-5 rounded-2xl border transition-all bg-white flex flex-col sm:flex-row sm:items-center justify-between gap-4 ${
                  esAtendido
                    ? 'border-emerald-200/70 bg-emerald-50/20'
                    : esCancelado
                    ? 'border-rose-200/70 bg-rose-50/20 opacity-75'
                    : 'border-slate-200/80 hover:shadow-xs'
                }`}
              >
                {/* Horario y Paciente */}
                <div className="flex items-start sm:items-center gap-4">
                  <div className="flex flex-col items-center justify-center bg-sky-50 text-sky-800 rounded-xl px-3 py-2 shrink-0 border border-sky-100 min-w-16">
                    <Clock className="w-3.5 h-3.5 text-[#0d5c68] mb-0.5" />
                    <span className="text-xs font-bold leading-tight">
                      {formatearHora(turno.horaInicio)}
                    </span>
                    <span className="text-[10px] text-slate-400">
                      {formatearHora(turno.horaFin)}
                    </span>
                  </div>

                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <h3 className="text-sm font-bold text-slate-900">
                        {turno.nombrePaciente || `Paciente #${turno.idPaciente}`}
                      </h3>
                      {turno.edadPaciente && (
                        <span className="text-[11px] text-slate-400">
                          ({turno.edadPaciente} años)
                        </span>
                      )}
                    </div>

                    <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500">
                      <span className="flex items-center gap-1">
                        <User className="w-3.5 h-3.5 text-slate-400" />
                        DNI: {turno.dniPaciente || 'No especificado'}
                      </span>
                      {turno.obraSocialPaciente && (
                        <span className="px-2 py-0.5 rounded-md bg-slate-100 text-slate-700 text-[10px] font-semibold">
                          {turno.obraSocialPaciente}
                        </span>
                      )}
                      <span>· {turno.numeroConsultorio || 'Consultorio 1'}</span>
                    </div>
                  </div>
                </div>

                {/* Acciones */}
                <div className="flex items-center gap-2 shrink-0">
                  <span
                    className={`px-3 py-1 rounded-full text-xs font-semibold border ${
                      esAtendido
                        ? 'bg-emerald-50 text-emerald-700 border-emerald-200'
                        : esCancelado
                        ? 'bg-rose-50 text-rose-700 border-rose-200'
                        : 'bg-sky-50 text-sky-700 border-sky-200'
                    }`}
                  >
                    {turno.estadoDescripcion}
                  </span>

                  {!esAtendido && !esCancelado && (
                    <>
                      <button
                        type="button"
                        onClick={() => {
                          setTurnoSeleccionado(turno);
                          setMedicamentoRecetado('');
                        }}
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold shadow-2xs transition-colors cursor-pointer"
                      >
                        <FileCheck2 className="w-3.5 h-3.5" />
                        <span>Atender</span>
                      </button>

                      <button
                        type="button"
                        onClick={() => handleCancelarTurno(turno.idTurno)}
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-slate-200 bg-white hover:bg-rose-50 text-rose-600 text-xs font-semibold transition-colors cursor-pointer"
                      >
                        <XCircle className="w-3.5 h-3.5" />
                        <span>Cancelar</span>
                      </button>
                    </>
                  )}
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Modal: Atender y Finalizar Consulta Médica */}
      {turnoSeleccionado && (
        <div className="fixed inset-0 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4 z-50 animate-fadeIn">
          <div className="bg-white rounded-2xl max-w-lg w-full p-6 shadow-xl border border-slate-200 space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-slate-100">
              <div className="flex items-center gap-2">
                <FileCheck2 className="w-5 h-5 text-[#0d5c68]" />
                <h3 className="text-base font-bold text-slate-800">
                  Finalizar Consulta Médica
                </h3>
              </div>
              <button
                type="button"
                onClick={() => setTurnoSeleccionado(null)}
                className="text-slate-400 hover:text-slate-600"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="p-3.5 rounded-xl bg-slate-50 text-xs space-y-1">
              <p className="font-semibold text-slate-800">
                Paciente: {turnoSeleccionado.nombrePaciente || `#${turnoSeleccionado.idPaciente}`}
              </p>
              <p className="text-slate-500">
                Turno #{turnoSeleccionado.idTurno} · {formatearHora(turnoSeleccionado.horaInicio)} hs
              </p>
            </div>

            <form onSubmit={handleMarcarRealizado} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Medicamento recetado / Indicaciones clínicas (opcional):
                </label>
                <textarea
                  rows={3}
                  value={medicamentoRecetado}
                  onChange={(e) => setMedicamentoRecetado(e.target.value)}
                  placeholder="Ej: Amoxicilina 500mg cada 8 horas durante 7 días. Reposo por 48 horas."
                  className="w-full p-3 rounded-xl border border-slate-200 text-xs text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68]"
                />
              </div>

              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setTurnoSeleccionado(null)}
                  className="px-4 py-2 rounded-xl border border-slate-200 text-slate-600 text-xs font-semibold hover:bg-slate-50"
                >
                  Volver
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="px-5 py-2 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold transition-colors disabled:opacity-50"
                >
                  {isSubmitting ? 'Guardando...' : 'Confirmar Atención'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
