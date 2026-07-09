import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import type { RolUsuario } from '../types/usuario.types';

interface RoleBasedRouteProps {
  roles: RolUsuario[];
  children: ReactNode;
}

// Restringe el acceso segun la matriz de permisos por rol (Seguridad.md SS5).
export default function RoleBasedRoute({ roles, children }: RoleBasedRouteProps) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const usuario = useAuthStore((state) => state.usuario);

  if (!isAuthenticated || !usuario) {
    return <Navigate to="/login" replace />;
  }

  if (!roles.includes(usuario.rol)) {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
}
