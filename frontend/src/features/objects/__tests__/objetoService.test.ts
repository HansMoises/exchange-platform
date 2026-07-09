import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { objetoService } from '../services/objetoService';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('objetoService', () => {
  it('obtenerMisObjetos trae los objetos del usuario', async () => {
    server.use(http.get('*/objects/me', () => respuestaOk([{ id: 'obj-1' }])));

    const { data } = await objetoService.obtenerMisObjetos();

    expect(data.data).toHaveLength(1);
  });

  it('eliminar borra un objeto por id', async () => {
    server.use(http.delete('*/objects/obj-1', () => respuestaOk(null)));

    const { data } = await objetoService.eliminar('obj-1');

    expect(data.success).toBe(true);
  });
});
