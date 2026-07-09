import { useSearchParams } from 'react-router-dom';
import ObjectList from '../features/objects/components/ObjectList';
import SearchFilters from '../features/objects/components/SearchFilters';
import { useObjects } from '../features/objects/hooks/useObjects';
import type { ObjetosFiltroParams } from '../features/objects/types/objeto.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

function numeroOIndefinido(valor: string | null): number | undefined {
  if (!valor) return undefined;
  const numero = Number(valor);
  return Number.isNaN(numero) ? undefined : numero;
}

export default function Search() {
  useDocumentTitle('Buscar objetos');
  const [searchParams, setSearchParams] = useSearchParams();

  const filtros: ObjetosFiltroParams = {
    search: searchParams.get('search') ?? undefined,
    categoriaId: numeroOIndefinido(searchParams.get('categoriaId')),
    departamentoId: numeroOIndefinido(searchParams.get('departamentoId')),
    provinciaId: numeroOIndefinido(searchParams.get('provinciaId')),
    distritoId: numeroOIndefinido(searchParams.get('distritoId')),
    pageNumber: numeroOIndefinido(searchParams.get('pageNumber')) ?? 1,
    pageSize: 20,
  };

  const { resultado, isLoading, error, recargar } = useObjects(filtros);

  const actualizarFiltros = (nuevosFiltros: ObjetosFiltroParams) => {
    const params: Record<string, string> = {};
    if (nuevosFiltros.search) params.search = nuevosFiltros.search;
    if (nuevosFiltros.categoriaId) params.categoriaId = String(nuevosFiltros.categoriaId);
    if (nuevosFiltros.departamentoId) params.departamentoId = String(nuevosFiltros.departamentoId);
    if (nuevosFiltros.provinciaId) params.provinciaId = String(nuevosFiltros.provinciaId);
    if (nuevosFiltros.distritoId) params.distritoId = String(nuevosFiltros.distritoId);
    if (nuevosFiltros.pageNumber && nuevosFiltros.pageNumber > 1) {
      params.pageNumber = String(nuevosFiltros.pageNumber);
    }
    setSearchParams(params);
  };

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-lg p-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Buscar objetos</h1>
      <SearchFilters filtros={filtros} onChange={actualizarFiltros} />
      <ObjectList
        resultado={resultado}
        isLoading={isLoading}
        error={error}
        onReintentar={recargar}
        onCambiarPagina={(pagina) => actualizarFiltros({ ...filtros, pageNumber: pagina })}
      />
    </div>
  );
}
