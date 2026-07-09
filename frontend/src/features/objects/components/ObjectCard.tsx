import { Star } from 'lucide-react';
import { Link } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import FavoriteButton from '../../favorites/components/FavoriteButton';
import type { ObjetoDto } from '../types/objeto.types';
import EstadoObjetoBadge from './EstadoObjetoBadge';

interface ObjectCardProps {
  objeto: ObjetoDto;
}

// Tarjeta de objeto (UI.md SS7.2).
export default function ObjectCard({ objeto }: ObjectCardProps) {
  const portada = objeto.imagenes[0]?.url;

  return (
    <article className="group flex flex-col overflow-hidden rounded-lg bg-bg shadow-card transition-all duration-300 hover:-translate-y-1 hover:shadow-elevated">
      <div className="relative aspect-video w-full overflow-hidden bg-bg-alt">
        {portada ? (
          <img
            src={portada}
            alt={objeto.titulo}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-body-s text-text-secondary">
            Sin imagen
          </div>
        )}
        <div className="absolute right-2 top-2 rounded-full bg-bg/80 shadow-soft backdrop-blur-sm">
          <FavoriteButton objetoId={objeto.id} />
        </div>
      </div>
      <div className="flex flex-col gap-xs p-md">
        <h3 className="text-h3 text-text transition-colors duration-200 group-hover:text-primary">{objeto.titulo}</h3>
        <p className="text-caption text-text-secondary">{objeto.categoriaNombre}</p>
        <div className="flex items-center gap-sm">
          <span className="flex items-center gap-xs text-body-s text-text">
            <Star size={16} className="text-warning" aria-hidden="true" />
            {objeto.usuarioCalificacion.toFixed(1)}
          </span>
          <EstadoObjetoBadge estado={objeto.estado} />
        </div>
        <Link to={`/objects/${objeto.id}`}>
          <Button variant="secundario" className="w-full">
            Ver detalle
          </Button>
        </Link>
      </div>
    </article>
  );
}
