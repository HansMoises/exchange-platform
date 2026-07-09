import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import type { ObjetoDto } from '../types/objeto.types';
import ObjectForm from '../components/ObjectForm';

function renderObjectForm() {
  return render(
    <MemoryRouter>
      <ObjectForm />
    </MemoryRouter>,
  );
}

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

const respuestaVacia = () => HttpResponse.json({ success: true, message: 'OK', data: [], errors: null, timestamp: '' });

async function llenarCamposValidos(usuario: ReturnType<typeof userEvent.setup>) {
  await usuario.type(screen.getByLabelText('Titulo'), 'Bicicleta rodado 26');
  await usuario.type(screen.getByLabelText('Descripcion'), 'Descripcion con al menos veinte caracteres.');
  await usuario.selectOptions(screen.getByLabelText('Categoria'), '6');
  await usuario.selectOptions(screen.getByLabelText('Condicion fisica'), 'Bueno');
  await usuario.selectOptions(screen.getByLabelText('Departamento'), '1');
  await waitFor(() => expect(screen.getByLabelText('Provincia')).toBeEnabled());
  await usuario.selectOptions(screen.getByLabelText('Provincia'), '1');
  await waitFor(() => expect(screen.getByLabelText('Distrito')).toBeEnabled());
  await usuario.selectOptions(screen.getByLabelText('Distrito'), '1');
}

function objetoDePrueba(): ObjetoDto {
  return {
    id: 'obj-1',
    titulo: 'Bicicleta rodado 26',
    descripcion: 'Descripcion con al menos veinte caracteres.',
    categoriaId: 6,
    categoriaNombre: 'Deportes',
    usuarioId: 'u1',
    usuarioNombres: 'Rosa Quispe',
    usuarioCalificacion: 0,
    estado: 'Disponible',
    condicionFisica: 'Bueno',
    departamentoId: 1,
    provinciaId: 1,
    distritoId: 1,
    imagenes: [{ id: 'img-1', url: 'https://example.com/foto.jpg', orden: 1 }],
    creadoEn: '2026-01-01T00:00:00Z',
  };
}

