import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import EstadoReporteBadge from '../components/EstadoReporteBadge';

describe('EstadoReporteBadge', () => {
  it.each([
    [0, 'Pendiente'],
    [1, 'EnRevision'],
    [2, 'Resuelto'],
    [3, 'Descartado'],
  ])('muestra el estado correcto para el indice %i', (indice, esperado) => {
    render(<EstadoReporteBadge estadoIndice={indice} />);
    expect(screen.getByText(esperado)).toBeInTheDocument();
  });
});
