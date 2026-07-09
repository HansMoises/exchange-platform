import { useEffect, useState } from 'react';
import { Star } from 'lucide-react';
import Card from '../components/ui/Card';
import EmptyState from '../components/ui/EmptyState';
import Loading from '../components/ui/Loading';
import ChangePasswordForm from '../features/users/components/ChangePasswordForm';
import ProfileForm from '../features/users/components/ProfileForm';
import ProfilePhotoUploader from '../features/users/components/ProfilePhotoUploader';
import { usuarioService } from '../features/users/services/usuarioService';
import type { PerfilUsuarioDto } from '../features/users/types/perfil.types';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function Profile() {
  useDocumentTitle('Mi perfil');
  const [perfil, setPerfil] = useState<PerfilUsuarioDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  const cargar = () => {
    setIsLoading(true);
    setError(false);
    usuarioService
      .obtenerMiPerfil()
      .then(({ data }) => setPerfil(data.data))
      .catch(() => setError(true))
      .finally(() => setIsLoading(false));
  };

  useEffect(cargar, []);

  if (isLoading) return <Loading lineas={6} />;
  if (error || !perfil) {
    return <EmptyState mensaje="No pudimos cargar tu perfil." accion="Reintentar" onAccion={cargar} />;
  }

  return (
    <div className="mx-auto flex max-w-lg animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Mi perfil</h1>

      <Card className="flex flex-col gap-md">
        <ProfilePhotoUploader />
        <div className="flex items-center gap-md text-body-s text-text-secondary">
          <span className="flex items-center gap-xs">
            <Star size={16} className="text-warning" aria-hidden="true" />
            {perfil.calificacionPromedio.toFixed(1)}
          </span>
          <span>{perfil.totalIntercambios} intercambios</span>
          <span>Miembro desde {new Date(perfil.miembroDesde).toLocaleDateString('es-PE')}</span>
        </div>
      </Card>

      <Card>
        <h2 className="mb-md text-h3 text-text">Datos personales</h2>
        <ProfileForm perfil={perfil} />
      </Card>

      <Card>
        <h2 className="mb-md text-h3 text-text">Seguridad</h2>
        <ChangePasswordForm />
      </Card>
    </div>
  );
}
