import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import ChangePasswordForm from '../components/ChangePasswordForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('ChangePasswordForm', () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
  });

  it('muestra errores de validacion con el formulario vacio', async () => {
    const usuario = userEvent.setup();
    render(<ChangePasswordForm />);

    await usuario.click(screen.getByRole('button', { name: 'Cambiar contrasena' }));

    expect(await screen.findByText('La contrasena actual es requerida.')).toBeInTheDocument();
    expect(screen.getByText('Minimo 8 caracteres.')).toBeInTheDocument();
  });

  it('muestra error cuando las contrasenas nuevas no coinciden', async () => {
    const usuario = userEvent.setup();
    render(<ChangePasswordForm />);

    await usuario.type(screen.getByLabelText('Contrasena actual'), 'ClaveActual@1');
    await usuario.type(screen.getByLabelText('Contrasena nueva'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena nueva'), 'OtraClave@456');
    await usuario.click(screen.getByRole('button', { name: 'Cambiar contrasena' }));

    expect(await screen.findByText('Las contrasenas no coinciden.')).toBeInTheDocument();
  });

  it('cambia la contrasena exitosamente y limpia el formulario', async () => {
    server.use(http.patch('*/users/me/password', () => respuestaOk(null)));

    const usuario = userEvent.setup();
    render(<ChangePasswordForm />);

    await usuario.type(screen.getByLabelText('Contrasena actual'), 'ClaveActual@1');
    await usuario.type(screen.getByLabelText('Contrasena nueva'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena nueva'), 'ClaveNueva@123');
    await usuario.click(screen.getByRole('button', { name: 'Cambiar contrasena' }));

    await waitFor(() => expect(screen.getByLabelText('Contrasena actual')).toHaveValue(''));
    expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'Contrasena actualizada exitosamente.')).toBe(
      true,
    );
  });

  it('muestra un toast de error si la contrasena actual es incorrecta', async () => {
    server.use(
      http.patch('*/users/me/password', () =>
        HttpResponse.json(
          {
            success: false,
            message: 'La contrasena actual es incorrecta.',
            data: null,
            errors: null,
            timestamp: '2026-01-01T00:00:00Z',
          },
          { status: 409 },
        ),
      ),
    );

    const usuario = userEvent.setup();
    render(<ChangePasswordForm />);

    await usuario.type(screen.getByLabelText('Contrasena actual'), 'ClaveIncorrecta@1');
    await usuario.type(screen.getByLabelText('Contrasena nueva'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena nueva'), 'ClaveNueva@123');
    await usuario.click(screen.getByRole('button', { name: 'Cambiar contrasena' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'La contrasena actual es incorrecta.')).toBe(
        true,
      );
    });
  });

  it('muestra el texto de carga mientras se envia el formulario', async () => {
    server.use(
      http.patch('*/users/me/password', async () => {
        await new Promise((resolve) => setTimeout(resolve, 30));
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    render(<ChangePasswordForm />);

    await usuario.type(screen.getByLabelText('Contrasena actual'), 'ClaveActual@1');
    await usuario.type(screen.getByLabelText('Contrasena nueva'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena nueva'), 'ClaveNueva@123');
    const clickPromise = usuario.click(screen.getByRole('button', { name: 'Cambiar contrasena' }));

    expect(await screen.findByText('Guardando...')).toBeInTheDocument();
    await clickPromise;
  });
});
