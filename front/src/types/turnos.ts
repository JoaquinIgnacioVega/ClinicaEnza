export type EstadoTurno = 'Pendiente' | 'Realizado' | 'Cancelado' | number;

export interface Turno {
  idTurno: number;
  horaInicio: string;
  horaFin: string;
  estado: EstadoTurno;
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

