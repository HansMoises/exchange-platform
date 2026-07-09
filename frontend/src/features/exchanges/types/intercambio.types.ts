export type EstadoIntercambio =
  'Pendiente' | 'Aceptado' | 'PendienteConfirmacion' | 'Completado' | 'Rechazado' | 'Cancelado';

// Refleja el IntercambioDto real del backend (Exchanges/DTOs/IntercambioDto.cs).
export interface IntercambioDto {
  id: string;
  objetoSolicitadoId: string;
  objetoSolicitadoTitulo: string;
  objetoOfrecidoId: string;
  objetoOfrecidoTitulo: string;
  usuarioSolicitanteId: string;
  usuarioSolicitanteNombres: string;
  usuarioPropietarioId: string;
  usuarioPropietarioNombres: string;
  estado: EstadoIntercambio;
  mensajeInicial: string | null;
  confirmacionSolicitante: boolean;
  confirmacionPropietario: boolean;
  fechaAceptacion: string | null;
  fechaCompletado: string | null;
  creadoEn: string;
}

export interface CrearIntercambioDto {
  objetoSolicitadoId: string;
  objetoOfrecidoId: string;
  mensajeInicial?: string;
}
