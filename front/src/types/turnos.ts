export type EstadoTurno = 'Pendiente' | 'Realizado' | 'Cancelado';
export type FiltroEstadoAgenda = 'Todos' | EstadoTurno;

export interface Turno {
  idTurno: number;
  horaInicio: string;
  horaFin: string;
  estado: EstadoTurno | number;
  estadoDescripcion?: string;
  idPaciente: number;
  nombrePaciente?: string | null;
  dniPaciente?: string | null;
  obraSocialPaciente?: string | null;
  edadPaciente?: number | null;
  idMedico: number;
  nombreMedico?: string | null;
  idConsultorio: number;
  numeroConsultorio?: string | null;
  ubicacionConsultorio?: string | null;
}

export interface CancelarTurnoResponse {
  mensaje: string;
  turno: Turno;
}

export interface TurnoAgendaMedico {
  idTurno: number;
  horaInicio: string;
  horaFin: string;
  estado: number | EstadoTurno;
  estadoDescripcion: string;
  idPaciente: number;
  nombreCompletoPaciente: string;
  dniPaciente: string;
  obraSocialPaciente: string;
  edadPaciente: number;
  idConsultorio: number;
  ubicacionConsultorio: string;
}

export interface MarcarTurnoRealizadoRequest {
  medicamentoRecetado?: string | null;
}

