import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import ReportForm from '../components/ReportForm';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

describe('ReportForm', () => {
  it('muestra error de validacion si no se elige un motivo', async () => {
    const usuario = userEvent.setup();
    render(<ReportForm entidadTipo="Objeto" entidadId="obj1" />);

    await usuario.click(screen.getByRole('button', { name: 'Enviar reporte' }));

    expect(await screen.findByRole('alert')).toBeInTheDocument();
  });

  it('envia el reporte y muestra el mensaje de agradecimiento', async () => {
    server.use(http.post('*/reports', () => respuestaOk({ id: 'reporte-1' })));
    const onExito = vi.fn();

    const usuario = userEvent.setup();
    render(<ReportForm entidadTipo="Objeto" entidadId="obj1" onExito={onExito} />);

    await usuario.selectOptions(screen.getByLabelText('Motivo'), 'Spam');
    await usuario.click(screen.getByRole('button', { name: 'Enviar reporte' }));

    expect(await screen.findByText('Gracias, revisaremos tu reporte.')).toBeInTheDocument();
    expect(onExito).toHaveBeenCalled();
  });

  it('muestra un toast de error si el envio del reporte falla', async () => {
    useToastStore.setState({ toasts: [] });
    server.use(http.post('*/reports', () => HttpResponse.error()));

    const usuario = userEvent.setup();
    render(<ReportForm entidadTipo="Objeto" entidadId="obj1" />);

    await usuario.selectOptions(screen.getByLabelText('Motivo'), 'Spam');
    await usuario.click(screen.getByRole('button', { name: 'Enviar reporte' }));

    await vi.waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos enviar tu reporte.')).toBe(true);
    });
  });

  it('muestra error de validacion si la descripcion supera los 500 caracteres', async () => {
    const usuario = userEvent.setup();
    render(<ReportForm entidadTipo="Objeto" entidadId="obj1" />);

    await usuario.selectOptions(screen.getByLabelText('Motivo'), 'Spam');
    fireEvent.change(screen.getByLabelText('Descripcion (opcional)'), { target: { value: 'a'.repeat(501) } });
    await usuario.click(screen.getByRole('button', { name: 'Enviar reporte' }));

    expect(await screen.findByText('Maximo 500 caracteres.')).toBeInTheDocument();
  });

  it('muestra el texto de carga mientras se envia el reporte', async () => {
    server.use(
      http.post('*/reports', async () => {
        await new Promise((resolve) => setTimeout(resolve, 30));
        return respuestaOk({ id: 'reporte-1' });
      }),
    );

    const usuario = userEvent.setup();
    render(<ReportForm entidadTipo="Objeto" entidadId="obj1" />);

    await usuario.selectOptions(screen.getByLabelText('Motivo'), 'Spam');
    const clickPromise = usuario.click(screen.getByRole('button', { name: 'Enviar reporte' }));

    expect(await screen.findByText('Enviando...')).toBeInTheDocument();
    await clickPromise;
  });
});
