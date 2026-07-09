import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { server } from '../../../test/server';
import { useAuthStore } from '../../../stores/authStore';
import { useToastStore } from '../../../stores/toastStore';
import ProfilePhotoUploader from '../components/ProfilePhotoUploader';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function usuarioDePrueba(fotoPerfil: string | null = null) {
  return {
    id: 'u1',
    nombres: 'Rosa',
    apellidos: 'Quispe',
    email: 'rosa@example.com',
    rol: 'Usuario' as const,
    fotoPerfil,
    calificacionPromedio: 0,
    totalIntercambios: 0,
  };
}

describe('ProfilePhotoUploader', () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
  });

  it('muestra el icono por defecto cuando no hay foto', () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba() });
    render(<ProfilePhotoUploader />);

    expect(screen.queryByAltText('Tu foto de perfil')).not.toBeInTheDocument();
  });

  it('muestra la foto actual cuando existe', () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba('https://example.com/foto.jpg') });
    render(<ProfilePhotoUploader />);

    expect(screen.getByAltText('Tu foto de perfil')).toHaveAttribute('src', 'https://example.com/foto.jpg');
  });

  it('sube la foto y actualiza el store de sesion', async () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba() });
    server.use(http.patch('*/users/me/photo', () => respuestaOk({ url: 'https://example.com/nueva.jpg' })));

    const usuario = userEvent.setup();
    render(<ProfilePhotoUploader />);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await usuario.upload(input, new File(['contenido'], 'foto.jpg', { type: 'image/jpeg' }));

    await vi.waitFor(() => expect(useAuthStore.getState().usuario?.fotoPerfil).toBe('https://example.com/nueva.jpg'));
    expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'Foto de perfil actualizada exitosamente.')).toBe(
      true,
    );
  });

  it('no actualiza el usuario si la respuesta no trae una url', async () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba() });
    server.use(http.patch('*/users/me/photo', () => respuestaOk(null)));

    const usuario = userEvent.setup();
    render(<ProfilePhotoUploader />);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await usuario.upload(input, new File(['contenido'], 'foto.jpg', { type: 'image/jpeg' }));

    await vi.waitFor(() => expect(screen.getByText('Cambiar foto')).toBeInTheDocument());
    expect(useAuthStore.getState().usuario?.fotoPerfil).toBeNull();
    expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'Foto de perfil actualizada exitosamente.')).toBe(
      false,
    );
  });

  it('no hace nada si se cancela la seleccion de archivo', () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba() });
    render(<ProfilePhotoUploader />);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    fireEvent.change(input, { target: { files: [] } });

    expect(screen.getByText('Cambiar foto')).toBeInTheDocument();
    expect(useAuthStore.getState().usuario?.fotoPerfil).toBeNull();
  });

  it('muestra un toast de error si la subida falla', async () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba() });
    server.use(http.patch('*/users/me/photo', () => HttpResponse.error()));

    const usuario = userEvent.setup();
    render(<ProfilePhotoUploader />);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await usuario.upload(input, new File(['contenido'], 'foto.jpg', { type: 'image/jpeg' }));

    await vi.waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos subir la imagen.')).toBe(true);
    });
  });
});
