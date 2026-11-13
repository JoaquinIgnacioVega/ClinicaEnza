export type EstadoTurno = 'Pendiente' | 'Realizado' | 'Cancelado';
export type FiltroEstadoAgenda = 'Todos' | EstadoTurno;

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

