import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import Button from '../Button';

describe('Button', () => {
  it('renderiza el texto y responde al click', async () => {
    const alClick = vi.fn();
    const usuario = userEvent.setup();

    render(<Button onClick={alClick}>Publicar objeto</Button>);

    const boton = screen.getByRole('button', { name: 'Publicar objeto' });
    await usuario.click(boton);

    expect(alClick).toHaveBeenCalledOnce();
  });

  it('no dispara onClick cuando esta deshabilitado', async () => {
    const alClick = vi.fn();
    const usuario = userEvent.setup();

    render(
      <Button onClick={alClick} disabled>
        Deshabilitado
      </Button>,
    );

    await usuario.click(screen.getByRole('button', { name: 'Deshabilitado' }));

    expect(alClick).not.toHaveBeenCalled();
  });

  it('respeta el atributo type pasado por props (para submit en formularios)', () => {
    render(<Button type="submit">Enviar</Button>);

    expect(screen.getByRole('button', { name: 'Enviar' })).toHaveAttribute('type', 'submit');
  });
});
