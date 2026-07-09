import { useEffect, useState } from 'react';
import { Heart, PackagePlus, Repeat, Search, User as UserIcon } from 'lucide-react';
import { Link } from 'react-router-dom';
import Card from '../components/ui/Card';
import Loading from '../components/ui/Loading';
import { objetoService } from '../features/objects/services/objetoService';
import { intercambioService } from '../features/exchanges/services/intercambioService';
import { useDocumentTitle } from '../hooks/useDocumentTitle';
import { useAuthStore } from '../stores/authStore';

const ACCIONES = [
  { to: '/publish', etiqueta: 'Publicar objeto', icono: PackagePlus },
  { to: '/search', etiqueta: 'Buscar objetos', icono: Search },
  { to: '/exchanges', etiqueta: 'Mis intercambios', icono: Repeat },
  { to: '/favorites', etiqueta: 'Mis favoritos', icono: Heart },
  { to: '/profile', etiqueta: 'Mi perfil', icono: UserIcon },
];

export default function Dashboard() {
  useDocumentTitle('Dashboard');
  const usuario = useAuthStore((state) => state.usuario);
  const [totalObjetos, setTotalObjetos] = useState<number | null>(null);
  const [intercambiosPendientes, setIntercambiosPendientes] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      objetoService.obtenerMisObjetos().then(({ data }) => data.data ?? []),
      intercambioService.listar({ pageNumber: 1, pageSize: 50 }).then(({ data }) => data.data ?? []),
    ])
      .then(([objetos, intercambios]) => {
        setTotalObjetos(objetos.length);
        setIntercambiosPendientes(intercambios.filter((i) => i.estado === 'Pendiente').length);
      })
      .catch(() => {
        setTotalObjetos(null);
        setIntercambiosPendientes(null);
      })
      .finally(() => setIsLoading(false));
  }, []);

  return (
    <div className="mx-auto flex max-w-4xl animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Hola, {usuario?.nombres}</h1>

      {isLoading ? (
        <Loading lineas={2} />
      ) : (
        <div className="grid grid-cols-1 gap-md sm:grid-cols-2">
          <Card className="flex flex-col gap-xs">
            <span className="text-body-s text-text-secondary">Objetos publicados</span>
            <span className="text-h1 text-primary">{totalObjetos ?? '-'}</span>
          </Card>
          <Card className="flex flex-col gap-xs">
            <span className="text-body-s text-text-secondary">Intercambios pendientes</span>
            <span className="text-h1 text-primary">{intercambiosPendientes ?? '-'}</span>
          </Card>
        </div>
      )}

      <div className="grid grid-cols-2 gap-md sm:grid-cols-3 lg:grid-cols-5">
        {ACCIONES.map((accion) => (
          <Link
            key={accion.to}
            to={accion.to}
            className="group flex flex-col items-center gap-sm rounded-lg bg-bg p-md text-center shadow-card transition-all duration-200 hover:-translate-y-0.5 hover:shadow-elevated"
          >
            <span className="flex h-11 w-11 items-center justify-center rounded-full bg-primary-soft transition-colors duration-200 group-hover:bg-primary group-hover:text-white">
              <accion.icono size={20} className="text-primary group-hover:text-white" aria-hidden="true" />
            </span>
            <span className="text-body-s text-text">{accion.etiqueta}</span>
          </Link>
        ))}
      </div>
    </div>
  );
}
