import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { EstadisticasPublicasDto } from '../types/stats.types';

export const statsService = {
  obtenerPublicas: () => apiClient.get<ApiResponse<EstadisticasPublicasDto>>('/stats/public'),
};
