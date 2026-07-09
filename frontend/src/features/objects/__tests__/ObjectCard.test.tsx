import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { useAuthStore } from '../../../stores/authStore';
import { useFavoritosStore } from '../../../stores/favoritosStore';
import type { ObjetoDto } from '../types/objeto.types';
import ObjectCard from '../components/ObjectCard';

function objetoDePrueba(overrides: Partial<ObjetoDto> = {}): ObjetoDto {
  return {
    id: 'obj-1',
    titulo: 'Bicicleta rodado 26',
    descripcion: 'En buen estado',
    categoriaId: 6,
    categoriaNombre: 'Deportes',
    usuarioId: 'u2',
    usuarioNombres: 'Rosa Quispe',
    usuarioCalificacion: 4.5,
    estado: 'Disponible',
    condicionFisica: 'Bueno',
    departamentoId: 1,
    provinciaId: 1,
    distritoId: 1,
    imagenes: [],
    creadoEn: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

function renderObjectCard(objeto: ObjetoDto) {
  return render(
    <MemoryRouter>
      <ObjectCard objeto={objeto} />
    </MemoryRouter>,
  );
}

describe('ObjectCard', () => {
  beforeEach(() => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
    useFavoritosStore.setState({ ids: new Set(), cargado: false });
  });

  it('muestra el titulo, categoria, calificacion y estado', () => {
    renderObjectCard(objetoDePrueba());

    expect(screen.getByText('Bicicleta rodado 26')).toBeInTheDocument();
    expect(screen.getByText('Deportes')).toBeInTheDocument();
    expect(screen.getByText('4.5')).toBeInTheDocument();
    expect(screen.getByText('Disponible')).toBeInTheDocument();
  });

  it('muestra "Sin imagen" cuando el objeto no tiene imagenes', () => {
    renderObjectCard(objetoDePrueba({ imagenes: [] }));

    expect(screen.getByText('Sin imagen')).toBeInTheDocument();
  });

  it('muestra la primera imagen cuando existe', () => {
    renderObjectCard(
      objetoDePrueba({
        imagenes: [{ id: 'img-1', url: 'https://example.com/foto.jpg', orden: 1 }],
      }),
    );

    const imagen = screen.getByAltText('Bicicleta rodado 26') as HTMLImageElement;
    expect(imagen.src).toBe('https://example.com/foto.jpg');
  });

  it('enlaza al detalle del objeto', () => {
    renderObjectCard(objetoDePrueba());

    expect(screen.getByRole('link', { name: 'Ver detalle' })).toHaveAttribute('href', '/objects/obj-1');
  });

  it('no muestra el boton de favoritos si no hay sesion', () => {
    renderObjectCard(objetoDePrueba());

    expect(screen.queryByRole('button', { name: /favoritos/ })).not.toBeInTheDocument();
  });

  it('muestra el boton de favoritos si hay sesion', () => {
    useAuthStore.setState({ isAuthenticated: true });

    renderObjectCard(objetoDePrueba());

    expect(screen.getByRole('button', { name: /favoritos/ })).toBeInTheDocument();
  });
});
