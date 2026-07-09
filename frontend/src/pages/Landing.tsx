import { useEffect, useState } from 'react';
import { ArrowRight, Handshake, PackagePlus, Repeat, Search as SearchIcon, Star } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import Button from '../components/ui/Button';
import Input from '../components/ui/Input';
import { geoService } from '../features/geo/services/geoService';
import type { CategoriaDto } from '../features/geo/types/geo.types';
import { statsService } from '../features/landing/services/statsService';
import type { EstadisticasPublicasDto } from '../features/landing/types/stats.types';
import ObjectCard from '../features/objects/components/ObjectCard';
import { objetoService } from '../features/objects/services/objetoService';
import type { ObjetoDto } from '../features/objects/types/objeto.types';
import { useAuthStore } from '../stores/authStore';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

const PASOS = [
  { icono: PackagePlus, titulo: 'Publica', descripcion: 'Sube fotos y detalles de un objeto que ya no uses.' },
  { icono: SearchIcon, titulo: 'Explora', descripcion: 'Busca por categoria y ubicacion cerca de ti.' },
  { icono: Repeat, titulo: 'Intercambia', descripcion: 'Propon un intercambio y coordina con la otra persona.' },
  { icono: Star, titulo: 'Califica', descripcion: 'Comparte tu experiencia y fortalece la confianza en la comunidad.' },
];

