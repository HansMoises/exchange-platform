import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { NotificacionDto } from '../types/notificacion.types';

// GET /notifications devuelve un array plano (List<NotificacionDto>), no
// PagedResult pese a lo documentado en API.md SS10.
export const notificacionService = {
  listar: () => apiClient.get<ApiResponse<NotificacionDto[]>>('/notifications'),

  marcarLeida: (id: string) => apiClient.patch<ApiResponse<null>>(`/notifications/${id}/read`),

  marcarTodasLeidas: () => apiClient.patch<ApiResponse<null>>('/notifications/read-all'),
};
