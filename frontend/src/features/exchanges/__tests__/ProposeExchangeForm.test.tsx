import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import type { ObjetoDto } from '../../objects/types/objeto.types';
import ProposeExchangeForm from '../components/ProposeExchangeForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function objetoDePrueba(overrides: Partial<ObjetoDto> = {}): ObjetoDto {
  return {
    id: 'obj-solicitado',
    titulo: 'Bicicleta rodado 26',
    descripcion: 'En buen estado',
    categoriaId: 6,
    categoriaNombre: 'Deportes',
    usuarioId: 'u2',
    usuarioNombres: 'Juan Perez',
    usuarioCalificacion: 4.5,
    estado: 'Disponible',
    condicionFisica: 'Bueno',
    departamentoId: 1,
    provinciaId: 1,
    distritoId: 1,
    imagenes: [],
    creadoEn: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

function renderForm(onExito = vi.fn()) {
  render(
    <MemoryRouter>
      <Routes>
        <Route path="/" element={<ProposeExchangeForm objetoSolicitado={objetoDePrueba()} onExito={onExito} />} />
        <Route path="/publish" element={<p>Publicar objeto</p>} />
      </Routes>
    </MemoryRouter>,
  );
  return onExito;
}

describe('ProposeExchangeForm', () => {
  it('invita a publicar un objeto si no tiene ninguno disponible', async () => {
    server.use(http.get('*/objects/me/available', () => respuestaOk([])));

    renderForm();

    expect(await screen.findByText('No tienes objetos disponibles para ofrecer.')).toBeInTheDocument();
  });

  it('muestra los objetos disponibles como opciones', async () => {
    server.use(
      http.get('*/objects/me/available', () =>
        respuestaOk([objetoDePrueba({ id: 'obj-propio', titulo: 'Libros de cocina' })]),
      ),
    );

    renderForm();

    expect(await screen.findByRole('option', { name: 'Libros de cocina' })).toBeInTheDocument();
  });

  it('advierte si se envia sin seleccionar un objeto', async () => {
    server.use(
      http.get('*/objects/me/available', () =>
        respuestaOk([objetoDePrueba({ id: 'obj-propio', titulo: 'Libros de cocina' })]),
      ),
    );

    const usuario = userEvent.setup();
    renderForm();

    await usuario.click(await screen.findByRole('button', { name: 'Enviar solicitud' }));

    expect(screen.getByRole('button', { name: 'Enviar solicitud' })).toBeInTheDocument();
  });

  it('envia la solicitud y llama a onExito con el id del intercambio', async () => {
    server.use(
      http.get('*/objects/me/available', () =>
        respuestaOk([objetoDePrueba({ id: 'obj-propio', titulo: 'Libros de cocina' })]),
      ),
      http.post('*/exchanges', () => respuestaOk({ id: 'nuevo-intercambio-id' })),
    );

    const usuario = userEvent.setup();
    const onExito = renderForm();

    await usuario.selectOptions(await screen.findByLabelText('Tu objeto a ofrecer'), 'obj-propio');
    await usuario.click(screen.getByRole('button', { name: 'Enviar solicitud' }));

    await vi.waitFor(() => expect(onExito).toHaveBeenCalledWith('nuevo-intercambio-id'));
  });

  it('invita a publicar un objeto si la carga de disponibles falla', async () => {
    server.use(http.get('*/objects/me/available', () => HttpResponse.error()));

    renderForm();

    expect(await screen.findByText('No tienes objetos disponibles para ofrecer.')).toBeInTheDocument();
  });

  it('navega a publicar al hacer click en el boton del estado vacio', async () => {
    server.use(http.get('*/objects/me/available', () => respuestaOk([])));

    const usuario = userEvent.setup();
    renderForm();

    await usuario.click(await screen.findByRole('button', { name: 'Publicar un objeto' }));

    expect(await screen.findByText('Publicar objeto')).toBeInTheDocument();
  });

  it('muestra un toast de error si el envio de la solicitud falla', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(
      http.get('*/objects/me/available', () =>
        respuestaOk([objetoDePrueba({ id: 'obj-propio', titulo: 'Libros de cocina' })]),
      ),
      http.post('*/exchanges', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    renderForm();

    await usuario.selectOptions(await screen.findByLabelText('Tu objeto a ofrecer'), 'obj-propio');
    await usuario.click(screen.getByRole('button', { name: 'Enviar solicitud' }));

    await vi.waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos enviar la solicitud.')).toBe(true);
    });
  });

  it('invita a publicar un objeto si la respuesta no trae datos', async () => {
    server.use(http.get('*/objects/me/available', () => respuestaOk(null)));

    renderForm();

    expect(await screen.findByText('No tienes objetos disponibles para ofrecer.')).toBeInTheDocument();
  });

  it('no llama a onExito si la respuesta del envio no trae id', async () => {
    server.use(
      http.get('*/objects/me/available', () =>
        respuestaOk([objetoDePrueba({ id: 'obj-propio', titulo: 'Libros de cocina' })]),
      ),
      http.post('*/exchanges', () => respuestaOk(null)),
    );

    const usuario = userEvent.setup();
    const onExito = renderForm();

    await usuario.selectOptions(await screen.findByLabelText('Tu objeto a ofrecer'), 'obj-propio');
    await usuario.click(screen.getByRole('button', { name: 'Enviar solicitud' }));

    await vi.waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'Solicitud de intercambio enviada exitosamente.')).toBe(
        true,
      );
    });
    expect(onExito).not.toHaveBeenCalled();
  });

  it('permite escribir un mensaje opcional', async () => {
    server.use(
      http.get('*/objects/me/available', () =>
        respuestaOk([objetoDePrueba({ id: 'obj-propio', titulo: 'Libros de cocina' })]),
      ),
    );

    const usuario = userEvent.setup();
    renderForm();

    const textarea = await screen.findByLabelText('Mensaje (opcional)');
    await usuario.type(textarea, 'Hola, me interesa tu objeto');

    expect(textarea).toHaveValue('Hola, me interesa tu objeto');
  });
});
