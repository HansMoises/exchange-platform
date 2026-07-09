import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { useAuthStore } from '../../stores/authStore';
import PublicRoute from '../PublicRoute';

function renderConRuta() {
  return render(
    <MemoryRouter initialEntries={['/login']}>
      <Routes>
        <Route path="/dashboard" element={<p>Dashboard</p>} />
        <Route
          path="/login"
          element={
            <PublicRoute>
              <p>Formulario de login</p>
            </PublicRoute>
          }
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe('PublicRoute', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
  });

  it('muestra el contenido cuando no hay sesion', () => {
    renderConRuta();

    expect(screen.getByText('Formulario de login')).toBeInTheDocument();
  });

  it('redirige a /dashboard cuando ya hay sesion', () => {
    useAuthStore.setState({ isAuthenticated: true });

    renderConRuta();

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.queryByText('Formulario de login')).not.toBeInTheDocument();
  });
});
