import { useEffect } from 'react';
import { Heart } from 'lucide-react';
import { useAuthStore } from '../../../stores/authStore';
import { useFavoritosStore } from '../../../stores/favoritosStore';

interface FavoriteButtonProps {
  objetoId: string;
}

export default function FavoriteButton({ objetoId }: FavoriteButtonProps) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const cargado = useFavoritosStore((state) => state.cargado);
  const esFavorito = useFavoritosStore((state) => state.ids.has(objetoId));
  const cargar = useFavoritosStore((state) => state.cargar);
  const alternar = useFavoritosStore((state) => state.alternar);

  useEffect(() => {
    if (isAuthenticated && !cargado) cargar();
  }, [isAuthenticated, cargado, cargar]);

  if (!isAuthenticated) return null;

  return (
    <button
      type="button"
      onClick={() => alternar(objetoId)}
      aria-label={esFavorito ? 'Quitar de favoritos' : 'Agregar a favoritos'}
      aria-pressed={esFavorito}
      className="flex min-h-11 min-w-11 items-center justify-center rounded-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
    >
      <Heart size={20} className={esFavorito ? 'fill-error text-error' : 'text-text-secondary'} aria-hidden="true" />
    </button>
  );
}
