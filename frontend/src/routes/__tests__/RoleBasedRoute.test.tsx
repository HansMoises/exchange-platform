import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { useAuthStore } from '../../stores/authStore';
import type { UsuarioDto } from '../../types/usuario.types';
import RoleBasedRoute from '../RoleBasedRoute';

function usuarioDePrueba(rol: UsuarioDto['rol']): UsuarioDto {
  return {
    id: 'u1',
    nombres: 'Test',
    apellidos: 'Usuario',
    email: 'test@example.com',
    rol,
    fotoPerfil: null,
    calificacionPromedio: 0,
    totalIntercambios: 0,
  };
}

function renderConRuta() {
  return render(
    <MemoryRouter initialEntries={['/admin']}>
      <Routes>
        <Route path="/login" element={<p>Pagina de login</p>} />
        <Route path="/dashboard" element={<p>Dashboard</p>} />
        <Route
          path="/admin"
          element={
            <RoleBasedRoute roles={['Administrador', 'Moderador']}>
              <p>Panel admin</p>
            </RoleBasedRoute>
          }
        />
      </Routes>
    </MemoryRouter>,
  );
}

// Refleja en el frontend la misma matriz de autorizacion que se probo en el
// backend (AdminController): sin sesion -> login; rol sin permiso ->
// dashboard; rol permitido -> contenido.
describe('RoleBasedRoute', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
  });

  it('redirige a /login cuando no hay sesion', () => {
    renderConRuta();

    expect(screen.getByText('Pagina de login')).toBeInTheDocument();
    expect(screen.queryByText('Panel admin')).not.toBeInTheDocument();
  });

  it('redirige a /dashboard cuando el rol no esta permitido', () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba('Usuario') });

    renderConRuta();

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.queryByText('Panel admin')).not.toBeInTheDocument();
  });

  it('muestra el contenido cuando el rol esta permitido', () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba('Moderador') });

    renderConRuta();

    expect(screen.getByText('Panel admin')).toBeInTheDocument();
  });

  it('tambien permite Administrador', () => {
    useAuthStore.setState({ isAuthenticated: true, usuario: usuarioDePrueba('Administrador') });

    renderConRuta();

    expect(screen.getByText('Panel admin')).toBeInTheDocument();
  });
});
