import { useEffect, useState } from 'react';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import Pagination from '../components/ui/Pagination';
import { adminService } from '../features/admin/services/adminService';
import type { AuditLogDto } from '../features/admin/types/admin.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

const PAGE_SIZE = 50;

// Solo lectura (RN-062/063): el log de auditoria es inmutable.
export default function AdminAuditLogs() {
  useDocumentTitle('Auditoria');
  const [pageNumber, setPageNumber] = useState(1);
  const [logs, setLogs] = useState<AuditLogDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    adminService
      .obtenerAuditLogs({ pageNumber, pageSize: PAGE_SIZE })
      .then(({ data }) => {
        setLogs(data.data?.items ?? []);
        setTotalPages(data.data?.totalPages ?? 1);
      })
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, [pageNumber]);

  return (
    <div className="flex flex-col gap-lg">
      <h1 className="text-h2 text-primary">Auditoria</h1>

      {isLoading && <Loading lineas={6} />}

      {!isLoading && error && (
        <EmptyState mensaje="No pudimos cargar la auditoria." accion="Reintentar" onAccion={cargar} />
      )}

      {!isLoading && !error && logs.length === 0 && <EmptyState mensaje="No hay registros de auditoria." />}

      {!isLoading && !error && logs.length > 0 && (
        <div className="overflow-x-auto rounded-lg bg-bg shadow-card">
          <table className="w-full text-left text-body-s">
            <thead>
              <tr className="border-b border-text-secondary/10 bg-bg-alt">
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Accion
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Entidad
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Resultado
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  IP
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Fecha
                </th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log) => (
                <tr
                  key={log.id}
                  className="border-b border-text-secondary/10 transition-colors duration-150 hover:bg-bg-alt/60"
                >
                  <td className="p-sm text-text">{log.accion}</td>
                  <td className="p-sm text-text">{log.entidadTipo}</td>
                  <td className="p-sm text-text">{log.resultado}</td>
                  <td className="p-sm text-text-secondary">{log.ipAddress ?? '-'}</td>
                  <td className="p-sm text-text-secondary">{new Date(log.ocurridoEn).toLocaleString('es-PE')}</td>
                </tr>
              ))}
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
