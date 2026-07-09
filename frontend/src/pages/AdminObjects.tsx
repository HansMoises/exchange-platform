import { useEffect, useState } from 'react';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import Pagination from '../components/ui/Pagination';
import { adminService } from '../features/admin/services/adminService';
import { ESTADOS_OBJETO_POR_INDICE, type AdminObjetoDto } from '../features/admin/types/admin.types';
import EstadoObjetoBadge from '../features/objects/components/EstadoObjetoBadge';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useToast } from '../hooks/useToast';
import { obtenerMensajeError } from '../utils/apiError';

const PAGE_SIZE = 20;

export default function AdminObjects() {
  useDocumentTitle('Gestion de objetos');
  const { mostrar } = useToast();
  const [pageNumber, setPageNumber] = useState(1);
  const [objetos, setObjetos] = useState<AdminObjetoDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [procesandoId, setProcesandoId] = useState<string | null>(null);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    adminService
      .obtenerObjetos({ pageNumber, pageSize: PAGE_SIZE })
      .then(({ data }) => {
        setObjetos(data.data?.items ?? []);
        setTotalPages(data.data?.totalPages ?? 1);
      })
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, [pageNumber]);

  const suspenderORestaurar = async (objeto: AdminObjetoDto) => {
    const estado = ESTADOS_OBJETO_POR_INDICE[objeto.estado];
    setProcesandoId(objeto.id);
    try {
      if (estado === 'Suspendido') {
        await adminService.restaurarObjeto(objeto.id);
        mostrar('Objeto restaurado.', 'success');
      } else {
        await adminService.suspenderObjeto(objeto.id);
        mostrar('Objeto suspendido.', 'success');
      }
      cargar();
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos actualizar el objeto.'), 'error');
    } finally {
      setProcesandoId(null);
    }
  };

  return (
    <div className="flex flex-col gap-lg">
      <h1 className="text-h2 text-primary">Gestion de objetos</h1>

      {isLoading && <Loading lineas={6} />}

      {!isLoading && error && (
        <EmptyState mensaje="No pudimos cargar los objetos." accion="Reintentar" onAccion={cargar} />
      )}

      {!isLoading && !error && objetos.length === 0 && <EmptyState mensaje="No hay objetos publicados." />}

      {!isLoading && !error && objetos.length > 0 && (
        <div className="overflow-x-auto rounded-lg bg-bg shadow-card">
          <table className="w-full text-left text-body-s">
            <thead>
              <tr className="border-b border-text-secondary/10 bg-bg-alt">
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Titulo
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Estado
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody>
              {objetos.map((objeto) => {
                const estado = ESTADOS_OBJETO_POR_INDICE[objeto.estado];
                return (
                  <tr
                    key={objeto.id}
                    className="border-b border-text-secondary/10 transition-colors duration-150 hover:bg-bg-alt/60"
                  >
                    <td className="p-sm text-text">{objeto.titulo}</td>
                    <td className="p-sm">
                      <EstadoObjetoBadge estado={estado} />
                    </td>
                    <td className="p-sm">
                      <Button
                        variant={estado === 'Suspendido' ? 'secundario' : 'peligro'}
                        disabled={procesandoId === objeto.id}
                        onClick={() => suspenderORestaurar(objeto)}
                      >
                        {estado === 'Suspendido' ? 'Restaurar' : 'Suspender'}
                      </Button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <Pagination
        pageNumber={pageNumber}
        totalPages={totalPages}
        hasPrevious={pageNumber > 1}
        hasNext={pageNumber < totalPages}
        onChange={setPageNumber}
      />
    </div>
  );
}
