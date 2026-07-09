export interface NotificacionDto {
  id: string;
  tipo: string;
  titulo: string;
  mensaje: string;
  isLeida: boolean;
  entidadTipo: string | null;
  entidadId: string | null;
  creadaEn: string;
}
