import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { intercambioService } from '../services/intercambioService';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('intercambioService', () => {
  it('crear envia una solicitud de intercambio', async () => {
    server.use(http.post('*/exchanges', () => respuestaOk({ id: 'ex-1' })));

    const { data } = await intercambioService.crear({
      objetoSolicitadoId: 'obj-1',
      objetoOfrecidoId: 'obj-2',
      mensajeInicial: 'Hola',
    });

    expect(data.data?.id).toBe('ex-1');
  });

  it('listar obtiene los intercambios del usuario', async () => {
    server.use(http.get('*/exchanges', () => respuestaOk([])));

    const { data } = await intercambioService.listar({ pageNumber: 1, pageSize: 20 });

    expect(data.data).toEqual([]);
  });

  it('obtener trae un intercambio por id', async () => {
    server.use(http.get('*/exchanges/ex-1', () => respuestaOk({ id: 'ex-1' })));

    const { data } = await intercambioService.obtener('ex-1');

    expect(data.data?.id).toBe('ex-1');
  });

  it('aceptar acepta la solicitud', async () => {
    server.use(http.patch('*/exchanges/ex-1/accept', () => respuestaOk(null)));

    const { data } = await intercambioService.aceptar('ex-1');

    expect(data.success).toBe(true);
  });

  it('rechazar rechaza la solicitud', async () => {
    server.use(http.patch('*/exchanges/ex-1/reject', () => respuestaOk(null)));

    const { data } = await intercambioService.rechazar('ex-1');

    expect(data.success).toBe(true);
  });

  it('confirmar registra la confirmacion', async () => {
    server.use(http.patch('*/exchanges/ex-1/confirm', () => respuestaOk({ id: 'ex-1', estado: 'Completado' })));

    const { data } = await intercambioService.confirmar('ex-1');

    expect(data.data?.estado).toBe('Completado');
  });

  it('cancelar cancela el intercambio', async () => {
    server.use(http.patch('*/exchanges/ex-1/cancel', () => respuestaOk(null)));

    const { data } = await intercambioService.cancelar('ex-1');

    expect(data.success).toBe(true);
  });
});
