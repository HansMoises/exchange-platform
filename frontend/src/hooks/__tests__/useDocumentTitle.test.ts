import { renderHook } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { useDocumentTitle } from '../useDocumentTitle';

describe('useDocumentTitle', () => {
  it('establece el titulo del documento con el sufijo de la plataforma', () => {
    renderHook(() => useDocumentTitle('Dashboard'));
    expect(document.title).toBe('Dashboard · Intercambio Ayacucho');
  });

  it('restaura el titulo anterior al desmontar', () => {
    document.title = 'Titulo original';
    const { unmount } = renderHook(() => useDocumentTitle('Perfil'));

    expect(document.title).toBe('Perfil · Intercambio Ayacucho');
    unmount();

    expect(document.title).toBe('Titulo original');
  });
});
