import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { CrearReporteDto } from '../types/reporte.types';

export const reporteService = {
  crear: (dto: CrearReporteDto) => apiClient.post<ApiResponse<{ id: string }>>('/reports', dto),
};
