import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import { useAuthStore } from '../../stores/authStore';
import { apiClient } from '../apiClient';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function respuesta401() {
  return HttpResponse.json(
    { success: false, message: 'No autorizado.', data: null, errors: null, timestamp: '2026-01-01T00:00:00Z' },
    { status: 401 },
  );
}

describe('apiClient', () => {
  beforeEach(() => {
    useAuthStore.setState({
      usuario: {
        id: 'u1',
        nombres: 'Rosa',
        apellidos: 'Quispe',
        email: 'rosa@example.com',
        rol: 'Usuario',
        fotoPerfil: null,
        calificacionPromedio: 0,
        totalIntercambios: 0,
      },
      accessToken: 'token-viejo',
      refreshToken: 'refresh-valido',
      isAuthenticated: true,
    });
  });

  it('adjunta el access token en las peticiones', async () => {
    let autorizacionRecibida: string | null = null;
    server.use(
      http.get('*/perfil-protegido', ({ request }) => {
        autorizacionRecibida = request.headers.get('Authorization');
        return respuestaOk({ ok: true });
      }),
    );

    await apiClient.get('/perfil-protegido');

    expect(autorizacionRecibida).toBe('Bearer token-viejo');
  });

  it('reintenta la peticion tras renovar el token exitosamente en un 401', async () => {
    let intentos = 0;
    server.use(
      http.get('*/protegido', () => {
        intentos += 1;
        return intentos === 1 ? respuesta401() : respuestaOk({ ok: true });
      }),
      http.post('*/auth/refresh', () =>
        respuestaOk({
          accessToken: 'token-nuevo',
          refreshToken: 'refresh-nuevo',
          accessTokenExpiresAt: '2026-01-01T01:00:00Z',
        }),
      ),
    );

    const { data } = await apiClient.get('/protegido');

    expect(data.data).toEqual({ ok: true });
    expect(intentos).toBe(2);
    expect(useAuthStore.getState().accessToken).toBe('token-nuevo');
  });

  it('cierra la sesion si la renovacion del token falla tras un 401', async () => {
    server.use(
      http.get('*/protegido', () => respuesta401()),
      http.post('*/auth/refresh', () => HttpResponse.error()),
    );

    await expect(apiClient.get('/protegido')).rejects.toBeTruthy();

    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it('no intenta renovar el token para un 401 en una ruta de auth', async () => {
    let vecesLlamadoRefresh = 0;
    server.use(
      http.post('*/auth/login', () => respuesta401()),
      http.post('*/auth/refresh', () => {
        vecesLlamadoRefresh += 1;
        return respuestaOk({ accessToken: 'x', refreshToken: 'y', accessTokenExpiresAt: '' });
      }),
    );

    await expect(apiClient.post('/auth/login', {})).rejects.toBeTruthy();

    expect(vecesLlamadoRefresh).toBe(0);
  });
});
