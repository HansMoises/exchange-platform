import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { CalificacionDto, CrearCalificacionDto } from '../types/calificacion.types';

export const calificacionService = {
  crear: (dto: CrearCalificacionDto) => apiClient.post<ApiResponse<{ id: string }>>('/ratings', dto),

  obtenerPorUsuario: (usuarioId: string) => apiClient.get<ApiResponse<CalificacionDto[]>>(`/ratings/user/${usuarioId}`),
};
