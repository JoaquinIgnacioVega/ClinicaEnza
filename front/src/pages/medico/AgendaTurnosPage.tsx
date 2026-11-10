import React, { useState, useEffect, useRef } from 'react';
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
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  Info,
} from 'lucide-react';

const getLocalTodayString = (): string => {
  const d = new Date();
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

export const AgendaTurnosPage: React.FC = () => {
  const { user } = useAuth();
  const idMedico = user?.idMedico || 3;

  const [fecha, setFecha] = useState<string>(getLocalTodayString);
  const [turnos, setTurnos] = useState<TurnoDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [esModoDemo, setEsModoDemo] = useState(false);

  // Set con las fechas ("YYYY-MM-DD") que poseen turnos registrados
  const [fechasConTurnos, setFechasConTurnos] = useState<Set<string>>(new Set());

  // Estado del desplegable del calendario interactivo
  const [isCalendarOpen, setIsCalendarOpen] = useState(false);
  const calendarContainerRef = useRef<HTMLDivElement>(null);

  // Mes que se está visualizando en el popover
  const [mesVisualizado, setMesVisualizado] = useState<Date>(() => {
    const [y, m] = getLocalTodayString().split('-').map(Number);
    return new Date(y, m - 1, 1);
  });

  // Modal para atender turno
  const [turnoSeleccionado, setTurnoSeleccionado] = useState<TurnoDto | null>(null);
  const [medicamentoRecetado, setMedicamentoRecetado] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Cargar todas las fechas con turnos para colorear el calendario
  const cargarTodasLasFechasConTurnos = async () => {
    try {
      const todos = await medicoService.obtenerTurnosPorMedico(idMedico);
      const setFechas = new Set<string>();
      todos.forEach((t) => {
        if (t.horaInicio) {
          setFechas.add(t.horaInicio.split('T')[0]);
        }
      });
      if (setFechas.size > 0) {
        setFechasConTurnos(setFechas);
      } else {
        // Si la BD no tiene turnos, habilitamos al menos hoy para pruebas
        setFechasConTurnos(new Set([getLocalTodayString()]));
      }
    } catch {
      // Modo demostrativo / fallback sin conexión: fechas de ejemplo
      const hoy = getLocalTodayString();
      const [y, m, d] = hoy.split('-').map(Number);
      const demoFechas = new Set<string>([
        hoy,
        `${y}-${String(m).padStart(2, '0')}-${String(Math.max(1, d - 1)).padStart(2, '0')}`,
        `${y}-${String(m).padStart(2, '0')}-${String(Math.min(28, d + 2)).padStart(2, '0')}`,
        `${y}-${String(m).padStart(2, '0')}-${String(Math.min(28, d + 4)).padStart(2, '0')}`,
      ]);
      setFechasConTurnos(demoFechas);
    }
  };

  const cargarTurnos = async (fechaConsulta = fecha) => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
      const data = await medicoService.obtenerTurnosPorMedico(idMedico, fechaConsulta);
      setTurnos(data);
      setEsModoDemo(false);
      return data.length;
    } catch {
      setEsModoDemo(true);
      const mockTurnos: TurnoDto[] = [
        {
          idTurno: 101,
          horaInicio: `${fechaConsulta}T09:00:00`,
          horaFin: `${fechaConsulta}T09:20:00`,
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
          horaInicio: `${fechaConsulta}T09:40:00`,
          horaFin: `${fechaConsulta}T10:00:00`,
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
      ];
      setTurnos(mockTurnos);
      return mockTurnos.length;
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    cargarTodasLasFechasConTurnos();
  }, [idMedico]);

  useEffect(() => {
    cargarTurnos(fecha);
  }, [fecha, idMedico]);

  // Cerrar el popover del calendario al hacer clic afuera
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        calendarContainerRef.current &&
        !calendarContainerRef.current.contains(event.target as Node)
      ) {
        setIsCalendarOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Botón Refrescar: Devuelve al día actual y refresca la agenda
  const handleRefrescar = async () => {
    const hoy = getLocalTodayString();
    setFecha(hoy);

    // Ajustar mes visualizado en el calendario a hoy
    const [y, m] = hoy.split('-').map(Number);
    setMesVisualizado(new Date(y, m - 1, 1));

    // Refrescar fechas del mes y turnos del día
    await cargarTodasLasFechasConTurnos();
    await cargarTurnos(hoy);
  };

  // Navegar día anterior o siguiente con las flechas
  const navegarDia = (diasOffset: number) => {
    const [y, m, d] = fecha.split('-').map(Number);
    const dateObj = new Date(y, m - 1, d);
    dateObj.setDate(dateObj.getDate() + diasOffset);
    const newY = dateObj.getFullYear();
    const newM = String(dateObj.getMonth() + 1).padStart(2, '0');
    const newD = String(dateObj.getDate()).padStart(2, '0');
    const nuevaFecha = `${newY}-${newM}-${newD}`;
    setFecha(nuevaFecha);

    // Si el nuevo día cae en otro mes, sincronizar el calendario desplegable
    setMesVisualizado(new Date(newY, dateObj.getMonth(), 1));
  };

  const cambiarMesVisualizado = (offsetMeses: number) => {
    setMesVisualizado((prev) => {
      const nuevo = new Date(prev);
      nuevo.setMonth(nuevo.getMonth() + offsetMeses);
      return nuevo;
    });
  };

  const formatearFechaCorta = (fechaIso: string) => {
    try {
      const [y, m, d] = fechaIso.split('-').map(Number);
      const date = new Date(y, m - 1, d);
      return date.toLocaleDateString('es-AR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      });
    } catch {
      return fechaIso;
    }
  };

  const formatearFechaLegible = (fechaIso: string) => {
    try {
      const [y, m, d] = fechaIso.split('-').map(Number);
      const date = new Date(y, m - 1, d);
      return date.toLocaleDateString('es-AR', {
        weekday: 'long',
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      });
    } catch {
      return fechaIso;
    }
  };

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
      await cargarTurnos(fecha);
      await cargarTodasLasFechasConTurnos();
    } catch {
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
      await cargarTurnos(fecha);
      await cargarTodasLasFechasConTurnos();
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

  // Construcción de la matriz de días para el mes visualizado
  const mesActualAño = mesVisualizado.getFullYear();
  const mesActualMes = mesVisualizado.getMonth(); // 0 a 11
  const nombreMesVisualizado = mesVisualizado.toLocaleDateString('es-AR', {
    month: 'long',
    year: 'numeric',
  });

  const primerDiaSemana = new Date(mesActualAño, mesActualMes, 1).getDay(); // 0: Dom
  const totalDiasMes = new Date(mesActualAño, mesActualMes + 1, 0).getDate();

  const slotsCalendario: (number | null)[] = [];
  for (let i = 0; i < primerDiaSemana; i++) {
    slotsCalendario.push(null);
  }
  for (let d = 1; d <= totalDiasMes; d++) {
    slotsCalendario.push(d);
  }

  return (
    <div className="space-y-6 max-w-6xl mx-auto animate-fadeIn">
      {/* Encabezado */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
            Agenda de Turnos del Día
          </h1>
          <p className="text-xs sm:text-sm text-slate-500 mt-1 capitalize">
            {formatearFechaLegible(fecha)}
          </p>
        </div>

        {/* Controles de Navegación: Flecha Izq + Calendario Interactivo + Flecha Der + Refrescar */}
        <div className="flex items-center gap-2">
          {/* Flecha Izquierda: Día anterior */}
          <button
            type="button"
            onClick={() => navegarDia(-1)}
            className="p-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 transition-colors cursor-pointer shadow-2xs hover:border-[#0d5c68]/40"
            title="Día anterior"
          >
            <ChevronLeft className="w-4 h-4" />
          </button>

          {/* Selector de Calendario con Popover */}
          <div className="relative" ref={calendarContainerRef}>
            <button
              type="button"
              onClick={() => setIsCalendarOpen(!isCalendarOpen)}
              className={`flex items-center gap-2.5 bg-white px-3.5 py-2 rounded-xl border transition-all shadow-2xs cursor-pointer ${
                isCalendarOpen
                  ? 'border-[#0d5c68] ring-2 ring-[#0d5c68]/15'
                  : 'border-slate-200/90 hover:border-[#0d5c68]/60'
              }`}
              title="Abrir calendario interactivo"
            >
              <Calendar className="w-4 h-4 text-[#0d5c68] shrink-0" />
              <span className="text-xs font-semibold text-slate-700">
                {formatearFechaCorta(fecha)}
              </span>
              <ChevronDown
                className={`w-3.5 h-3.5 text-slate-400 transition-transform duration-200 ${
                  isCalendarOpen ? 'rotate-180 text-[#0d5c68]' : ''
                }`}
              />
            </button>

            {/* Popover desplegable del Calendario */}
            {isCalendarOpen && (
              <div className="absolute right-0 sm:left-0 mt-2 z-40 bg-white rounded-2xl shadow-xl border border-slate-200 p-4 w-72 sm:w-80 animate-fadeIn">
                {/* Cabecera del Mes */}
                <div className="flex items-center justify-between mb-3 pb-2 border-b border-slate-100">
                  <button
                    type="button"
                    onClick={() => cambiarMesVisualizado(-1)}
                    className="p-1 rounded-lg text-slate-500 hover:text-slate-800 hover:bg-slate-100 transition-colors cursor-pointer"
                    title="Mes anterior"
                  >
                    <ChevronLeft className="w-4 h-4" />
                  </button>

                  <span className="text-xs font-bold text-slate-800 capitalize">
                    {nombreMesVisualizado}
                  </span>

                  <button
                    type="button"
                    onClick={() => cambiarMesVisualizado(1)}
                    className="p-1 rounded-lg text-slate-500 hover:text-slate-800 hover:bg-slate-100 transition-colors cursor-pointer"
                    title="Mes siguiente"
                  >
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </div>

                {/* Días de la semana */}
                <div className="grid grid-cols-7 gap-1 text-center mb-1.5">
                  {['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá'].map((dia) => (
                    <span
                      key={dia}
                      className="text-[11px] font-bold text-slate-400 uppercase py-1"
                    >
                      {dia}
                    </span>
                  ))}
                </div>

                {/* Cuadrícula de Días */}
                <div className="grid grid-cols-7 gap-1">
                  {slotsCalendario.map((d, index) => {
                    if (d === null) {
                      return <div key={`empty-${index}`} className="h-8 w-8" />;
                    }

                    const fechaSlot = `${mesActualAño}-${String(mesActualMes + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
                    const tieneTurnos = fechasConTurnos.has(fechaSlot);
                    const esSeleccionado = fecha === fechaSlot;

                    if (tieneTurnos) {
                      return (
                        <button
                          key={fechaSlot}
                          type="button"
                          onClick={() => {
                            setFecha(fechaSlot);
                            setIsCalendarOpen(false);
                          }}
                          className={`relative h-8 w-8 flex flex-col items-center justify-center rounded-xl text-xs font-bold transition-all cursor-pointer ${
                            esSeleccionado
                              ? 'bg-[#0d5c68] text-white shadow-2xs'
                              : 'text-slate-800 hover:bg-teal-50 hover:text-[#0d5c68] border border-teal-100/60'
                          }`}
                          title={`Turnos disponibles para ${fechaSlot}`}
                        >
                          <span className="leading-none">{d}</span>
                          <span
                            className={`w-1 h-1 rounded-full mt-0.5 ${
                              esSeleccionado ? 'bg-white' : 'bg-[#0d5c68]'
                            }`}
                          />
                        </button>
                      );
                    }

                    // Fecha sin turnos: en Gris y no accesible
                    return (
                      <button
                        key={fechaSlot}
                        type="button"
                        disabled
                        className="h-8 w-8 flex items-center justify-center rounded-xl text-xs text-slate-300 bg-slate-50/50 cursor-not-allowed select-none font-medium"
                        title="Sin turnos asignados"
                      >
                        {d}
                      </button>
                    );
                  })}
                </div>

                {/* Leyenda aclaratoria */}
                <div className="mt-3.5 pt-2.5 border-t border-slate-100 flex items-center justify-between text-[10px] text-slate-500 font-medium">
                  <span className="flex items-center gap-1.5">
                    <span className="w-2 h-2 rounded-full bg-[#0d5c68]"></span>
                    Con turnos (clickeable)
                  </span>
                  <span className="flex items-center gap-1.5 text-slate-400">
                    <span className="w-2 h-2 rounded-full bg-slate-200"></span>
                    Sin turnos (en gris)
                  </span>
                </div>
              </div>
            )}
          </div>

          {/* Flecha Derecha: Día siguiente */}
          <button
            type="button"
            onClick={() => navegarDia(1)}
            className="p-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 transition-colors cursor-pointer shadow-2xs hover:border-[#0d5c68]/40"
            title="Día siguiente"
          >
            <ChevronRight className="w-4 h-4" />
          </button>

          {/* Botón Refrescar: Te devuelve al día actual y refresca los turnos */}
          <button
            type="button"
            onClick={handleRefrescar}
            disabled={isLoading}
            className="flex items-center gap-2 px-3.5 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-semibold transition-colors cursor-pointer disabled:opacity-50 shadow-2xs ml-1 hover:border-[#0d5c68]/40"
            title="Volver a la fecha actual y refrescar turnos"
          >
            <RefreshCw
              className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin text-[#0d5c68]' : 'text-slate-500'}`}
            />
            <span>Refrescar</span>
          </button>
        </div>
      </div>

      {/* Alertas descartables con botón de cierre (X) a la derecha */}
      {errorMessage && (
        <div className="p-3.5 rounded-xl bg-red-50 border border-red-200 text-red-700 text-xs flex items-center justify-between gap-3 animate-fadeIn">
          <div className="flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0 text-red-600" />
            <span>{errorMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => setErrorMessage(null)}
            className="p-1 rounded-lg text-red-500 hover:text-red-800 hover:bg-red-100/70 transition-colors cursor-pointer shrink-0"
            title="Cerrar aviso"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {successMessage && (
        <div className="p-3.5 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-800 text-xs flex items-center justify-between gap-3 animate-fadeIn">
          <div className="flex items-center gap-2">
            <CheckCircle2 className="w-4 h-4 shrink-0 text-emerald-600" />
            <span>{successMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => setSuccessMessage(null)}
            className="p-1 rounded-lg text-emerald-600 hover:text-emerald-900 hover:bg-emerald-100/70 transition-colors cursor-pointer shrink-0"
            title="Cerrar aviso"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {esModoDemo && (
        <div className="p-3 rounded-xl bg-amber-50 border border-amber-200 text-amber-800 text-xs flex items-center justify-between gap-3 animate-fadeIn">
          <div className="flex items-center gap-2">
            <Info className="w-4 h-4 shrink-0 text-amber-600" />
            <span>
              Mostrando datos simulados para la fecha seleccionada mientras la API se encuentra en desarrollo o sin conexión.
            </span>
          </div>
          <button
            type="button"
            onClick={() => setEsModoDemo(false)}
            className="p-1 rounded-lg text-amber-700 hover:text-amber-900 hover:bg-amber-100/70 transition-colors cursor-pointer shrink-0"
            title="Cerrar aviso"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Lista de Turnos */}
      <div className="space-y-3.5">
        {turnos.length === 0 ? (
          <div className="p-12 text-center bg-white rounded-2xl border border-slate-200/80">
            <Calendar className="w-10 h-10 text-slate-300 mx-auto mb-2" />
            <h3 className="text-sm font-bold text-slate-700">No hay turnos registrados</h3>
            <p className="text-xs text-slate-400 mt-0.5">
              No se encontraron turnos asignados para {formatearFechaLegible(fecha)}.
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
                className="text-slate-400 hover:text-slate-600 cursor-pointer"
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
                  className="px-4 py-2 rounded-xl border border-slate-200 text-slate-600 text-xs font-semibold hover:bg-slate-50 cursor-pointer"
                >
                  Volver
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="px-5 py-2 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold transition-colors disabled:opacity-50 cursor-pointer"
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
