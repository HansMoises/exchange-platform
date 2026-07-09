import { apiClient } from '../../../services/apiClient';
import type { ApiResponse, PagedResult } from '../../../types/api.types';
import type { ActualizarObjetoDto, CrearObjetoDto, ObjetoDto, ObjetosFiltroParams } from '../types/objeto.types';

export const objetoService = {
  listar: (params: ObjetosFiltroParams) => apiClient.get<ApiResponse<PagedResult<ObjetoDto>>>('/objects', { params }),

  obtener: (id: string) => apiClient.get<ApiResponse<ObjetoDto>>(`/objects/${id}`),

  obtenerMisObjetos: () => apiClient.get<ApiResponse<ObjetoDto[]>>('/objects/me'),

  obtenerMisObjetosDisponibles: () => apiClient.get<ApiResponse<ObjetoDto[]>>('/objects/me/available'),

  crear: (dto: CrearObjetoDto) => apiClient.post<ApiResponse<{ id: string }>>('/objects', dto),

  actualizar: (id: string, dto: ActualizarObjetoDto) => apiClient.put<ApiResponse<null>>(`/objects/${id}`, dto),

  eliminar: (id: string) => apiClient.delete<ApiResponse<null>>(`/objects/${id}`),
};
