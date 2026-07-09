import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { act } from 'react';
import { beforeEach, describe, expect, it } from 'vitest';
import { useToastStore } from '../../../stores/toastStore';
import ToastContainer from '../Toast';

describe('ToastContainer', () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
  });

  it('no renderiza nada si no hay toasts', () => {
    const { container } = render(<ToastContainer />);
    expect(container).toBeEmptyDOMElement();
  });

  it('muestra el mensaje de cada toast', () => {
    act(() => useToastStore.getState().mostrar('Operacion exitosa.', 'success'));
    render(<ToastContainer />);

    expect(screen.getByText('Operacion exitosa.')).toBeInTheDocument();
  });

  it('cierra el toast al hacer click en el boton de cerrar', async () => {
    act(() => useToastStore.getState().mostrar('Se cerrara', 'info'));
    const usuario = userEvent.setup();
    render(<ToastContainer />);

    await usuario.click(screen.getByRole('button', { name: 'Cerrar notificacion' }));

    expect(screen.queryByText('Se cerrara')).not.toBeInTheDocument();
  });
});
