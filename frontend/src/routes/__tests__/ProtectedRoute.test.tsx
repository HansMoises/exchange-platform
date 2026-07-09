import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { useAuthStore } from '../../stores/authStore';
import ProtectedRoute from '../ProtectedRoute';

function renderConRuta() {
  return render(
    <MemoryRouter initialEntries={['/dashboard']}>
      <Routes>
        <Route path="/login" element={<p>Pagina de login</p>} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <p>Contenido protegido</p>
            </ProtectedRoute>
          }
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
  });

  it('redirige a /login cuando no hay sesion', () => {
    renderConRuta();

    expect(screen.getByText('Pagina de login')).toBeInTheDocument();
    expect(screen.queryByText('Contenido protegido')).not.toBeInTheDocument();
  });

  it('muestra el contenido cuando el usuario esta autenticado', () => {
    useAuthStore.setState({ isAuthenticated: true });

    renderConRuta();

    expect(screen.getByText('Contenido protegido')).toBeInTheDocument();
  });
});
