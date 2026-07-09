import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useAuthStore } from '../../../stores/authStore';
import { useFavoritosStore } from '../../../stores/favoritosStore';
import FavoriteButton from '../components/FavoriteButton';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('FavoriteButton', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
    useFavoritosStore.setState({ ids: new Set(), cargado: false });
  });

  it('no renderiza nada si el usuario no tiene sesion', () => {
    const { container } = render(<FavoriteButton objetoId="obj-1" />);
    expect(container).toBeEmptyDOMElement();
  });

  it('carga los favoritos y muestra el boton cuando hay sesion', async () => {
    useAuthStore.setState({ isAuthenticated: true });
    server.use(http.get('*/favorites', () => respuestaOk([])));

    render(<FavoriteButton objetoId="obj-1" />);

    expect(await screen.findByRole('button', { name: 'Agregar a favoritos' })).toBeInTheDocument();
  });

  it('muestra "Quitar de favoritos" cuando el objeto ya es favorito', () => {
    useAuthStore.setState({ isAuthenticated: true });
    useFavoritosStore.setState({ ids: new Set(['obj-1']), cargado: true });

    render(<FavoriteButton objetoId="obj-1" />);

    expect(screen.getByRole('button', { name: 'Quitar de favoritos' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('alterna el favorito al hacer click', async () => {
    useAuthStore.setState({ isAuthenticated: true });
    useFavoritosStore.setState({ ids: new Set(), cargado: true });
    server.use(http.post('*/favorites', () => respuestaOk({ id: 'fav-1' })));

    const usuario = userEvent.setup();
    render(<FavoriteButton objetoId="obj-1" />);

    await usuario.click(screen.getByRole('button', { name: 'Agregar a favoritos' }));

    expect(await screen.findByRole('button', { name: 'Quitar de favoritos' })).toBeInTheDocument();
  });
});