export default function Landing() {
  useDocumentTitle('Inicio');
  const navigate = useNavigate();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const [busqueda, setBusqueda] = useState('');
  const [categorias, setCategorias] = useState<CategoriaDto[]>([]);
  const [objetos, setObjetos] = useState<ObjetoDto[]>([]);
  const [stats, setStats] = useState<EstadisticasPublicasDto | null>(null);

  useEffect(() => {
    geoService
      .obtenerCategorias()
      .then(({ data }) => setCategorias((data.data ?? []).slice(0, 8)))
      .catch(() => setCategorias([]));
    objetoService
      .listar({ pageNumber: 1, pageSize: 8 })
      .then(({ data }) => setObjetos(data.data?.items ?? []))
      .catch(() => setObjetos([]));
    statsService
      .obtenerPublicas()
      .then(({ data }) => setStats(data.data))
      .catch(() => setStats(null));
  }, []);

  const buscar = (evento: React.FormEvent) => {
    evento.preventDefault();
    navigate(busqueda ? `/search?search=${encodeURIComponent(busqueda)}` : '/search');
  };

  return (
    <div className="flex flex-col">
      <section className="relative overflow-hidden bg-gradient-to-b from-primary-100 via-bg to-bg px-lg py-3xl text-center">
        <div className="mx-auto flex max-w-3xl animate-slide-up flex-col items-center gap-lg">
          <span className="inline-flex items-center gap-xs rounded-full bg-primary-soft px-md py-xs text-label font-medium text-primary">
            <Handshake size={16} aria-hidden="true" />
            Economia circular en Ayacucho
          </span>
          <h1 className="font-display text-hero text-text">
            Dale una <span className="text-primary">segunda vida</span> a lo que ya no usas
          </h1>
          <p className="max-w-xl text-body text-text-secondary">
            Intercambia objetos con tu comunidad: sin dinero de por medio, sin desperdicio y con confianza.
          </p>

          <form onSubmit={buscar} className="flex w-full max-w-lg flex-col gap-sm sm:flex-row sm:items-end">
            <div className="flex-1">
              <Input
                label="Que estas buscando?"
                placeholder="Bicicleta, libros, ropa..."
                value={busqueda}
                onChange={(evento) => setBusqueda(evento.target.value)}
              />
            </div>
            <Button type="submit" variant="primario" className="sm:min-h-[44px]">
              <SearchIcon size={18} aria-hidden="true" />
              Buscar
            </Button>
          </form>

          {!isAuthenticated && (
            <div className="flex flex-wrap items-center justify-center gap-sm">
              <Link to="/register">
                <Button variant="primario">
                  Crear cuenta gratis
                  <ArrowRight size={18} aria-hidden="true" />
                </Button>
              </Link>
              <Link to="/search">
                <Button variant="secundario">Explorar objetos</Button>
              </Link>
            </div>
          )}
        </div>
      </section>

      {stats && (
        <section className="border-y border-text-secondary/10 bg-bg-alt px-lg py-xl">
          <div className="mx-auto grid max-w-4xl grid-cols-1 gap-lg text-center sm:grid-cols-3">
            <div>
              <p className="font-display text-h1 font-bold text-primary">{stats.totalUsuarios}+</p>
              <p className="text-body-s text-text-secondary">Miembros de la comunidad</p>
            </div>
            <div>
              <p className="font-display text-h1 font-bold text-primary">{stats.totalObjetos}+</p>
              <p className="text-body-s text-text-secondary">Objetos disponibles hoy</p>
            </div>
            <div>
              <p className="font-display text-h1 font-bold text-primary">{stats.intercambiosCompletados}+</p>
              <p className="text-body-s text-text-secondary">Intercambios completados</p>
            </div>
          </div>
        </section>
      )}

      {categorias.length > 0 && (
        <section className="mx-auto flex w-full max-w-7xl flex-col gap-lg px-lg py-2xl">
          <h2 className="font-display text-h2 font-bold text-text">Explora por categoria</h2>
          <div className="grid grid-cols-2 gap-md sm:grid-cols-4">
            {categorias.map((categoria) => (
              <Link
                key={categoria.id}
                to={`/search?categoriaId=${categoria.id}`}
                className="group flex flex-col items-center gap-sm rounded-lg bg-bg p-lg text-center shadow-card transition-all duration-200 hover:-translate-y-0.5 hover:shadow-elevated"
              >
                <span className="text-3xl transition-transform duration-200 group-hover:scale-110" aria-hidden="true">
                  {categoria.icono}
                </span>
                <span className="text-body-s font-medium text-text">{categoria.nombre}</span>
              </Link>
            ))}
          </div>
        </section>
      )}

      {objetos.length > 0 && (
        <section className="mx-auto flex w-full max-w-7xl flex-col gap-lg px-lg py-2xl">
          <div className="flex items-center justify-between">
            <h2 className="font-display text-h2 font-bold text-text">Publicados recientemente</h2>
            <Link to="/search" className="text-body-s font-medium text-primary hover:text-primary-hover">
              Ver todos
            </Link>
          </div>
          <div className="grid grid-cols-1 gap-md sm:grid-cols-2 lg:grid-cols-4">
            {objetos.map((objeto) => (
              <ObjectCard key={objeto.id} objeto={objeto} />
            ))}
          </div>
        </section>
      )}

      <section className="bg-bg-alt px-lg py-2xl">
        <div className="mx-auto flex max-w-5xl flex-col gap-xl">
          <h2 className="text-center font-display text-h2 font-bold text-text">Como funciona</h2>
          <div className="grid grid-cols-1 gap-lg sm:grid-cols-2 lg:grid-cols-4">
            {PASOS.map((paso, indice) => (
              <div key={paso.titulo} className="flex flex-col items-center gap-sm text-center">
                <span className="flex h-14 w-14 items-center justify-center rounded-full bg-primary text-white shadow-soft">
                  <paso.icono size={24} aria-hidden="true" />
                </span>
                <h3 className="text-h3 text-text">
                  {indice + 1}. {paso.titulo}
                </h3>
                <p className="text-body-s text-text-secondary">{paso.descripcion}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {!isAuthenticated && (
        <section className="px-lg py-2xl text-center">
          <div className="mx-auto flex max-w-2xl flex-col items-center gap-md">
            <h2 className="font-display text-h2 font-bold text-text">Listo para intercambiar?</h2>
            <p className="text-body text-text-secondary">
              Unete a la comunidad y encuentra lo que buscas sin gastar de mas.
            </p>
            <Link to="/register">
              <Button variant="primario">
                Crear cuenta gratis
                <ArrowRight size={18} aria-hidden="true" />
              </Button>
            </Link>
          </div>
        </section>
      )}
    </div>
  );
}
