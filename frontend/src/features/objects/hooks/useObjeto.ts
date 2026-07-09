import { useEffect, useState } from 'react';
import { objetoService } from '../services/objetoService';
import type { ObjetoDto } from '../types/objeto.types';

export function useObjeto(id: string | undefined) {
  const [objeto, setObjeto] = useState<ObjetoDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!id) return;
    setIsLoading(true);
    setError(false);
    objetoService
      .obtener(id)
      .then(({ data }) => setObjeto(data.data))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  }, [id]);

  return { objeto, isLoading, error };
}
