import { act, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import { useFavoritosStore } from '../favoritosStore';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('favoritosStore', () => {
  beforeEach(() => {
    useFavoritosStore.setState({ ids: new Set(), cargado: false });
  });

  it('cargar guarda los ids de los objetos favoritos', async () => {
    server.use(http.get('*/favorites', () => respuestaOk([{ id: 'obj-1' }, { id: 'obj-2' }])));

    await act(() => useFavoritosStore.getState().cargar());

    const { ids, cargado } = useFavoritosStore.getState();
    expect(cargado).toBe(true);
    expect(ids.has('obj-1')).toBe(true);
    expect(ids.has('obj-2')).toBe(true);
  });

  it('cargar deja los ids vacios si la respuesta no trae datos', async () => {
    server.use(http.get('*/favorites', () => respuestaOk(null)));

    await act(() => useFavoritosStore.getState().cargar());

    const { ids, cargado } = useFavoritosStore.getState();
    expect(cargado).toBe(true);
    expect(ids.size).toBe(0);
  });

  it('cargar marca cargado en true incluso si la peticion falla', async () => {
    server.use(http.get('*/favorites', () => HttpResponse.error()));

    await act(() => useFavoritosStore.getState().cargar());

    expect(useFavoritosStore.getState().cargado).toBe(true);
  });

  it('alternar agrega el id de forma optimista antes de que responda el servidor', async () => {
    server.use(http.post('*/favorites', () => respuestaOk({ id: 'fav-1' })));

    const promesa = act(() => useFavoritosStore.getState().alternar('obj-1'));

    expect(useFavoritosStore.getState().ids.has('obj-1')).toBe(true);
    await promesa;
  });

  it('alternar quita el id si ya era favorito', async () => {
    useFavoritosStore.setState({ ids: new Set(['obj-1']), cargado: true });
    server.use(http.delete('*/favorites/obj-1', () => respuestaOk(null)));

    await act(() => useFavoritosStore.getState().alternar('obj-1'));

    expect(useFavoritosStore.getState().ids.has('obj-1')).toBe(false);
  });

  it('revierte el cambio optimista si la peticion falla', async () => {
    server.use(http.post('*/favorites', () => HttpResponse.error()));

    await act(() => useFavoritosStore.getState().alternar('obj-1'));

    await waitFor(() => {
      expect(useFavoritosStore.getState().ids.has('obj-1')).toBe(false);
    });
  });
});