describe('ObjectForm', () => {
  it('muestra los errores de validacion segun las reglas del backend al enviar vacio', async () => {
    server.use(http.get('*/geo/categorias', respuestaVacia), http.get('*/geo/departamentos', respuestaVacia));

    const usuario = userEvent.setup();
    renderObjectForm();

    await usuario.click(screen.getByRole('button', { name: 'Publicar objeto' }));

    expect(await screen.findByText('Minimo 5 caracteres.')).toBeInTheDocument();
    expect(screen.getByText('Minimo 20 caracteres.')).toBeInTheDocument();
    expect(screen.getByText('La categoria es requerida.')).toBeInTheDocument();
    expect(screen.getByText('El departamento es requerido.')).toBeInTheDocument();
    expect(screen.getByText('La provincia es requerida.')).toBeInTheDocument();
    expect(screen.getByText('El distrito es requerido.')).toBeInTheDocument();
  });

  it('deshabilita provincia y distrito hasta elegir el nivel anterior', async () => {
    server.use(http.get('*/geo/categorias', respuestaVacia), http.get('*/geo/departamentos', respuestaVacia));

    renderObjectForm();

    expect(await screen.findByLabelText('Provincia')).toBeDisabled();
    expect(screen.getByLabelText('Distrito')).toBeDisabled();
  });

  it('en modo edicion no muestra el cargador de imagenes y usa "Guardar cambios"', async () => {
    server.use(http.get('*/geo/categorias', respuestaVacia), http.get('*/geo/departamentos', respuestaVacia));

    render(
      <MemoryRouter>
        <ObjectForm objetoId="obj-1" valoresIniciales={objetoDePrueba()} />
      </MemoryRouter>,
    );

    expect(await screen.findByLabelText('Titulo')).toHaveValue('Bicicleta rodado 26');
    expect(screen.getByRole('button', { name: 'Guardar cambios' })).toBeInTheDocument();
    expect(screen.queryByText('Imagenes (1 a 5)')).not.toBeInTheDocument();
  });

  it('actualiza el objeto y navega al detalle en modo edicion', async () => {
    server.use(
      http.get('*/geo/categorias', respuestaVacia),
      http.get('*/geo/departamentos', respuestaVacia),
      http.put('*/objects/obj-1', () => respuestaOk(null)),
    );

    render(
      <MemoryRouter initialEntries={['/objects/obj-1/edit']}>
        <Routes>
          <Route
            path="/objects/:id/edit"
            element={<ObjectForm objetoId="obj-1" valoresIniciales={objetoDePrueba()} />}
          />
          <Route path="/objects/:id" element={<p>Detalle del objeto</p>} />
        </Routes>
      </MemoryRouter>,
    );

    const usuario = userEvent.setup();
    await usuario.click(await screen.findByRole('button', { name: 'Guardar cambios' }));

    expect(await screen.findByText('Detalle del objeto')).toBeInTheDocument();
  });

  it('bloquea el envio sin imagenes aunque los demas campos sean validos', async () => {
    server.use(
      http.get('*/geo/categorias', () => respuestaOk([{ id: 6, nombre: 'Deportes', descripcion: '', icono: '' }])),
      http.get('*/geo/departamentos', () => respuestaOk([{ id: 1, ubigeo: '05', nombre: 'Ayacucho' }])),
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
      http.get('*/geo/distritos', () =>
        respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }]),
      ),
    );

    const usuario = userEvent.setup();
    renderObjectForm();
    await waitFor(() => expect(screen.getByRole('option', { name: 'Deportes' })).toBeInTheDocument());

    await llenarCamposValidos(usuario);
    await usuario.click(screen.getByRole('button', { name: 'Publicar objeto' }));

    expect(await screen.findByText('Debes subir al menos 1 imagen.')).toBeInTheDocument();
  });

  it('publica exitosamente con una imagen subida y navega al detalle', async () => {
    server.use(
      http.get('*/geo/categorias', () => respuestaOk([{ id: 6, nombre: 'Deportes', descripcion: '', icono: '' }])),
      http.get('*/geo/departamentos', () => respuestaOk([{ id: 1, ubigeo: '05', nombre: 'Ayacucho' }])),
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
      http.get('*/geo/distritos', () =>
        respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }]),
      ),
      http.post('*/objects/images', () => respuestaOk({ url: 'https://example.com/subida.jpg' })),
      http.post('*/objects', () => respuestaOk({ id: 'nuevo-objeto-id' })),
    );

    const usuario = userEvent.setup();
    render(
      <MemoryRouter initialEntries={['/publish']}>
        <Routes>
          <Route path="/publish" element={<ObjectForm />} />
          <Route path="/objects/:id" element={<p>Detalle del objeto</p>} />
        </Routes>
      </MemoryRouter>,
    );
    await waitFor(() => expect(screen.getByRole('option', { name: 'Deportes' })).toBeInTheDocument());

    await llenarCamposValidos(usuario);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await usuario.upload(input, new File(['contenido'], 'foto.jpg', { type: 'image/jpeg' }));
    await screen.findByRole('button', { name: 'Quitar imagen' });

    await usuario.click(screen.getByRole('button', { name: 'Publicar objeto' }));

    expect(await screen.findByText('Detalle del objeto')).toBeInTheDocument();
  });

  it('muestra un toast de error si la publicacion falla', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(
      http.get('*/geo/categorias', () => respuestaOk([{ id: 6, nombre: 'Deportes', descripcion: '', icono: '' }])),
      http.get('*/geo/departamentos', () => respuestaOk([{ id: 1, ubigeo: '05', nombre: 'Ayacucho' }])),
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
      http.get('*/geo/distritos', () =>
        respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }]),
      ),
      http.post('*/objects/images', () => respuestaOk({ url: 'https://example.com/subida.jpg' })),
      http.post('*/objects', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    renderObjectForm();
    await waitFor(() => expect(screen.getByRole('option', { name: 'Deportes' })).toBeInTheDocument());

    await llenarCamposValidos(usuario);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await usuario.upload(input, new File(['contenido'], 'foto.jpg', { type: 'image/jpeg' }));
    await screen.findByRole('button', { name: 'Quitar imagen' });

    await usuario.click(screen.getByRole('button', { name: 'Publicar objeto' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos guardar el objeto.')).toBe(true);
    });
  });
});
