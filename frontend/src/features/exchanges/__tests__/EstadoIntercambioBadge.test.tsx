import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { EstadoIntercambio } from '../types/intercambio.types';
import EstadoIntercambioBadge from '../components/EstadoIntercambioBadge';

describe('EstadoIntercambioBadge', () => {
  it.each<[EstadoIntercambio, string]>([
    ['Pendiente', 'Pendiente'],
    ['Aceptado', 'Aceptado'],
    ['PendienteConfirmacion', 'Por confirmar'],
    ['Completado', 'Completado'],
    ['Rechazado', 'Rechazado'],
    ['Cancelado', 'Cancelado'],
  ])('muestra la etiqueta correcta para %s', (estado, etiquetaEsperada) => {
    render(<EstadoIntercambioBadge estado={estado} />);
    expect(screen.getByText(etiquetaEsperada)).toBeInTheDocument();
  });
});
