import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import { useToastStore } from '../../stores/toastStore';
import AdminUsers from '../AdminUsers';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function pagedResult(items: unknown[], overrides: Record<string, unknown> = {}) {
  return {
    items,
    pageNumber: 1,
    pageSize: 20,
    totalRecords: items.length,
    totalPages: 1,
    hasPrevious: false,
    hasNext: false,
    ...overrides,
  };
}

function usuarioDePrueba(overrides: Record<string, unknown> = {}) {
  return {
    id: 'u1',
    nombres: 'Rosa',
    apellidos: 'Quispe',
    email: 'rosa@example.com',
    rolId: 3,
    isActive: true,
    calificacionPromedio: 4.5,
    totalIntercambios: 2,
    createdAt: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

describe('AdminUsers', () => {
  it('muestra el estado vacio cuando no hay usuarios', async () => {
    server.use(http.get('*/admin/users', () => respuestaOk(pagedResult([]))));
    render(<AdminUsers />);

    expect(await screen.findByText('No se encontraron usuarios.')).toBeInTheDocument();
  });

  it('muestra el estado de error con boton de reintentar', async () => {
    server.use(http.get('*/admin/users', () => HttpResponse.error()));
    render(<AdminUsers />);

    expect(await screen.findByText('No pudimos cargar los usuarios.')).toBeInTheDocument();
  });

  it('muestra el estado vacio si la respuesta no trae datos', async () => {
    server.use(http.get('*/admin/users', () => respuestaOk(null)));
    render(<AdminUsers />);

    expect(await screen.findByText('No se encontraron usuarios.')).toBeInTheDocument();
  });

  it('lista los usuarios con su rol y estado', async () => {
    server.use(http.get('*/admin/users', () => respuestaOk(pagedResult([usuarioDePrueba()]))));
    render(<AdminUsers />);

    expect(await screen.findByText('Rosa Quispe')).toBeInTheDocument();
    expect(screen.getByText('Usuario')).toBeInTheDocument();
    expect(screen.getByText('Activo')).toBeInTheDocument();
  });

  it('muestra el id del rol cuando no esta en el mapa conocido', async () => {
    server.use(http.get('*/admin/users', () => respuestaOk(pagedResult([usuarioDePrueba({ rolId: 99 })]))));
    render(<AdminUsers />);

    expect(await screen.findByText('99')).toBeInTheDocument();
  });

  it('busca por nombre o correo y reinicia a la pagina 1', async () => {
    let ultimaBusqueda: string | null = null;
    server.use(
      http.get('*/admin/users', ({ request }) => {
        const url = new URL(request.url);
        ultimaBusqueda = url.searchParams.get('search');
        return respuestaOk(pagedResult([]));
      }),
    );

    const usuario = userEvent.setup();
    render(<AdminUsers />);

    await screen.findByText('No se encontraron usuarios.');
    await usuario.type(screen.getByLabelText('Buscar por nombre o correo'), 'rosa');
    await usuario.click(screen.getByRole('button', { name: 'Buscar' }));

    await waitFor(() => expect(ultimaBusqueda).toBe('rosa'));
  });

  it('desactiva un usuario activo y recarga la lista', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/users', () => {
        vecesConsultado += 1;
        return respuestaOk(pagedResult([usuarioDePrueba()]));
      }),
      http.patch('*/admin/users/u1/deactivate', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminUsers />);

    await usuario.click(await screen.findByRole('button', { name: 'Desactivar' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('activa un usuario inactivo y recarga la lista', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/users', () => {
        vecesConsultado += 1;
        return respuestaOk(pagedResult([usuarioDePrueba({ isActive: false })]));
      }),
      http.patch('*/admin/users/u1/activate', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminUsers />);

    await usuario.click(await screen.findByRole('button', { name: 'Activar' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('muestra un toast de error si la actualizacion del usuario falla', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(
      http.get('*/admin/users', () => respuestaOk(pagedResult([usuarioDePrueba()]))),
      http.patch('*/admin/users/u1/deactivate', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    render(<AdminUsers />);

    await usuario.click(await screen.findByRole('button', { name: 'Desactivar' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos actualizar el usuario.')).toBe(
        true,
      );
    });
  });
});
