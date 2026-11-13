import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Ban,
  Building2,
  CalendarPlus,
  Clock3,
  Eye,
  Loader2,
  Pencil,
  Plus,
  RefreshCw,
  Search,
  Stethoscope,
  Users,
  X,
} from 'lucide-react';
import type { ApiError } from '../../api/client';
import { medicoService } from '../../api/medicoService';
import {
  recepcionistaService,
  type CrearPacienteRequest,
  type ModificarPacienteRequest,
} from '../../api/recepcionistaService';
import type {
  ConsultorioDto,
  EspecialidadItem,
  HorarioAtencionDto,
  MedicoDto,
  PacienteDto,
  SlotDisponibleDto,
  TurnoDto,
} from '../../types/medico';

const hoy = () => new Date().toISOString().slice(0, 10);
const fecha = (valor: string) => new Date(valor).toLocaleDateString('es-AR');
const hora = (valor: string) => new Date(valor).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
const soloFecha = (valor: string) => valor.slice(0, 10);
const dias = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

const mensajeError = (error: unknown) => {
  if (error && typeof error === 'object' && 'error' in error) return String((error as ApiError).error);
  return 'No se pudo completar la operación.';
};

const estadoClase = (valor: string) => {
  const normalizado = valor.toLowerCase();
  if (normalizado.includes('cancel')) return 'bg-red-100 text-red-700';
  if (normalizado.includes('real') || normalizado.includes('atend')) return 'bg-emerald-100 text-emerald-700';
  return 'bg-sky-100 text-sky-700';
};

const turnoEditable = (turno: TurnoDto) => {
  const valor = turno.estadoDescripcion.toLowerCase();
  return !valor.includes('cancel') && !valor.includes('real') && !valor.includes('atend');
};

const Titulo: React.FC<{ titulo: string; detalle: string; accion?: React.ReactNode }> = ({ titulo, detalle, accion }) => (
  <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
    <div>
      <h1 className="text-3xl font-bold tracking-tight text-slate-900">{titulo}</h1>
      <p className="mt-1 text-sm text-slate-500">{detalle}</p>
    </div>
    {accion}
  </div>
);

const Filtro: React.FC<{ value: string; onChange: (value: string) => void; placeholder: string }> = ({ value, onChange, placeholder }) => (
  <div className="relative w-full max-w-md">
    <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
    <input
      value={value}
      onChange={(event) => onChange(event.target.value)}
      placeholder={placeholder}
      className="w-full rounded-xl border border-slate-200 bg-white py-3 pl-10 pr-4 text-sm outline-none focus:border-[#55bdc7] focus:ring-2 focus:ring-[#55bdc7]/20"
    />
  </div>
);

const Boton: React.FC<React.ButtonHTMLAttributes<HTMLButtonElement> & { secundario?: boolean }> = ({ secundario, className = '', children, ...props }) => (
  <button
    {...props}
    className={`inline-flex items-center justify-center gap-2 rounded-xl px-4 py-2.5 text-xs font-bold transition disabled:cursor-not-allowed disabled:opacity-50 ${secundario ? 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50' : 'bg-[#0d5c68] text-white hover:bg-[#094b55]'} ${className}`}
  >
    {children}
  </button>
);

const Campo: React.FC<React.InputHTMLAttributes<HTMLInputElement> & { etiqueta: string }> = ({ etiqueta, className = '', ...props }) => (
  <label className="text-xs font-medium text-slate-600">
    {etiqueta}
    <input {...props} className={`mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none focus:border-[#55bdc7] ${className}`} />
  </label>
);

const Modal: React.FC<{ titulo: string; onClose: () => void; children: React.ReactNode; amplio?: boolean }> = ({ titulo, onClose, children, amplio }) => (
  <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/45 p-4" role="dialog" aria-modal="true">
    <div className={`max-h-[92vh] w-full overflow-y-auto rounded-2xl bg-white shadow-2xl ${amplio ? 'max-w-4xl' : 'max-w-2xl'}`}>
      <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-100 bg-white px-6 py-4">
        <h2 className="text-lg font-bold text-slate-900">{titulo}</h2>
        <button onClick={onClose} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100" aria-label="Cerrar"><X className="h-5 w-5" /></button>
      </div>
      <div className="p-6">{children}</div>
    </div>
  </div>
);

const Aviso: React.FC<{ texto: string; error?: boolean }> = ({ texto, error }) => (
  <p className={`rounded-xl px-4 py-3 text-xs font-medium ${error ? 'bg-red-50 text-red-700' : 'bg-emerald-50 text-emerald-700'}`}>{texto}</p>
);

interface EditorTurno {
  modo: 'crear' | 'reagendar';
  turno?: TurnoDto;
  idPaciente: string;
  idMedico: string;
  fecha: string;
  horaInicio: string;
}

