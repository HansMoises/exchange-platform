import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import Input from '../Input';

describe('Input', () => {
  it('asocia el label con el campo por accesibilidad', () => {
    render(<Input label="Correo electronico" />);

    const campo = screen.getByLabelText('Correo electronico');
    expect(campo).toBeInTheDocument();
  });

  it('muestra el mensaje de error y marca aria-invalid', () => {
    render(<Input label="Correo electronico" error="Formato de correo invalido." />);

    const campo = screen.getByLabelText('Correo electronico');
    expect(campo).toHaveAttribute('aria-invalid', 'true');
    expect(screen.getByRole('alert')).toHaveTextContent('Formato de correo invalido.');
  });

  it('muestra el texto de ayuda cuando no hay error', () => {
    render(<Input label="Correo electronico" helperText="Usaremos este correo para contactarte." />);

    expect(screen.getByText('Usaremos este correo para contactarte.')).toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });
});
