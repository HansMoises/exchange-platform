export interface CalificacionDto {
  id: string;
  intercambioId: string;
  calificadorId: string;
  calificadorNombres: string;
  calificadoId: string;
  puntuacion: number;
  comentario: string | null;
  creadoEn: string;
}

export interface CrearCalificacionDto {
  intercambioId: string;
  puntuacion: number;
  comentario?: string;
}
