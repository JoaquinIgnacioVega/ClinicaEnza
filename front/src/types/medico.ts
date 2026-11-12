export type EspecialidadNombre =
  | 'ClinicaMedica'
  | 'Pediatria'
  | 'Cardiologia'
  | 'Dermatologia'
  | 'Traumatologia'
  | 'Ginecologia'
  | 'Oftalmologia';

export interface EspecialidadItem {
  id: number;
  nombre: string;
}

export interface MedicoDto {
  idMedico: number;
  nombre: string;
  apellido: string;
  nombreCompleto: string;
  dni: string;
  fechaNacimiento: string;
  mail: string;
  matricula: string;
  especialidad: number;
  especialidadNombre: string;
  fechaInicio: string;
  fichaProfesional: string;
}

export interface HorarioAtencionDto {
  idHorarioAtencion: number;
  idMedico: number;
  nombreMedico: string;
  diaSemana: number; // 0: Domingo, 1: Lunes, ..., 6: Sábado
  horaDesde: string; // formato "hh:mm:ss" o "hh:mm"
  horaHasta: string;
  idConsultorio: number;
  numeroConsultorio: string;
}

export interface ConfigurarHorarioRequest {
  idMedico?: number;
  diaSemana: number;
  horaDesde: string;
  horaHasta: string;
  idConsultorio: number;
}

export interface SlotDisponibleDto {
  horaInicio: string;
  horaFin: string;
  idConsultorio: number;
  ubicacionConsultorio: string;
}

export interface ConsultorioDto {
  idConsultorio: number;
  numero: string;
  piso: number;
  sector: string;
  ubicacion: string;
  activo?: boolean;
}

export type EstadoTurno = 'Pendiente' | 'Confirmado' | 'Cancelado' | 'Atendido';

export interface TurnoDto {
  idTurno: number;
  horaInicio: string;
  horaFin: string;
  estado: number;
  estadoDescripcion: EstadoTurno | string;
  idPaciente: number;
  nombrePaciente?: string;
  dniPaciente?: string;
  obraSocialPaciente?: string;
  edadPaciente?: number;
  idMedico: number;
  nombreMedico?: string;
  idConsultorio: number;
  numeroConsultorio?: string;
  ubicacionConsultorio?: string;
}

export interface PrescripcionMedicaDto {
  idPrescripcion: number;
  idHistoriaClinica: number;
  idPaciente: number;
  fecha: string;
  medicamento: string;
  idMedico: number;
  nombreMedico: string;
  matriculaMedico: string;
}

export interface RegistrarPrescripcionRequest {
  idMedico: number;
  medicamento: string;
}

export interface MarcarTurnoRealizadoRequest {
  medicamentoRecetado?: string;
}

export interface PacienteDto {
  idPaciente: number;
  nombre: string;
  apellido: string;
  nombreCompleto: string;
  dni: string;
  fechaNacimiento: string;
  edad: number;
  mail: string;
  obraSocial: string;
  peso: number;
  altura: number;
  idHistoriaClinica: number | null;
}

export interface HistoriaClinicaDto {
  idHistoriaClinica: number;
  idPaciente: number;
  nombrePaciente: string;
  dniPaciente: string;
  mailPaciente: string;
  obraSocial?: string;
  fechaNacimiento?: string;
  edadPaciente: number;
  peso: number;
  altura: number;
  medicamentosRecetados: string[];
  prescripciones: PrescripcionMedicaDto[];
}
