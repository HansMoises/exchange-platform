import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { CrearIntercambioDto, IntercambioDto } from '../types/intercambio.types';

// GET /exchanges devuelve un array plano (List<IntercambioDto>), no PagedResult,
// pese a que API.md SS7 documenta "lista paginada".
export const intercambioService = {
  crear: (dto: CrearIntercambioDto) => apiClient.post<ApiResponse<{ id: string }>>('/exchanges', dto),

  listar: (params: { pageNumber?: number; pageSize?: number }) =>
    apiClient.get<ApiResponse<IntercambioDto[]>>('/exchanges', { params }),

  obtener: (id: string) => apiClient.get<ApiResponse<IntercambioDto>>(`/exchanges/${id}`),

  aceptar: (id: string) => apiClient.patch<ApiResponse<null>>(`/exchanges/${id}/accept`),

  rechazar: (id: string) => apiClient.patch<ApiResponse<null>>(`/exchanges/${id}/reject`),

  confirmar: (id: string) => apiClient.patch<ApiResponse<IntercambioDto>>(`/exchanges/${id}/confirm`),

  cancelar: (id: string) => apiClient.patch<ApiResponse<null>>(`/exchanges/${id}/cancel`),
};
