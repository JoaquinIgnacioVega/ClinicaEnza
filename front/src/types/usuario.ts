export type RolUsuario = 'Administrador' | 'Recepcionista' | 'Medico';

export interface Usuario {
  idUsuario: number;
  idPersona: number;
  nombre: string;
  apellido: string;
  dni: string;
  fechaNacimiento: string;
  mail: string;
  legajo: string;
  fechaInicio: string;
  rol: number;
  rolNombre: string;
  activo: boolean;
  idMedico: number | null;
}

export interface CrearUsuarioRequest {
  nombre: string;
  apellido: string;
  dni: string;
  fechaNacimiento: string;
  mail: string;
  legajo: string;
  password: string;
  rol: number;
  idMedico: number | null;
}

export interface ModificarUsuarioRequest {
  nombre: string;
  apellido: string;
  mail: string;
  rol: number;
  idMedico: number | null;
}
