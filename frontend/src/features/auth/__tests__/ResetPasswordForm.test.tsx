import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import ResetPasswordForm from '../components/ResetPasswordForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function renderForm(ruta = '/reset-password?token=abc123') {
  return render(
    <MemoryRouter initialEntries={[ruta]}>
      <Routes>
        <Route path="/reset-password" element={<ResetPasswordForm />} />
        <Route path="/login" element={<p>Pagina de login</p>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('ResetPasswordForm', () => {
  it('muestra un mensaje de enlace invalido cuando no hay token', () => {
    renderForm('/reset-password');

    expect(screen.getByText('El enlace de recuperacion no es valido.')).toBeInTheDocument();
    expect(screen.queryByLabelText('Nueva contrasena')).not.toBeInTheDocument();
  });

  it('muestra error cuando las contrasenas no coinciden', async () => {
    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Nueva contrasena'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar nueva contrasena'), 'OtraClave@456');
    await usuario.click(screen.getByRole('button', { name: 'Restablecer contrasena' }));

    expect(await screen.findByText('Las contrasenas no coinciden.')).toBeInTheDocument();
  });

  it('restablece la contrasena y navega a login', async () => {
    server.use(http.post('*/auth/reset-password', () => respuestaOk(null)));

    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Nueva contrasena'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar nueva contrasena'), 'ClaveNueva@123');
    await usuario.click(screen.getByRole('button', { name: 'Restablecer contrasena' }));

    expect(await screen.findByText('Pagina de login')).toBeInTheDocument();
  });

  it('muestra un toast de error si el enlace expiro', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(http.post('*/auth/reset-password', () => HttpResponse.error()));

    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Nueva contrasena'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar nueva contrasena'), 'ClaveNueva@123');
    await usuario.click(screen.getByRole('button', { name: 'Restablecer contrasena' }));

    await waitFor(() => {
      expect(
        useToastStore.getState().toasts.some((t) => t.mensaje === 'El enlace ha expirado. Solicita uno nuevo.'),
      ).toBe(true);
    });
  });

  it('muestra el texto de carga mientras se envia el formulario', async () => {
    server.use(
      http.post('*/auth/reset-password', async () => {
        await new Promise((resolve) => setTimeout(resolve, 30));
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Nueva contrasena'), 'ClaveNueva@123');
    await usuario.type(screen.getByLabelText('Confirmar nueva contrasena'), 'ClaveNueva@123');
    const clickPromise = usuario.click(screen.getByRole('button', { name: 'Restablecer contrasena' }));

    expect(await screen.findByText('Guardando...')).toBeInTheDocument();
    await clickPromise;
  });
});
