import { render, screen } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import AdminDashboard from '../AdminDashboard';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('AdminDashboard', () => {
  it('muestra el estado de error con boton de reintentar', async () => {
    server.use(http.get('*/admin/dashboard', () => HttpResponse.error()));
    render(<AdminDashboard />);

    expect(await screen.findByText('No pudimos cargar el dashboard.')).toBeInTheDocument();
  });

  it('muestra las estadisticas del dashboard', async () => {
    server.use(
      http.get('*/admin/dashboard', () =>
        respuestaOk({
          totalUsuarios: 10,
          totalObjetos: 25,
          intercambiosCompletados: 5,
          intercambiosPendientes: 2,
          reportesPendientes: 1,
          usuariosActivos30d: 8,
        }),
      ),
    );

    render(<AdminDashboard />);

    expect(await screen.findByText('Usuarios totales')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument();
    expect(screen.getByText('25')).toBeInTheDocument();
  });
});
