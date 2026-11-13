import { apiClient } from './client';
import type {
  MedicoDto,
  EspecialidadItem,
  HorarioAtencionDto,
  ConfigurarHorarioRequest,
  SlotDisponibleDto,
  ConsultorioDto,
  TurnoDto,
  PrescripcionMedicaDto,
  RegistrarPrescripcionRequest,
  PacienteDto,
  HistoriaClinicaDto,
} from '../types/medico';

export const medicoService = {
  // --- Médicos y Especialidades ---
  async obtenerMedicos(especialidad?: number): Promise<MedicoDto[]> {
    const query = especialidad !== undefined ? `?especialidad=${especialidad}` : '';
    return apiClient<MedicoDto[]>(`/api/medicos${query}`);
  },

  async obtenerEspecialidades(): Promise<EspecialidadItem[]> {
    return apiClient<EspecialidadItem[]>('/api/medicos/especialidades');
  },

  async obtenerMedicoPorId(id: number): Promise<MedicoDto> {
    return apiClient<MedicoDto>(`/api/medicos/${id}`);
  },

  async registrarMedico(command: Partial<MedicoDto>): Promise<MedicoDto> {
    return apiClient<MedicoDto>('/api/medicos', {
      method: 'POST',
      body: JSON.stringify(command),
    });
  },

  async modificarMedico(id: number, command: Partial<MedicoDto>): Promise<MedicoDto> {
    return apiClient<MedicoDto>(`/api/medicos/${id}`, {
      method: 'PUT',
      body: JSON.stringify(command),
    });
  },

  // --- Horarios y Consultorios ---
  async obtenerHorariosPorMedico(idMedico: number): Promise<HorarioAtencionDto[]> {
    return apiClient<HorarioAtencionDto[]>(`/api/medicos/${idMedico}/horarios`);
  },

  async configurarHorario(idMedico: number, data: ConfigurarHorarioRequest): Promise<HorarioAtencionDto> {
    return apiClient<HorarioAtencionDto>(`/api/medicos/${idMedico}/horarios`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  async obtenerDisponibilidad(idMedico: number, fecha: string): Promise<SlotDisponibleDto[]> {
    return apiClient<SlotDisponibleDto[]>(`/api/medicos/${idMedico}/disponibilidad?fecha=${fecha}`);
  },

  async asignarConsultorio(idHorario: number, idConsultorio: number): Promise<HorarioAtencionDto> {
    return apiClient<HorarioAtencionDto>(`/api/horarios/${idHorario}/consultorio`, {
      method: 'PUT',
      body: JSON.stringify({ idConsultorio }),
    });
  },

  async obtenerConsultorios(): Promise<ConsultorioDto[]> {
    return apiClient<ConsultorioDto[]>('/api/consultorios');
  },

  async modificarConsultorio(id: number, data: Pick<ConsultorioDto, 'numero' | 'piso' | 'sector'>): Promise<ConsultorioDto> {
    return apiClient<ConsultorioDto>(`/api/consultorios/${id}`, { method: 'PUT', body: JSON.stringify(data) });
  },

  async desactivarConsultorio(id: number): Promise<void> {
    return apiClient<void>(`/api/consultorios/${id}/desactivar`, { method: 'PATCH' });
  },

  // --- Turnos del Médico ---
  async obtenerTurnosPorMedico(idMedico: number, fecha?: string, estado?: number): Promise<TurnoDto[]> {
    const params = new URLSearchParams();
    if (fecha) params.append('fecha', fecha);
    if (estado !== undefined) params.append('estado', estado.toString());
    const query = params.toString() ? `?${params.toString()}` : '';
    return apiClient<TurnoDto[]>(`/api/turnos/medico/${idMedico}${query}`);
  },

  async marcarTurnoRealizado(idTurno: number, medicamentoRecetado?: string): Promise<TurnoDto> {
    return apiClient<TurnoDto>(`/api/turnos/${idTurno}/realizar`, {
      method: 'PUT',
      body: JSON.stringify({ medicamentoRecetado: medicamentoRecetado || '' }),
    });
  },

  async cancelarTurno(idTurno: number): Promise<void> {
    return apiClient<void>(`/api/turnos/${idTurno}/cancelar`, {
      method: 'PUT',
    });
  },

  // --- Pacientes e Historia Clínica (RF 25) ---
  async buscarPacientes(filtro?: string): Promise<PacienteDto[]> {
    const query = filtro && filtro.trim() ? `?filtro=${encodeURIComponent(filtro.trim())}` : '';
    return apiClient<PacienteDto[]>(`/api/pacientes${query}`);
  },

  async obtenerPacientePorId(idPaciente: number): Promise<PacienteDto> {
    return apiClient<PacienteDto>(`/api/pacientes/${idPaciente}`);
  },

  async obtenerHistoriaClinicaPorPaciente(idPaciente: number): Promise<HistoriaClinicaDto> {
    return apiClient<HistoriaClinicaDto>(`/api/pacientes/${idPaciente}/historia-clinica`);
  },

  // --- Prescripciones Médicas (RF 22) ---
  async registrarPrescripcion(
    idPaciente: number,
    request: RegistrarPrescripcionRequest
  ): Promise<PrescripcionMedicaDto> {
    return apiClient<PrescripcionMedicaDto>(`/api/pacientes/${idPaciente}/prescripciones`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  async obtenerPrescripcionesPorPaciente(idPaciente: number): Promise<PrescripcionMedicaDto[]> {
    return apiClient<PrescripcionMedicaDto[]>(`/api/pacientes/${idPaciente}/prescripciones`);
  },
};
