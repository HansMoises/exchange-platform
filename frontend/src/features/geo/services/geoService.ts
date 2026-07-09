import { apiClient } from '../../../services/apiClient';
import type { ApiResponse } from '../../../types/api.types';
import type { CategoriaDto, DepartamentoDto, DistritoDto, ProvinciaDto } from '../types/geo.types';

// Datos maestros UBIGEO Peru y categorias, publicos para poblar selectores (API.md SS13).
export const geoService = {
  obtenerDepartamentos: () => apiClient.get<ApiResponse<DepartamentoDto[]>>('/geo/departamentos'),

  obtenerProvincias: (departamentoId: number) =>
    apiClient.get<ApiResponse<ProvinciaDto[]>>('/geo/provincias', { params: { departamentoId } }),

  obtenerDistritos: (provinciaId: number) =>
    apiClient.get<ApiResponse<DistritoDto[]>>('/geo/distritos', { params: { provinciaId } }),

  obtenerCategorias: () => apiClient.get<ApiResponse<CategoriaDto[]>>('/geo/categorias'),
};
