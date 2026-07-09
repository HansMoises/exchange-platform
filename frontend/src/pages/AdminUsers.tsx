import { useEffect, useState } from 'react';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';
import Input from '../components/ui/Input';
import Loading from '../components/ui/Loading';
import Pagination from '../components/ui/Pagination';
import { adminService } from '../features/admin/services/adminService';
import { ROLES_POR_ID, type AdminUsuarioDto } from '../features/admin/types/admin.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useToast } from '../hooks/useToast';
import { obtenerMensajeError } from '../utils/apiError';

const PAGE_SIZE = 20;

export default function AdminUsers() {
  useDocumentTitle('Gestion de usuarios');
  const { mostrar } = useToast();
  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [usuarios, setUsuarios] = useState<AdminUsuarioDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [procesandoId, setProcesandoId] = useState<string | null>(null);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    adminService
      .obtenerUsuarios({ search: search || undefined, pageNumber, pageSize: PAGE_SIZE })
      .then(({ data }) => {
        setUsuarios(data.data?.items ?? []);
        setTotalPages(data.data?.totalPages ?? 1);
      })
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(cargar, [pageNumber]);

  const buscar = (evento: React.FormEvent) => {
    evento.preventDefault();
    setPageNumber(1);
    cargar();
  };

  const alternarActivo = async (usuario: AdminUsuarioDto) => {
    setProcesandoId(usuario.id);
    try {
      if (usuario.isActive) {
        await adminService.desactivarUsuario(usuario.id);
        mostrar('Usuario desactivado.', 'success');
      } else {
        await adminService.activarUsuario(usuario.id);
        mostrar('Usuario activado.', 'success');
      }
      cargar();
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos actualizar el usuario.'), 'error');
    } finally {
      setProcesandoId(null);
    }
  };

  return (
    <div className="flex flex-col gap-lg">
      <h1 className="text-h2 text-primary">Gestion de usuarios</h1>

      <form onSubmit={buscar} className="flex items-end gap-sm">
        <div className="flex-1">
          <Input
            label="Buscar por nombre o correo"
            value={search}
            onChange={(evento) => setSearch(evento.target.value)}
          />
        </div>
        <Button type="submit" variant="primario">
          Buscar
        </Button>
      </form>

      {isLoading && <Loading lineas={6} />}

      {!isLoading && error && (
        <EmptyState mensaje="No pudimos cargar los usuarios." accion="Reintentar" onAccion={cargar} />
      )}

      {!isLoading && !error && usuarios.length === 0 && <EmptyState mensaje="No se encontraron usuarios." />}

      {!isLoading && !error && usuarios.length > 0 && (
        <div className="overflow-x-auto rounded-lg bg-bg shadow-card">
          <table className="w-full text-left text-body-s">
            <thead>
              <tr className="border-b border-text-secondary/10 bg-bg-alt">
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Nombre
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Correo
                </th>
                <th scope="col" className="p-sm text-caption font-semibold uppercase tracking-wide text-text-secondary">
                  Rol
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
              {usuarios.map((usuario) => (
                <tr
                  key={usuario.id}
                  className="border-b border-text-secondary/10 transition-colors duration-150 hover:bg-bg-alt/60"
                >
                  <td className="p-sm text-text">
                    {usuario.nombres} {usuario.apellidos}
                  </td>
                  <td className="p-sm text-text">{usuario.email}</td>
                  <td className="p-sm text-text">{ROLES_POR_ID[usuario.rolId] ?? usuario.rolId}</td>
                  <td className="p-sm text-text">{usuario.isActive ? 'Activo' : 'Inactivo'}</td>
                  <td className="p-sm">
                    <Button
                      variant={usuario.isActive ? 'peligro' : 'secundario'}
                      disabled={procesandoId === usuario.id}
                      onClick={() => alternarActivo(usuario)}
                    >
                      {usuario.isActive ? 'Desactivar' : 'Activar'}
                    </Button>
                  </td>
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
