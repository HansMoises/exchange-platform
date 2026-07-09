export type CondicionFisica = 'Nuevo' | 'Bueno' | 'Regular';

export type EstadoObjeto = 'Disponible' | 'Reservado' | 'Intercambiado' | 'Suspendido' | 'Eliminado';

export interface ImagenObjetoDto {
  id: string;
  url: string;
  orden: number;
}

// Refleja el ObjetoDto real del backend (Objects/DTOs/ObjetoDto.cs), no el
// ejemplo de API.md SS6 (que documenta "estadoObjeto"/"createdAt"/"propietario"
// anidado y no coincide con la implementacion real).
export interface ObjetoDto {
  id: string;
  titulo: string;
  descripcion: string;
  categoriaId: number;
  categoriaNombre: string;
  usuarioId: string;
  usuarioNombres: string;
  usuarioCalificacion: number;
  estado: EstadoObjeto;
  condicionFisica: CondicionFisica;
  departamentoId: number;
  provinciaId: number;
  distritoId: number;
  imagenes: ImagenObjetoDto[];
  creadoEn: string;
}

export interface CrearObjetoDto {
  titulo: string;
  descripcion: string;
  categoriaId: number;
  condicionFisica: CondicionFisica;
  departamentoId: number;
  provinciaId: number;
  distritoId: number;
  imagenesUrl: string[];
}

export interface ActualizarObjetoDto {
  titulo: string;
  descripcion: string;
  categoriaId: number;
  condicionFisica: CondicionFisica;
  departamentoId: number;
  provinciaId: number;
  distritoId: number;
}

// Solo los filtros que el backend real soporta hoy (ObjectsController.ObtenerObjetos):
// API.md documenta ademas estadoObjeto/sortBy/sortOrder, pero no estan implementados.
export interface ObjetosFiltroParams {
  search?: string;
  categoriaId?: number;
  departamentoId?: number;
  provinciaId?: number;
  distritoId?: number;
  pageNumber?: number;
  pageSize?: number;
}
