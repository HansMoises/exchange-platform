import { renderHook, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useCategorias, useDepartamentos, useDistritos, useProvincias } from '../hooks/useGeoSelects';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('useDepartamentos', () => {
  it('carga los departamentos al montar', async () => {
    server.use(http.get('*/geo/departamentos', () => respuestaOk([{ id: 1, ubigeo: '05', nombre: 'Ayacucho' }])));

    const { result } = renderHook(() => useDepartamentos());

    await waitFor(() => expect(result.current).toHaveLength(1));
    expect(result.current[0].nombre).toBe('Ayacucho');
  });

  it('devuelve una lista vacia si la peticion falla', async () => {
    server.use(http.get('*/geo/departamentos', () => HttpResponse.error()));

    const { result } = renderHook(() => useDepartamentos());

    await waitFor(() => expect(result.current).toEqual([]));
  });

  it('devuelve una lista vacia si la respuesta no trae datos', async () => {
    server.use(http.get('*/geo/departamentos', () => respuestaOk(null)));

    const { result } = renderHook(() => useDepartamentos());

    await waitFor(() => expect(result.current).toEqual([]));
  });
});

describe('useProvincias', () => {
  it('no pide nada si no hay departamentoId', () => {
    const { result } = renderHook(() => useProvincias(undefined));
    expect(result.current).toEqual([]);
  });

  it('carga las provincias del departamento indicado', async () => {
    server.use(
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
    );

    const { result } = renderHook(() => useProvincias(1));

    await waitFor(() => expect(result.current).toHaveLength(1));
    expect(result.current[0].nombre).toBe('Huamanga');
  });

  it('devuelve una lista vacia si la peticion falla', async () => {
    server.use(http.get('*/geo/provincias', () => HttpResponse.error()));

    const { result } = renderHook(() => useProvincias(1));

    await waitFor(() => expect(result.current).toEqual([]));
  });

  it('devuelve una lista vacia si la respuesta no trae datos', async () => {
    server.use(http.get('*/geo/provincias', () => respuestaOk(null)));

    const { result } = renderHook(() => useProvincias(1));

    await waitFor(() => expect(result.current).toEqual([]));
  });
});

describe('useDistritos', () => {
  it('no pide nada si no hay provinciaId', () => {
    const { result } = renderHook(() => useDistritos(undefined));
    expect(result.current).toEqual([]);
  });

  it('carga los distritos de la provincia indicada', async () => {
    server.use(
      http.get('*/geo/distritos', () => respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }])),
    );

    const { result } = renderHook(() => useDistritos(1));

    await waitFor(() => expect(result.current).toHaveLength(1));
    expect(result.current[0].nombre).toBe('Ayacucho');
  });

  it('devuelve una lista vacia si la peticion falla', async () => {
    server.use(http.get('*/geo/distritos', () => HttpResponse.error()));

    const { result } = renderHook(() => useDistritos(1));

    await waitFor(() => expect(result.current).toEqual([]));
  });

  it('devuelve una lista vacia si la respuesta no trae datos', async () => {
    server.use(http.get('*/geo/distritos', () => respuestaOk(null)));

    const { result } = renderHook(() => useDistritos(1));

    await waitFor(() => expect(result.current).toEqual([]));
  });
});

describe('useCategorias', () => {
  it('carga las categorias al montar', async () => {
    server.use(
      http.get('*/geo/categorias', () => respuestaOk([{ id: 6, nombre: 'Deportes', icono: 'dumbbell' }])),
    );

    const { result } = renderHook(() => useCategorias());

    await waitFor(() => expect(result.current).toHaveLength(1));
    expect(result.current[0].nombre).toBe('Deportes');
  });

  it('devuelve una lista vacia si la peticion falla', async () => {
    server.use(http.get('*/geo/categorias', () => HttpResponse.error()));

    const { result } = renderHook(() => useCategorias());

    await waitFor(() => expect(result.current).toEqual([]));
  });

  it('devuelve una lista vacia si la respuesta no trae datos', async () => {
    server.use(http.get('*/geo/categorias', () => respuestaOk(null)));

    const { result } = renderHook(() => useCategorias());

    await waitFor(() => expect(result.current).toEqual([]));
  });
});
