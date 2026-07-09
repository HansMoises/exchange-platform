import { useEffect, useState } from 'react';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import { favoritoService } from '../features/favorites/services/favoritoService';
import ObjectCard from '../features/objects/components/ObjectCard';
import type { ObjetoDto } from '../features/objects/types/objeto.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function Favorites() {
  useDocumentTitle('Mis favoritos');
  const [favoritos, setFavoritos] = useState<ObjetoDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    favoritoService
      .listar()
      .then(({ data }) => setFavoritos(data.data ?? []))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, []);

  return (
    <div className="mx-auto flex max-w-7xl animate-fade-in flex-col gap-lg p-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Mis favoritos</h1>

      {isLoading && <Loading lineas={6} />}

      {!isLoading && error && (
        <EmptyState mensaje="No pudimos cargar tus favoritos." accion="Reintentar" onAccion={cargar} />
      )}

      {!isLoading && !error && favoritos.length === 0 && <EmptyState mensaje="Aun no tienes favoritos." />}

      {!isLoading && !error && favoritos.length > 0 && (
        <div className="grid grid-cols-1 gap-md sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {favoritos.map((objeto) => (
            <ObjectCard key={objeto.id} objeto={objeto} />
          ))}
        </div>
      )}
    </div>
  );
}
