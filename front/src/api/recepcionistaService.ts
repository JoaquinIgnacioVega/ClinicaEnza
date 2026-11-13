import { apiClient } from './client';
import type { PacienteDto, TurnoDto } from '../types/medico';

export interface CrearPacienteRequest {
  nombre: string;
  apellido: string;
  dni: string;
  fechaNacimiento: string;
  mail: string;
  obraSocial: string;
  peso: number;
  altura: number;
}

export type ModificarPacienteRequest = Omit<CrearPacienteRequest, 'dni' | 'fechaNacimiento'>;

export interface CrearTurnoRequest {
  idPaciente: number;
  idMedico: number;
  horaInicio: string;
}

export interface FiltrosTurnos {
  fecha?: string;
  idPaciente?: number;
  idMedico?: number;
  estado?: number;
}

export const recepcionistaService = {
  obtenerTurnos(filtros: FiltrosTurnos = {}): Promise<TurnoDto[]> {
    const params = new URLSearchParams();
    if (filtros.fecha) params.set('fecha', filtros.fecha);
    if (filtros.idPaciente !== undefined) params.set('idPaciente', String(filtros.idPaciente));
    if (filtros.idMedico !== undefined) params.set('idMedico', String(filtros.idMedico));
    if (filtros.estado !== undefined) params.set('estado', String(filtros.estado));
    const query = params.toString() ? `?${params.toString()}` : '';
    return apiClient<TurnoDto[]>(`/api/turnos${query}`);
  },

  obtenerTurnoPorId(idTurno: number): Promise<TurnoDto> {
    return apiClient<TurnoDto>(`/api/turnos/${idTurno}`);
  },

  crearTurno(request: CrearTurnoRequest): Promise<TurnoDto> {
    return apiClient<TurnoDto>('/api/turnos', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  reagendarTurno(idTurno: number, nuevaHoraInicio: string): Promise<TurnoDto> {
    return apiClient<TurnoDto>(`/api/turnos/${idTurno}/reagendar`, {
      method: 'PUT',
      body: JSON.stringify({ nuevaHoraInicio }),
    });
  },

  cancelarTurno(idTurno: number): Promise<void> {
    return apiClient<void>(`/api/turnos/${idTurno}/cancelar`, { method: 'PUT' });
  },

  obtenerPacientes(filtro?: string): Promise<PacienteDto[]> {
    const query = filtro?.trim() ? `?filtro=${encodeURIComponent(filtro.trim())}` : '';
    return apiClient<PacienteDto[]>(`/api/pacientes${query}`);
  },

  crearPaciente(request: CrearPacienteRequest): Promise<PacienteDto> {
    return apiClient<PacienteDto>('/api/pacientes', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  modificarPaciente(idPaciente: number, request: ModificarPacienteRequest): Promise<PacienteDto> {
    return apiClient<PacienteDto>(`/api/pacientes/${idPaciente}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
  },
};
