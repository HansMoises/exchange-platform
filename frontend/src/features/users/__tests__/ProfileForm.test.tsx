import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useAuthStore } from '../../../stores/authStore';
import { useToastStore } from '../../../stores/toastStore';
import type { PerfilUsuarioDto } from '../types/perfil.types';
import ProfileForm from '../components/ProfileForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function perfilDePrueba(): PerfilUsuarioDto {
  return {
    id: 'u1',
    nombres: 'Rosa',
    apellidos: 'Quispe',
    email: 'rosa@example.com',
    telefono: '987654321',
    fotoPerfil: null,
    rolId: 3,
    departamentoId: 1,
    provinciaId: 1,
    distritoId: 1,
    calificacionPromedio: 4.5,
    totalIntercambios: 3,
    miembroDesde: '2026-01-01T00:00:00Z',
  };
}

describe('ProfileForm', () => {
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
        calificacionPromedio: 4.5,
        totalIntercambios: 3,
      },
    });
    server.use(http.get('*/geo/departamentos', () => respuestaOk([{ id: 1, ubigeo: '05', nombre: 'Ayacucho' }])));
  });

  it('precarga los datos del perfil actual', () => {
    render(<ProfileForm perfil={perfilDePrueba()} />);

    expect(screen.getByLabelText('Nombres')).toHaveValue('Rosa');
    expect(screen.getByLabelText('Apellidos')).toHaveValue('Quispe');
    expect(screen.getByLabelText('Telefono')).toHaveValue('987654321');
  });

  it('muestra error si se borra el nombre', async () => {
    const usuario = userEvent.setup();
    render(<ProfileForm perfil={perfilDePrueba()} />);

    await usuario.clear(screen.getByLabelText('Nombres'));
    await usuario.click(screen.getByRole('button', { name: 'Guardar cambios' }));

    expect(await screen.findByText('Minimo 2 caracteres.')).toBeInTheDocument();
  });

  it('actualiza el perfil y refleja el nombre en el store de sesion', async () => {
    server.use(http.put('*/users/me', () => respuestaOk(null)));

    const usuario = userEvent.setup();
    render(<ProfileForm perfil={perfilDePrueba()} />);

    await usuario.clear(screen.getByLabelText('Nombres'));
    await usuario.type(screen.getByLabelText('Nombres'), 'Rosa Maria');
    await usuario.click(screen.getByRole('button', { name: 'Guardar cambios' }));

    await waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'Perfil actualizado exitosamente.')).toBe(true);
    });
    expect(useAuthStore.getState().usuario?.nombres).toBe('Rosa Maria');
  });

  it('muestra un toast de error si la actualizacion falla', async () => {
    server.use(http.put('*/users/me', () => HttpResponse.error()));

    const usuario = userEvent.setup();
    render(<ProfileForm perfil={perfilDePrueba()} />);

    await usuario.click(screen.getByRole('button', { name: 'Guardar cambios' }));

    await waitFor(() => {
      expect(
        useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos actualizar tu perfil.'),
      ).toBe(true);
    });
  });

  it('al cambiar de departamento limpia provincia y distrito', async () => {
    server.use(
      http.get('*/geo/departamentos', () =>
        respuestaOk([
          { id: 1, ubigeo: '05', nombre: 'Ayacucho' },
          { id: 2, ubigeo: '02', nombre: 'Ancash' },
        ]),
      ),
    );

    const usuario = userEvent.setup();
    render(<ProfileForm perfil={perfilDePrueba()} />);

    await waitFor(() => expect(screen.getByRole('option', { name: 'Ancash' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Departamento'), '2');

    expect(screen.getByLabelText('Provincia')).toHaveValue('0');
    expect(screen.getByLabelText('Distrito')).toHaveValue('0');
  });

  it('al cambiar de provincia limpia el distrito', async () => {
    server.use(
      http.get('*/geo/provincias', () =>
        respuestaOk([
          { id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 },
          { id: 2, ubigeo: '0502', nombre: 'Cangallo', departamentoId: 1 },
        ]),
      ),
    );

    const usuario = userEvent.setup();
    render(<ProfileForm perfil={perfilDePrueba()} />);

    await waitFor(() => expect(screen.getByRole('option', { name: 'Cangallo' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Provincia'), '2');

    expect(screen.getByLabelText('Distrito')).toHaveValue('0');
  });

  it('carga los distritos de la provincia precargada', async () => {
    server.use(
      http.get('*/geo/distritos', () =>
        respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }]),
      ),
    );

    render(<ProfileForm perfil={perfilDePrueba()} />);

    const selectDistrito = screen.getByLabelText('Distrito');
    expect(await within(selectDistrito).findByRole('option', { name: 'Ayacucho' })).toBeInTheDocument();
  });

  it('no pide provincias ni distritos si el perfil no tiene ubicacion', () => {
    render(<ProfileForm perfil={{ ...perfilDePrueba(), departamentoId: 0, provinciaId: 0, distritoId: 0 }} />);

    expect(screen.getByLabelText('Provincia')).toBeDisabled();
    expect(screen.getByLabelText('Distrito')).toBeDisabled();
  });

  it('muestra el texto de carga mientras se guarda el perfil', async () => {
    server.use(
      http.put('*/users/me', async () => {
        await new Promise((resolve) => setTimeout(resolve, 30));
        return respuestaOk(null);
      }),
    );

    const usuario = userEvent.setup();
    render(<ProfileForm perfil={perfilDePrueba()} />);

    const clickPromise = usuario.click(screen.getByRole('button', { name: 'Guardar cambios' }));

    expect(await screen.findByText('Guardando...')).toBeInTheDocument();
    await clickPromise;
  });
});
