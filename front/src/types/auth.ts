export type RolUsuario = 'Administrador' | 'Recepcionista' | 'Medico';

export interface LoginCredentials {
  identificador: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  nombreCompleto: string;
  rol: number;
  rolNombre: string;
  idMedico: number | null;
}

export interface User {
  nombreCompleto: string;
  rol: number;
  rolNombre: string;
  idMedico: number | null;
}
