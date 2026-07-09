import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { ActualizarPerfilDto, CambiarPasswordDto, PerfilUsuarioDto } from '../types/perfil.types';

export const usuarioService = {
  obtenerMiPerfil: () => apiClient.get<ApiResponse<PerfilUsuarioDto>>('/users/me'),

  actualizarPerfil: (dto: ActualizarPerfilDto) => apiClient.put<ApiResponse<null>>('/users/me', dto),

  cambiarPassword: (dto: CambiarPasswordDto) => apiClient.patch<ApiResponse<null>>('/users/me/password', dto),

  actualizarFoto: (archivo: File) => {
    const formData = new FormData();
    formData.append('archivo', archivo);
    return apiClient.patch<ApiResponse<{ url: string }>>('/users/me/photo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
};
