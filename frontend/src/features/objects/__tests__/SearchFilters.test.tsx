import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { server } from '../../../test/server';
import SearchFilters from '../components/SearchFilters';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('SearchFilters', () => {
  beforeEach(() => {
    server.use(
      http.get('*/geo/categorias', () => respuestaOk([{ id: 6, nombre: 'Deportes', descripcion: '', icono: '' }])),
      http.get('*/geo/departamentos', () => respuestaOk([{ id: 1, ubigeo: '05', nombre: 'Ayacucho' }])),
    );
  });

  it('envia la busqueda con el texto ingresado', async () => {
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{}} onChange={onChange} />);

    await usuario.type(screen.getByLabelText('Buscar'), 'bicicleta');
    await usuario.click(screen.getByRole('button', { name: 'Buscar' }));

    expect(onChange).toHaveBeenCalledWith({ search: 'bicicleta', pageNumber: 1 });
  });

  it('envia la busqueda sin texto como indefinido', async () => {
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{}} onChange={onChange} />);

    await usuario.click(screen.getByRole('button', { name: 'Buscar' }));

    expect(onChange).toHaveBeenCalledWith({ search: undefined, pageNumber: 1 });
  });

  it('carga las categorias y departamentos como opciones', async () => {
    render(<SearchFilters filtros={{}} onChange={vi.fn()} />);

    expect(await screen.findByRole('option', { name: 'Deportes' })).toBeInTheDocument();
    expect(await screen.findByRole('option', { name: 'Ayacucho' })).toBeInTheDocument();
  });

  it('al cambiar de categoria actualiza el filtro y reinicia la pagina', async () => {
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{ pageNumber: 3 }} onChange={onChange} />);

    await waitFor(() => expect(screen.getByRole('option', { name: 'Deportes' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Categoria'), '6');

    expect(onChange).toHaveBeenCalledWith({ pageNumber: 1, categoriaId: 6 });
  });

  it('al volver a "Todas" en categoria limpia el filtro', async () => {
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{ categoriaId: 6 }} onChange={onChange} />);

    await waitFor(() => expect(screen.getByRole('option', { name: 'Deportes' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Categoria'), '0');

    expect(onChange).toHaveBeenCalledWith({ categoriaId: undefined, pageNumber: 1 });
  });

  it('deshabilita provincia y distrito cuando no hay departamento seleccionado', () => {
    render(<SearchFilters filtros={{}} onChange={vi.fn()} />);

    expect(screen.getByLabelText('Provincia')).toBeDisabled();
    expect(screen.getByLabelText('Distrito')).toBeDisabled();
  });

  it('al cambiar de departamento limpia provincia y distrito', async () => {
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{ departamentoId: 1, provinciaId: 1, distritoId: 1 }} onChange={onChange} />);

    await waitFor(() => expect(screen.getByRole('option', { name: 'Ayacucho' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Departamento'), '0');

    expect(onChange).toHaveBeenCalledWith({
      departamentoId: undefined,
      provinciaId: undefined,
      distritoId: undefined,
      pageNumber: 1,
    });
  });

  it('al cambiar de provincia actualiza el filtro y limpia el distrito', async () => {
    server.use(
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
    );
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{ departamentoId: 1, distritoId: 5 }} onChange={onChange} />);

    await waitFor(() => expect(screen.getByRole('option', { name: 'Huamanga' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Provincia'), '1');

    expect(onChange).toHaveBeenCalledWith({
      departamentoId: 1,
      distritoId: undefined,
      provinciaId: 1,
      pageNumber: 1,
    });
  });

  it('al volver a "Todas" en provincia limpia el filtro', async () => {
    server.use(
      http.get('*/geo/provincias', () =>
        respuestaOk([{ id: 1, ubigeo: '0501', nombre: 'Huamanga', departamentoId: 1 }]),
      ),
    );
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{ departamentoId: 1, provinciaId: 1 }} onChange={onChange} />);

    await waitFor(() => expect(screen.getByRole('option', { name: 'Huamanga' })).toBeInTheDocument());
    await usuario.selectOptions(screen.getByLabelText('Provincia'), '0');

    expect(onChange).toHaveBeenCalledWith({
      departamentoId: 1,
      provinciaId: undefined,
      distritoId: undefined,
      pageNumber: 1,
    });
  });

  it('al cambiar de distrito actualiza el filtro', async () => {
    server.use(
      http.get('*/geo/distritos', () =>
        respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }]),
      ),
    );
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<SearchFilters filtros={{ departamentoId: 1, provinciaId: 1 }} onChange={onChange} />);

    const selectDistrito = screen.getByLabelText('Distrito');
    await waitFor(() => expect(within(selectDistrito).getByRole('option', { name: 'Ayacucho' })).toBeInTheDocument());
    await usuario.selectOptions(selectDistrito, '1');

    expect(onChange).toHaveBeenCalledWith({
      departamentoId: 1,
      provinciaId: 1,
      distritoId: 1,
      pageNumber: 1,
    });
  });

  it('al volver a "Todos" en distrito limpia el filtro', async () => {
    server.use(
      http.get('*/geo/distritos', () =>
        respuestaOk([{ id: 1, ubigeo: '050101', nombre: 'Ayacucho', provinciaId: 1 }]),
      ),
    );
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(
      <SearchFilters filtros={{ departamentoId: 1, provinciaId: 1, distritoId: 1 }} onChange={onChange} />,
    );

    const selectDistrito = screen.getByLabelText('Distrito');
    await waitFor(() => expect(within(selectDistrito).getByRole('option', { name: 'Ayacucho' })).toBeInTheDocument());
    await usuario.selectOptions(selectDistrito, '0');

    expect(onChange).toHaveBeenCalledWith({
      departamentoId: 1,
      provinciaId: 1,
      distritoId: undefined,
      pageNumber: 1,
    });
  });
});
