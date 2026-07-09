import ObjectForm from '../features/objects/components/ObjectForm';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function PublishObject() {
  useDocumentTitle('Publicar objeto');

  return (
    <div className="mx-auto flex max-w-lg animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Publicar objeto</h1>
      <ObjectForm />
    </div>
  );
}
