import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import Card from '../Card';

describe('Card', () => {
  it('renderiza su contenido', () => {
    render(<Card>Contenido de la tarjeta</Card>);
    expect(screen.getByText('Contenido de la tarjeta')).toBeInTheDocument();
  });

  it('combina className adicional con las clases base', () => {
    render(<Card className="mi-clase" data-testid="card" />);
    expect(screen.getByTestId('card')).toHaveClass('mi-clase', 'shadow-card', 'rounded-lg');
  });
});
