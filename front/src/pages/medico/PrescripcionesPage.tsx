import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { medicoService } from '../../api/medicoService';
import type { PrescripcionMedicaDto } from '../../types/medico';
import {
  Pill,
  Search,
  Plus,
  Clock,
  User,
  CheckCircle2,
  AlertCircle,
  FileText,
  X,
} from 'lucide-react';

export const PrescripcionesPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const idMedico = user?.idMedico || 3;

  const initialId = Number(searchParams.get('pacienteId')) || 4;
  const [idPaciente, setIdPaciente] = useState<number>(initialId); // Joaco Vega (ID: 4 en la base de datos real)
  const [pacienteInput, setPacienteInput] = useState<string>(String(initialId));
  const [prescripciones, setPrescripciones] = useState<PrescripcionMedicaDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Formulario de emisión de receta
  const [medicamento, setMedicamento] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const cargarPrescripciones = async (pacienteId: number) => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
      const data = await medicoService.obtenerPrescripcionesPorPaciente(pacienteId);
      setPrescripciones(data);
    } catch {
      // Fallback si no tiene prescripciones cargadas aún
      setPrescripciones([]);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    cargarPrescripciones(idPaciente);
  }, [idPaciente]);

  const handleBuscarPaciente = (e: React.FormEvent) => {
    e.preventDefault();
    const idNum = parseInt(pacienteInput, 10);
    if (isNaN(idNum) || idNum <= 0) {
      setErrorMessage('Ingresá un ID de paciente válido (número mayor a 0).');
      return;
    }
    setIdPaciente(idNum);
    cargarPrescripciones(idNum);
  };

  const handleEmitirPrescripcion = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    const medTrimmed = medicamento.trim();
    if (!medTrimmed) {
      setErrorMessage('El texto del medicamento y la indicación no pueden estar vacíos.');
      return;
    }

    try {
      setIsSubmitting(true);
      const nueva = await medicoService.registrarPrescripcion(idPaciente, {
        idMedico,
        medicamento: medTrimmed,
      });

      setSuccessMessage('¡Prescripción médica registrada en la historia clínica del paciente!');
      setMedicamento('');
      setPrescripciones((prev) => [nueva, ...prev]);
    } catch (err: unknown) {
      const errObj = err as { error?: string };
      setErrorMessage(
        errObj?.error || 'No se pudo registrar la prescripción. Verificá los datos ingresados.'
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const formatearFecha = (isoStr: string) => {
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

  return (
    <div className="space-y-6 max-w-5xl mx-auto animate-fadeIn">
      {/* Encabezado */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold text-[#0d5c68] uppercase tracking-wider">
            <Pill className="w-4 h-4" />
            <span>Requerimiento Funcional 22</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight mt-1">
            Prescripciones Médicas
          </h1>
          <p className="text-xs sm:text-sm text-slate-500 mt-1">
            Emite recetas asociadas a la historia clínica del paciente y consulta indicaciones previas.
          </p>
        </div>

        <button
          type="button"
          onClick={() => navigate(`/medico/historia-clinica?pacienteId=${idPaciente}`)}
          className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-[#0d5c68] text-xs font-semibold shadow-2xs transition-all cursor-pointer self-start sm:self-auto"
        >
          <FileText className="w-4 h-4" />
          <span>Ver Historia Clínica Completa</span>
        </button>
      </div>

      {/* Alertas */}
      {errorMessage && (
        <div className="p-3.5 rounded-xl bg-red-50 border border-red-200 text-red-700 text-xs flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{errorMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => setErrorMessage(null)}
            className="text-red-500 hover:text-red-800 cursor-pointer"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {successMessage && (
        <div className="p-3.5 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-800 text-xs flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <CheckCircle2 className="w-4 h-4 shrink-0" />
            <span>{successMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => setSuccessMessage(null)}
            className="text-emerald-600 hover:text-emerald-900 cursor-pointer"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Selector de Paciente */}
      <div className="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-xs flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-teal-50 text-[#0d5c68] flex items-center justify-center shrink-0">
            <User className="w-5 h-5" />
          </div>
          <div>
            <h3 className="text-sm font-bold text-slate-800">
              Paciente seleccionado ID: #{idPaciente}
            </h3>
            <p className="text-xs text-slate-400">
              (Ejemplo: Paciente ID 4 corresponde a VEGA, Joaco en la base de datos)
            </p>
          </div>
        </div>

        <form onSubmit={handleBuscarPaciente} className="flex items-center gap-2">
          <input
            type="number"
            min="1"
            value={pacienteInput}
            onChange={(e) => setPacienteInput(e.target.value)}
            placeholder="ID de Paciente"
            className="w-32 px-3 py-2 text-xs rounded-xl border border-slate-200 text-slate-800 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68]"
          />
          <button
            type="submit"
            className="px-4 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold flex items-center gap-1.5 transition-colors cursor-pointer"
          >
            <Search className="w-3.5 h-3.5" />
            <span>Consultar</span>
          </button>
        </form>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
        {/* Formulario: Emitir Nueva Prescripción (2 cols) */}
        <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4">
          <div className="flex items-center gap-2 pb-2 border-b border-slate-100">
            <Plus className="w-4 h-4 text-[#0d5c68]" />
            <h3 className="text-sm font-bold text-slate-800">
              Emitir Nueva Prescripción
            </h3>
          </div>

          <form onSubmit={handleEmitirPrescripcion} className="space-y-4">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                Medicamento, posología y duración:
              </label>
              <textarea
                rows={4}
                value={medicamento}
                onChange={(e) => setMedicamento(e.target.value)}
                placeholder="Ej: Amoxicilina 500mg - 1 comprimido cada 8 horas durante 7 días."
                className="w-full p-3 rounded-xl border border-slate-200 text-xs text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/15 focus:border-[#0d5c68]"
                required
              />
            </div>

            <div className="p-3 rounded-xl bg-slate-50 text-[11px] text-slate-500 space-y-0.5">
              <p>
                <strong>Profesional:</strong> {user?.nombreCompleto || 'Dr. Juan Pérez'}
              </p>
              <p>
                <strong>Fecha:</strong> Se registrará automáticamente la fecha actual.
              </p>
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-2.5 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs font-semibold shadow-sm transition-colors cursor-pointer disabled:opacity-50 flex items-center justify-center gap-2"
            >
              <Pill className="w-4 h-4" />
              <span>{isSubmitting ? 'Registrando...' : 'Registrar en Historia Clínica'}</span>
            </button>
          </form>
        </div>

        {/* Historial de Prescripciones Anteriores (3 cols) */}
        <div className="lg:col-span-3 bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-slate-100">
            <div className="flex items-center gap-2">
              <FileText className="w-4 h-4 text-[#0d5c68]" />
              <h3 className="text-sm font-bold text-slate-800">
                Historial de Prescripciones ({prescripciones.length})
              </h3>
            </div>
            {isLoading && <span className="text-xs text-slate-400">Cargando...</span>}
          </div>

          {prescripciones.length === 0 ? (
            <div className="p-8 text-center bg-slate-50/50 rounded-xl border border-dashed border-slate-200">
              <Pill className="w-8 h-8 text-slate-300 mx-auto mb-1.5" />
              <p className="text-xs font-semibold text-slate-600">
                El paciente no registra prescripciones anteriores.
              </p>
              <p className="text-[11px] text-slate-400 mt-0.5">
                Usa el formulario para asentar la primera receta en su historia médica.
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {prescripciones.map((p) => (
                <div
                  key={p.idPrescripcion}
                  className="p-4 rounded-xl border border-slate-200/90 bg-white hover:border-[#0d5c68]/40 transition-all shadow-2xs space-y-2"
                >
                  <div className="flex items-start justify-between gap-3">
                    <p className="text-xs font-bold text-slate-900 leading-snug">
                      {p.medicamento}
                    </p>
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-teal-50 text-[#0d5c68] border border-teal-100 shrink-0">
                      Receta #{p.idPrescripcion}
                    </span>
                  </div>

                  <div className="flex flex-wrap items-center justify-between text-[11px] text-slate-500 pt-2 border-t border-slate-100">
                    <span className="flex items-center gap-1">
                      <Clock className="w-3.5 h-3.5 text-slate-400" />
                      {formatearFecha(p.fecha)}
                    </span>
                    <span className="font-medium text-slate-700">
                      {p.nombreMedico || 'Dr. Juan Pérez'}{' '}
                      {p.matriculaMedico ? `(${p.matriculaMedico})` : ''}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
