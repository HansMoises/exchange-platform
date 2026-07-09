import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { server } from '../../../test/server';
import { useToastStore } from '../../../stores/toastStore';
import ImageUploader from '../components/ImageUploader';

function respuestaOk<T>(data: T) {
  return HttpResponse.json({ success: true, message: 'ok', data, errors: null, timestamp: '2026-01-01T00:00:00Z' });
}

function archivoDePrueba(nombre = 'foto.jpg') {
  return new File(['contenido'], nombre, { type: 'image/jpeg' });
}

describe('ImageUploader', () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
  });

  it('muestra las imagenes ya cargadas y permite quitarlas', async () => {
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    const { container } = render(<ImageUploader value={['https://example.com/foto1.jpg']} onChange={onChange} />);

    expect(container.querySelectorAll('img')).toHaveLength(1);
    await usuario.click(screen.getByRole('button', { name: 'Quitar imagen' }));

    expect(onChange).toHaveBeenCalledWith([]);
  });

  it('sube un archivo y agrega la url devuelta', async () => {
    server.use(http.post('*/objects/images', () => respuestaOk({ url: 'https://example.com/subida.jpg' })));
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<ImageUploader value={[]} onChange={onChange} />);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await usuario.upload(input, archivoDePrueba());

    await vi.waitFor(() => expect(onChange).toHaveBeenCalledWith(['https://example.com/subida.jpg']));
  });

  it('oculta el boton de agregar cuando ya hay 5 imagenes', () => {
    const cincoImagenes = Array.from({ length: 5 }, (_, i) => `https://example.com/foto${i}.jpg`);
    render(<ImageUploader value={cincoImagenes} onChange={vi.fn()} />);

    expect(document.querySelector('input[type="file"]')).not.toBeInTheDocument();
  });

  it('muestra el mensaje de error cuando se pasa la prop error', () => {
    render(<ImageUploader value={[]} onChange={vi.fn()} error="Debes subir al menos 1 imagen." />);
    expect(screen.getByRole('alert')).toHaveTextContent('Debes subir al menos 1 imagen.');
  });

  it('muestra un toast de error si la subida falla', async () => {
    server.use(http.post('*/objects/images', () => HttpResponse.error()));
    const onChange = vi.fn();
    const usuario = userEvent.setup();

    render(<ImageUploader value={[]} onChange={onChange} />);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await usuario.upload(input, archivoDePrueba());

    await vi.waitFor(() => {
      expect(useToastStore.getState().toasts.some((t) => t.mensaje === 'No pudimos subir la imagen.')).toBe(true);
    });
    expect(onChange).not.toHaveBeenCalled();
  });
});
