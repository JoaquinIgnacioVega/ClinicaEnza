import { apiClient } from './client';
import type { Turno, CancelarTurnoResponse } from '../types/turnos';
import type { PacienteDto } from '../types/medico';

const PACIENTE_TOKEN_KEY = 'clinica_enza_paciente_token';
const PACIENTE_DATA_KEY = 'clinica_enza_paciente_data';

export interface AutenticarPacienteResponse {
  token: string;
  paciente: PacienteDto;
}

const getPacienteAuthHeaders = (): Record<string, string> => {
  const pacienteToken = sessionStorage.getItem(PACIENTE_TOKEN_KEY);
  if (pacienteToken) {
    return { Authorization: `Bearer ${pacienteToken}` };
  }
  return {};
};

export const turnosService = {
  autenticarPaciente: async (
    dni: string,
    mail: string
  ): Promise<AutenticarPacienteResponse> => {
    const response = await apiClient<AutenticarPacienteResponse>(
      '/api/pacientes/autenticar',
      {
        method: 'POST',
        body: JSON.stringify({ dni: dni.trim(), mail: mail.trim() }),
      }
    );

    if (response?.token && response?.paciente) {
      sessionStorage.setItem(PACIENTE_TOKEN_KEY, response.token);
      sessionStorage.setItem(PACIENTE_DATA_KEY, JSON.stringify(response.paciente));
      localStorage.removeItem('clinica_enza_paciente_id');
    }

    return response;
  },

  obtenerSesionPaciente: (): AutenticarPacienteResponse | null => {
    try {
      const token = sessionStorage.getItem(PACIENTE_TOKEN_KEY);
      const rawPaciente = sessionStorage.getItem(PACIENTE_DATA_KEY);
      if (!token || !rawPaciente) return null;
      const paciente = JSON.parse(rawPaciente) as PacienteDto;
      if (!paciente || !paciente.idPaciente) return null;
      return { token, paciente };
    } catch {
      return null;
    }
  },

  cerrarSesionPaciente: (): void => {
    sessionStorage.removeItem(PACIENTE_TOKEN_KEY);
    sessionStorage.removeItem(PACIENTE_DATA_KEY);
    localStorage.removeItem('clinica_enza_paciente_id');
  },

  buscarPacientes: async (filtro?: string): Promise<PacienteDto[]> => {
    const query =
      filtro && filtro.trim() ? `?filtro=${encodeURIComponent(filtro.trim())}` : '';
    return apiClient<PacienteDto[]>(`/api/pacientes${query}`, {
      method: 'GET',
      headers: getPacienteAuthHeaders(),
    });
  },

  obtenerPacientePorId: async (idPaciente: number): Promise<PacienteDto> => {
    return apiClient<PacienteDto>(`/api/pacientes/${idPaciente}`, {
      method: 'GET',
      headers: getPacienteAuthHeaders(),
    });
  },

  obtenerTurnosPorPaciente: async (
    idPaciente: number,
    soloPendientes = true
  ): Promise<Turno[]> => {
    return apiClient<Turno[]>(
      `/api/pacientes/${idPaciente}/turnos?soloPendientes=${soloPendientes}`,
      {
        method: 'GET',
        headers: getPacienteAuthHeaders(),
      }
    );
  },

  obtenerTurnosPaciente: async (
    idPaciente: number,
    soloPendientes = true
  ): Promise<Turno[]> => {
    return turnosService.obtenerTurnosPorPaciente(idPaciente, soloPendientes);
  },

  cancelarTurnoPaciente: async (
    idPaciente: number,
    idTurno: number
  ): Promise<CancelarTurnoResponse> => {
    return apiClient<CancelarTurnoResponse>(
      `/api/pacientes/${idPaciente}/turnos/${idTurno}/cancelar`,
      {
        method: 'PUT',
        headers: getPacienteAuthHeaders(),
      }
    );
  },
};