export const RecepcionistaTurnosPage: React.FC = () => {
  const [turnos, setTurnos] = useState<TurnoDto[]>([]);
  const [pacientes, setPacientes] = useState<PacienteDto[]>([]);
  const [medicos, setMedicos] = useState<MedicoDto[]>([]);
  const [slots, setSlots] = useState<SlotDisponibleDto[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [fechaSeleccionada, setFechaSeleccionada] = useState('');
  const [estadoSeleccionado, setEstadoSeleccionado] = useState('');
  const [pacienteSeleccionado, setPacienteSeleccionado] = useState('');
  const [medicoSeleccionado, setMedicoSeleccionado] = useState('');
  const [editor, setEditor] = useState<EditorTurno | null>(null);
  const [detalle, setDetalle] = useState<TurnoDto | null>(null);
  const [cargando, setCargando] = useState(true);
  const [guardando, setGuardando] = useState(false);
  const [cargandoSlots, setCargandoSlots] = useState(false);
  const [aviso, setAviso] = useState<{ texto: string; error?: boolean } | null>(null);

  const cargar = useCallback(async () => {
    setCargando(true);
    try {
      const [agenda, listaPacientes, listaMedicos] = await Promise.all([
        recepcionistaService.obtenerTurnos({
          fecha: fechaSeleccionada || undefined,
          idPaciente: pacienteSeleccionado ? Number(pacienteSeleccionado) : undefined,
          idMedico: medicoSeleccionado ? Number(medicoSeleccionado) : undefined,
          estado: estadoSeleccionado ? Number(estadoSeleccionado) : undefined,
        }),
        recepcionistaService.obtenerPacientes(),
        medicoService.obtenerMedicos(),
      ]);
      setTurnos(agenda);
      setPacientes(listaPacientes);
      setMedicos(listaMedicos);
    } catch (error) {
      setAviso({ texto: mensajeError(error), error: true });
    } finally {
      setCargando(false);
    }
  }, [fechaSeleccionada, pacienteSeleccionado, medicoSeleccionado, estadoSeleccionado]);

  useEffect(() => { void cargar(); }, [cargar]);

  useEffect(() => {
    if (!editor?.idMedico || !editor.fecha) {
      setSlots([]);
      return;
    }
    setCargandoSlots(true);
    medicoService.obtenerDisponibilidad(Number(editor.idMedico), editor.fecha)
      .then(setSlots)
      .catch((error) => {
        setSlots([]);
        setAviso({ texto: mensajeError(error), error: true });
      })
      .finally(() => setCargandoSlots(false));
  }, [editor?.idMedico, editor?.fecha]);

  const filtrados = useMemo(() => turnos.filter((turno) => {
    const coincideTexto = `${turno.nombrePaciente} ${turno.dniPaciente} ${turno.nombreMedico} ${turno.numeroConsultorio}`.toLowerCase().includes(busqueda.toLowerCase());
    return coincideTexto;
  }), [turnos, busqueda]);

  const abrirDetalle = async (idTurno: number) => {
    setAviso(null);
    try {
      setDetalle(await recepcionistaService.obtenerTurnoPorId(idTurno));
    } catch (error) {
      setAviso({ texto: mensajeError(error), error: true });
    }
  };

  const abrirCreacion = () => {
    setAviso(null);
    setEditor({ modo: 'crear', idPaciente: '', idMedico: '', fecha: hoy(), horaInicio: '' });
  };

  const abrirReagenda = (turno: TurnoDto) => {
    setAviso(null);
    setEditor({ modo: 'reagendar', turno, idPaciente: String(turno.idPaciente), idMedico: String(turno.idMedico), fecha: soloFecha(turno.horaInicio), horaInicio: '' });
  };

  const guardarTurno = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!editor?.horaInicio) return;
    setGuardando(true);
    setAviso(null);
    try {
      if (editor.modo === 'crear') {
        await recepcionistaService.crearTurno({ idPaciente: Number(editor.idPaciente), idMedico: Number(editor.idMedico), horaInicio: editor.horaInicio });
        setAviso({ texto: 'Turno registrado correctamente.' });
      } else if (editor.turno) {
        await recepcionistaService.reagendarTurno(editor.turno.idTurno, editor.horaInicio);
        setAviso({ texto: 'Turno reprogramado correctamente.' });
      }
      setEditor(null);
      await cargar();
    } catch (error) {
      setAviso({ texto: mensajeError(error), error: true });
    } finally {
      setGuardando(false);
    }
  };

  const cancelar = async (turno: TurnoDto) => {
    if (!window.confirm(`¿Cancelar el turno de ${turno.nombrePaciente || 'este paciente'}?`)) return;
    setAviso(null);
    try {
      await recepcionistaService.cancelarTurno(turno.idTurno);
      setAviso({ texto: 'Turno cancelado correctamente.' });
      await cargar();
    } catch (error) {
      setAviso({ texto: mensajeError(error), error: true });
    }
  };

  return (
    <div className="space-y-7">
      <Titulo titulo="Turnos" detalle="Creá, reprogramá y cancelá turnos desde la agenda general." accion={<Boton onClick={abrirCreacion}><CalendarPlus className="h-4 w-4" />Nuevo turno</Boton>} />
      {aviso && <Aviso {...aviso} />}
      <div className="grid grid-cols-1 gap-3 border-y border-slate-200 py-5 md:grid-cols-2 xl:grid-cols-5">
        <Campo etiqueta="Fecha" type="date" value={fechaSeleccionada} onChange={(event) => setFechaSeleccionada(event.target.value)} />
        <label className="text-xs font-medium text-slate-600">Paciente<select value={pacienteSeleccionado} onChange={(event) => setPacienteSeleccionado(event.target.value)} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm"><option value="">Todos</option>{pacientes.map((paciente) => <option key={paciente.idPaciente} value={paciente.idPaciente}>{paciente.nombreCompleto}</option>)}</select></label>
        <label className="text-xs font-medium text-slate-600">Médico<select value={medicoSeleccionado} onChange={(event) => setMedicoSeleccionado(event.target.value)} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm"><option value="">Todos</option>{medicos.map((medico) => <option key={medico.idMedico} value={medico.idMedico}>{medico.nombreCompleto}</option>)}</select></label>
        <label className="text-xs font-medium text-slate-600">Estado<select value={estadoSeleccionado} onChange={(event) => setEstadoSeleccionado(event.target.value)} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm"><option value="">Todos</option><option value="0">Pendiente</option><option value="1">Realizado</option><option value="2">Cancelado</option></select></label>
        <div className="xl:pt-5"><Filtro value={busqueda} onChange={setBusqueda} placeholder="Búsqueda rápida" /></div>
      </div>
      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
        <div className="flex items-center justify-between p-5 text-xs text-slate-500"><span>{filtrados.length} turnos encontrados</span><button onClick={() => void cargar()} className="flex items-center gap-1 font-semibold text-[#0d5c68]"><RefreshCw className="h-3.5 w-3.5" />Actualizar</button></div>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 text-slate-500"><tr><th className="px-5 py-3">Fecha y hora</th><th className="px-5 py-3">Paciente</th><th className="px-5 py-3">Profesional</th><th className="px-5 py-3">Consultorio</th><th className="px-5 py-3">Estado</th><th className="px-5 py-3 text-right">Acciones</th></tr></thead>
            <tbody>
              {cargando ? <tr><td colSpan={6} className="p-10 text-center text-slate-400"><Loader2 className="mx-auto h-5 w-5 animate-spin" /></td></tr> : filtrados.map((turno) => (
                <tr key={turno.idTurno} className="border-t border-slate-100">
                  <td className="px-5 py-4"><strong>{hora(turno.horaInicio)}</strong><br /><span className="text-slate-400">{fecha(turno.horaInicio)}</span></td>
                  <td className="px-5 py-4">{turno.nombrePaciente || 'Paciente'}<br /><span className="text-slate-400">DNI {turno.dniPaciente || '—'}</span></td>
                  <td className="px-5 py-4">{turno.nombreMedico || 'Médico'}</td>
                  <td className="px-5 py-4">{turno.numeroConsultorio || 'Sin asignar'}</td>
                  <td className="px-5 py-4"><span className={`rounded-full px-2.5 py-1 text-[10px] font-semibold ${estadoClase(turno.estadoDescripcion)}`}>{turno.estadoDescripcion}</span></td>
                  <td className="px-5 py-4">
                    <div className="flex justify-end gap-2">
                      <button onClick={() => void abrirDetalle(turno.idTurno)} className="rounded-lg border border-cyan-200 p-2 text-[#0d5c68] hover:bg-cyan-50" title="Ver detalle"><Eye className="h-4 w-4" /></button>
                      {turnoEditable(turno) && <><button onClick={() => abrirReagenda(turno)} className="rounded-lg border border-slate-200 p-2 text-slate-600 hover:bg-slate-50" title="Reprogramar"><Clock3 className="h-4 w-4" /></button><button onClick={() => void cancelar(turno)} className="rounded-lg border border-red-200 p-2 text-red-600 hover:bg-red-50" title="Cancelar"><Ban className="h-4 w-4" /></button></>}
                    </div>
                  </td>
                </tr>
              ))}
              {!cargando && filtrados.length === 0 && <tr><td colSpan={6} className="p-10 text-center text-slate-400">No hay turnos para los filtros seleccionados.</td></tr>}
            </tbody>
          </table>
        </div>
      </section>

      {editor && <Modal titulo={editor.modo === 'crear' ? 'Registrar turno' : 'Reprogramar turno'} onClose={() => setEditor(null)}>
        <form onSubmit={guardarTurno} className="space-y-5">
          {editor.modo === 'crear' && <label className="block text-xs font-medium text-slate-600">Paciente<select required value={editor.idPaciente} onChange={(event) => setEditor({ ...editor, idPaciente: event.target.value })} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm"><option value="">Seleccionar paciente</option>{pacientes.map((paciente) => <option key={paciente.idPaciente} value={paciente.idPaciente}>{paciente.nombreCompleto} · DNI {paciente.dni}</option>)}</select></label>}
          <label className="block text-xs font-medium text-slate-600">Médico<select required disabled={editor.modo === 'reagendar'} value={editor.idMedico} onChange={(event) => setEditor({ ...editor, idMedico: event.target.value, horaInicio: '' })} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm disabled:bg-slate-50"><option value="">Seleccionar médico</option>{medicos.map((medico) => <option key={medico.idMedico} value={medico.idMedico}>{medico.nombreCompleto} · {medico.especialidadNombre}</option>)}</select></label>
          <Campo etiqueta="Fecha" type="date" min={hoy()} required value={editor.fecha} onChange={(event) => setEditor({ ...editor, fecha: event.target.value, horaInicio: '' })} />
          <label className="block text-xs font-medium text-slate-600">Horario disponible<select required value={editor.horaInicio} onChange={(event) => setEditor({ ...editor, horaInicio: event.target.value })} disabled={!editor.idMedico || !editor.fecha || cargandoSlots} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm disabled:bg-slate-50"><option value="">{cargandoSlots ? 'Buscando horarios…' : 'Seleccionar horario'}</option>{slots.map((slot) => <option key={`${slot.horaInicio}-${slot.idConsultorio}`} value={slot.horaInicio}>{hora(slot.horaInicio)} · {slot.ubicacionConsultorio}</option>)}</select></label>
          {!cargandoSlots && editor.idMedico && editor.fecha && slots.length === 0 && <p className="text-xs text-amber-700">El médico no tiene horarios libres para esa fecha.</p>}
          <div className="flex justify-end gap-3 pt-2"><Boton type="button" secundario onClick={() => setEditor(null)}>Cancelar</Boton><Boton type="submit" disabled={guardando || !editor.horaInicio}>{guardando && <Loader2 className="h-4 w-4 animate-spin" />}{editor.modo === 'crear' ? 'Registrar turno' : 'Guardar reprogramación'}</Boton></div>
        </form>
      </Modal>}

      {detalle && <Modal titulo={`Detalle del turno #${detalle.idTurno}`} onClose={() => setDetalle(null)}>
        <div className="space-y-6">
          <div className="flex items-center justify-between rounded-xl bg-slate-50 p-4">
            <div><p className="text-xs text-slate-500">Fecha y horario</p><p className="mt-1 font-bold text-slate-900">{fecha(detalle.horaInicio)} · {hora(detalle.horaInicio)} a {hora(detalle.horaFin)}</p></div>
            <span className={`rounded-full px-3 py-1.5 text-xs font-semibold ${estadoClase(detalle.estadoDescripcion)}`}>{detalle.estadoDescripcion}</span>
          </div>
          <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
            <section className="rounded-xl border border-slate-200 p-4">
              <p className="text-[11px] font-bold uppercase tracking-wider text-slate-400">Paciente</p>
              <h3 className="mt-2 font-bold text-slate-900">{detalle.nombrePaciente || 'Paciente'}</h3>
              <dl className="mt-3 grid grid-cols-2 gap-y-2 text-xs"><dt className="text-slate-500">DNI</dt><dd>{detalle.dniPaciente || '—'}</dd><dt className="text-slate-500">Edad</dt><dd>{detalle.edadPaciente ? `${detalle.edadPaciente} años` : '—'}</dd><dt className="text-slate-500">Obra social</dt><dd>{detalle.obraSocialPaciente || '—'}</dd></dl>
            </section>
            <section className="rounded-xl border border-slate-200 p-4">
              <p className="text-[11px] font-bold uppercase tracking-wider text-slate-400">Atención</p>
              <h3 className="mt-2 font-bold text-slate-900">{detalle.nombreMedico || 'Médico'}</h3>
              <dl className="mt-3 grid grid-cols-2 gap-y-2 text-xs"><dt className="text-slate-500">Consultorio</dt><dd>{detalle.numeroConsultorio || '—'}</dd><dt className="text-slate-500">Ubicación</dt><dd>{detalle.ubicacionConsultorio || '—'}</dd><dt className="text-slate-500">Duración</dt><dd>20 minutos</dd></dl>
            </section>
          </div>
          <div className="flex justify-end"><Boton onClick={() => setDetalle(null)}>Cerrar detalle</Boton></div>
        </div>
      </Modal>}
    </div>
  );
};

