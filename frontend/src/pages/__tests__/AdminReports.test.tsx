import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import { useToastStore } from '../../stores/toastStore';
import AdminReports from '../AdminReports';

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

function reporteDePrueba(overrides: Record<string, unknown> = {}) {
  return {
    id: 'rep-1',
    reportanteId: 'u1',
    entidadTipo: 'Objeto',
    entidadId: 'obj-1',
    motivo: 'Spam',
    descripcion: null,
    estadoReporte: 0,
    createdAt: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

describe('AdminReports', () => {
  it('muestra el estado vacio cuando no hay reportes', async () => {
    server.use(http.get('*/admin/reports', () => respuestaOk(pagedResult([]))));
    render(<AdminReports />);

    expect(await screen.findByText('No hay reportes.')).toBeInTheDocument();
  });

  it('muestra las acciones de resolver/descartar para un reporte pendiente', async () => {
    server.use(http.get('*/admin/reports', () => respuestaOk(pagedResult([reporteDePrueba()]))));
    render(<AdminReports />);

    expect(await screen.findByText('Spam')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Resolver' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Descartar' })).toBeInTheDocument();
  });

  it('no muestra acciones para un reporte ya resuelto', async () => {
    server.use(http.get('*/admin/reports', () => respuestaOk(pagedResult([reporteDePrueba({ estadoReporte: 2 })]))));
    render(<AdminReports />);

    await screen.findByText('Spam');
    expect(screen.queryByRole('button', { name: 'Resolver' })).not.toBeInTheDocument();
  });

  it('resuelve un reporte y recarga', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/reports', () => {
        vecesConsultado += 1;
        return respuestaOk(pagedResult([reporteDePrueba()]));
      }),
      http.patch('*/admin/reports/rep-1/resolve', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminReports />);

    await usuario.click(await screen.findByRole('button', { name: 'Resolver' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('descarta un reporte y recarga', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/reports', () => {
        vecesConsultado += 1;
        return respuestaOk(pagedResult([reporteDePrueba()]));
      }),
      http.patch('*/admin/reports/rep-1/discard', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminReports />);

    await usuario.click(await screen.findByRole('button', { name: 'Descartar' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('muestra el estado de error con boton de reintentar', async () => {
    server.use(http.get('*/admin/reports', () => HttpResponse.error()));
    render(<AdminReports />);

    expect(await screen.findByText('No pudimos cargar los reportes.')).toBeInTheDocument();
  });

  it('muestra el estado vacio si la respuesta no trae datos', async () => {
    server.use(http.get('*/admin/reports', () => respuestaOk(null)));
    render(<AdminReports />);

    expect(await screen.findByText('No hay reportes.')).toBeInTheDocument();
  });

  it('muestra un toast de error si resolver falla', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(
      http.get('*/admin/reports', () => respuestaOk(pagedResult([reporteDePrueba()]))),
      http.patch('*/admin/reports/rep-1/resolve', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    render(<AdminReports />);

    await usuario.click(await screen.findByRole('button', { name: 'Resolver' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos resolver el reporte.')).toBe(true);
    });
  });

  it('muestra un toast de error si descartar falla', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(
      http.get('*/admin/reports', () => respuestaOk(pagedResult([reporteDePrueba()]))),
      http.patch('*/admin/reports/rep-1/discard', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    render(<AdminReports />);

    await usuario.click(await screen.findByRole('button', { name: 'Descartar' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos descartar el reporte.')).toBe(
        true,
      );
    });
  });
});
