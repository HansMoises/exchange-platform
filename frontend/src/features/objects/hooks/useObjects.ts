import { useEffect, useState } from 'react';
import type { PagedResult } from '../../../types/api.types';
import { objetoService } from '../services/objetoService';
import type { ObjetoDto, ObjetosFiltroParams } from '../types/objeto.types';

// Carga/paginacion/filtros de la lista de objetos (Frontend.md SS3, anatomia de feature).
export function useObjects(filtros: ObjetosFiltroParams) {
  const [resultado, setResultado] = useState<PagedResult<ObjetoDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [intento, setIntento] = useState(0);

  useEffect(() => {
    setIsLoading(true);
    setError(false);
    objetoService
      .listar(filtros)
      .then(({ data }) => setResultado(data.data))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    filtros.search,
    filtros.categoriaId,
    filtros.departamentoId,
    filtros.provinciaId,
    filtros.distritoId,
    filtros.pageNumber,
    filtros.pageSize,
    intento,
  ]);

  const recargar = () => setIntento((valor) => valor + 1);

  return { resultado, isLoading, error, recargar };
}
