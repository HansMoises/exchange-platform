import { useState } from 'react';
import { Menu, Recycle, X } from 'lucide-react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { authService } from '../../features/auth/services/authService';
import NotificationBell from '../../features/notifications/components/NotificationBell';
import { useToast } from '../../hooks/useToast';
import { useAuthStore } from '../../stores/authStore';
import Button from '../ui/Button';

const enlacesPublicos = [{ to: '/search', label: 'Buscar objetos' }];

const enlacesAutenticado = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/publish', label: 'Publicar objeto' },
  { to: '/exchanges', label: 'Intercambios' },
  { to: '/favorites', label: 'Favoritos' },
];

// Mapa de navegacion segun UX.md SS4; mobile-first con menu hamburguesa (UI.md SS9).
export default function Navbar() {
  const [menuAbierto, setMenuAbierto] = useState(false);
  const navigate = useNavigate();
  const { mostrar } = useToast();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const usuario = useAuthStore((state) => state.usuario);

  const enlaces = isAuthenticated ? [...enlacesPublicos, ...enlacesAutenticado] : enlacesPublicos;

  const cerrarSesion = async () => {
    setMenuAbierto(false);
    await authService.cerrarSesion();
    mostrar('Sesion cerrada exitosamente.', 'success');
    navigate('/');
  };

  const claseEnlace = ({ isActive }: { isActive: boolean }) =>
    `relative py-xs text-body-s font-medium transition-colors duration-200 after:absolute after:-bottom-1 after:left-0 after:h-0.5 after:rounded-full after:bg-primary after:transition-all after:duration-200 ${
      isActive ? 'text-primary after:w-full' : 'text-text after:w-0 hover:text-primary hover:after:w-full'
    }`;

  return (
    <header className="sticky top-0 z-40 border-b border-text-secondary/10 bg-bg/80 backdrop-blur-md">
      <div className="mx-auto flex max-w-7xl items-center justify-between p-md">
        <Link to="/" className="flex items-center gap-xs font-display text-h3 font-bold text-primary">
          <Recycle size={22} aria-hidden="true" />
          Intercambio Ayacucho
        </Link>

        <nav className="hidden items-center gap-lg md:flex" aria-label="Principal">
          {enlaces.map((enlace) => (
            <NavLink key={enlace.to} to={enlace.to} className={claseEnlace}>
              {enlace.label}
            </NavLink>
          ))}
        </nav>

        <div className="hidden items-center gap-sm md:flex">
          {isAuthenticated ? (
            <>
              <NotificationBell />
              <Link to="/profile" className="text-body-s font-medium text-text hover:text-primary">
                {usuario?.nombres}
              </Link>
              <Button variant="texto" onClick={cerrarSesion}>
                Cerrar sesion
              </Button>
            </>
          ) : (
            <>
              <Link to="/login">
                <Button variant="secundario">Iniciar sesion</Button>
              </Link>
              <Link to="/register">
                <Button variant="primario">Registrarse</Button>
              </Link>
            </>
          )}
        </div>

        <button
          type="button"
          className="flex min-h-11 min-w-11 items-center justify-center rounded-full transition-colors duration-200 hover:bg-bg-alt md:hidden"
          aria-label={menuAbierto ? 'Cerrar menu' : 'Abrir menu'}
          aria-expanded={menuAbierto}
          onClick={() => setMenuAbierto((valor) => !valor)}
        >
          {menuAbierto ? <X size={24} aria-hidden="true" /> : <Menu size={24} aria-hidden="true" />}
        </button>
      </div>

      {menuAbierto && (
        <nav
          className="flex animate-slide-up flex-col gap-sm border-t border-text-secondary/10 bg-bg p-md md:hidden"
          aria-label="Principal movil"
        >
          {enlaces.map((enlace) => (
            <NavLink key={enlace.to} to={enlace.to} className={claseEnlace} onClick={() => setMenuAbierto(false)}>
              {enlace.label}
            </NavLink>
          ))}
          {isAuthenticated ? (
            <>
              <NavLink to="/profile" className={claseEnlace} onClick={() => setMenuAbierto(false)}>
                {usuario?.nombres}
              </NavLink>
              <Button variant="texto" onClick={cerrarSesion}>
                Cerrar sesion
              </Button>
            </>
          ) : (
            <>
              <Link to="/login" onClick={() => setMenuAbierto(false)}>
                <Button variant="secundario" className="w-full">
                  Iniciar sesion
                </Button>
              </Link>
              <Link to="/register" onClick={() => setMenuAbierto(false)}>
                <Button variant="primario" className="w-full">
                  Registrarse
                </Button>
              </Link>
            </>
          )}
        </nav>
      )}
    </header>
  );
}