const pacienteVacio: CrearPacienteRequest = { nombre: '', apellido: '', dni: '', fechaNacimiento: '', mail: '', obraSocial: '', peso: 0, altura: 0 };

export const RecepcionistaPacientesPage: React.FC = () => {
  const [pacientes, setPacientes] = useState<PacienteDto[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [editando, setEditando] = useState<PacienteDto | null>(null);
  const [formulario, setFormulario] = useState<CrearPacienteRequest>(pacienteVacio);
  const [modalAbierto, setModalAbierto] = useState(false);
  const [guardando, setGuardando] = useState(false);
  const [aviso, setAviso] = useState<{ texto: string; error?: boolean } | null>(null);

  const cargar = useCallback(async () => {
    try { setPacientes(await recepcionistaService.obtenerPacientes()); }
    catch (error) { setAviso({ texto: mensajeError(error), error: true }); }
  }, []);
  useEffect(() => { void cargar(); }, [cargar]);

  const lista = useMemo(() => pacientes.filter((paciente) => `${paciente.nombreCompleto} ${paciente.dni} ${paciente.mail}`.toLowerCase().includes(busqueda.toLowerCase())), [pacientes, busqueda]);

  const abrirNuevo = () => { setEditando(null); setFormulario(pacienteVacio); setModalAbierto(true); setAviso(null); };
  const abrirEdicion = (paciente: PacienteDto) => {
    setEditando(paciente);
    setFormulario({ nombre: paciente.nombre, apellido: paciente.apellido, dni: paciente.dni, fechaNacimiento: soloFecha(paciente.fechaNacimiento), mail: paciente.mail, obraSocial: paciente.obraSocial, peso: paciente.peso, altura: paciente.altura });
    setModalAbierto(true);
    setAviso(null);
  };

  const guardar = async (event: React.FormEvent) => {
    event.preventDefault();
    setGuardando(true);
    try {
      if (editando) {
        const request: ModificarPacienteRequest = { nombre: formulario.nombre, apellido: formulario.apellido, mail: formulario.mail, obraSocial: formulario.obraSocial, peso: Number(formulario.peso), altura: Number(formulario.altura) };
        await recepcionistaService.modificarPaciente(editando.idPaciente, request);
        setAviso({ texto: 'Paciente actualizado correctamente.' });
      } else {
        await recepcionistaService.crearPaciente({ ...formulario, peso: Number(formulario.peso), altura: Number(formulario.altura) });
        setAviso({ texto: 'Paciente registrado correctamente.' });
      }
      setModalAbierto(false);
      await cargar();
    } catch (error) {
      setAviso({ texto: mensajeError(error), error: true });
    } finally { setGuardando(false); }
  };

  return <div className="space-y-7">
    <Titulo titulo="Pacientes" detalle={`${pacientes.length} pacientes registrados`} accion={<Boton onClick={abrirNuevo}><Plus className="h-4 w-4" />Nuevo paciente</Boton>} />
    {aviso && <Aviso {...aviso} />}
    <Filtro value={busqueda} onChange={setBusqueda} placeholder="Buscar por nombre, DNI o correo" />
    <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">{lista.map((paciente) => <article key={paciente.idPaciente} className="rounded-2xl border border-slate-200 bg-white p-5"><div className="flex items-start justify-between gap-3"><div className="flex items-center gap-3"><Users className="h-5 w-5 text-[#0d8b9a]" /><h2 className="font-bold text-slate-900">{paciente.nombreCompleto}</h2></div><button onClick={() => abrirEdicion(paciente)} className="rounded-lg border border-slate-200 p-2 text-slate-600 hover:bg-slate-50" title="Editar paciente"><Pencil className="h-4 w-4" /></button></div><div className="mt-5 grid grid-cols-2 gap-y-2 text-xs"><span className="text-slate-500">DNI</span><span>{paciente.dni}</span><span className="text-slate-500">Nacimiento</span><span>{fecha(paciente.fechaNacimiento)}</span><span className="text-slate-500">Cobertura</span><span>{paciente.obraSocial}</span><span className="text-slate-500">Peso / altura</span><span>{paciente.peso} kg · {paciente.altura} m</span><span className="text-slate-500">Correo</span><span className="text-[#0d5c68]">{paciente.mail}</span></div></article>)}</div>
    {modalAbierto && <Modal titulo={editando ? 'Editar paciente' : 'Registrar paciente'} onClose={() => setModalAbierto(false)}><form onSubmit={guardar} className="grid grid-cols-1 gap-4 sm:grid-cols-2"><Campo etiqueta="Nombre" required value={formulario.nombre} onChange={(event) => setFormulario({ ...formulario, nombre: event.target.value })} /><Campo etiqueta="Apellido" required value={formulario.apellido} onChange={(event) => setFormulario({ ...formulario, apellido: event.target.value })} /><Campo etiqueta="DNI" required disabled={Boolean(editando)} value={formulario.dni} onChange={(event) => setFormulario({ ...formulario, dni: event.target.value })} /><Campo etiqueta="Fecha de nacimiento" type="date" required disabled={Boolean(editando)} max={hoy()} value={formulario.fechaNacimiento} onChange={(event) => setFormulario({ ...formulario, fechaNacimiento: event.target.value })} /><Campo etiqueta="Correo electrónico" type="email" required value={formulario.mail} onChange={(event) => setFormulario({ ...formulario, mail: event.target.value })} /><Campo etiqueta="Obra social" required value={formulario.obraSocial} onChange={(event) => setFormulario({ ...formulario, obraSocial: event.target.value })} /><Campo etiqueta="Peso (kg)" type="number" min="1" step="0.1" required value={formulario.peso || ''} onChange={(event) => setFormulario({ ...formulario, peso: Number(event.target.value) })} /><Campo etiqueta="Altura (m)" type="number" min="0.1" step="0.01" required value={formulario.altura || ''} onChange={(event) => setFormulario({ ...formulario, altura: Number(event.target.value) })} /><div className="flex justify-end gap-3 pt-2 sm:col-span-2"><Boton type="button" secundario onClick={() => setModalAbierto(false)}>Cancelar</Boton><Boton type="submit" disabled={guardando}>{guardando && <Loader2 className="h-4 w-4 animate-spin" />}Guardar paciente</Boton></div></form></Modal>}
  </div>;
};

interface FormularioMedico {
  nombre: string; apellido: string; dni: string; fechaNacimiento: string; mail: string; matricula: string; especialidad: string; fechaInicio: string;
}
const medicoVacio: FormularioMedico = { nombre: '', apellido: '', dni: '', fechaNacimiento: '', mail: '', matricula: '', especialidad: '', fechaInicio: hoy() };

export const RecepcionistaMedicosPage: React.FC = () => {
  const [medicos, setMedicos] = useState<MedicoDto[]>([]);
  const [especialidades, setEspecialidades] = useState<EspecialidadItem[]>([]);
  const [consultorios, setConsultorios] = useState<ConsultorioDto[]>([]);
  const [horarios, setHorarios] = useState<HorarioAtencionDto[]>([]);
  const [busqueda, setBusqueda] = useState('');
  const [editando, setEditando] = useState<MedicoDto | null>(null);
  const [formulario, setFormulario] = useState<FormularioMedico>(medicoVacio);
  const [modalAbierto, setModalAbierto] = useState(false);
  const [medicoHorarios, setMedicoHorarios] = useState<MedicoDto | null>(null);
  const [nuevoHorario, setNuevoHorario] = useState({ diaSemana: '1', horaDesde: '08:00', horaHasta: '12:00', idConsultorio: '' });
  const [guardando, setGuardando] = useState(false);
  const [aviso, setAviso] = useState<{ texto: string; error?: boolean } | null>(null);

  const cargar = useCallback(async () => {
    try {
      const [listaMedicos, listaEspecialidades, listaConsultorios] = await Promise.all([medicoService.obtenerMedicos(), medicoService.obtenerEspecialidades(), medicoService.obtenerConsultorios()]);
      setMedicos(listaMedicos); setEspecialidades(listaEspecialidades); setConsultorios(listaConsultorios);
    } catch (error) { setAviso({ texto: mensajeError(error), error: true }); }
  }, []);
  useEffect(() => { void cargar(); }, [cargar]);

  const lista = useMemo(() => medicos.filter((medico) => `${medico.nombreCompleto} ${medico.especialidadNombre} ${medico.matricula}`.toLowerCase().includes(busqueda.toLowerCase())), [medicos, busqueda]);

  const abrirNuevo = () => { setEditando(null); setFormulario(medicoVacio); setModalAbierto(true); setAviso(null); };
  const abrirEdicion = (medico: MedicoDto) => { setEditando(medico); setFormulario({ nombre: medico.nombre, apellido: medico.apellido, dni: medico.dni, fechaNacimiento: soloFecha(medico.fechaNacimiento), mail: medico.mail, matricula: medico.matricula, especialidad: String(medico.especialidad), fechaInicio: soloFecha(medico.fechaInicio) }); setModalAbierto(true); setAviso(null); };
  const abrirHorarios = async (medico: MedicoDto) => { setMedicoHorarios(medico); setAviso(null); try { setHorarios(await medicoService.obtenerHorariosPorMedico(medico.idMedico)); } catch (error) { setHorarios([]); setAviso({ texto: mensajeError(error), error: true }); } };

  const guardarMedico = async (event: React.FormEvent) => {
    event.preventDefault(); setGuardando(true);
    try {
      if (editando) {
        await medicoService.modificarMedico(editando.idMedico, { nombre: formulario.nombre, apellido: formulario.apellido, mail: formulario.mail, matricula: formulario.matricula, especialidad: Number(formulario.especialidad) });
        setAviso({ texto: 'Médico actualizado correctamente.' });
      } else {
        await medicoService.registrarMedico({ ...formulario, especialidad: Number(formulario.especialidad) });
        setAviso({ texto: 'Médico registrado correctamente.' });
      }
      setModalAbierto(false); await cargar();
    } catch (error) { setAviso({ texto: mensajeError(error), error: true }); }
    finally { setGuardando(false); }
  };

  const agregarHorario = async (event: React.FormEvent) => {
    event.preventDefault(); if (!medicoHorarios) return; setGuardando(true);
    try {
      await medicoService.configurarHorario(medicoHorarios.idMedico, { diaSemana: Number(nuevoHorario.diaSemana), horaDesde: `${nuevoHorario.horaDesde}:00`, horaHasta: `${nuevoHorario.horaHasta}:00`, idConsultorio: Number(nuevoHorario.idConsultorio) });
      setHorarios(await medicoService.obtenerHorariosPorMedico(medicoHorarios.idMedico));
      setAviso({ texto: 'Horario agregado correctamente.' });
    } catch (error) { setAviso({ texto: mensajeError(error), error: true }); }
    finally { setGuardando(false); }
  };

  const reasignarConsultorio = async (horario: HorarioAtencionDto, idConsultorio: number) => {
    if (horario.idConsultorio === idConsultorio) return;
    try {
      await medicoService.asignarConsultorio(horario.idHorarioAtencion, idConsultorio);
      if (medicoHorarios) setHorarios(await medicoService.obtenerHorariosPorMedico(medicoHorarios.idMedico));
      setAviso({ texto: 'Consultorio reasignado correctamente.' });
    } catch (error) { setAviso({ texto: mensajeError(error), error: true }); }
  };

  return <div className="space-y-7">
    <Titulo titulo="Médicos y disponibilidad" detalle={`${medicos.length} profesionales registrados`} accion={<Boton onClick={abrirNuevo}><Plus className="h-4 w-4" />Nuevo médico</Boton>} />
    {aviso && <Aviso {...aviso} />}
    <div className="rounded-2xl border border-cyan-200 bg-cyan-50 p-4 text-sm text-cyan-900">
      <strong>¿Querés asignar un horario y consultorio?</strong> Elegí un médico y presioná <span className="font-semibold">Gestionar horarios y consultorio</span>.
    </div>
    <Filtro value={busqueda} onChange={setBusqueda} placeholder="Buscar médico, especialidad o matrícula" />
    <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
      {lista.map((medico) => (
        <article key={medico.idMedico} className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center gap-3">
            <Stethoscope className="h-5 w-5 text-[#0d8b9a]" />
            <div><h2 className="font-bold text-slate-900">{medico.nombreCompleto}</h2><p className="mt-1 text-xs text-[#0d5c68]">{medico.especialidadNombre}</p></div>
          </div>
          <div className="mt-5 grid grid-cols-2 gap-y-2 border-t border-slate-100 pt-4 text-xs">
            <span className="text-slate-500">Matrícula</span><span>{medico.matricula}</span>
            <span className="text-slate-500">Correo</span><span>{medico.mail}</span>
            <span className="text-slate-500">DNI</span><span>{medico.dni}</span>
          </div>
          <div className="mt-5 flex flex-wrap gap-2 border-t border-slate-100 pt-4">
            <Boton onClick={() => void abrirHorarios(medico)}><CalendarPlus className="h-4 w-4" />Gestionar horarios y consultorio</Boton>
            <Boton secundario onClick={() => abrirEdicion(medico)}><Pencil className="h-4 w-4" />Editar médico</Boton>
          </div>
        </article>
      ))}
    </div>
    {modalAbierto && <Modal titulo={editando ? 'Editar médico' : 'Registrar médico'} onClose={() => setModalAbierto(false)}><form onSubmit={guardarMedico} className="grid grid-cols-1 gap-4 sm:grid-cols-2"><Campo etiqueta="Nombre" required value={formulario.nombre} onChange={(event) => setFormulario({ ...formulario, nombre: event.target.value })} /><Campo etiqueta="Apellido" required value={formulario.apellido} onChange={(event) => setFormulario({ ...formulario, apellido: event.target.value })} /><Campo etiqueta="DNI" required disabled={Boolean(editando)} value={formulario.dni} onChange={(event) => setFormulario({ ...formulario, dni: event.target.value })} /><Campo etiqueta="Fecha de nacimiento" type="date" required disabled={Boolean(editando)} max={hoy()} value={formulario.fechaNacimiento} onChange={(event) => setFormulario({ ...formulario, fechaNacimiento: event.target.value })} /><Campo etiqueta="Correo electrónico" type="email" required value={formulario.mail} onChange={(event) => setFormulario({ ...formulario, mail: event.target.value })} /><Campo etiqueta="Matrícula" required value={formulario.matricula} onChange={(event) => setFormulario({ ...formulario, matricula: event.target.value })} /><label className="text-xs font-medium text-slate-600">Especialidad<select required value={formulario.especialidad} onChange={(event) => setFormulario({ ...formulario, especialidad: event.target.value })} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm"><option value="">Seleccionar especialidad</option>{especialidades.map((item) => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></label><Campo etiqueta="Fecha de inicio" type="date" required disabled={Boolean(editando)} value={formulario.fechaInicio} onChange={(event) => setFormulario({ ...formulario, fechaInicio: event.target.value })} /><div className="flex justify-end gap-3 pt-2 sm:col-span-2"><Boton type="button" secundario onClick={() => setModalAbierto(false)}>Cancelar</Boton><Boton type="submit" disabled={guardando}>{guardando && <Loader2 className="h-4 w-4 animate-spin" />}Guardar médico</Boton></div></form></Modal>}
    {medicoHorarios && <Modal titulo={`Horarios de ${medicoHorarios.nombreCompleto}`} onClose={() => setMedicoHorarios(null)} amplio><div className="space-y-6"><div className="overflow-x-auto rounded-xl border border-slate-200"><table className="w-full text-left text-xs"><thead className="bg-slate-50 text-slate-500"><tr><th className="px-4 py-3">Día</th><th className="px-4 py-3">Desde</th><th className="px-4 py-3">Hasta</th><th className="px-4 py-3">Consultorio</th></tr></thead><tbody>{horarios.map((horario) => <tr key={horario.idHorarioAtencion} className="border-t border-slate-100"><td className="px-4 py-3 font-semibold">{dias[horario.diaSemana]}</td><td className="px-4 py-3">{horario.horaDesde.slice(0, 5)}</td><td className="px-4 py-3">{horario.horaHasta.slice(0, 5)}</td><td className="px-4 py-3"><select value={horario.idConsultorio} onChange={(event) => void reasignarConsultorio(horario, Number(event.target.value))} className="rounded-lg border border-slate-200 px-2 py-1.5"><option value="">Sin asignar</option>{consultorios.map((consultorio) => <option key={consultorio.idConsultorio} value={consultorio.idConsultorio}>Consultorio {consultorio.numero} · {consultorio.ubicacion}</option>)}</select></td></tr>)}{horarios.length === 0 && <tr><td colSpan={4} className="p-6 text-center text-slate-400">El médico todavía no tiene horarios configurados.</td></tr>}</tbody></table></div><form onSubmit={agregarHorario} className="grid grid-cols-1 gap-4 rounded-xl bg-slate-50 p-4 md:grid-cols-4"><label className="text-xs font-medium text-slate-600">Día<select required value={nuevoHorario.diaSemana} onChange={(event) => setNuevoHorario({ ...nuevoHorario, diaSemana: event.target.value })} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm">{dias.map((dia, indice) => <option key={dia} value={indice}>{dia}</option>)}</select></label><Campo etiqueta="Desde" type="time" required value={nuevoHorario.horaDesde} onChange={(event) => setNuevoHorario({ ...nuevoHorario, horaDesde: event.target.value })} /><Campo etiqueta="Hasta" type="time" required value={nuevoHorario.horaHasta} onChange={(event) => setNuevoHorario({ ...nuevoHorario, horaHasta: event.target.value })} /><label className="text-xs font-medium text-slate-600">Consultorio<select required value={nuevoHorario.idConsultorio} onChange={(event) => setNuevoHorario({ ...nuevoHorario, idConsultorio: event.target.value })} className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm"><option value="">Seleccionar</option>{consultorios.map((consultorio) => <option key={consultorio.idConsultorio} value={consultorio.idConsultorio}>Consultorio {consultorio.numero}</option>)}</select></label><div className="md:col-span-4"><Boton type="submit" disabled={guardando}>{guardando && <Loader2 className="h-4 w-4 animate-spin" />}Agregar horario</Boton></div></form></div></Modal>}
  </div>;
};

export const RecepcionistaConsultoriosPage: React.FC = () => {
  const [consultorios, setConsultorios] = useState<ConsultorioDto[]>([]);
  const [editando, setEditando] = useState<ConsultorioDto | null>(null);
  const [formulario, setFormulario] = useState({ numero: '', piso: 0, sector: '' });
  const [guardando, setGuardando] = useState(false);
  const navigate = useNavigate();
  const cargar = () => medicoService.obtenerConsultorios().then(setConsultorios).catch(() => setConsultorios([]));
  useEffect(() => { void cargar(); }, []);
  const abrirEdicion = (consultorio: ConsultorioDto) => { setEditando(consultorio); setFormulario({ numero: consultorio.numero, piso: consultorio.piso, sector: consultorio.sector }); };
  const guardar = async (event: React.FormEvent) => { event.preventDefault(); if (!editando) return; setGuardando(true); try { await medicoService.modificarConsultorio(editando.idConsultorio, formulario); setEditando(null); await cargar(); } finally { setGuardando(false); } };
  const desactivar = async (consultorio: ConsultorioDto) => { if (!window.confirm(`¿Desactivar el consultorio ${consultorio.numero}?`)) return; await medicoService.desactivarConsultorio(consultorio.idConsultorio); await cargar(); };
  return <div className="space-y-7">
    <Titulo titulo="Consultorios" detalle={`${consultorios.length} espacios de atención disponibles.`} accion={<Boton onClick={() => navigate('/recepcionista/medicos')}><CalendarPlus className="h-4 w-4" />Asignar a un médico</Boton>} />
    <div className="rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900">
      Los consultorios se asignan a un <strong>horario médico</strong>. Presioná “Asignar a un médico”, elegí al profesional y luego agregá el día, la franja horaria y el consultorio.
    </div>
    <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">{consultorios.map((consultorio) => <article key={consultorio.idConsultorio} className="rounded-2xl border border-slate-200 bg-white p-5"><div className="flex items-center justify-between"><div className="flex items-center gap-3"><Building2 className="h-5 w-5 text-[#0d8b9a]" /><h2 className="font-bold text-slate-900">Consultorio {consultorio.numero}</h2></div><div className="flex gap-2"><Boton secundario onClick={() => abrirEdicion(consultorio)}>Editar</Boton><Boton secundario onClick={() => void desactivar(consultorio)}>Desactivar</Boton></div></div><p className="mt-5 text-sm">{consultorio.piso}° piso</p><p className="mt-1 text-xs text-slate-500">{consultorio.ubicacion}</p><div className="mt-5 border-t border-slate-100 pt-4"><p className="text-[11px] font-semibold uppercase text-slate-500">Sector</p><p className="mt-1 text-sm text-slate-700">{consultorio.sector || 'Atención general'}</p></div></article>)}</div>
    {editando && <Modal titulo={`Editar consultorio ${editando.numero}`} onClose={() => setEditando(null)}><form onSubmit={guardar} className="grid gap-4"><Campo etiqueta="Número" required value={formulario.numero} onChange={(e) => setFormulario({ ...formulario, numero: e.target.value })} /><Campo etiqueta="Piso" type="number" min="0" required value={formulario.piso} onChange={(e) => setFormulario({ ...formulario, piso: Number(e.target.value) })} /><Campo etiqueta="Sector" required value={formulario.sector} onChange={(e) => setFormulario({ ...formulario, sector: e.target.value })} /><div className="flex justify-end gap-3"><Boton type="button" secundario onClick={() => setEditando(null)}>Cancelar</Boton><Boton type="submit" disabled={guardando}>{guardando ? 'Guardando…' : 'Guardar cambios'}</Boton></div></form></Modal>}
  </div>;
};
