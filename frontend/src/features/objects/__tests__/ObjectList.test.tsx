import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ComponentProps } from 'react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { useAuthStore } from '../../../stores/authStore';
import type { PagedResult } from '../../../types/api.types';
import type { ObjetoDto } from '../types/objeto.types';
import ObjectList from '../components/ObjectList';

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

function resultadoDePrueba(
  items: ObjetoDto[],
  overrides: Partial<PagedResult<ObjetoDto>> = {},
): PagedResult<ObjetoDto> {
  return {
    items,
    pageNumber: 1,
    pageSize: 20,
    totalRecords: items.length,
    totalPages: 1,
    hasPrevious: false,
    hasNext: false,
    ...overrides,
  };
}

function renderList(props: Partial<ComponentProps<typeof ObjectList>>) {
  return render(
    <MemoryRouter>
      <ObjectList
        resultado={null}
        isLoading={false}
        error={false}
        onCambiarPagina={vi.fn()}
        onReintentar={vi.fn()}
        {...props}
      />
    </MemoryRouter>,
  );
}

describe('ObjectList', () => {
  it('muestra el estado de carga', () => {
    renderList({ isLoading: true });
    expect(screen.getByRole('status', { name: 'Cargando' })).toBeInTheDocument();
  });

  it('muestra el estado de error con boton de reintentar', async () => {
    const onReintentar = vi.fn();
    const usuario = userEvent.setup();
    renderList({ error: true, onReintentar });

    expect(screen.getByText('No pudimos cargar los objetos.')).toBeInTheDocument();
    await usuario.click(screen.getByRole('button', { name: 'Reintentar' }));
    expect(onReintentar).toHaveBeenCalledTimes(1);
  });

  it('muestra el estado vacio cuando no hay objetos', () => {
    renderList({ resultado: resultadoDePrueba([]) });
    expect(screen.getByText('Aun no hay objetos publicados.')).toBeInTheDocument();
  });

  it('renderiza las tarjetas de objetos y la paginacion', () => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
    renderList({ resultado: resultadoDePrueba([objetoDePrueba()], { totalPages: 2, hasNext: true }) });

    expect(screen.getByText('Bicicleta rodado 26')).toBeInTheDocument();
    expect(screen.getByText('Pagina 1 de 2')).toBeInTheDocument();
  });

  it('llama a onCambiarPagina al navegar', async () => {
    useAuthStore.setState({ isAuthenticated: false, usuario: null, accessToken: null, refreshToken: null });
    const onCambiarPagina = vi.fn();
    const usuario = userEvent.setup();

    renderList({
      resultado: resultadoDePrueba([objetoDePrueba()], { totalPages: 2, hasNext: true }),
      onCambiarPagina,
    });

    await usuario.click(screen.getByRole('button', { name: 'Siguiente' }));
    expect(onCambiarPagina).toHaveBeenCalledWith(2);
  });
});
