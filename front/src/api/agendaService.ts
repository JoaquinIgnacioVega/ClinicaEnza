import { apiClient } from './client';
import type {
  FiltroEstadoAgenda,
  MarcarTurnoRealizadoRequest,
  TurnoAgendaMedico,
} from '../types/turnos';

export interface ObtenerAgendaParams {
  idMedico: number;
  fecha?: string | null;
  estado?: FiltroEstadoAgenda;
  idMedicoSolicitante?: number;
}

export const agendaService = {
  obtenerAgendaMedico: async ({
    idMedico,
    fecha,
    estado,
    idMedicoSolicitante,
  }: ObtenerAgendaParams): Promise<TurnoAgendaMedico[]> => {
    const params = new URLSearchParams();
    if (fecha) {
      params.append('fecha', fecha);
    }
    if (estado && estado !== 'Todos') {
      params.append('estado', estado);
    }

    const queryString = params.toString();
    const endpoint = `/api/medicos/${idMedico}/turnos/agenda${queryString ? `?${queryString}` : ''}`;

    const headers: Record<string, string> = {};
    if (idMedicoSolicitante !== undefined) {
      headers['X-Id-Medico'] = String(idMedicoSolicitante);
    }

    return apiClient<TurnoAgendaMedico[]>(endpoint, {
      method: 'GET',
      headers,
    });
  },

  marcarTurnoRealizado: async (
    idTurno: number,
    medicamentoRecetado?: string | null
  ): Promise<void> => {
    const body: MarcarTurnoRealizadoRequest = {
      medicamentoRecetado: medicamentoRecetado?.trim() || null,
    };

    await apiClient<unknown>(`/api/turnos/${idTurno}/realizar`, {
      method: 'PUT',
      body: JSON.stringify(body),
    });
  },

  cancelarTurno: async (idTurno: number): Promise<void> => {
    await apiClient<unknown>(`/api/turnos/${idTurno}/cancelar`, {
      method: 'PUT',
    });
  },
};

