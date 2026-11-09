import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { medicoService } from '../../api/medicoService';
import type { PacienteDto, HistoriaClinicaDto } from '../../types/medico';
import {
  FileText,
  Search,
  User,
  Pill,
  Clock,
  Stethoscope,
  AlertCircle,
  Activity,
  Scale,
  Ruler,
  ShieldCheck,
  Mail,
  CreditCard,
  Calendar,
  X,
  Plus,
  Users,
  CheckCircle2,
  Info,
} from 'lucide-react';

export const HistoriaClinicaMedicoPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();

  // Estado del buscador de pacientes
  const [busquedaInput, setBusquedaInput] = useState<string>('');
  const [pacientesEncontrados, setPacientesEncontrados] = useState<PacienteDto[]>([]);
  const [isSearchingPacientes, setIsSearchingPacientes] = useState<boolean>(false);
  const [busquedaRealizada, setBusquedaRealizada] = useState<boolean>(false);
  const [terminoBuscado, setTerminoBuscado] = useState<string>('');

  // Estado de la Historia Clínica seleccionada
  const [pacienteSeleccionado, setPacienteSeleccionado] = useState<PacienteDto | null>(null);
  const [historiaClinica, setHistoriaClinica] = useState<HistoriaClinicaDto | null>(null);
  const [isLoadingHistoria, setIsLoadingHistoria] = useState<boolean>(false);

  // Estados de mensajes y validaciones (Criterios de aceptación)
  const [errorPacienteInexistente, setErrorPacienteInexistente] = useState<string | null>(null);
  const [avisoSinHistoriaClinica, setAvisoSinHistoriaClinica] = useState<string | null>(null);
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  // Filtro interno de prescripciones
  const [filtroPrescripcion, setFiltroPrescripcion] = useState<string>('');

  // Formatear fecha legible
  const formatearFechaHora = (isoStr: string) => {
    try {
      return new Date(isoStr).toLocaleDateString('es-AR', {
        day: '2-digit',
        month: 'long',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    } catch {
      return isoStr;
    }
  };

  const formatearFechaCorta = (isoStr?: string) => {
    if (!isoStr) return 'No registrada';
    try {
      return new Date(isoStr).toLocaleDateString('es-AR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      });
    } catch {
      return isoStr;
    }
  };

  // Calcular IMC manejando altura tanto en metros (ej: 1.70) como en centímetros (ej: 170)
  const calcularIMC = (peso: number, altura: number): { valor: string; categoria: string } | null => {
    if (!peso || !altura || peso <= 0 || altura <= 0) return null;
    const alturaMetros = altura > 3 ? altura / 100 : altura;
    const imc = peso / (alturaMetros * alturaMetros);
    if (!isFinite(imc) || imc <= 0) return null;

    let categoria = 'Normal';
    if (imc < 18.5) categoria = 'Bajo peso';
    else if (imc < 25) categoria = 'Peso saludable';
    else if (imc < 30) categoria = 'Sobrepeso';
    else categoria = 'Obesidad';

    return { valor: imc.toFixed(1), categoria };
  };

  const formatearAltura = (altura: number): string => {
    if (!altura || altura <= 0) return 'No registrada';
    if (altura > 3) {
      return `${altura} cm (${(altura / 100).toFixed(2)} m)`;
    }
    return `${altura.toFixed(2)} m (${Math.round(altura * 100)} cm)`;
  };

  // Solicitar la historia clínica de un paciente por su ID
  const solicitarHistoriaClinica = async (idPaciente: number, pacienteInfo?: PacienteDto) => {
    setIsLoadingHistoria(true);
    setErrorPacienteInexistente(null);
    setAvisoSinHistoriaClinica(null);
    setErrorGeneral(null);
    setFiltroPrescripcion('');

    if (pacienteInfo) {
      setPacienteSeleccionado(pacienteInfo);
    }

    try {
      const hc = await medicoService.obtenerHistoriaClinicaPorPaciente(idPaciente);
      setHistoriaClinica(hc);
      setSearchParams({ pacienteId: String(idPaciente) }, { replace: true });

      if (!pacienteInfo) {
        try {
          const pDto = await medicoService.obtenerPacientePorId(idPaciente);
          setPacienteSeleccionado(pDto);
        } catch {
          // Si falla la carga extra, igual tenemos los datos en HistoriaClinicaDto
        }
      }
    } catch (err: unknown) {
      setHistoriaClinica(null);
      const apiErr = err as { status?: number; error?: string };
      const msg = apiErr?.error || '';

      if (msg.toLowerCase().includes('no se encontró la historia clínica')) {
        // El paciente existe pero no tiene historia clínica asociada
        setAvisoSinHistoriaClinica(
          `El paciente #${idPaciente} existe en el padrón, pero aún no posee una historia clínica abierta en el sistema.`
        );
      } else if (apiErr?.status === 404 || msg.toLowerCase().includes('no se encontró el paciente')) {
        // El paciente no existe
        setPacienteSeleccionado(null);
        setErrorPacienteInexistente(
          `No existe ningún paciente registrado con el identificador #${idPaciente}. Verificá los datos e intentá nuevamente.`
        );
      } else {
        setErrorGeneral(
          msg || 'Ocurrió un inconveniente al solicitar la historia clínica del paciente.'
        );
      }
    } finally {
      setIsLoadingHistoria(false);
    }
  };

  // Cargar padrón inicial de pacientes y verificar si viene pacienteId por URL
  useEffect(() => {
    const inicializar = async () => {
      try {
        setIsSearchingPacientes(true);
        const lista = await medicoService.buscarPacientes();
        setPacientesEncontrados(lista);

        const paramId = searchParams.get('pacienteId');
        if (paramId) {
          const idNum = parseInt(paramId, 10);
          if (!isNaN(idNum) && idNum > 0) {
            const encontrado = lista.find((p) => p.idPaciente === idNum);
            await solicitarHistoriaClinica(idNum, encontrado);
            return;
          }
        }

        // Si hay pacientes en la base de datos y no se especificó parámetro, seleccionar el primero automáticamente para agilizar la vista
        if (lista.length > 0) {
          await solicitarHistoriaClinica(lista[0].idPaciente, lista[0]);
        }
      } catch {
        // Silencioso al iniciar
      } finally {
        setIsSearchingPacientes(false);
      }
    };

    inicializar();
  }, []);

  // Buscar pacientes por DNI, Nombre, Apellido o ID numérico
  const handleBuscarPacientes = async (e: React.FormEvent) => {
    e.preventDefault();
    const termino = busquedaInput.trim();

    setErrorPacienteInexistente(null);
    setAvisoSinHistoriaClinica(null);
    setErrorGeneral(null);
    setTerminoBuscado(termino);
    setBusquedaRealizada(true);

    try {
      setIsSearchingPacientes(true);

      // 1. Buscar por coincidencia de DNI, Nombre o Apellido en el backend
      let resultados = await medicoService.buscarPacientes(termino || undefined);

      // 2. Si el término es un número (ej. ID de paciente "4") y no coincidió por DNI, probar buscar por ID exacto
      if (termino && /^\d+$/.test(termino)) {
        const idBuscado = parseInt(termino, 10);
        const yaEsta = resultados.some((p) => p.idPaciente === idBuscado);
        if (!yaEsta && idBuscado > 0) {
          try {
            const porId = await medicoService.obtenerPacientePorId(idBuscado);
            if (porId) {
              resultados = [porId, ...resultados];
            }
          } catch {
            // No existe con ese ID
          }
        }
      }

      setPacientesEncontrados(resultados);

      if (resultados.length === 0) {
        setHistoriaClinica(null);
        setPacienteSeleccionado(null);
        setErrorPacienteInexistente(
          `No se encontró ningún paciente con los datos identificatorios «${termino}». Verificá el DNI, apellido, nombre o N° de paciente.`
        );
      } else if (resultados.length === 1) {
        // Si hay una única coincidencia, cargar automáticamente su historia clínica
        await solicitarHistoriaClinica(resultados[0].idPaciente, resultados[0]);
      }
    } catch {
      setErrorGeneral('No se pudo realizar la búsqueda de pacientes. Verificá tu conexión.');
    } finally {
      setIsSearchingPacientes(false);
    }
  };

  const handleLimpiarBusqueda = async () => {
    setBusquedaInput('');
    setBusquedaRealizada(false);
    setTerminoBuscado('');
    setErrorPacienteInexistente(null);
    setAvisoSinHistoriaClinica(null);
    setErrorGeneral(null);

    try {
      setIsSearchingPacientes(true);
      const todos = await medicoService.buscarPacientes();
      setPacientesEncontrados(todos);
      if (todos.length > 0 && !historiaClinica) {
        await solicitarHistoriaClinica(todos[0].idPaciente, todos[0]);
      }
    } finally {
      setIsSearchingPacientes(false);
    }
  };

  const prescripcionesFiltradas =
    historiaClinica?.prescripciones.filter((p) => {
      if (!filtroPrescripcion.trim()) return true;
      const q = filtroPrescripcion.toLowerCase();
      return (
        p.medicamento.toLowerCase().includes(q) ||
        p.nombreMedico.toLowerCase().includes(q) ||
        p.matriculaMedico.toLowerCase().includes(q)
      );
    }) || [];

  const imcInfo = historiaClinica
    ? calcularIMC(historiaClinica.peso, historiaClinica.altura)
    : null;

  return (
    <div className="space-y-6 max-w-6xl mx-auto animate-fadeIn">
      {/* Encabezado Principal */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold text-[#0d5c68] uppercase tracking-wider">
            <FileText className="w-4 h-4" />
            <span>Módulo de Atención Médica · RF25</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight mt-1">
            Consulta de Historia Clínica
          </h1>
          <p className="text-xs sm:text-sm text-slate-500 mt-1">
            Buscá un paciente por sus datos identificatorios para consultar sus antecedentes, parámetros clínicos y prescripciones.
          </p>
        </div>

        {historiaClinica && (
          <button
            type="button"
            onClick={() =>
              navigate(`/medico/prescripciones?pacienteId=${historiaClinica.idPaciente}`)
            }
            className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold shadow-xs transition-all cursor-pointer self-start sm:self-auto"
          >
            <Plus className="w-4 h-4" />
            <span>Nueva Prescripción para este Paciente</span>
          </button>
        )}
      </div>

      {/* Panel de Búsqueda de Paciente por Datos Identificatorios */}
      <div className="bg-white rounded-2xl border border-slate-200/80 p-5 sm:p-6 shadow-xs space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <div className="w-9 h-9 rounded-xl bg-teal-50 text-[#0d5c68] flex items-center justify-center shrink-0 border border-teal-100">
              <Search className="w-4 h-4" />
            </div>
            <div>
              <h2 className="text-sm font-bold text-slate-800">
                Buscar Paciente en el Padrón
              </h2>
              <p className="text-[11px] text-slate-400">
                Ingresá DNI, Apellido, Nombre o N° de Paciente para localizar su ficha médica
              </p>
            </div>
          </div>

          {busquedaRealizada && (
            <button
              type="button"
              onClick={handleLimpiarBusqueda}
              className="text-xs text-slate-500 hover:text-slate-800 flex items-center gap-1 font-medium cursor-pointer"
            >
              <X className="w-3.5 h-3.5" />
              <span>Mostrar todos</span>
            </button>
          )}
        </div>

        <form onSubmit={handleBuscarPacientes} className="flex flex-col sm:flex-row gap-2.5">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2 pointer-events-none" />
            <input
              type="text"
              value={busquedaInput}
              onChange={(e) => setBusquedaInput(e.target.value)}
              placeholder="Ej: 45573520, Vega, Joaco o ID 4..."
              className="w-full pl-10 pr-9 py-2.5 rounded-xl border border-slate-200 text-xs sm:text-sm text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68]"
            />
            {busquedaInput && (
              <button
                type="button"
                onClick={() => setBusquedaInput('')}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 cursor-pointer"
                title="Limpiar texto"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>

          <button
            type="submit"
            disabled={isSearchingPacientes}
            className="px-5 py-2.5 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs sm:text-sm font-semibold transition-colors cursor-pointer disabled:opacity-50 flex items-center justify-center gap-2 shrink-0"
          >
            <Search className="w-4 h-4" />
            <span>{isSearchingPacientes ? 'Buscando...' : 'Buscar Paciente'}</span>
          </button>
        </form>

        {/* Resultados de pacientes disponibles para seleccionar */}
        {pacientesEncontrados.length > 0 && (
          <div className="pt-2">
            <div className="flex items-center justify-between mb-2">
              <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wider flex items-center gap-1.5">
                <Users className="w-3.5 h-3.5" />
                {busquedaRealizada && terminoBuscado
                  ? `Resultados para «${terminoBuscado}» (${pacientesEncontrados.length})`
                  : `Pacientes registrados (${pacientesEncontrados.length})`}
              </span>
              <span className="text-[11px] text-slate-400">
                Hacé clic en un paciente para solicitar su historia clínica
              </span>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2.5">
              {pacientesEncontrados.map((p) => {
                const isSelected =
                  historiaClinica?.idPaciente === p.idPaciente ||
                  pacienteSeleccionado?.idPaciente === p.idPaciente;

                return (
                  <button
                    key={p.idPaciente}
                    type="button"
                    onClick={() => solicitarHistoriaClinica(p.idPaciente, p)}
                    className={`p-3 rounded-xl border text-left transition-all cursor-pointer flex items-center justify-between gap-3 ${
                      isSelected
                        ? 'border-[#0d5c68] bg-teal-50/50 ring-2 ring-[#0d5c68]/10'
                        : 'border-slate-200/90 hover:border-[#0d5c68]/40 hover:bg-slate-50/70 bg-white'
                    }`}
                  >
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <p className="text-xs font-bold text-slate-900 truncate">
                          {p.nombreCompleto || `${p.apellido.toUpperCase()}, ${p.nombre}`}
                        </p>
                        <span className="text-[10px] font-semibold px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 shrink-0">
                          ID #{p.idPaciente}
                        </span>
                      </div>
                      <p className="text-[11px] text-slate-500 mt-0.5">
                        DNI: <strong className="text-slate-700">{p.dni}</strong> · {p.obraSocial || 'Particular'}
                      </p>
                    </div>

                    <div
                      className={`px-2.5 py-1 rounded-lg text-[10px] font-semibold shrink-0 ${
                        isSelected
                          ? 'bg-[#0d5c68] text-white'
                          : 'bg-slate-100 text-slate-700'
                      }`}
                    >
                      {isSelected ? 'Viendo HC' : 'Solicitar HC'}
                    </div>
                  </button>
                );
              })}
            </div>
          </div>
        )}
      </div>

      {/* Mensaje cuando el Paciente NO existe (Criterio de aceptación) */}
      {errorPacienteInexistente && (
        <div className="bg-red-50/90 border border-red-200 rounded-2xl p-6 text-center max-w-2xl mx-auto space-y-3 animate-fadeIn">
          <div className="w-12 h-12 rounded-2xl bg-red-100 text-red-600 flex items-center justify-center mx-auto">
            <AlertCircle className="w-6 h-6" />
          </div>
          <div className="space-y-1">
            <h3 className="text-base font-bold text-red-900">
              Paciente no encontrado en el sistema
            </h3>
            <p className="text-xs sm:text-sm text-red-700 leading-relaxed">
              {errorPacienteInexistente}
            </p>
          </div>
          <div className="pt-1">
            <button
              type="button"
              onClick={handleLimpiarBusqueda}
              className="px-4 py-2 rounded-xl bg-white hover:bg-red-100/60 text-red-800 border border-red-200 text-xs font-semibold transition-colors cursor-pointer"
            >
              Ver lista completa de pacientes registrados
            </button>
          </div>
        </div>
      )}

      {/* Mensaje cuando el Paciente existe pero NO tiene Historia Clínica (Criterio de aceptación) */}
      {avisoSinHistoriaClinica && (
        <div className="bg-amber-50 border border-amber-200 rounded-2xl p-6 text-center max-w-2xl mx-auto space-y-3 animate-fadeIn">
          <div className="w-12 h-12 rounded-2xl bg-amber-100 text-amber-700 flex items-center justify-center mx-auto">
            <Info className="w-6 h-6" />
          </div>
          <div className="space-y-1">
            <h3 className="text-base font-bold text-amber-900">
              Paciente sin Historia Clínica registrada
            </h3>
            <p className="text-xs sm:text-sm text-amber-800 leading-relaxed">
              {avisoSinHistoriaClinica}
            </p>
          </div>
        </div>
      )}

      {/* Alerta de Error General */}
      {errorGeneral && (
        <div className="p-4 rounded-xl bg-red-50 border border-red-200 text-red-700 text-xs flex items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{errorGeneral}</span>
          </div>
          <button
            type="button"
            onClick={() => setErrorGeneral(null)}
            className="text-red-500 hover:text-red-800 cursor-pointer"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Estado de Carga */}
      {isLoadingHistoria && (
        <div className="bg-white rounded-2xl border border-slate-200/80 p-12 text-center space-y-3">
          <div className="w-8 h-8 border-3 border-[#0d5c68] border-t-transparent rounded-full animate-spin mx-auto" />
          <p className="text-xs font-semibold text-slate-600">
            Solicitando historia clínica y antecedentes del paciente...
          </p>
        </div>
      )}

      {/* Contenido Principal de la Historia Clínica */}
      {!isLoadingHistoria && historiaClinica && (
        <div className="space-y-6 animate-fadeIn">
          {/* Bloque 1: Ficha Identificatoria y Parámetros Antropométricos del Paciente */}
          <div className="bg-white rounded-2xl border border-slate-200/80 shadow-xs overflow-hidden">
            {/* Barra superior institucional de la ficha */}
            <div className="bg-gradient-to-r from-[#05242c] to-[#0d5c68] px-6 py-4 text-white flex flex-col sm:flex-row sm:items-center justify-between gap-3">
              <div className="flex items-center gap-3">
                <div className="w-11 h-11 rounded-xl bg-white/15 backdrop-blur-xs flex items-center justify-center font-bold text-base tracking-wide border border-white/20 shrink-0">
                  <User className="w-5 h-5 text-teal-200" />
                </div>
                <div>
                  <div className="flex items-center gap-2.5 flex-wrap">
                    <h2 className="text-lg sm:text-xl font-bold tracking-tight">
                      {historiaClinica.nombrePaciente}
                    </h2>
                    <span className="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-teal-400/20 text-teal-100 border border-teal-300/30">
                      Historia Clínica #{String(historiaClinica.idHistoriaClinica).padStart(4, '0')}
                    </span>
                  </div>
                  <p className="text-xs text-teal-100/80 mt-0.5">
                    Paciente ID #{historiaClinica.idPaciente} · Información clínica confidencial para atención médica
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-2 self-start sm:self-auto">
                <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-500/20 text-emerald-100 border border-emerald-400/30">
                  <CheckCircle2 className="w-3.5 h-3.5 text-emerald-300" />
                  Expediente Activo
                </span>
              </div>
            </div>

            {/* Grilla de Datos del Paciente y Antecedentes Físicos */}
            <div className="p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Datos Identificatorios */}
              <div className="p-4 rounded-xl bg-slate-50/80 border border-slate-200/70 space-y-2">
                <div className="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-wider">
                  <CreditCard className="w-3.5 h-3.5 text-[#0d5c68]" />
                  <span>Identificación</span>
                </div>
                <div className="space-y-1 pt-1">
                  <p className="text-xs text-slate-500">
                    DNI: <strong className="text-slate-900 font-bold text-sm">{historiaClinica.dniPaciente}</strong>
                  </p>
                  <p className="text-xs text-slate-500">
                    Edad: <strong className="text-slate-800">{historiaClinica.edadPaciente} años</strong>
                  </p>
                  {(historiaClinica.fechaNacimiento || pacienteSeleccionado?.fechaNacimiento) && (
                    <p className="text-[11px] text-slate-400 flex items-center gap-1">
                      <Calendar className="w-3 h-3" />
                      Nac: {formatearFechaCorta(historiaClinica.fechaNacimiento || pacienteSeleccionado?.fechaNacimiento)}
                    </p>
                  )}
                </div>
              </div>

              {/* Cobertura y Contacto */}
              <div className="p-4 rounded-xl bg-slate-50/80 border border-slate-200/70 space-y-2">
                <div className="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-wider">
                  <ShieldCheck className="w-3.5 h-3.5 text-[#0d5c68]" />
                  <span>Cobertura y Contacto</span>
                </div>
                <div className="space-y-1 pt-1">
                  <p className="text-xs text-slate-500">
                    Obra Social:{' '}
                    <span className="inline-flex items-center px-2 py-0.5 rounded-md bg-teal-50 text-[#0d5c68] font-bold text-xs border border-teal-200/60">
                      {historiaClinica.obraSocial || pacienteSeleccionado?.obraSocial || 'Particular'}
                    </span>
                  </p>
                  <p className="text-xs text-slate-500 flex items-center gap-1.5 truncate pt-0.5" title={historiaClinica.mailPaciente}>
                    <Mail className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                    <span className="truncate text-slate-700 font-medium">{historiaClinica.mailPaciente}</span>
                  </p>
                </div>
              </div>

              {/* Parámetros Físicos (Peso y Altura) */}
              <div className="p-4 rounded-xl bg-slate-50/80 border border-slate-200/70 space-y-2">
                <div className="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-wider">
                  <Activity className="w-3.5 h-3.5 text-[#0d5c68]" />
                  <span>Biometría / Antecedentes</span>
                </div>
                <div className="space-y-1.5 pt-1">
                  <p className="text-xs text-slate-600 flex items-center gap-1.5">
                    <Scale className="w-3.5 h-3.5 text-slate-400" />
                    Peso: <strong className="text-slate-900">{historiaClinica.peso} kg</strong>
                  </p>
                  <p className="text-xs text-slate-600 flex items-center gap-1.5">
                    <Ruler className="w-3.5 h-3.5 text-slate-400" />
                    Altura: <strong className="text-slate-900">{formatearAltura(historiaClinica.altura)}</strong>
                  </p>
                </div>
              </div>

              {/* Resumen Clínico e IMC */}
              <div className="p-4 rounded-xl bg-slate-50/80 border border-slate-200/70 space-y-2">
                <div className="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-wider">
                  <Stethoscope className="w-3.5 h-3.5 text-[#0d5c68]" />
                  <span>Indicadores Clínicos</span>
                </div>
                <div className="space-y-1 pt-1">
                  {imcInfo ? (
                    <p className="text-xs text-slate-600">
                      IMC Estimado:{' '}
                      <strong className="text-slate-900">{imcInfo.valor} kg/m²</strong>{' '}
                      <span className="text-[11px] text-slate-500">({imcInfo.categoria})</span>
                    </p>
                  ) : (
                    <p className="text-xs text-slate-400">IMC sin calcular</p>
                  )}
                  <p className="text-xs text-slate-600">
                    Prescripciones registradas:{' '}
                    <strong className="text-[#0d5c68]">{historiaClinica.prescripciones.length}</strong>
                  </p>
                  <p className="text-xs text-slate-600">
                    Fármacos distintos:{' '}
                    <strong className="text-slate-900">{historiaClinica.medicamentosRecetados.length}</strong>
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Bloque 2: Antecedentes Farmacológicos y Medicamentos Recetados */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Columna Izquierda (1 col): Resumen de Medicamentos Recetados / Antecedentes */}
            <div className="bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4 self-start">
              <div className="flex items-center justify-between pb-3 border-b border-slate-100">
                <div className="flex items-center gap-2">
                  <Pill className="w-4 h-4 text-[#0d5c68]" />
                  <h3 className="text-sm font-bold text-slate-800">
                    Antecedentes Farmacológicos
                  </h3>
                </div>
                <span className="px-2 py-0.5 rounded-full text-[11px] font-bold bg-teal-50 text-[#0d5c68] border border-teal-100">
                  {historiaClinica.medicamentosRecetados.length}
                </span>
              </div>

              <p className="text-xs text-slate-500 leading-relaxed">
                Resumen consolidado de medicamentos e indicaciones recetados previamente al paciente en su historia clínica:
              </p>

              {historiaClinica.medicamentosRecetados.length === 0 ? (
                <div className="p-6 text-center rounded-xl bg-slate-50 border border-dashed border-slate-200">
                  <Pill className="w-6 h-6 text-slate-300 mx-auto mb-1.5" />
                  <p className="text-xs font-semibold text-slate-600">
                    Sin antecedentes farmacológicos
                  </p>
                  <p className="text-[11px] text-slate-400 mt-0.5">
                    El paciente aún no registra medicamentos recetados en su expediente.
                  </p>
                </div>
              ) : (
                <div className="flex flex-wrap gap-2">
                  {historiaClinica.medicamentosRecetados.map((med, idx) => (
                    <div
                      key={`${med}-${idx}`}
                      className="px-3 py-2 rounded-xl bg-teal-50/70 border border-teal-200/80 text-xs font-semibold text-[#094852] flex items-start gap-2 w-full"
                    >
                      <Pill className="w-3.5 h-3.5 text-[#0d5c68] shrink-0 mt-0.5" />
                      <span className="leading-snug">{med}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Columna Derecha (2 cols): Historial Cronológico de Prescripciones Médicas */}
            <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-slate-100">
                <div className="flex items-center gap-2">
                  <FileText className="w-4 h-4 text-[#0d5c68]" />
                  <h3 className="text-sm font-bold text-slate-800">
                    Registro Histórico de Prescripciones ({historiaClinica.prescripciones.length})
                  </h3>
                </div>

                {historiaClinica.prescripciones.length > 0 && (
                  <div className="relative w-full sm:w-64">
                    <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" />
                    <input
                      type="text"
                      value={filtroPrescripcion}
                      onChange={(e) => setFiltroPrescripcion(e.target.value)}
                      placeholder="Filtrar por medicamento o médico..."
                      className="w-full pl-8 pr-7 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-700 placeholder:text-slate-400 focus:outline-none focus:border-[#0d5c68]"
                    />
                    {filtroPrescripcion && (
                      <button
                        type="button"
                        onClick={() => setFiltroPrescripcion('')}
                        className="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 cursor-pointer"
                      >
                        <X className="w-3.5 h-3.5" />
                      </button>
                    )}
                  </div>
                )}
              </div>

              {historiaClinica.prescripciones.length === 0 ? (
                <div className="p-10 text-center bg-slate-50/60 rounded-2xl border border-dashed border-slate-200 space-y-2">
                  <FileText className="w-9 h-9 text-slate-300 mx-auto" />
                  <h4 className="text-xs sm:text-sm font-bold text-slate-700">
                    Historia clínica sin prescripciones registradas
                  </h4>
                  <p className="text-xs text-slate-500 max-w-md mx-auto">
                    El expediente clínico #{historiaClinica.idHistoriaClinica} de{' '}
                    <strong>{historiaClinica.nombrePaciente}</strong> está activo, pero todavía no se han emitido recetas médicas.
                  </p>
                  <div className="pt-2">
                    <button
                      type="button"
                      onClick={() =>
                        navigate(`/medico/prescripciones?pacienteId=${historiaClinica.idPaciente}`)
                      }
                      className="inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold transition-colors cursor-pointer"
                    >
                      <Plus className="w-3.5 h-3.5" />
                      <span>Registrar primera prescripción</span>
                    </button>
                  </div>
                </div>
              ) : prescripcionesFiltradas.length === 0 ? (
                <div className="p-8 text-center bg-slate-50/50 rounded-xl border border-slate-200/80">
                  <p className="text-xs font-semibold text-slate-600">
                    No hay prescripciones que coincidan con «{filtroPrescripcion}».
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {prescripcionesFiltradas.map((prescripcion) => (
                    <div
                      key={prescripcion.idPrescripcion}
                      className="p-4 sm:p-5 rounded-xl border border-slate-200/90 bg-white hover:border-[#0d5c68]/40 transition-all shadow-2xs space-y-3"
                    >
                      {/* Cabecera de la prescripción: Fecha y Número */}
                      <div className="flex flex-wrap items-center justify-between gap-2">
                        <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-slate-600 bg-slate-100 px-2.5 py-1 rounded-lg">
                          <Clock className="w-3.5 h-3.5 text-[#0d5c68]" />
                          Fecha: {formatearFechaHora(prescripcion.fecha)}
                        </span>

                        <span className="px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-teal-50 text-[#0d5c68] border border-teal-200/70 uppercase tracking-wider">
                          Prescripción #{prescripcion.idPrescripcion}
                        </span>
                      </div>

                      {/* Cuerpo: Medicamento Indicado */}
                      <div className="p-3.5 rounded-xl bg-slate-50/90 border border-slate-200/60">
                        <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">
                          Medicamento / Indicación Médica
                        </p>
                        <p className="text-xs sm:text-sm font-bold text-slate-900 leading-relaxed">
                          {prescripcion.medicamento}
                        </p>
                      </div>

                      {/* Pie: Médico Responsable */}
                      <div className="flex flex-wrap items-center justify-between gap-2 pt-1 text-xs text-slate-600">
                        <div className="flex items-center gap-2">
                          <div className="w-6 h-6 rounded-full bg-[#0d5c68]/10 text-[#0d5c68] flex items-center justify-center shrink-0">
                            <Stethoscope className="w-3.5 h-3.5" />
                          </div>
                          <span>
                            Médico responsable:{' '}
                            <strong className="text-slate-800">
                              {prescripcion.nombreMedico || `Médico ID #${prescripcion.idMedico}`}
                            </strong>
                          </span>
                        </div>

                        {prescripcion.matriculaMedico && (
                          <span className="px-2.5 py-0.5 rounded-md bg-slate-100 text-slate-700 font-semibold text-[11px]">
                            Matrícula: {prescripcion.matriculaMedico}
                          </span>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
