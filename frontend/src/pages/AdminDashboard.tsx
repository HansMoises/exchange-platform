import { useEffect, useState } from 'react';
import Card from '../components/ui/Card';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import { adminService } from '../features/admin/services/adminService';
import type { DashboardStatsDto } from '../features/admin/types/admin.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

const TARJETAS: { clave: keyof DashboardStatsDto; etiqueta: string }[] = [
  { clave: 'totalUsuarios', etiqueta: 'Usuarios totales' },
  { clave: 'usuariosActivos30d', etiqueta: 'Activos (30 dias)' },
  { clave: 'totalObjetos', etiqueta: 'Objetos publicados' },
  { clave: 'intercambiosCompletados', etiqueta: 'Intercambios completados' },
  { clave: 'intercambiosPendientes', etiqueta: 'Intercambios pendientes' },
  { clave: 'reportesPendientes', etiqueta: 'Reportes pendientes' },
];

export default function AdminDashboard() {
  useDocumentTitle('Panel administrativo');
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    adminService
      .obtenerDashboard()
      .then(({ data }) => setStats(data.data))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, []);

  if (isLoading) return <Loading lineas={4} />;
  if (error || !stats) {
    return <EmptyState mensaje="No pudimos cargar el dashboard." accion="Reintentar" onAccion={cargar} />;
  }

  return (
    <div className="flex animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Panel administrativo</h1>
      <div className="grid grid-cols-1 gap-md sm:grid-cols-2 lg:grid-cols-3">
        {TARJETAS.map((tarjeta) => (
          <Card
            key={tarjeta.clave}
            className="flex flex-col gap-xs transition-shadow duration-200 hover:shadow-elevated"
          >
            <span className="text-body-s text-text-secondary">{tarjeta.etiqueta}</span>
            <span className="text-h1 text-primary">{stats[tarjeta.clave]}</span>
          </Card>
        ))}
      </div>
    </div>
  );
}
