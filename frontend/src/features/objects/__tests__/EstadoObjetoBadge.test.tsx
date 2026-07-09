import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { EstadoObjeto } from '../types/objeto.types';
import EstadoObjetoBadge from '../components/EstadoObjetoBadge';

describe('EstadoObjetoBadge', () => {
  it.each<EstadoObjeto>(['Disponible', 'Reservado', 'Intercambiado', 'Suspendido', 'Eliminado'])(
    'muestra el texto del estado %s',
    (estado) => {
      render(<EstadoObjetoBadge estado={estado} />);
      expect(screen.getByText(estado)).toBeInTheDocument();
    },
  );
});
