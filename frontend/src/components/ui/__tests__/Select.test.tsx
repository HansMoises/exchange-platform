import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import Select from '../Select';

describe('Select', () => {
  it('asocia el label con el select y muestra las opciones', () => {
    render(
      <Select label="Departamento" value="" onChange={() => {}}>
        <option value="">Selecciona...</option>
        <option value="1">Ayacucho</option>
      </Select>,
    );

    const select = screen.getByLabelText('Departamento');
    expect(select).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Ayacucho' })).toBeInTheDocument();
  });

  it('dispara onChange al elegir una opcion', async () => {
    const usuario = userEvent.setup();
    const alCambiar = vi.fn();

    render(
      <Select label="Departamento" value="" onChange={alCambiar}>
        <option value="">Selecciona...</option>
        <option value="1">Ayacucho</option>
      </Select>,
    );

    await usuario.selectOptions(screen.getByLabelText('Departamento'), '1');

    expect(alCambiar).toHaveBeenCalled();
  });

  it('muestra el mensaje de error y marca aria-invalid', () => {
    render(
      <Select label="Departamento" error="Campo requerido" value="" onChange={() => {}}>
        <option value="">Selecciona...</option>
      </Select>,
    );

    expect(screen.getByText('Campo requerido')).toBeInTheDocument();
    expect(screen.getByLabelText('Departamento')).toHaveAttribute('aria-invalid', 'true');
  });
});
