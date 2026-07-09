import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useAuthStore } from '../../../stores/authStore';
import { authService } from '../services/authService';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('authService.cerrarSesion', () => {
  beforeEach(() => {
    useAuthStore.setState({
      isAuthenticated: true,
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
      accessToken: 'token',
      refreshToken: 'refresh-token',
    });
  });

  it('revoca el refresh token en el backend y limpia el estado local', async () => {
    let cuerpoEnviado: unknown = null;
    server.use(
      http.post('*/auth/logout', async ({ request }) => {
        cuerpoEnviado = await request.json();
        return respuestaOk(null);
      }),
    );

    await authService.cerrarSesion();

    expect(cuerpoEnviado).toEqual({ refreshToken: 'refresh-token' });
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it('limpia el estado local sin llamar al backend si no hay refresh token', async () => {
    useAuthStore.setState({ refreshToken: null });
    let seLlamo = false;
    server.use(
      http.post('*/auth/logout', () => {
        seLlamo = true;
        return respuestaOk(null);
      }),
    );

    await authService.cerrarSesion();

    expect(seLlamo).toBe(false);
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });
});
