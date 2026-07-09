import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { usuarioService } from '../services/usuarioService';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('usuarioService', () => {
  it('obtenerMiPerfil trae el perfil del usuario en sesion', async () => {
    server.use(http.get('*/users/me', () => respuestaOk({ id: 'u1', nombres: 'Rosa' })));

    const { data } = await usuarioService.obtenerMiPerfil();

    expect(data.data?.nombres).toBe('Rosa');
  });
});
