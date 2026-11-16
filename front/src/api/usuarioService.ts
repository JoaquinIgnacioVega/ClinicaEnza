import { apiClient } from './client';
import type { CrearUsuarioRequest, ModificarUsuarioRequest, Usuario } from '../types/usuario';

export const usuarioService = {
  async obtenerTodos(soloActivos?: boolean): Promise<Usuario[]> {
    const query = soloActivos === undefined ? '' : `?soloActivos=${soloActivos}`;
    const result = await apiClient<{ value?: Usuario[] } | Usuario[]>(`/api/usuarios${query}`);
    return Array.isArray(result) ? result : result.value ?? [];
  },

  crear(request: CrearUsuarioRequest): Promise<Usuario> {
    return apiClient<Usuario>('/api/usuarios', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  modificar(idUsuario: number, request: ModificarUsuarioRequest): Promise<Usuario> {
    return apiClient<Usuario>(`/api/usuarios/${idUsuario}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
  },

  async darDeBaja(idUsuario: number): Promise<void> {
    await apiClient(`/api/usuarios/${idUsuario}/baja`, { method: 'PUT' });
  },
};
