import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import Loading from '../Loading';

describe('Loading', () => {
  it('anuncia el estado de carga para lectores de pantalla', () => {
    render(<Loading />);
    expect(screen.getByRole('status', { name: 'Cargando' })).toBeInTheDocument();
  });

  it('renderiza la cantidad de lineas indicada', () => {
    const { container } = render(<Loading lineas={5} />);
    expect(container.querySelectorAll('.animate-pulse')).toHaveLength(5);
  });

  it('usa 3 lineas por defecto', () => {
    const { container } = render(<Loading />);
    expect(container.querySelectorAll('.animate-pulse')).toHaveLength(3);
  });
});
