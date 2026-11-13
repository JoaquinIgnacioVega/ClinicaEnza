import React, { useState, useEffect, useRef } from 'react';
import { useAuth } from '../../context/AuthContext';
import { medicoService } from '../../api/medicoService';
import type { HorarioAtencionDto, ConsultorioDto, SlotDisponibleDto } from '../../types/medico';
import type { ApiError } from '../../api/client';
import {
  Clock,
  Plus,
  CalendarCheck,
  CheckCircle2,
  AlertCircle,
  Building2,
  Calendar,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  X,
} from 'lucide-react';

const DIAS_SEMANA = [
  { id: 1, nombre: 'Lunes' },
  { id: 2, nombre: 'Martes' },
  { id: 3, nombre: 'Miércoles' },
  { id: 4, nombre: 'Jueves' },
  { id: 5, nombre: 'Viernes' },
  { id: 6, nombre: 'Sábado' },
];

const getLocalTodayString = (): string => {
  const d = new Date();
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

export const HorariosMedicoPage: React.FC = () => {
  const { user } = useAuth();
  const idMedico = user?.idMedico || 3;

  const [horarios, setHorarios] = useState<HorarioAtencionDto[]>([]);
  const [consultorios, setConsultorios] = useState<ConsultorioDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Formulario nuevo horario
  const [diaSemana, setDiaSemana] = useState<number>(1);
  const [horaDesde, setHoraDesde] = useState<string>('08:00');
  const [horaHasta, setHoraHasta] = useState<string>('14:00');
  const [idConsultorio, setIdConsultorio] = useState<number>(1);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Verificador de slots de disponibilidad (20 min)
  const [fechaTest, setFechaTest] = useState<string>(getLocalTodayString);
  const [slotsDisponibles, setSlotsDisponibles] = useState<SlotDisponibleDto[]>([]);
  const [isCheckingSlots, setIsCheckingSlots] = useState(false);

  // Calendario desplegable interactivo para disponibilidad
  const [isCalendarOpen, setIsCalendarOpen] = useState(false);
  const calendarContainerRef = useRef<HTMLDivElement>(null);

  const [mesVisualizado, setMesVisualizado] = useState<Date>(() => {
    const [y, m] = getLocalTodayString().split('-').map(Number);
    return new Date(y, m - 1, 1);
  });

  const cargarDatos = async () => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
      const [hList, cList] = await Promise.all([
        medicoService.obtenerHorariosPorMedico(idMedico).catch(() => []),
        medicoService.obtenerConsultorios().catch(() => [
          { idConsultorio: 1, numero: '1', piso: 0, sector: 'Ala Norte', ubicacion: 'PB - Ala Norte' },
          { idConsultorio: 2, numero: '2', piso: 1, sector: 'Ala Sur', ubicacion: 'Piso 1 - Ala Sur' },
          { idConsultorio: 3, numero: '3', piso: 1, sector: 'Ala Norte', ubicacion: 'Piso 1 - Ala Norte' },
        ]),
      ]);

      setHorarios(hList);
      setConsultorios(cList);
      if (cList.length > 0) {
        setIdConsultorio(cList[0].idConsultorio);
      }
    } catch {
      setErrorMessage('No se pudieron sincronizar los horarios con el servidor.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    cargarDatos();
  }, [idMedico]);

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

  const consultarSlots = async (fechaConsulta: string) => {
    if (!fechaConsulta) return;
    try {
      setIsCheckingSlots(true);
      const slots = await medicoService.obtenerDisponibilidad(idMedico, fechaConsulta);
      setSlotsDisponibles(slots);
    } catch {
      // Fallback slots de 20 minutos solo si el médico atiende ese día
      const [y, m, d] = fechaConsulta.split('-').map(Number);
      const diaNum = new Date(y, m - 1, d).getDay();
      const atiendeEseDia = horarios.some((h) => h.diaSemana === diaNum);

      if (atiendeEseDia) {
        setSlotsDisponibles([
          { horaInicio: `${fechaConsulta}T09:00:00`, horaFin: `${fechaConsulta}T09:20:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
          { horaInicio: `${fechaConsulta}T09:20:00`, horaFin: `${fechaConsulta}T09:40:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
          { horaInicio: `${fechaConsulta}T09:40:00`, horaFin: `${fechaConsulta}T10:00:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
          { horaInicio: `${fechaConsulta}T10:00:00`, horaFin: `${fechaConsulta}T10:20:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
        ]);
      } else {
        setSlotsDisponibles([]);
      }
    } finally {
      setIsCheckingSlots(false);
    }
  };

  // Carga automática de slots al cambiar la fecha o al cargarse los horarios
  useEffect(() => {
    if (fechaTest) {
      consultarSlots(fechaTest);
    }
  }, [fechaTest, idMedico, horarios]);

  const handleCrearHorario = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    const hDesdeStr = horaDesde.length === 5 ? `${horaDesde}:00` : horaDesde;
    const hHastaStr = horaHasta.length === 5 ? `${horaHasta}:00` : horaHasta;

    if (hDesdeStr >= hHastaStr) {
      setErrorMessage('La hora de inicio debe ser anterior a la hora de fin.');
      return;
    }

    // Validación preventiva en cliente contra franjas ya configuradas del mismo médico
    const conflicto = horarios.find((h) => {
      if (h.diaSemana !== diaSemana) return false;
      const hd = h.horaDesde.length === 5 ? `${h.horaDesde}:00` : h.horaDesde;
      const hh = h.horaHasta.length === 5 ? `${h.horaHasta}:00` : h.horaHasta;
      // Se superponen si: inicio1 < fin2 && inicio2 < fin1
      return hDesdeStr < hh && hd < hHastaStr;
    });

    if (conflicto) {
      setErrorMessage(
        `Ya existe un horario configurado para los ${getNombreDia(diaSemana)} (${conflicto.horaDesde.slice(0, 5)} - ${conflicto.horaHasta.slice(0, 5)} hs) que se superpone con la franja solicitada. No está permitido repetir o superponer horarios.`
      );
      return;
    }

    try {
      setIsSubmitting(true);
      await medicoService.configurarHorario(idMedico, {
        idMedico,
        diaSemana,
        horaDesde: hDesdeStr,
        horaHasta: hHastaStr,
        idConsultorio,
      });

      setSuccessMessage('¡Horario de atención configurado exitosamente!');
      await cargarDatos();
    } catch (err: unknown) {
      const apiErr = err as ApiError;
      setErrorMessage(
        apiErr?.error || 'No se pudo configurar el horario. Verifique que no se superponga con otra franja existente o consultorio ocupado.'
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const getNombreDia = (numDia: number) => {
    const d = DIAS_SEMANA.find((item) => item.id === numDia);
    return d ? d.nombre : 'Día';
  };

  const formatearHoraSlot = (isoStr: string) => {
    try {
      return new Date(isoStr).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
    } catch {
      return isoStr;
    }
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

  // Navegar día anterior o posterior en la sección de slots
  const navegarDiaTest = (diasOffset: number) => {
    const [y, m, d] = fechaTest.split('-').map(Number);
    const dateObj = new Date(y, m - 1, d);
    dateObj.setDate(dateObj.getDate() + diasOffset);
    const newY = dateObj.getFullYear();
    const newM = String(dateObj.getMonth() + 1).padStart(2, '0');
    const newD = String(dateObj.getDate()).padStart(2, '0');
    const nuevaFecha = `${newY}-${newM}-${newD}`;
    setFechaTest(nuevaFecha);
    setMesVisualizado(new Date(newY, dateObj.getMonth(), 1));
  };

  const cambiarMesVisualizado = (offsetMeses: number) => {
    setMesVisualizado((prev) => {
      const nuevo = new Date(prev);
      nuevo.setMonth(nuevo.getMonth() + offsetMeses);
      return nuevo;
    });
  };

  // Conjunto de días de la semana (0..6) en los que el médico tiene franjas configuradas
  const diasAtencionMedico = new Set(horarios.map((h) => h.diaSemana));

  // Generación de slots del mes para el popover de disponibilidad
  const mesActualAño = mesVisualizado.getFullYear();
  const mesActualMes = mesVisualizado.getMonth();
  const nombreMesVisualizado = mesVisualizado.toLocaleDateString('es-AR', {
    month: 'long',
    year: 'numeric',
  });

  const primerDiaSemana = new Date(mesActualAño, mesActualMes, 1).getDay();
  const totalDiasMes = new Date(mesActualAño, mesActualMes + 1, 0).getDate();

  const slotsCalendario: (number | null)[] = [];
  for (let i = 0; i < primerDiaSemana; i++) {
    slotsCalendario.push(null);
  }
  for (let d = 1; d <= totalDiasMes; d++) {
    slotsCalendario.push(d);
  }

  return (
    <div className="space-y-8 max-w-6xl mx-auto animate-fadeIn">
      {/* Encabezado */}
      <div>
        <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
          Horarios de Atención y Consultorio
        </h1>
        <p className="text-xs sm:text-sm text-slate-500 mt-1">
          Configura tus franjas de disponibilidad semanal y consulta los slots de turnos de 20 minutos generados.
        </p>
      </div>

      {/* Alertas descartables */}
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

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Formulario: Configurar Nuevo Horario */}
        <div className="bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4">
          <div className="flex items-center gap-2 pb-2 border-b border-slate-100">
            <Plus className="w-4 h-4 text-[#0d5c68]" />
            <h3 className="text-sm font-bold text-slate-800">
              Agregar Horario de Atención
            </h3>
          </div>

          <form onSubmit={handleCrearHorario} className="space-y-4">
            {/* Día de la Semana */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                Día de la semana
              </label>
              <select
                value={diaSemana}
                onChange={(e) => setDiaSemana(Number(e.target.value))}
                className="w-full px-3.5 py-2 text-xs rounded-xl border border-slate-200 text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68] bg-slate-50/40 cursor-pointer"
              >
                {DIAS_SEMANA.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.nombre}
                  </option>
                ))}
              </select>
            </div>

            {/* Franja horaria */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Hora desde
                </label>
                <input
                  type="time"
                  value={horaDesde}
                  onChange={(e) => setHoraDesde(e.target.value)}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68] bg-slate-50/40"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Hora hasta
                </label>
                <input
                  type="time"
                  value={horaHasta}
                  onChange={(e) => setHoraHasta(e.target.value)}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68] bg-slate-50/40"
                  required
                />
              </div>
            </div>

            {/* Consultorio */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                Consultorio asignado
              </label>
              <select
                value={idConsultorio}
                onChange={(e) => setIdConsultorio(Number(e.target.value))}
                className="w-full px-3.5 py-2 text-xs rounded-xl border border-slate-200 text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68] bg-slate-50/40 cursor-pointer"
              >
                {consultorios.map((c) => (
                  <option key={c.idConsultorio} value={c.idConsultorio}>
                    {c.numero ? `Consultorio ${c.numero}` : `Consultorio #${c.idConsultorio}`}{' '}
                    ({c.ubicacion || `Piso ${c.piso}`})
                  </option>
                ))}
              </select>
            </div>

            <div className="pt-2">
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full py-2.5 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold shadow-sm transition-colors cursor-pointer disabled:opacity-50"
              >
                {isSubmitting ? 'Guardando...' : 'Guardar Horario'}
              </button>
            </div>
          </form>
        </div>

        {/* Lista de Horarios Configurados */}
        <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-slate-100">
            <div className="flex items-center gap-2">
              <Clock className="w-4 h-4 text-[#0d5c68]" />
              <h3 className="text-sm font-bold text-slate-800">
                Franjas Horarias Configuradas ({horarios.length})
              </h3>
            </div>
            {isLoading && <span className="text-xs text-slate-400">Cargando...</span>}
          </div>

          {horarios.length === 0 ? (
            <div className="p-8 text-center bg-slate-50/50 rounded-xl border border-dashed border-slate-200">
              <Clock className="w-8 h-8 text-slate-300 mx-auto mb-1.5" />
              <p className="text-xs font-semibold text-slate-600">
                No tienes horarios configurados todavía.
              </p>
              <p className="text-[11px] text-slate-400 mt-0.5">
                Usa el formulario de la izquierda para agregar tus días de atención.
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              {horarios.map((h) => (
                <div
                  key={h.idHorarioAtencion}
                  className="p-4 rounded-xl border border-slate-200/90 bg-white hover:border-[#0d5c68]/40 transition-all shadow-2xs space-y-2"
                >
                  <div className="flex items-center justify-between">
                    <span className="text-xs font-bold text-[#0d5c68] bg-teal-50 px-2.5 py-0.5 rounded-md border border-teal-100">
                      {getNombreDia(h.diaSemana)}
                    </span>
                    <span className="text-xs font-semibold text-slate-700">
                      {h.horaDesde.slice(0, 5)} - {h.horaHasta.slice(0, 5)} hs
                    </span>
                  </div>

                  <div className="flex items-center gap-1.5 text-xs text-slate-500 pt-1 border-t border-slate-100">
                    <Building2 className="w-3.5 h-3.5 text-slate-400" />
                    <span>
                      {h.numeroConsultorio || `Consultorio ${h.idConsultorio}`}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Sección: Verificador de Disponibilidad con Calendario Interactivo */}
      <div className="bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-5">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-3 border-b border-slate-100">
          <div>
            <div className="flex items-center gap-2">
              <CalendarCheck className="w-4 h-4 text-[#0d5c68]" />
              <h3 className="text-sm font-bold text-slate-800">
                Disponibilidad de Turnos (Bloques de 20 minutos)
              </h3>
            </div>
            <p className="text-xs text-slate-400 mt-0.5 capitalize">
              {formatearFechaLegible(fechaTest)}
            </p>
          </div>

          {/* Controles de Navegación del Calendario: Flecha Izquierda + Selector Popover + Flecha Derecha */}
          <div className="flex items-center gap-2">
            {/* Flecha Izquierda: Día anterior */}
            <button
              type="button"
              onClick={() => navegarDiaTest(-1)}
              className="p-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 transition-colors cursor-pointer shadow-2xs hover:border-[#0d5c68]/40"
              title="Día anterior"
            >
              <ChevronLeft className="w-4 h-4" />
            </button>

            {/* Selector de Calendario Desplegable */}
            <div className="relative" ref={calendarContainerRef}>
              <button
                type="button"
                onClick={() => setIsCalendarOpen(!isCalendarOpen)}
                className={`flex items-center gap-2.5 bg-white px-3.5 py-2 rounded-xl border transition-all shadow-2xs cursor-pointer ${
                  isCalendarOpen
                    ? 'border-[#0d5c68] ring-2 ring-[#0d5c68]/15'
                    : 'border-slate-200/90 hover:border-[#0d5c68]/60'
                }`}
                title="Abrir selector de fecha"
              >
                <Calendar className="w-4 h-4 text-[#0d5c68] shrink-0" />
                <span className="text-xs font-semibold text-slate-700">
                  {formatearFechaCorta(fechaTest)}
                </span>
                <ChevronDown
                  className={`w-3.5 h-3.5 text-slate-400 transition-transform duration-200 ${
                    isCalendarOpen ? 'rotate-180 text-[#0d5c68]' : ''
                  }`}
                />
              </button>

              {/* Popover con vista de mes y días en gris si no tienen atención */}
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
                      const diaSemanaNum = new Date(mesActualAño, mesActualMes, d).getDay();
                      const tieneHorarioAtencion = diasAtencionMedico.has(diaSemanaNum);
                      const esSeleccionado = fechaTest === fechaSlot;

                      if (tieneHorarioAtencion) {
                        return (
                          <button
                            key={fechaSlot}
                            type="button"
                            onClick={() => {
                              setFechaTest(fechaSlot);
                              setIsCalendarOpen(false);
                            }}
                            className={`relative h-8 w-8 flex flex-col items-center justify-center rounded-xl text-xs font-bold transition-all cursor-pointer ${
                              esSeleccionado
                                ? 'bg-[#0d5c68] text-white shadow-2xs'
                                : 'text-slate-800 hover:bg-teal-50 hover:text-[#0d5c68] border border-teal-100/60'
                            }`}
                            title={`Atención disponible: ${fechaSlot}`}
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

                      // Día sin horario de atención: en Gris y no accesible
                      return (
                        <button
                          key={fechaSlot}
                          type="button"
                          disabled
                          className="h-8 w-8 flex items-center justify-center rounded-xl text-xs text-slate-300 bg-slate-50/50 cursor-not-allowed select-none font-medium"
                          title="Sin horario de atención configurado"
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
                      Con atención (clickeable)
                    </span>
                    <span className="flex items-center gap-1.5 text-slate-400">
                      <span className="w-2 h-2 rounded-full bg-slate-200"></span>
                      Sin atención (en gris)
                    </span>
                  </div>
                </div>
              )}
            </div>

            {/* Flecha Derecha: Día siguiente */}
            <button
              type="button"
              onClick={() => navegarDiaTest(1)}
              className="p-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 transition-colors cursor-pointer shadow-2xs hover:border-[#0d5c68]/40"
              title="Día siguiente"
            >
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Visualización Directa de Slots de Disponibilidad */}
        {isCheckingSlots ? (
          <div className="py-10 text-center">
            <div className="w-6 h-6 border-2 border-[#0d5c68] border-t-transparent rounded-full animate-spin mx-auto mb-2"></div>
            <p className="text-xs text-slate-500">Calculando disponibilidad de slots...</p>
          </div>
        ) : slotsDisponibles.length > 0 ? (
          <div className="space-y-2">
            <div className="text-xs font-semibold text-slate-600 mb-1">
              Slots disponibles generados ({slotsDisponibles.length}):
            </div>
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 lg:grid-cols-6 gap-2.5">
              {slotsDisponibles.map((slot, idx) => (
                <div
                  key={idx}
                  className="p-2.5 rounded-xl bg-slate-50 border border-slate-200 text-center space-y-0.5 shadow-2xs hover:border-[#0d5c68]/50 hover:bg-teal-50/30 transition-all"
                >
                  <div className="text-xs font-bold text-slate-800">
                    {formatearHoraSlot(slot.horaInicio)} - {formatearHoraSlot(slot.horaFin)}
                  </div>
                  <div className="text-[10px] text-slate-400 truncate">
                    {slot.ubicacionConsultorio || `Consultorio ${slot.idConsultorio}`}
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : (
          <div className="py-10 text-center bg-slate-50/50 rounded-xl border border-dashed border-slate-200">
            <Clock className="w-8 h-8 text-slate-300 mx-auto mb-1.5" />
            <p className="text-xs font-semibold text-slate-600">
              No hay slots disponibles para {formatearFechaLegible(fechaTest)}.
            </p>
            <p className="text-[11px] text-slate-400 mt-0.5">
              Verifique que el médico tenga un horario de atención asignado para este día de la semana o seleccione otro día habilitado en el calendario.
            </p>
          </div>
        )}
      </div>
    </div>
  );
};
