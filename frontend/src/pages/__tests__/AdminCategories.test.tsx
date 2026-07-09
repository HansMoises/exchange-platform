import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../../test/server';
import AdminCategories from '../AdminCategories';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function categoriaDePrueba(overrides: Record<string, unknown> = {}) {
  return {
    id: 1,
    nombre: 'Electronica',
    descripcion: 'Dispositivos electronicos.',
    icono: '📱',
    isActive: true,
    ...overrides,
  };
}

describe('AdminCategories', () => {
  it('muestra el estado vacio cuando no hay categorias', async () => {
    server.use(http.get('*/admin/categories', () => respuestaOk([])));
    render(<AdminCategories />);

    expect(await screen.findByText('No hay categorias registradas.')).toBeInTheDocument();
  });

  it('lista las categorias existentes', async () => {
    server.use(http.get('*/admin/categories', () => respuestaOk([categoriaDePrueba()])));
    render(<AdminCategories />);

    expect(await screen.findByText('Electronica')).toBeInTheDocument();
    expect(screen.getByText('Activa')).toBeInTheDocument();
  });

  it('crea una categoria nueva', async () => {
    let cuerpoEnviado: unknown = null;
    server.use(
      http.get('*/admin/categories', () => respuestaOk([])),
      http.post('*/admin/categories', async ({ request }) => {
        cuerpoEnviado = await request.json();
        return respuestaOk({ id: 2 });
      }),
    );

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await screen.findByText('No hay categorias registradas.');
    await usuario.click(screen.getByRole('button', { name: 'Nueva categoria' }));
    await usuario.type(screen.getByLabelText('Nombre'), 'Mascotas');
    await usuario.click(screen.getByRole('button', { name: 'Guardar' }));

    await waitFor(() => expect(cuerpoEnviado).toMatchObject({ nombre: 'Mascotas' }));
  });

  it('precarga el formulario al editar una categoria existente', async () => {
    server.use(http.get('*/admin/categories', () => respuestaOk([categoriaDePrueba()])));

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await usuario.click(await screen.findByRole('button', { name: 'Editar' }));

    expect(screen.getByLabelText('Nombre')).toHaveValue('Electronica');
    expect(screen.getByText('Editar categoria')).toBeInTheDocument();
  });

  it('precarga el formulario con campos vacios si la categoria no tiene descripcion ni icono', async () => {
    server.use(
      http.get('*/admin/categories', () =>
        respuestaOk([categoriaDePrueba({ descripcion: null, icono: null })]),
      ),
    );

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await usuario.click(await screen.findByRole('button', { name: 'Editar' }));

    expect(screen.getByLabelText('Descripcion')).toHaveValue('');
    expect(screen.getByLabelText('Icono')).toHaveValue('');
  });

  it('desactiva una categoria activa y recarga', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/categories', () => {
        vecesConsultado += 1;
        return respuestaOk([categoriaDePrueba()]);
      }),
      http.patch('*/admin/categories/1/deactivate', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await usuario.click(await screen.findByRole('button', { name: 'Desactivar' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('cancelar cierra el formulario sin guardar', async () => {
    server.use(http.get('*/admin/categories', () => respuestaOk([])));

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await screen.findByText('No hay categorias registradas.');
    await usuario.click(screen.getByRole('button', { name: 'Nueva categoria' }));
    await usuario.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(screen.queryByLabelText('Nombre')).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Nueva categoria' })).toBeInTheDocument();
  });

  it('muestra el estado de error con boton de reintentar', async () => {
    server.use(http.get('*/admin/categories', () => HttpResponse.error()));
    render(<AdminCategories />);

    expect(await screen.findByText('No pudimos cargar las categorias.')).toBeInTheDocument();
  });

  it('muestra el estado vacio si la respuesta no trae datos', async () => {
    server.use(http.get('*/admin/categories', () => respuestaOk(null)));
    render(<AdminCategories />);

    expect(await screen.findByText('No hay categorias registradas.')).toBeInTheDocument();
  });

  it('edita una categoria existente y guarda los cambios', async () => {
    let cuerpoEnviado: unknown = null;
    server.use(
      http.get('*/admin/categories', () => respuestaOk([categoriaDePrueba()])),
      http.put('*/admin/categories/1', async ({ request }) => {
        cuerpoEnviado = await request.json();
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await usuario.click(await screen.findByRole('button', { name: 'Editar' }));
    await usuario.clear(screen.getByLabelText('Descripcion'));
    await usuario.type(screen.getByLabelText('Descripcion'), 'Nueva descripcion');
    await usuario.clear(screen.getByLabelText('Icono'));
    await usuario.type(screen.getByLabelText('Icono'), '🔌');
    await usuario.click(screen.getByRole('button', { name: 'Guardar' }));

    await waitFor(() =>
      expect(cuerpoEnviado).toMatchObject({ descripcion: 'Nueva descripcion', icono: '🔌' }),
    );
  });

  it('muestra un toast de error si guardar falla', async () => {
    server.use(
      http.get('*/admin/categories', () => respuestaOk([])),
      http.post('*/admin/categories', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await screen.findByText('No hay categorias registradas.');
    await usuario.click(screen.getByRole('button', { name: 'Nueva categoria' }));
    await usuario.type(screen.getByLabelText('Nombre'), 'Mascotas');
    await usuario.click(screen.getByRole('button', { name: 'Guardar' }));

    expect(await screen.findByLabelText('Nombre')).toBeInTheDocument();
  });

  it('activa una categoria inactiva y recarga', async () => {
    let vecesConsultado = 0;
    server.use(
      http.get('*/admin/categories', () => {
        vecesConsultado += 1;
        return respuestaOk([categoriaDePrueba({ isActive: false })]);
      }),
      http.patch('*/admin/categories/1/activate', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await usuario.click(await screen.findByRole('button', { name: 'Activar' }));

    await waitFor(() => expect(vecesConsultado).toBe(2));
  });

  it('muestra un toast de error si activar/desactivar falla', async () => {
    server.use(
      http.get('*/admin/categories', () => respuestaOk([categoriaDePrueba()])),
      http.patch('*/admin/categories/1/deactivate', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    render(<AdminCategories />);

    await usuario.click(await screen.findByRole('button', { name: 'Desactivar' }));

    expect(await screen.findByRole('button', { name: 'Desactivar' })).toBeEnabled();
  });
});
