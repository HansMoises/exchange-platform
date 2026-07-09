import { apiClient } from '../../../services/apiClient';
import type { ApiResponse, PagedResult } from '../../../types/api.types';
import type {
  AdminObjetoDto,
  AdminReporteDto,
  AdminUsuarioDto,
  AuditLogDto,
  CategoriaAdminDto,
  DashboardStatsDto,
} from '../types/admin.types';

interface CategoriaRequest {
  nombre: string;
  descripcion?: string;
  icono?: string;
}

interface PaginacionParams {
  pageNumber?: number;
  pageSize?: number;
}

export const adminService = {
  obtenerDashboard: () => apiClient.get<ApiResponse<DashboardStatsDto>>('/admin/dashboard'),

  obtenerUsuarios: (params: PaginacionParams & { search?: string }) =>
    apiClient.get<ApiResponse<PagedResult<AdminUsuarioDto>>>('/admin/users', { params }),

  activarUsuario: (id: string) => apiClient.patch<ApiResponse<null>>(`/admin/users/${id}/activate`),

  desactivarUsuario: (id: string) => apiClient.patch<ApiResponse<null>>(`/admin/users/${id}/deactivate`),

  obtenerObjetos: (params: PaginacionParams) =>
    apiClient.get<ApiResponse<PagedResult<AdminObjetoDto>>>('/admin/objects', { params }),

  suspenderObjeto: (id: string) => apiClient.patch<ApiResponse<null>>(`/admin/objects/${id}/suspend`),

  restaurarObjeto: (id: string) => apiClient.patch<ApiResponse<null>>(`/admin/objects/${id}/restore`),

  obtenerReportes: (params: PaginacionParams) =>
    apiClient.get<ApiResponse<PagedResult<AdminReporteDto>>>('/admin/reports', { params }),

  resolverReporte: (id: string) => apiClient.patch<ApiResponse<null>>(`/admin/reports/${id}/resolve`),

  descartarReporte: (id: string) => apiClient.patch<ApiResponse<null>>(`/admin/reports/${id}/discard`),

  obtenerAuditLogs: (params: PaginacionParams) =>
    apiClient.get<ApiResponse<PagedResult<AuditLogDto>>>('/admin/audit-logs', { params }),

  obtenerCategorias: () => apiClient.get<ApiResponse<CategoriaAdminDto[]>>('/admin/categories'),

  crearCategoria: (datos: CategoriaRequest) => apiClient.post<ApiResponse<{ id: number }>>('/admin/categories', datos),

  actualizarCategoria: (id: number, datos: CategoriaRequest) =>
    apiClient.put<ApiResponse<null>>(`/admin/categories/${id}`, datos),

  activarCategoria: (id: number) => apiClient.patch<ApiResponse<null>>(`/admin/categories/${id}/activate`),

  desactivarCategoria: (id: number) => apiClient.patch<ApiResponse<null>>(`/admin/categories/${id}/deactivate`),
};
