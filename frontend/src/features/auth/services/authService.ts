import { apiClient } from '../../../services/apiClient';
import { useAuthStore } from '../../../stores/authStore';
import type { ApiResponse } from '../../../types/api.types';
import type { LoginResponseDto } from '../../../types/usuario.types';
import type {
  IniciarSesionDto,
  OlvidarPasswordDto,
  RegistrarUsuarioDto,
  RestablecerPasswordDto,
} from '../types/auth.types';

export const authService = {
  registrar: (dto: RegistrarUsuarioDto) => apiClient.post<ApiResponse<{ id: string }>>('/auth/register', dto),

  iniciarSesion: (dto: IniciarSesionDto) => apiClient.post<ApiResponse<LoginResponseDto>>('/auth/login', dto),

  olvidarPassword: (dto: OlvidarPasswordDto) => apiClient.post<ApiResponse<null>>('/auth/forgot-password', dto),

  restablecerPassword: (dto: RestablecerPasswordDto) => apiClient.post<ApiResponse<null>>('/auth/reset-password', dto),

  // Revoca el refresh token en el backend y limpia el estado local (Seguridad.md SS4).
  cerrarSesion: async () => {
    const refreshToken = useAuthStore.getState().refreshToken;
    try {
      if (refreshToken) {
        await apiClient.post<ApiResponse<null>>('/auth/logout', { refreshToken });
      }
    } finally {
      useAuthStore.getState().logout();
    }
  },
};
