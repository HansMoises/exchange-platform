import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { ObjetoDto } from '../../objects/types/objeto.types';

// GET /favorites devuelve un array plano de ObjetoDto (List<ObjetoDto>), no
// PagedResult pese a lo documentado en API.md SS11. Nota: el handler real no
// completa categoriaNombre/usuarioNombres/usuarioCalificacion en estos items.
export const favoritoService = {
  listar: () => apiClient.get<ApiResponse<ObjetoDto[]>>('/favorites'),

  agregar: (objetoId: string) => apiClient.post<ApiResponse<{ id: string }>>('/favorites', { objetoId }),

  quitar: (objetoId: string) => apiClient.delete<ApiResponse<null>>(`/favorites/${objetoId}`),
};
