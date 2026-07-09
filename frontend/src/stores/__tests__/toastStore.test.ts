import { act } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useToastStore } from '../toastStore';

describe('toastStore', () => {
  beforeEach(() => {
    useToastStore.setState({ toasts: [] });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('mostrar agrega un toast con variante info por defecto', () => {
    act(() => useToastStore.getState().mostrar('Operacion exitosa.'));

    const { toasts } = useToastStore.getState();
    expect(toasts).toHaveLength(1);
    expect(toasts[0].mensaje).toBe('Operacion exitosa.');
    expect(toasts[0].variant).toBe('info');
  });

  it('mostrar respeta la variante indicada', () => {
    act(() => useToastStore.getState().mostrar('Algo salio mal.', 'error'));

    expect(useToastStore.getState().toasts[0].variant).toBe('error');
  });

  it('cerrar quita solo el toast indicado', () => {
    act(() => {
      useToastStore.getState().mostrar('Primero');
      useToastStore.getState().mostrar('Segundo');
    });

    const [primero, segundo] = useToastStore.getState().toasts;
    act(() => useToastStore.getState().cerrar(primero.id));

    const restantes = useToastStore.getState().toasts;
    expect(restantes).toHaveLength(1);
    expect(restantes[0].id).toBe(segundo.id);
  });

  it('se auto-cierra despues de 4 segundos', () => {
    vi.useFakeTimers();

    act(() => useToastStore.getState().mostrar('Desaparece solo'));
    expect(useToastStore.getState().toasts).toHaveLength(1);

    act(() => vi.advanceTimersByTime(4000));

    expect(useToastStore.getState().toasts).toHaveLength(0);
  });
});
