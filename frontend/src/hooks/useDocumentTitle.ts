import { useEffect } from 'react';

const SUFIJO = 'Intercambio Ayacucho';

// Titulo de pagina por ruta (WCAG 2.4.2 "Titulado de paginas").
export function useDocumentTitle(titulo: string) {
  useEffect(() => {
    const anterior = document.title;
    document.title = `${titulo} · ${SUFIJO}`;
    return () => {
      document.title = anterior;
    };
  }, [titulo]);
}
