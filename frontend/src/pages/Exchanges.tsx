import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import ExchangeCard from '../features/exchanges/components/ExchangeCard';
import { intercambioService } from '../features/exchanges/services/intercambioService';
import type { IntercambioDto } from '../features/exchanges/types/intercambio.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useAuthStore } from '../stores/authStore';

export default function Exchanges() {
  useDocumentTitle('Mis intercambios');
  const navigate = useNavigate();
  const usuario = useAuthStore((state) => state.usuario);
  const [intercambios, setIntercambios] = useState<IntercambioDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    intercambioService
      .listar({ pageNumber: 1, pageSize: 20 })
      .then(({ data }) => setIntercambios(data.data ?? []))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, []);

  return (
    <div className="mx-auto flex max-w-4xl animate-fade-in flex-col gap-md p-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Mis intercambios</h1>

      {isLoading && <Loading lineas={6} />}

      {!isLoading && error && (
        <EmptyState mensaje="No pudimos cargar tus intercambios." accion="Reintentar" onAccion={cargar} />
      )}

      {!isLoading && !error && intercambios.length === 0 && (
        <EmptyState
          mensaje="Aun no tienes intercambios."
          accion="Buscar objetos"
          onAccion={() => navigate('/search')}
        />
      )}

      {!isLoading && !error && intercambios.length > 0 && usuario && (
        <div className="flex flex-col gap-md">
          {intercambios.map((intercambio) => (
            <ExchangeCard key={intercambio.id} intercambio={intercambio} usuarioActualId={usuario.id} />
          ))}
        </div>
      )}
    </div>
  );
}
