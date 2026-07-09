import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import Footer from '../Footer';

describe('Footer', () => {
  it('muestra el nombre de la plataforma y el anio actual', () => {
    render(<Footer />);
    const anioActual = new Date().getFullYear();
    expect(
      screen.getByText(`© ${anioActual} Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Peru.`),
    ).toBeInTheDocument();
  });
});
