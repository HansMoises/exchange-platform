export interface DepartamentoDto {
  id: number;
  ubigeo: string;
  nombre: string;
}

export interface ProvinciaDto {
  id: number;
  ubigeo: string;
  nombre: string;
  departamentoId: number;
}

export interface DistritoDto {
  id: number;
  ubigeo: string;
  nombre: string;
  provinciaId: number;
}

export interface CategoriaDto {
  id: number;
  nombre: string;
  descripcion: string;
  icono: string;
}
