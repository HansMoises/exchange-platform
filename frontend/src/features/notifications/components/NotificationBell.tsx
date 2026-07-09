import { useEffect, useState } from 'react';
import { Bell } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { notificacionService } from '../services/notificacionService';
import type { NotificacionDto } from '../types/notificacion.types';

// Campana de notificaciones en el Navbar (UI.md SS8 iconografia).
export default function NotificationBell() {
  const navigate = useNavigate();
  const [notificaciones, setNotificaciones] = useState<NotificacionDto[]>([]);
  const [abierto, setAbierto] = useState(false);
  const [cargado, setCargado] = useState(false);

  const cargar = () => {
    notificacionService
      .listar()
      .then(({ data }) => setNotificaciones(data.data ?? []))
      .catch(() => setNotificaciones([]))
      .finally(() => setCargado(true));
  };

  useEffect(cargar, []);

  const noLeidas = notificaciones.filter((notificacion) => !notificacion.isLeida).length;

  const manejarClick = async (notificacion: NotificacionDto) => {
    setAbierto(false);
    if (!notificacion.isLeida) {
      try {
        await notificacionService.marcarLeida(notificacion.id);
        cargar();
      } catch {
        // silencioso: no bloquea la navegacion
      }
    }
    if (notificacion.entidadTipo === 'Intercambio' && notificacion.entidadId) {
      navigate(`/exchanges/${notificacion.entidadId}`);
    }
  };

  const marcarTodas = async () => {
    try {
      await notificacionService.marcarTodasLeidas();
      cargar();
    } catch {
      // silencioso
    }
  };

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setAbierto((valor) => !valor)}
        aria-label={`Notificaciones${noLeidas > 0 ? `, ${noLeidas} sin leer` : ''}`}
        aria-expanded={abierto}
        className="relative flex min-h-11 min-w-11 items-center justify-center rounded-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
      >
        <Bell size={20} aria-hidden="true" />
        {noLeidas > 0 && (
          <span className="absolute right-1 top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-error px-1 text-caption text-white">
            {noLeidas}
          </span>
        )}
      </button>

      {abierto && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setAbierto(false)} aria-hidden="true" />
          <div className="absolute right-0 z-50 mt-sm w-80 rounded-md bg-bg p-sm shadow-card">
            <div className="mb-sm flex items-center justify-between">
              <span className="text-label font-medium text-text">Notificaciones</span>
              {noLeidas > 0 && (
                <button
                  type="button"
                  onClick={marcarTodas}
                  className="text-caption text-secondary hover:text-secondary-hover"
                >
                  Marcar todas leidas
                </button>
              )}
            </div>
            {cargado && notificaciones.length === 0 && (
              <p className="p-sm text-body-s text-text-secondary">No tienes notificaciones.</p>
            )}
            <ul className="flex max-h-80 flex-col gap-xs overflow-y-auto">
              {notificaciones.map((notificacion) => (
                <li key={notificacion.id}>
                  <button
                    type="button"
                    onClick={() => manejarClick(notificacion)}
                    className={`flex w-full flex-col gap-xs rounded-sm p-sm text-left hover:bg-bg-alt ${
                      notificacion.isLeida ? '' : 'bg-primary-soft'
                    }`}
                  >
                    <span className="text-body-s font-medium text-text">{notificacion.titulo}</span>
                    <span className="text-caption text-text-secondary">{notificacion.mensaje}</span>
                  </button>
                </li>
              ))}
            </ul>
          </div>
        </>
      )}
    </div>
  );
}
