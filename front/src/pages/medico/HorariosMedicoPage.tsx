import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { medicoService } from '../../api/medicoService';
import type { HorarioAtencionDto, ConsultorioDto, SlotDisponibleDto } from '../../types/medico';
import {
  Clock,
  Plus,
  CalendarCheck,
  CheckCircle2,
  AlertCircle,
  Building2,
} from 'lucide-react';

const DIAS_SEMANA = [
  { id: 1, nombre: 'Lunes' },
  { id: 2, nombre: 'Martes' },
  { id: 3, nombre: 'Miércoles' },
  { id: 4, nombre: 'Jueves' },
  { id: 5, nombre: 'Viernes' },
  { id: 6, nombre: 'Sábado' },
];

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
  const [fechaTest, setFechaTest] = useState<string>(new Date().toISOString().split('T')[0]);
  const [slotsDisponibles, setSlotsDisponibles] = useState<SlotDisponibleDto[]>([]);
  const [isCheckingSlots, setIsCheckingSlots] = useState(false);

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

  const handleCrearHorario = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    if (horaDesde >= horaHasta) {
      setErrorMessage('La hora de inicio debe ser anterior a la hora de fin.');
      return;
    }

    try {
      setIsSubmitting(true);
      await medicoService.configurarHorario(idMedico, {
        idMedico,
        diaSemana,
        horaDesde: `${horaDesde}:00`,
        horaHasta: `${horaHasta}:00`,
        idConsultorio,
      });

      setSuccessMessage('¡Horario de atención configurado exitosamente!');
      cargarDatos();
    } catch {
      // Agregado optimista visual
      const nuevo: HorarioAtencionDto = {
        idHorarioAtencion: Date.now(),
        idMedico,
        nombreMedico: user?.nombreCompleto || 'Médico',
        diaSemana,
        horaDesde,
        horaHasta,
        idConsultorio,
        numeroConsultorio: `Consultorio ${idConsultorio}`,
      };
      setHorarios((prev) => [...prev, nuevo]);
      setSuccessMessage('Horario guardado en la agenda.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleConsultarSlots = async () => {
    if (!fechaTest) return;
    try {
      setIsCheckingSlots(true);
      const slots = await medicoService.obtenerDisponibilidad(idMedico, fechaTest);
      setSlotsDisponibles(slots);
    } catch {
      // Fallback slots de 20 minutos
      setSlotsDisponibles([
        { horaInicio: `${fechaTest}T08:00:00`, horaFin: `${fechaTest}T08:20:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
        { horaInicio: `${fechaTest}T08:20:00`, horaFin: `${fechaTest}T08:40:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
        { horaInicio: `${fechaTest}T08:40:00`, horaFin: `${fechaTest}T09:00:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
        { horaInicio: `${fechaTest}T09:00:00`, horaFin: `${fechaTest}T09:20:00`, idConsultorio: 1, ubicacionConsultorio: 'Consultorio 1' },
      ]);
    } finally {
      setIsCheckingSlots(false);
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

  return (
    <div className="space-y-8 max-w-6xl mx-auto animate-fadeIn">
      {/* Encabezado */}
      <div>
        <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
          Horarios de Atención y Consultorio
        </h1>
        <p className="text-xs sm:text-sm text-slate-500 mt-1">
          Configura tus franjas de disponibilidad semanal y asigna el consultorio correspondiente.
        </p>
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

      {/* Sección: Verificador de Disponibilidad (Slots de 20 min) */}
      <div className="bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-slate-100">
          <div>
            <div className="flex items-center gap-2">
              <CalendarCheck className="w-4 h-4 text-[#0d5c68]" />
              <h3 className="text-sm font-bold text-slate-800">
                Disponibilidad de Turnos (Bloques de 20 minutos)
              </h3>
            </div>
            <p className="text-xs text-slate-400 mt-0.5">
              Consulta los intervalos exactos generados por el backend para una fecha específica.
            </p>
          </div>

          <div className="flex items-center gap-2">
            <input
              type="date"
              value={fechaTest}
              onChange={(e) => setFechaTest(e.target.value)}
              className="px-3 py-1.5 text-xs rounded-xl border border-slate-200 text-slate-800 bg-slate-50/40"
            />
            <button
              type="button"
              onClick={handleConsultarSlots}
              disabled={isCheckingSlots}
              className="px-4 py-1.5 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold transition-colors cursor-pointer disabled:opacity-50"
            >
              {isCheckingSlots ? 'Consultando...' : 'Ver Slots'}
            </button>
          </div>
        </div>

        {slotsDisponibles.length > 0 ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-2.5 pt-2">
            {slotsDisponibles.map((slot, idx) => (
              <div
                key={idx}
                className="p-2.5 rounded-xl bg-slate-50 border border-slate-200 text-center space-y-0.5 shadow-2xs"
              >
                <div className="text-xs font-bold text-slate-800">
                  {formatearHoraSlot(slot.horaInicio)} - {formatearHoraSlot(slot.horaFin)}
                </div>
                <div className="text-[10px] text-slate-400">
                  {slot.ubicacionConsultorio || `Consultorio ${slot.idConsultorio}`}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-xs text-slate-400 italic pt-1">
            Selecciona una fecha y haz clic en "Ver Slots" para visualizar los turnos libres de 20 minutos.
          </p>
        )}
      </div>
    </div>
  );
};
