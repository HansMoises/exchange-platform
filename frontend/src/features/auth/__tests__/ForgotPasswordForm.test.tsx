import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import ForgotPasswordForm from '../components/ForgotPasswordForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function renderForm() {
  return render(
    <MemoryRouter>
      <ForgotPasswordForm />
    </MemoryRouter>,
  );
}

describe('ForgotPasswordForm', () => {
  it('muestra error de validacion con correo invalido', async () => {
    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'no-es-correo');
    await usuario.click(screen.getByRole('button', { name: 'Enviar instrucciones' }));

    expect(await screen.findByText('Formato de correo invalido.')).toBeInTheDocument();
  });

  it('muestra el mensaje de confirmacion tras enviar', async () => {
    server.use(http.post('*/auth/forgot-password', () => respuestaOk(null)));

    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'rosa@example.com');
    await usuario.click(screen.getByRole('button', { name: 'Enviar instrucciones' }));

    expect(
      await screen.findByText('Si el correo esta registrado, recibiras instrucciones en breve.'),
    ).toBeInTheDocument();
  });

  it('muestra un toast de error si la peticion falla', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(http.post('*/auth/forgot-password', () => HttpResponse.error()));

    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'rosa@example.com');
    await usuario.click(screen.getByRole('button', { name: 'Enviar instrucciones' }));

    await waitFor(() => expect(useToastStore.getState().toasts.length).toBeGreaterThan(0));
  });

  it('muestra el texto de carga mientras se envia el formulario', async () => {
    server.use(
      http.post('*/auth/forgot-password', async () => {
        await new Promise((resolve) => setTimeout(resolve, 30));
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    renderForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'rosa@example.com');
    const clickPromise = usuario.click(screen.getByRole('button', { name: 'Enviar instrucciones' }));

    expect(await screen.findByText('Enviando...')).toBeInTheDocument();
    await clickPromise;
  });
});
