export type EntidadTipoReporte = 'Objeto' | 'Usuario';

export type MotivoReporte = 'ContenidoInapropiado' | 'Fraude' | 'Spam' | 'InformacionFalsa' | 'Otro';

export interface CrearReporteDto {
  entidadTipo: EntidadTipoReporte;
  entidadId: string;
  motivo: MotivoReporte;
  descripcion?: string;
}
