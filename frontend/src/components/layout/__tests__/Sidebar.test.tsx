import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { useAuthStore } from '../../../stores/authStore';
import Sidebar from '../Sidebar';

function renderSidebar(ruta = '/') {
  return render(
    <MemoryRouter initialEntries={[ruta]}>
      <Sidebar />
    </MemoryRouter>,
  );
}

describe('Sidebar', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
  });

  it('muestra los enlaces base para cualquier moderador', () => {
    useAuthStore.setState({
      isAuthenticated: true,
      usuario: {
        id: 'u1',
        nombres: 'Mod',
        apellidos: 'Erador',
        email: 'mod@example.com',
        rol: 'Moderador',
        fotoPerfil: null,
        calificacionPromedio: 0,
        totalIntercambios: 0,
      },
    });

    renderSidebar();

    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Objetos' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Reportes' })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Usuarios' })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Categorias' })).not.toBeInTheDocument();
  });

  it('muestra los enlaces exclusivos de Administrador', () => {
    useAuthStore.setState({
      isAuthenticated: true,
      usuario: {
        id: 'u1',
        nombres: 'Admin',
        apellidos: 'Istrador',
        email: 'admin@example.com',
        rol: 'Administrador',
        fotoPerfil: null,
        calificacionPromedio: 0,
        totalIntercambios: 0,
      },
    });

    renderSidebar();

    expect(screen.getByRole('link', { name: 'Usuarios' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Auditoria' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Categorias' })).toBeInTheDocument();
  });

  it('no muestra los enlaces exclusivos de Administrador sin sesion', () => {
    renderSidebar();

    expect(screen.queryByRole('link', { name: 'Usuarios' })).not.toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeInTheDocument();
  });

  it('resalta el enlace de la ruta activa', () => {
    useAuthStore.setState({
      isAuthenticated: true,
      usuario: {
        id: 'u1',
        nombres: 'Mod',
        apellidos: 'Erador',
        email: 'mod@example.com',
        rol: 'Moderador',
        fotoPerfil: null,
        calificacionPromedio: 0,
        totalIntercambios: 0,
      },
    });

    renderSidebar('/admin/objects');

    expect(screen.getByRole('link', { name: 'Objetos' })).toHaveClass('bg-primary-soft');
  });
});
