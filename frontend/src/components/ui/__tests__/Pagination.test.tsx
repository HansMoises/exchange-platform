import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import Pagination from '../Pagination';

describe('Pagination', () => {
  it('no renderiza nada si solo hay una pagina', () => {
    const { container } = render(
      <Pagination pageNumber={1} totalPages={1} hasPrevious={false} hasNext={false} onChange={vi.fn()} />,
    );
    expect(container).toBeEmptyDOMElement();
  });

  it('muestra la pagina actual y el total', () => {
    render(<Pagination pageNumber={2} totalPages={5} hasPrevious hasNext onChange={vi.fn()} />);
    expect(screen.getByText('Pagina 2 de 5')).toBeInTheDocument();
  });

  it('deshabilita "Anterior" en la primera pagina', () => {
    render(<Pagination pageNumber={1} totalPages={3} hasPrevious={false} hasNext onChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: 'Anterior' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Siguiente' })).toBeEnabled();
  });

  it('deshabilita "Siguiente" en la ultima pagina', () => {
    render(<Pagination pageNumber={3} totalPages={3} hasPrevious hasNext={false} onChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: 'Siguiente' })).toBeDisabled();
  });

  it('llama a onChange con la pagina correcta', async () => {
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<Pagination pageNumber={2} totalPages={5} hasPrevious hasNext onChange={onChange} />);

    await usuario.click(screen.getByRole('button', { name: 'Siguiente' }));
    expect(onChange).toHaveBeenCalledWith(3);

    await usuario.click(screen.getByRole('button', { name: 'Anterior' }));
    expect(onChange).toHaveBeenCalledWith(1);
  });
});
