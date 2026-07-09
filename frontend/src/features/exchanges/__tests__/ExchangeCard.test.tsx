import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import type { IntercambioDto } from '../types/intercambio.types';
import ExchangeCard from '../components/ExchangeCard';

function intercambioDePrueba(overrides: Partial<IntercambioDto> = {}): IntercambioDto {
  return {
    id: 'ex1',
    objetoSolicitadoId: 'obj1',
    objetoSolicitadoTitulo: 'Bicicleta rodado 26',
    objetoOfrecidoId: 'obj2',
    objetoOfrecidoTitulo: 'Libros de cocina',
    usuarioSolicitanteId: 'u1',
    usuarioSolicitanteNombres: 'Rosa Quispe',
    usuarioPropietarioId: 'u2',
    usuarioPropietarioNombres: 'Juan Perez',
    estado: 'Pendiente',
    mensajeInicial: null,
    confirmacionSolicitante: false,
    confirmacionPropietario: false,
    fechaAceptacion: null,
    fechaCompletado: null,
    creadoEn: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

function renderCard(intercambio: IntercambioDto, usuarioActualId: string) {
  return render(
    <MemoryRouter>
      <ExchangeCard intercambio={intercambio} usuarioActualId={usuarioActualId} />
    </MemoryRouter>,
  );
}

describe('ExchangeCard', () => {
  it('muestra "Enviada" y el nombre del propietario cuando el usuario es el solicitante', () => {
    renderCard(intercambioDePrueba(), 'u1');

    expect(screen.getByText('Enviada')).toBeInTheDocument();
    expect(screen.getByText('Con Juan Perez')).toBeInTheDocument();
  });

  it('muestra "Recibida" y el nombre del solicitante cuando el usuario es el propietario', () => {
    renderCard(intercambioDePrueba(), 'u2');

    expect(screen.getByText('Recibida')).toBeInTheDocument();
    expect(screen.getByText('Con Rosa Quispe')).toBeInTheDocument();
  });

  it('muestra los titulos de ambos objetos y el estado', () => {
    renderCard(intercambioDePrueba({ estado: 'Aceptado' }), 'u1');

    expect(screen.getByText('Bicicleta rodado 26')).toBeInTheDocument();
    expect(screen.getByText('Libros de cocina')).toBeInTheDocument();
    expect(screen.getByText('Aceptado')).toBeInTheDocument();
  });

  it('enlaza al detalle del intercambio', () => {
    renderCard(intercambioDePrueba(), 'u1');

    expect(screen.getByRole('link', { name: 'Ver detalle' })).toHaveAttribute('href', '/exchanges/ex1');
  });
});
