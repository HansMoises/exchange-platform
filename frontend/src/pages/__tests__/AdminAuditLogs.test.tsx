import { render, screen } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import AdminAuditLogs from '../AdminAuditLogs';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function pagedResult(items: unknown[]) {
  return {
    items,
    pageNumber: 1,
    pageSize: 50,
    totalRecords: items.length,
    totalPages: 1,
    hasPrevious: false,
    hasNext: false,
  };
}

describe('AdminAuditLogs', () => {
  it('muestra el estado vacio cuando no hay registros', async () => {
    server.use(http.get('*/admin/audit-logs', () => respuestaOk(pagedResult([]))));
    render(<AdminAuditLogs />);

    expect(await screen.findByText('No hay registros de auditoria.')).toBeInTheDocument();
  });

  it('muestra el estado de error con boton de reintentar', async () => {
    server.use(http.get('*/admin/audit-logs', () => HttpResponse.error()));
    render(<AdminAuditLogs />);

    expect(await screen.findByText('No pudimos cargar la auditoria.')).toBeInTheDocument();
  });

  it('muestra el estado vacio si la respuesta no trae datos', async () => {
    server.use(http.get('*/admin/audit-logs', () => respuestaOk(null)));
    render(<AdminAuditLogs />);

    expect(await screen.findByText('No hay registros de auditoria.')).toBeInTheDocument();
  });

  it('lista los registros de auditoria', async () => {
    server.use(
      http.get('*/admin/audit-logs', () =>
        respuestaOk(
          pagedResult([
            {
              id: 'log-1',
              usuarioId: 'u1',
              accion: 'Login',
              entidadTipo: 'Usuario',
              entidadId: null,
              resultado: 'Exitoso',
              ipAddress: '127.0.0.1',
              ocurridoEn: '2026-01-01T00:00:00Z',
            },
          ]),
        ),
      ),
    );

    render(<AdminAuditLogs />);

    expect(await screen.findByText('Login')).toBeInTheDocument();
    expect(screen.getByText('Exitoso')).toBeInTheDocument();
    expect(screen.getByText('127.0.0.1')).toBeInTheDocument();
  });

  it('muestra un guion cuando no hay IP registrada', async () => {
    server.use(
      http.get('*/admin/audit-logs', () =>
        respuestaOk(
          pagedResult([
            {
              id: 'log-2',
              usuarioId: 'u1',
              accion: 'CambioPassword',
              entidadTipo: 'Usuario',
              entidadId: null,
              resultado: 'Exitoso',
              ipAddress: null,
              ocurridoEn: '2026-01-01T00:00:00Z',
            },
          ]),
        ),
      ),
    );

    render(<AdminAuditLogs />);

    expect(await screen.findByText('-')).toBeInTheDocument();
  });
});
