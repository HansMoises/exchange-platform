import { useEffect, useState } from 'react';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import Pagination from '../components/ui/Pagination';
import { adminService } from '../features/admin/services/adminService';
import EstadoReporteBadge from '../features/admin/components/EstadoReporteBadge';
import { ESTADOS_REPORTE_POR_INDICE, type AdminReporteDto } from '../features/admin/types/admin.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useToast } from '../hooks/useToast';
import { obtenerMensajeError } from '../utils/apiError';

const PAGE_SIZE = 20;

export default function AdminReports() {
  useDocumentTitle('Gestion de reportes');
  const { mostrar } = useToast();
  const [pageNumber, setPageNumber] = useState(1);
  const [reportes, setReportes] = useState<AdminReporteDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [procesandoId, setProcesandoId] = useState<string | null>(null);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    adminService
      .obtenerReportes({ pageNumber, pageSize: PAGE_SIZE })
      .then(({ data }) => {
        setReportes(data.data?.items ?? []);
        setTotalPages(data.data?.totalPages ?? 1);
      })
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, [pageNumber]);

  const resolver = async (id: string) => {
    setProcesandoId(id);
    try {
      await adminService.resolverReporte(id);
      mostrar('Reporte resuelto.', 'success');
      cargar();
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos resolver el reporte.'), 'error');
    } finally {
      setProcesandoId(null);
    }
  };

  const descartar = async (id: string) => {
    setProcesandoId(id);
    try {
      await adminService.descartarReporte(id);
      mostrar('Reporte descartado.', 'success');
      cargar();
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos descartar el reporte.'), 'error');
    } finally {
      setProcesandoId(null);
    }
  };

  return (
    <div className="flex flex-col gap-lg">
      <h1 className="text-h2 text-primary">Gestion de reportes</h1>

      {isLoading && <Loading lineas={6} />}

      {!isLoading && error && (
        <EmptyState mensaje="No pudimos cargar los reportes." accion="Reintentar" onAccion={cargar} />
      )}

      {!isLoading && !error && reportes.length === 0 && <EmptyState mensaje="No hay reportes." />}

      {!isLoading && !error && reportes.length > 0 && (
        <div className="overflow-x-auto rounded-lg bg-bg shadow-card">
          <table className="w-full text-left text-body-s">
            <thead>
              <tr className="border-b border-text-secondary/10 bg-bg-alt">
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Entidad
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Motivo
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
              {reportes.map((reporte) => {
                const pendiente =
                  ESTADOS_REPORTE_POR_INDICE[reporte.estadoReporte] !== 'Resuelto' &&
                  ESTADOS_REPORTE_POR_INDICE[reporte.estadoReporte] !== 'Descartado';
                return (
                  <tr
                    key={reporte.id}
                    className="border-b border-text-secondary/10 transition-colors duration-150 hover:bg-bg-alt/60"
                  >
                    <td className="p-sm text-text">{reporte.entidadTipo}</td>
                    <td className="p-sm text-text">{reporte.motivo}</td>
                    <td className="p-sm">
                      <EstadoReporteBadge estadoIndice={reporte.estadoReporte} />
                    </td>
                    <td className="p-sm">
                      {pendiente && (
                        <div className="flex gap-sm">
                          <Button
                            variant="primario"
                            disabled={procesandoId === reporte.id}
                            onClick={() => resolver(reporte.id)}
                          >
                            Resolver
                          </Button>
                          <Button
                            variant="texto"
                            disabled={procesandoId === reporte.id}
                            onClick={() => descartar(reporte.id)}
                          >
                            Descartar
                          </Button>
                        </div>
                      )}
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
