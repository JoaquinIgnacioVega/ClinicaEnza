import { apiClient } from './client';
import type { Turno, CancelarTurnoResponse } from '../types/turnos';

export const turnosService = {
  obtenerTurnosPorPaciente: async (idPaciente: number, soloPendientes = true): Promise<Turno[]> => {
    return apiClient<Turno[]>(`/api/pacientes/${idPaciente}/turnos?soloPendientes=${soloPendientes}`, {
      method: 'GET',
    });
  },

  cancelarTurnoPaciente: async (idPaciente: number, idTurno: number): Promise<CancelarTurnoResponse> => {
    return apiClient<CancelarTurnoResponse>(`/api/pacientes/${idPaciente}/turnos/${idTurno}/cancelar`, {
      method: 'PUT',
    });
  },
};

