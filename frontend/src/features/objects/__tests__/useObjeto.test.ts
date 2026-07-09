import { renderHook, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useObjeto } from '../hooks/useObjeto';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('useObjeto', () => {
  it('no pide nada si no hay id', () => {
    const { result } = renderHook(() => useObjeto(undefined));

    expect(result.current.objeto).toBeNull();
    expect(result.current.isLoading).toBe(true);
  });

  it('carga el objeto correctamente', async () => {
    server.use(http.get('*/objects/obj-1', () => respuestaOk({ id: 'obj-1', titulo: 'Bicicleta' })));

    const { result } = renderHook(() => useObjeto('obj-1'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.objeto?.titulo).toBe('Bicicleta');
    expect(result.current.error).toBe(false);
  });

  it('marca error si la peticion falla', async () => {
    server.use(http.get('*/objects/obj-inexistente', () => HttpResponse.json({}, { status: 404 })));

    const { result } = renderHook(() => useObjeto('obj-inexistente'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.error).toBe(true);
  });
});
