import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import type { NotificacionDto } from '../types/notificacion.types';
import NotificationBell from '../components/NotificationBell';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function notificacionDePrueba(overrides: Partial<NotificacionDto> = {}): NotificacionDto {
  return {
    id: 'n1',
    tipo: 'SolicitudRecibida',
    titulo: 'Nueva solicitud de intercambio',
    mensaje: 'Rosa quiere intercambiar por tu objeto.',
    isLeida: false,
    entidadTipo: 'Intercambio',
    entidadId: 'ex1',
    creadaEn: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

function renderBell() {
  return render(
    <MemoryRouter>
      <Routes>
        <Route path="/" element={<NotificationBell />} />
        <Route path="/exchanges/:id" element={<p>Detalle de intercambio</p>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('NotificationBell', () => {
  it('no muestra el contador cuando no hay notificaciones sin leer', async () => {
    server.use(http.get('*/notifications', () => respuestaOk([])));
    renderBell();

    await waitFor(() => expect(screen.getByRole('button', { name: 'Notificaciones' })).toBeInTheDocument());
  });

  it('muestra el contador de notificaciones sin leer', async () => {
    server.use(
      http.get('*/notifications', () => respuestaOk([notificacionDePrueba(), notificacionDePrueba({ id: 'n2' })])),
    );
    renderBell();

    expect(await screen.findByRole('button', { name: 'Notificaciones, 2 sin leer' })).toBeInTheDocument();
  });

  it('abre el listado al hacer click y muestra las notificaciones', async () => {
    server.use(http.get('*/notifications', () => respuestaOk([notificacionDePrueba()])));
    const usuario = userEvent.setup();
    renderBell();

    await usuario.click(await screen.findByRole('button', { name: /Notificaciones/ }));

    expect(screen.getByText('Nueva solicitud de intercambio')).toBeInTheDocument();
  });

  it('muestra un mensaje cuando no hay notificaciones', async () => {
    server.use(http.get('*/notifications', () => respuestaOk([])));
    const usuario = userEvent.setup();
    renderBell();

    await usuario.click(await screen.findByRole('button', { name: 'Notificaciones' }));

    expect(screen.getByText('No tienes notificaciones.')).toBeInTheDocument();
  });

  it('marca como leida y navega al hacer click en una notificacion de intercambio', async () => {
    let marcoLeida = false;
    server.use(
      http.get('*/notifications', () => respuestaOk([notificacionDePrueba()])),
      http.patch('*/notifications/n1/read', () => {
        marcoLeida = true;
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    renderBell();

    await usuario.click(await screen.findByRole('button', { name: /Notificaciones/ }));
    await usuario.click(screen.getByText('Nueva solicitud de intercambio'));

    expect(await screen.findByText('Detalle de intercambio')).toBeInTheDocument();
    expect(marcoLeida).toBe(true);
  });

  it('no marca leida una notificacion que ya estaba leida', async () => {
    let vecesMarcoLeida = 0;
    server.use(
      http.get('*/notifications', () =>
        respuestaOk([notificacionDePrueba({ isLeida: true, titulo: 'Ya leida', entidadTipo: null, entidadId: null })]),
      ),
      http.patch('*/notifications/n1/read', () => {
        vecesMarcoLeida += 1;
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    renderBell();

    await usuario.click(await screen.findByRole('button', { name: 'Notificaciones' }));
    await usuario.click(screen.getByText('Ya leida'));

    expect(vecesMarcoLeida).toBe(0);
  });

  it('deja la lista vacia si la respuesta no trae datos', async () => {
    server.use(http.get('*/notifications', () => respuestaOk(null)));
    renderBell();

    const usuario = userEvent.setup();
    await usuario.click(await screen.findByRole('button', { name: 'Notificaciones' }));

    expect(screen.getByText('No tienes notificaciones.')).toBeInTheDocument();
  });

  it('deja la lista vacia si la carga inicial falla', async () => {
    server.use(http.get('*/notifications', () => HttpResponse.error()));
    renderBell();

    await waitFor(() => expect(screen.getByRole('button', { name: 'Notificaciones' })).toBeInTheDocument());
  });

  it('ignora el error si marcar como leida falla', async () => {
    server.use(
      http.get('*/notifications', () => respuestaOk([notificacionDePrueba({ entidadTipo: null, entidadId: null })])),
      http.patch('*/notifications/n1/read', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    renderBell();

    await usuario.click(await screen.findByRole('button', { name: /Notificaciones/ }));
    await usuario.click(screen.getByText('Nueva solicitud de intercambio'));

    await waitFor(() => expect(screen.queryByText('Detalle de intercambio')).not.toBeInTheDocument());
  });

  it('marca todas como leidas al hacer click en el boton', async () => {
    let marcoTodas = false;
    server.use(
      http.get('*/notifications', () => respuestaOk([notificacionDePrueba()])),
      http.patch('*/notifications/read-all', () => {
        marcoTodas = true;
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    renderBell();

    await usuario.click(await screen.findByRole('button', { name: /Notificaciones/ }));
    await usuario.click(screen.getByRole('button', { name: 'Marcar todas leidas' }));

    await waitFor(() => expect(marcoTodas).toBe(true));
  });

  it('ignora el error si marcar todas como leidas falla', async () => {
    server.use(
      http.get('*/notifications', () => respuestaOk([notificacionDePrueba()])),
      http.patch('*/notifications/read-all', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    renderBell();

    await usuario.click(await screen.findByRole('button', { name: /Notificaciones/ }));
    await usuario.click(screen.getByRole('button', { name: 'Marcar todas leidas' }));

    expect(await screen.findByText('Nueva solicitud de intercambio')).toBeInTheDocument();
  });

  it('cierra el listado al hacer click fuera de el', async () => {
    server.use(http.get('*/notifications', () => respuestaOk([notificacionDePrueba()])));

    const usuario = userEvent.setup();
    const { container } = renderBell();

    await usuario.click(await screen.findByRole('button', { name: /Notificaciones/ }));
    expect(screen.getByText('Nueva solicitud de intercambio')).toBeInTheDocument();

    const fondo = container.querySelector('.fixed.inset-0') as HTMLElement;
    await usuario.click(fondo);

    expect(screen.queryByText('Nueva solicitud de intercambio')).not.toBeInTheDocument();
  });
});
