import { FileClock, LayoutDashboard, Package, ShieldAlert, Tags, Users } from 'lucide-react';
import { NavLink } from 'react-router-dom';
import { useAuthStore } from '../../stores/authStore';

// Enlaces segun la matriz de permisos por rol (Seguridad.md SS6).
export default function Sidebar() {
  const usuario = useAuthStore((state) => state.usuario);
  const esAdministrador = usuario?.rol === 'Administrador';

  const claseEnlace = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-sm rounded-md px-md py-sm text-body-s font-medium transition-all duration-200 ${
      isActive ? 'bg-primary-soft text-primary shadow-soft' : 'text-text hover:bg-bg-alt hover:translate-x-0.5'
    }`;

  return (
    <aside className="sticky top-0 flex h-screen w-56 flex-col gap-xs border-r border-text-secondary/10 bg-bg p-md">
      <NavLink to="/admin" end className={claseEnlace}>
        <LayoutDashboard size={20} aria-hidden="true" />
        Dashboard
      </NavLink>
      <NavLink to="/admin/objects" className={claseEnlace}>
        <Package size={20} aria-hidden="true" />
        Objetos
      </NavLink>
      <NavLink to="/admin/reports" className={claseEnlace}>
        <ShieldAlert size={20} aria-hidden="true" />
        Reportes
      </NavLink>
      {esAdministrador && (
        <>
          <NavLink to="/admin/users" className={claseEnlace}>
            <Users size={20} aria-hidden="true" />
            Usuarios
          </NavLink>
          <NavLink to="/admin/audit-logs" className={claseEnlace}>
            <FileClock size={20} aria-hidden="true" />
            Auditoria
          </NavLink>
          <NavLink to="/admin/categories" className={claseEnlace}>
            <Tags size={20} aria-hidden="true" />
            Categorias
          </NavLink>
        </>
      )}
    </aside>
  );
}
