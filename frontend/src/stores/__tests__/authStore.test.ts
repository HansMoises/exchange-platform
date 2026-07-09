import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import type { LoginResponseDto } from '../../types/usuario.types';
import { useAuthStore } from '../authStore';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

const respuestaLogin: LoginResponseDto = {
  accessToken: 'token-acceso',
  refreshToken: 'token-refresco',
  accessTokenExpiresAt: '2026-06-03T00:15:00Z',
  usuario: {
    id: 'u1',
    nombres: 'Juan',
    apellidos: 'Quispe',
    email: 'juan@example.com',
    rol: 'Usuario',
    fotoPerfil: null,
    calificacionPromedio: 0,
    totalIntercambios: 0,
  },
};

describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.setState({ usuario: null, accessToken: null, refreshToken: null, isAuthenticated: false });
  });

  it('login guarda usuario y tokens, y marca isAuthenticated', () => {
    useAuthStore.getState().login(respuestaLogin);

    const estado = useAuthStore.getState();
    expect(estado.isAuthenticated).toBe(true);
    expect(estado.usuario?.nombres).toBe('Juan');
    expect(estado.accessToken).toBe('token-acceso');
    expect(estado.refreshToken).toBe('token-refresco');
  });

  it('logout limpia toda la sesion', () => {
    useAuthStore.getState().login(respuestaLogin);
    useAuthStore.getState().logout();

    const estado = useAuthStore.getState();
    expect(estado.isAuthenticated).toBe(false);
    expect(estado.usuario).toBeNull();
    expect(estado.accessToken).toBeNull();
    expect(estado.refreshToken).toBeNull();
  });

  it('actualizarUsuario fusiona los cambios sobre el usuario actual', () => {
    useAuthStore.getState().login(respuestaLogin);

    useAuthStore.getState().actualizarUsuario({ nombres: 'Rosa', fotoPerfil: 'https://example.com/foto.jpg' });

    const usuario = useAuthStore.getState().usuario;
    expect(usuario?.nombres).toBe('Rosa');
    expect(usuario?.fotoPerfil).toBe('https://example.com/foto.jpg');
    expect(usuario?.apellidos).toBe('Quispe');
  });

  it('actualizarUsuario no hace nada si no hay sesion', () => {
    useAuthStore.getState().actualizarUsuario({ nombres: 'Rosa' });

    expect(useAuthStore.getState().usuario).toBeNull();
  });

  it('intentarRefresh devuelve false sin llamar a la API si no hay refreshToken', async () => {
    const resultado = await useAuthStore.getState().intentarRefresh();
    expect(resultado).toBe(false);
  });

  it('intentarRefresh actualiza los tokens cuando la API responde exitosamente', async () => {
    useAuthStore.getState().login(respuestaLogin);
    server.use(
      http.post('*/auth/refresh', () =>
        respuestaOk({
          accessToken: 'token-nuevo',
          refreshToken: 'refresco-nuevo',
          accessTokenExpiresAt: '2026-06-03T01:00:00Z',
        }),
      ),
    );

    const resultado = await useAuthStore.getState().intentarRefresh();

    expect(resultado).toBe(true);
    expect(useAuthStore.getState().accessToken).toBe('token-nuevo');
    expect(useAuthStore.getState().refreshToken).toBe('refresco-nuevo');
  });

  it('intentarRefresh devuelve false si la API falla', async () => {
    useAuthStore.getState().login(respuestaLogin);
    server.use(http.post('*/auth/refresh', () => HttpResponse.error()));

    const resultado = await useAuthStore.getState().intentarRefresh();

    expect(resultado).toBe(false);
  });

  it('intentarRefresh devuelve false si la respuesta no trae datos', async () => {
    useAuthStore.getState().login(respuestaLogin);
    server.use(http.post('*/auth/refresh', () => respuestaOk(null)));

    const resultado = await useAuthStore.getState().intentarRefresh();

    expect(resultado).toBe(false);
  });
});
