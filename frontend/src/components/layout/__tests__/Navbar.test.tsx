import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../../../test/server';
import { useAuthStore } from '../../../stores/authStore';
import Navbar from '../Navbar';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function renderNavbar() {
  return render(
    <MemoryRouter initialEntries={['/dashboard']}>
      <Routes>
        <Route path="/" element={<p>Landing</p>} />
        <Route path="*" element={<Navbar />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('Navbar', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
  });

  it('muestra enlaces de iniciar sesion y registro cuando no hay sesion', () => {
    renderNavbar();

    expect(screen.getByRole('link', { name: 'Iniciar sesion' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Registrarse' })).toBeInTheDocument();
    expect(screen.queryByText('Dashboard')).not.toBeInTheDocument();
  });

  it('muestra el menu autenticado y el nombre del usuario', async () => {
    server.use(http.get('*/notifications', () => respuestaOk([])));
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
        totalIntercambios: 0,
      },
    });

    renderNavbar();

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Rosa' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cerrar sesion' })).toBeInTheDocument();
  });

  it('abre y cierra el menu movil con el boton hamburguesa', async () => {
    const usuario = userEvent.setup();
    renderNavbar();

    const botonMenu = screen.getByRole('button', { name: 'Abrir menu' });
    expect(botonMenu).toHaveAttribute('aria-expanded', 'false');

    await usuario.click(botonMenu);

    expect(screen.getByRole('button', { name: 'Cerrar menu' })).toHaveAttribute('aria-expanded', 'true');
  });

  it('cierra sesion, muestra el toast y navega a la landing', async () => {
    server.use(
      http.get('*/notifications', () => respuestaOk([])),
      http.post('*/auth/logout', () => respuestaOk(null)),
    );
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
        totalIntercambios: 0,
      },
      accessToken: 'token',
      refreshToken: 'refresh-token',
    });

    const usuario = userEvent.setup();
    renderNavbar();

    await usuario.click(screen.getByRole('button', { name: 'Cerrar sesion' }));

    expect(await screen.findByText('Landing')).toBeInTheDocument();
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it('el menu movil muestra login/registro cuando no hay sesion', async () => {
    const usuario = userEvent.setup();
    renderNavbar();

    await usuario.click(screen.getByRole('button', { name: 'Abrir menu' }));

    const menuMovil = screen.getByRole('navigation', { name: 'Principal movil' });
    expect(within(menuMovil).getByRole('link', { name: 'Iniciar sesion' })).toBeInTheDocument();
    expect(within(menuMovil).getByRole('link', { name: 'Registrarse' })).toBeInTheDocument();
  });

  it('el menu movil muestra el usuario y permite cerrar sesion', async () => {
    server.use(
      http.get('*/notifications', () => respuestaOk([])),
      http.post('*/auth/logout', () => respuestaOk(null)),
    );
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
        totalIntercambios: 0,
      },
      accessToken: 'token',
      refreshToken: 'refresh-token',
    });

    const usuario = userEvent.setup();
    renderNavbar();

    await usuario.click(screen.getByRole('button', { name: 'Abrir menu' }));

    const menuMovil = screen.getByRole('navigation', { name: 'Principal movil' });
    expect(within(menuMovil).getByRole('link', { name: 'Rosa' })).toBeInTheDocument();

    await usuario.click(within(menuMovil).getByRole('button', { name: 'Cerrar sesion' }));

    expect(await screen.findByText('Landing')).toBeInTheDocument();
  });

  it('cierra el menu movil al hacer click en un enlace de login', async () => {
    const usuario = userEvent.setup();
    renderNavbar();

    await usuario.click(screen.getByRole('button', { name: 'Abrir menu' }));
    const menuMovil = screen.getByRole('navigation', { name: 'Principal movil' });
    await usuario.click(within(menuMovil).getByRole('link', { name: 'Iniciar sesion' }));

    expect(screen.queryByRole('navigation', { name: 'Principal movil' })).not.toBeInTheDocument();
  });

  it('cierra el menu movil al hacer click en el enlace de registro', async () => {
    const usuario = userEvent.setup();
    renderNavbar();

    await usuario.click(screen.getByRole('button', { name: 'Abrir menu' }));
    const menuMovil = screen.getByRole('navigation', { name: 'Principal movil' });
    await usuario.click(within(menuMovil).getByRole('link', { name: 'Registrarse' }));

    expect(screen.queryByRole('navigation', { name: 'Principal movil' })).not.toBeInTheDocument();
  });

  it('cierra el menu movil al hacer click en el enlace del perfil', async () => {
    server.use(http.get('*/notifications', () => respuestaOk([])));
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
        totalIntercambios: 0,
      },
    });

    const usuario = userEvent.setup();
    renderNavbar();

    await usuario.click(screen.getByRole('button', { name: 'Abrir menu' }));
    const menuMovil = screen.getByRole('navigation', { name: 'Principal movil' });
    await usuario.click(within(menuMovil).getByRole('link', { name: 'Rosa' }));

    expect(screen.queryByRole('navigation', { name: 'Principal movil' })).not.toBeInTheDocument();
  });
});
