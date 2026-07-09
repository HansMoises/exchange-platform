export interface DashboardStatsDto {
  totalUsuarios: number;
  totalObjetos: number;
  intercambiosCompletados: number;
  intercambiosPendientes: number;
  reportesPendientes: number;
  usuariosActivos30d: number;
}

export interface AdminUsuarioDto {
  id: string;
  nombres: string;
  apellidos: string;
  email: string;
  rolId: number;
  isActive: boolean;
  calificacionPromedio: number;
  totalIntercambios: number;
  createdAt: string;
}

// estado/estadoReporte llegan como numero (indice del enum de C#) porque
// AdminController arma estos endpoints con objetos anonimos que exponen el
// enum tal cual, y el backend no configura JsonStringEnumConverter (a
// diferencia de ObjetoDto/IntercambioDto, que sí hacen .ToString()).
export interface AdminObjetoDto {
  id: string;
  titulo: string;
  estado: number;
  categoriaId: number;
  usuarioId: string;
  createdAt: string;
}

export interface AdminReporteDto {
  id: string;
  reportanteId: string;
  entidadTipo: string;
  entidadId: string;
  motivo: string;
  descripcion: string | null;
  estadoReporte: number;
  createdAt: string;
}

export interface AuditLogDto {
  id: string;
  usuarioId: string;
  accion: string;
  entidadTipo: string;
  entidadId: string | null;
  resultado: string;
  ipAddress: string | null;
  ocurridoEn: string;
}

export interface CategoriaAdminDto {
  id: number;
  nombre: string;
  descripcion: string | null;
  icono: string | null;
  isActive: boolean;
}

// Mapeo fijo segun el seed real de roles (RolConfiguration.cs); no existe
// un endpoint GET /roles para obtenerlo dinamicamente.
export const ROLES_POR_ID: Record<number, string> = {
  1: 'Administrador',
  2: 'Moderador',
  3: 'Usuario',
};

// Orden real de los enums de C# (Domain/Enums), usado para mapear el indice
// numerico que devuelve el backend a la etiqueta de texto.
export const ESTADOS_OBJETO_POR_INDICE = [
  'Disponible',
  'Reservado',
  'Intercambiado',
  'Suspendido',
  'Eliminado',
] as const;

export const ESTADOS_REPORTE_POR_INDICE = ['Pendiente', 'EnRevision', 'Resuelto', 'Descartado'] as const;
