import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useAuthStore } from '../../../stores/authStore';
import { useToastStore } from '../../../stores/toastStore';
import LoginForm from '../components/LoginForm';

function renderLoginForm() {
  return render(
    <MemoryRouter initialEntries={['/login']}>
      <Routes>
        <Route path="/login" element={<LoginForm />} />
        <Route path="/dashboard" element={<p>Dashboard</p>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('LoginForm', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
    useToastStore.setState({ toasts: [] });
  });

  it('muestra errores de validacion con el formulario vacio', async () => {
    const usuario = userEvent.setup();
    renderLoginForm();

    await usuario.click(screen.getByRole('button', { name: 'Iniciar sesion' }));

    expect(await screen.findByText('Formato de correo invalido.')).toBeInTheDocument();
    expect(screen.getByText('La contrasena es requerida.')).toBeInTheDocument();
  });

  it('inicia sesion, guarda el usuario y navega al dashboard', async () => {
    server.use(
      http.post('*/auth/login', () =>
        HttpResponse.json({
          success: true,
          message: 'Inicio de sesion exitoso.',
          data: {
            accessToken: 'token-acceso',
            refreshToken: 'token-refresco',
            accessTokenExpiresAt: '2026-06-03T00:15:00Z',
            usuario: {
              id: 'u1',
              nombres: 'Juan',
              apellidos: 'Quispe',
              email: 'juan@example.com',
              rol: 'Usuario',
              fotoPerfil: null,
              calificacionPromedio: 0,
              totalIntercambios: 0,
            },
          },
          errors: null,
          timestamp: '2026-06-03T00:00:00Z',
        }),
      ),
    );

    const usuario = userEvent.setup();
    renderLoginForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'juan@example.com');
    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveSegura@123');
    await usuario.click(screen.getByRole('button', { name: 'Iniciar sesion' }));

    expect(await screen.findByText('Dashboard')).toBeInTheDocument();
    expect(useAuthStore.getState().isAuthenticated).toBe(true);
    expect(useAuthStore.getState().usuario?.nombres).toBe('Juan');
  });

  it('muestra un toast de error con credenciales invalidas', async () => {
    server.use(
      http.post(
        '*/auth/login',
        () =>
          HttpResponse.json(
            {
              success: false,
              message: 'Credenciales invalidas.',
              data: null,
              errors: null,
              timestamp: '2026-06-03T00:00:00Z',
            },
            { status: 401 },
          ),
        { once: true },
      ),
    );

    const usuario = userEvent.setup();
    renderLoginForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'juan@example.com');
    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveIncorrecta@1');
    await usuario.click(screen.getByRole('button', { name: 'Iniciar sesion' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'Credenciales invalidas.')).toBe(true);
    });
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it('no inicia sesion si la respuesta 200 no trae datos', async () => {
    server.use(
      http.post('*/auth/login', () =>
        HttpResponse.json({ success: true, message: 'ok', data: null, errors: null, timestamp: '2026-06-03T00:00:00Z' }),
      ),
    );

    const usuario = userEvent.setup();
    renderLoginForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'juan@example.com');
    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveSegura@123');
    await usuario.click(screen.getByRole('button', { name: 'Iniciar sesion' }));

    await waitFor(() => expect(screen.getByRole('button', { name: 'Iniciar sesion' })).toBeEnabled());
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
    expect(screen.queryByText('Dashboard')).not.toBeInTheDocument();
  });

  it('muestra el texto de carga mientras se envia el formulario', async () => {
    server.use(
      http.post('*/auth/login', async () => {
        await new Promise((resolve) => setTimeout(resolve, 30));
        return HttpResponse.json({
          success: false,
          message: 'Credenciales invalidas.',
          data: null,
          errors: null,
          timestamp: '2026-06-03T00:00:00Z',
        });
      }),
    );

    const usuario = userEvent.setup();
    renderLoginForm();

    await usuario.type(screen.getByLabelText('Correo electronico'), 'juan@example.com');
    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveSegura@123');
    const clickPromise = usuario.click(screen.getByRole('button', { name: 'Iniciar sesion' }));

    expect(await screen.findByText('Ingresando...')).toBeInTheDocument();
    await clickPromise;
  });
});
