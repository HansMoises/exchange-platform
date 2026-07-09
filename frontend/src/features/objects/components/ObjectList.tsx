import EmptyState from '../../../components/ui/EmptyState';
import Loading from '../../../components/ui/Loading';
import Pagination from '../../../components/ui/Pagination';
import type { PagedResult } from '../../../types/api.types';
import type { ObjetoDto } from '../types/objeto.types';
import ObjectCard from './ObjectCard';

interface ObjectListProps {
  resultado: PagedResult<ObjetoDto> | null;
  isLoading: boolean;
  error: boolean;
  onCambiarPagina: (pagina: number) => void;
  onReintentar: () => void;
}

// Contempla carga/vacio/error (UX.md SS6): nunca pantalla en blanco.
export default function ObjectList({ resultado, isLoading, error, onCambiarPagina, onReintentar }: ObjectListProps) {
  if (isLoading) return <Loading lineas={6} />;

  if (error) {
    return <EmptyState mensaje="No pudimos cargar los objetos." accion="Reintentar" onAccion={onReintentar} />;
  }

  if (!resultado || resultado.items.length === 0) {
    return <EmptyState mensaje="Aun no hay objetos publicados." />;
  }

  return (
    <div className="flex flex-col gap-lg">
      <div className="grid grid-cols-1 gap-md sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {resultado.items.map((objeto) => (
          <ObjectCard key={objeto.id} objeto={objeto} />
        ))}
      </div>
      <Pagination
        pageNumber={resultado.pageNumber}
        totalPages={resultado.totalPages}
        hasPrevious={resultado.hasPrevious}
        hasNext={resultado.hasNext}
        onChange={onCambiarPagina}
      />
    </div>
  );
}
