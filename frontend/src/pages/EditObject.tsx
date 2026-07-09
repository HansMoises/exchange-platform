import { useParams } from 'react-router-dom';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import ObjectForm from '../features/objects/components/ObjectForm';
import { useObjeto } from '../features/objects/hooks/useObjeto';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function EditObject() {
  const { id } = useParams<{ id: string }>();
  const { objeto, isLoading, error } = useObjeto(id);
  useDocumentTitle(objeto ? `Editar ${objeto.titulo}` : 'Editar objeto');

  if (isLoading) return <Loading lineas={6} className="p-lg" />;
  if (error || !objeto) return <EmptyState mensaje="No encontramos este objeto." />;

  return (
    <div className="mx-auto flex max-w-lg animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Editar objeto</h1>
      <ObjectForm objetoId={objeto.id} valoresIniciales={objeto} />
    </div>
  );
}
