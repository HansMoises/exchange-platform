import axios from 'axios';
import { describe, expect, it } from 'vitest';
import { obtenerMensajeError } from '../apiError';

describe('obtenerMensajeError', () => {
  it('devuelve el primer error del contrato de API cuando existe', () => {
    const error = new axios.AxiosError('Request failed', '422', undefined, undefined, {
      status: 422,
      data: { success: false, message: 'Error de validacion.', errors: ['El titulo es requerido.'], timestamp: '' },
      statusText: '',
      headers: {},
      config: {} as never,
    });

    expect(obtenerMensajeError(error)).toBe('El titulo es requerido.');
  });

  it('usa el message del contrato cuando no hay errores de campo', () => {
    const error = new axios.AxiosError('Request failed', '401', undefined, undefined, {
      status: 401,
      data: { success: false, message: 'Credenciales invalidas.', errors: null, timestamp: '' },
      statusText: '',
      headers: {},
      config: {} as never,
    });

    expect(obtenerMensajeError(error)).toBe('Credenciales invalidas.');
  });

  it('devuelve el mensaje por defecto cuando el error no es de axios', () => {
    expect(obtenerMensajeError(new Error('boom'), 'Mensaje por defecto.')).toBe('Mensaje por defecto.');
  });
});
