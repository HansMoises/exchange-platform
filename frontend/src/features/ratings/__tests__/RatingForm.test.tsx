import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { server } from '../../../test/server';
import { useAuthStore } from '../../../stores/authStore';
import { useToastStore } from '../../../stores/toastStore';
import RatingForm from '../components/RatingForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('RatingForm', () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
    useAuthStore.setState({
      isAuthenticated: true,
      usuario: {
        id: 'u1',
        nombres: 'Rosa',
        apellidos: 'Quispe',
        email: 'rosa@example.com',
        rol: 'Usuario',
        fotoPerfil: null,
        calificacionPromedio: 0,
        totalIntercambios: 1,
      },
    });
  });

  it('muestra el formulario cuando el usuario no ha calificado aun', async () => {
    server.use(http.get('*/ratings/user/u2', () => respuestaOk([])));

    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    expect(await screen.findByText('Califica a la otra parte')).toBeInTheDocument();
  });

  it('muestra un mensaje si ya califico este intercambio', async () => {
    server.use(
      http.get('*/ratings/user/u2', () =>
        respuestaOk([
          {
            id: 'r1',
            intercambioId: 'ex1',
            calificadorId: 'u1',
            calificadorNombres: 'Rosa Quispe',
            calificadoId: 'u2',
            puntuacion: 5,
            comentario: null,
            creadoEn: '2026-01-01T00:00:00Z',
          },
        ]),
      ),
    );

    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    expect(await screen.findByText('Ya calificaste este intercambio.')).toBeInTheDocument();
  });

  it('advierte si se envia sin seleccionar puntuacion', async () => {
    server.use(http.get('*/ratings/user/u2', () => respuestaOk([])));

    const usuario = userEvent.setup();
    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    await usuario.click(await screen.findByRole('button', { name: 'Enviar calificacion' }));

    expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'Selecciona una puntuacion.')).toBe(true);
  });

  it('envia la calificacion tras seleccionar estrellas', async () => {
    server.use(
      http.get('*/ratings/user/u2', () => respuestaOk([])),
      http.post('*/ratings', () => respuestaOk({ id: 'nueva-calificacion' })),
    );

    const usuario = userEvent.setup();
    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    await usuario.click(await screen.findByRole('radio', { name: '5 estrellas' }));
    await usuario.click(screen.getByRole('button', { name: 'Enviar calificacion' }));

    expect(await screen.findByText('Ya calificaste este intercambio.')).toBeInTheDocument();
  });

  it('muestra el formulario si la carga inicial de calificaciones falla', async () => {
    server.use(http.get('*/ratings/user/u2', () => HttpResponse.error()));

    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    expect(await screen.findByText('Califica a la otra parte')).toBeInTheDocument();
  });

  it('muestra un toast de error si el envio de la calificacion falla', async () => {
    server.use(http.get('*/ratings/user/u2', () => respuestaOk([])), http.post('*/ratings', () => HttpResponse.error()));

    const usuario = userEvent.setup();
    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    await usuario.click(await screen.findByRole('radio', { name: '5 estrellas' }));
    await usuario.click(screen.getByRole('button', { name: 'Enviar calificacion' }));

    await vi.waitFor(() => {
      expect(
        useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos registrar tu calificacion.'),
      ).toBe(true);
    });
  });

  it('muestra el formulario si existe una calificacion de otro intercambio', async () => {
    server.use(
      http.get('*/ratings/user/u2', () =>
        respuestaOk([
          {
            id: 'r1',
            intercambioId: 'otro-intercambio',
            calificadorId: 'u1',
            calificadorNombres: 'Rosa Quispe',
            calificadoId: 'u2',
            puntuacion: 5,
            comentario: null,
            creadoEn: '2026-01-01T00:00:00Z',
          },
        ]),
      ),
    );

    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    expect(await screen.findByText('Califica a la otra parte')).toBeInTheDocument();
  });

  it('muestra el formulario si la calificacion existente es de otro calificador', async () => {
    server.use(
      http.get('*/ratings/user/u2', () =>
        respuestaOk([
          {
            id: 'r1',
            intercambioId: 'ex1',
            calificadorId: 'otro-usuario',
            calificadorNombres: 'Otro Usuario',
            calificadoId: 'u2',
            puntuacion: 5,
            comentario: null,
            creadoEn: '2026-01-01T00:00:00Z',
          },
        ]),
      ),
    );

    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    expect(await screen.findByText('Califica a la otra parte')).toBeInTheDocument();
  });

  it('no consulta calificaciones ni renderiza nada sin usuario en sesion', () => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });

    const { container } = render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    expect(container).toBeEmptyDOMElement();
  });

  it('muestra el formulario si la respuesta de calificaciones no trae datos', async () => {
    server.use(http.get('*/ratings/user/u2', () => respuestaOk(null)));

    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    expect(await screen.findByText('Califica a la otra parte')).toBeInTheDocument();
  });

  it('permite escribir un comentario opcional', async () => {
    server.use(http.get('*/ratings/user/u2', () => respuestaOk([])));

    const usuario = userEvent.setup();
    render(<RatingForm intercambioId="ex1" calificadoId="u2" />);

    const textarea = await screen.findByLabelText('Comentario (opcional)');
    await usuario.type(textarea, 'Todo bien con el intercambio');

    expect(textarea).toHaveValue('Todo bien con el intercambio');
  });
});
