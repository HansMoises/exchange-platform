import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import RegisterForm from '../components/RegisterForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function renderRegisterForm() {
  return render(
    <MemoryRouter initialEntries={['/register']}>
      <Routes>
        <Route path="/register" element={<RegisterForm />} />
        <Route path="/login" element={<p>Pagina de login</p>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('RegisterForm', () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
    server.use(http.get('*/geo/departamentos', () => respuestaOk([{ id: 1, ubigeo: '05', nombre: 'Ayacucho' }])));
  });

  it('muestra errores de validacion con el formulario vacio', async () => {
    const usuario = userEvent.setup();
    renderRegisterForm();

    await usuario.click(screen.getByRole('button', { name: 'Crear cuenta' }));

    expect(await screen.findAllByText('Minimo 2 caracteres.')).toHaveLength(2);
    expect(screen.getByText('Formato de correo invalido.')).toBeInTheDocument();
    expect(screen.getByText('El telefono debe tener 9 digitos.')).toBeInTheDocument();
    expect(screen.getByText('El departamento es requerido.')).toBeInTheDocument();
  });

  it('muestra error cuando las contrasenas no coinciden', async () => {
    const usuario = userEvent.setup();
    renderRegisterForm();

    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveSegura@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena'), 'OtraClave@456');
    await usuario.click(screen.getByRole('button', { name: 'Crear cuenta' }));

    expect(await screen.findByText('Las contrasenas no coinciden.')).toBeInTheDocument();
  });

  it('registra exitosamente y navega a login', async () => {
    server.use(
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
      http.get('*/geo/distritos', () => respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }])),
      http.post('*/auth/register', () => respuestaOk({ id: 'nuevo-usuario-id' })),
    );

    const usuario = userEvent.setup();
    renderRegisterForm();

    await usuario.type(screen.getByLabelText('Nombres'), 'Rosa');
    await usuario.type(screen.getByLabelText('Apellidos'), 'Quispe');
    await usuario.type(screen.getByLabelText('Correo electronico'), 'rosa@example.com');
    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveSegura@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena'), 'ClaveSegura@123');
    await usuario.type(screen.getByLabelText('Telefono'), '987654321');

    await waitFor(() => expect(screen.getByRole('option', { name: 'Ayacucho' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Departamento'), '1');

    await waitFor(() => expect(screen.getByRole('option', { name: 'Huamanga' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Provincia'), '1');

    await waitFor(() => expect(screen.getByLabelText('Distrito')).not.toBeDisabled());
    await usuario.selectOptions(screen.getByLabelText('Distrito'), '1');

    await usuario.click(screen.getByRole('button', { name: 'Crear cuenta' }));

    expect(await screen.findByText('Pagina de login')).toBeInTheDocument();
  });

  it('muestra un toast de error si el registro falla', async () => {
    server.use(
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
      http.get('*/geo/distritos', () => respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }])),
      http.post('*/auth/register', () => HttpResponse.error()),
    );

    const usuario = userEvent.setup();
    renderRegisterForm();

    await usuario.type(screen.getByLabelText('Nombres'), 'Rosa');
    await usuario.type(screen.getByLabelText('Apellidos'), 'Quispe');
    await usuario.type(screen.getByLabelText('Correo electronico'), 'rosa@example.com');
    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveSegura@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena'), 'ClaveSegura@123');
    await usuario.type(screen.getByLabelText('Telefono'), '987654321');

    await waitFor(() => expect(screen.getByRole('option', { name: 'Ayacucho' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Departamento'), '1');
    await waitFor(() => expect(screen.getByRole('option', { name: 'Huamanga' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Provincia'), '1');
    await waitFor(() => expect(screen.getByLabelText('Distrito')).not.toBeDisabled());
    await usuario.selectOptions(screen.getByLabelText('Distrito'), '1');

    await usuario.click(screen.getByRole('button', { name: 'Crear cuenta' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos crear tu cuenta.')).toBe(true);
    });
  });

  it('muestra el texto de carga mientras se envia el formulario', async () => {
    server.use(
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
      http.get('*/geo/distritos', () => respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }])),
      http.post('*/auth/register', async () => {
        await new Promise((resolve) => setTimeout(resolve, 30));
        return respuestaOk({ id: 'nuevo-usuario-id' });
      }),
    );

    const usuario = userEvent.setup();
    renderRegisterForm();

    await usuario.type(screen.getByLabelText('Nombres'), 'Rosa');
    await usuario.type(screen.getByLabelText('Apellidos'), 'Quispe');
    await usuario.type(screen.getByLabelText('Correo electronico'), 'rosa@example.com');
    await usuario.type(screen.getByLabelText('Contrasena'), 'ClaveSegura@123');
    await usuario.type(screen.getByLabelText('Confirmar contrasena'), 'ClaveSegura@123');
    await usuario.type(screen.getByLabelText('Telefono'), '987654321');

    await waitFor(() => expect(screen.getByRole('option', { name: 'Ayacucho' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Departamento'), '1');
    await waitFor(() => expect(screen.getByRole('option', { name: 'Huamanga' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Provincia'), '1');
    await waitFor(() => expect(screen.getByLabelText('Distrito')).not.toBeDisabled());
    await usuario.selectOptions(screen.getByLabelText('Distrito'), '1');

    const clickPromise = usuario.click(screen.getByRole('button', { name: 'Crear cuenta' }));

    expect(await screen.findByText('Creando cuenta...')).toBeInTheDocument();
    await clickPromise;
  });
});
