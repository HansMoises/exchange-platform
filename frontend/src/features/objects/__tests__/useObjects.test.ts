import { renderHook, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useObjects } from '../hooks/useObjects';

function pagedResult(items: unknown[]) {
  return {
    items,
    pageNumber: 1,
    pageSize: 20,
    totalRecords: items.length,
    totalPages: 1,
    hasPrevious: false,
    hasNext: false,
  };
}

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('useObjects', () => {
  it('carga los objetos con los filtros indicados', async () => {
    server.use(http.get('*/objects', () => respuestaOk(pagedResult([{ id: 'obj-1', titulo: 'Bicicleta' }]))));

    const { result } = renderHook(() => useObjects({ pageNumber: 1, pageSize: 20 }));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.resultado?.items).toHaveLength(1);
    expect(result.current.error).toBe(false);
  });

  it('marca error si la peticion falla', async () => {
    server.use(http.get('*/objects', () => HttpResponse.error()));

    const { result } = renderHook(() => useObjects({ pageNumber: 1, pageSize: 20 }));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.error).toBe(true);
  });

  it('recargar vuelve a pedir los datos', async () => {
    let llamadas = 0;
    server.use(
      http.get('*/objects', () => {
        llamadas += 1;
        return respuestaOk(pagedResult([]));
      }),
    );

    const { result } = renderHook(() => useObjects({ pageNumber: 1, pageSize: 20 }));
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(llamadas).toBe(1);

    result.current.recargar();

    await waitFor(() => expect(llamadas).toBe(2));
  });
});
