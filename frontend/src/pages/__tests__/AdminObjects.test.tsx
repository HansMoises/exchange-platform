import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import { useToastStore } from '../../stores/toastStore';
import AdminObjects from '../AdminObjects';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

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

function objetoDePrueba(overrides: Record<string, unknown> = {}) {
  return {
    id: 'obj-1',
    titulo: 'Bicicleta rodado 26',
    estado: 0,
    categoriaId: 6,
    usuarioId: 'u1',
    createdAt: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

describe('AdminObjects', () => {
  it('muestra el estado vacio cuando no hay objetos', async () => {
    server.use(http.get('*/admin/objects', () => respuestaOk(pagedResult([]))));
    render(<AdminObjects />);

    expect(await screen.findByText('No hay objetos publicados.')).toBeInTheDocument();
  });

  it('lista los objetos con su estado', async () => {
    server.use(http.get('*/admin/objects', () => respuestaOk(pagedResult([objetoDePrueba()]))));
    render(<AdminObjects />);

    expect(await screen.findByText('Bicicleta rodado 26')).toBeInTheDocument();
    expect(screen.getByText('Disponible')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Suspender' })).toBeInTheDocument();
  });

  it('suspende un objeto disponible y recarga', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/objects', () => {
        vecesConsultado += 1;
        return respuestaOk(pagedResult([objetoDePrueba()]));
      }),
      http.patch('*/admin/objects/obj-1/suspend', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminObjects />);

    await usuario.click(await screen.findByRole('button', { name: 'Suspender' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('muestra "Restaurar" para un objeto suspendido', async () => {
    server.use(http.get('*/admin/objects', () => respuestaOk(pagedResult([objetoDePrueba({ estado: 3 })]))));

    render(<AdminObjects />);

    expect(await screen.findByRole('button', { name: 'Restaurar' })).toBeInTheDocument();
  });

  it('muestra el estado de error con boton de reintentar', async () => {
    server.use(http.get('*/admin/objects', () => HttpResponse.error()));
    render(<AdminObjects />);

    expect(await screen.findByText('No pudimos cargar los objetos.')).toBeInTheDocument();
  });

  it('muestra el estado vacio si la respuesta no trae datos', async () => {
    server.use(http.get('*/admin/objects', () => respuestaOk(null)));
    render(<AdminObjects />);

    expect(await screen.findByText('No hay objetos publicados.')).toBeInTheDocument();
  });

  it('restaura un objeto suspendido y recarga', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/objects', () => {
        vecesConsultado += 1;
        return respuestaOk(pagedResult([objetoDePrueba({ estado: 3 })]));
      }),
      http.patch('*/admin/objects/obj-1/restore', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminObjects />);

    await usuario.click(await screen.findByRole('button', { name: 'Restaurar' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('muestra un toast de error si la actualizacion del objeto falla', async () => {
    server.use(
      http.get('*/admin/objects', () => respuestaOk(pagedResult([objetoDePrueba()]))),
      http.patch('*/admin/objects/obj-1/suspend', () => HttpResponse.error()),
    );

    useToastStore.setState({ toasts: [] });
    const usuario = userEvent.setup();
    render(<AdminObjects />);

    await usuario.click(await screen.findByRole('button', { name: 'Suspender' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos actualizar el objeto.')).toBe(
        true,
      );
    });
  });
});
