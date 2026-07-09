import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import EmptyState from '../EmptyState';

describe('EmptyState', () => {
  it('muestra el mensaje', () => {
    render(<EmptyState mensaje="No hay resultados." />);
    expect(screen.getByText('No hay resultados.')).toBeInTheDocument();
  });

  it('no muestra un boton de accion si no se pasa onAccion', () => {
    render(<EmptyState mensaje="No hay resultados." accion="Reintentar" />);
    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  it('muestra el boton de accion y lo dispara al hacer click', async () => {
    const onAccion = vi.fn();
    const usuario = userEvent.setup();

    render(<EmptyState mensaje="No hay resultados." accion="Reintentar" onAccion={onAccion} />);
    await usuario.click(screen.getByRole('button', { name: 'Reintentar' }));

    expect(onAccion).toHaveBeenCalledTimes(1);
  });
});
