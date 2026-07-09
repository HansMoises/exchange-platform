import { isAxiosError } from 'axios';
import type { ApiResponse } from '../types/api.types';

// Extrae el mensaje del contrato de error estandar (API.md SS2.3).
export function obtenerMensajeError(
  error: unknown,
  mensajePorDefecto = 'Ocurrio un error. Intenta nuevamente.',
): string {
  if (isAxiosError<ApiResponse<unknown>>(error)) {
    const data = error.response?.data;
    if (data?.errors?.length) return data.errors[0];
    if (data?.message) return data.message;
  }
  return mensajePorDefecto;
}
