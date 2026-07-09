import { useState } from 'react';
import { User as UserIcon } from 'lucide-react';
import { useToast } from '../../../hooks/useToast';
import { useAuthStore } from '../../../stores/authStore';
import { obtenerMensajeError } from '../../../utils/apiError';
import { usuarioService } from '../services/usuarioService';

export default function ProfilePhotoUploader() {
  const usuario = useAuthStore((state) => state.usuario);
  const actualizarUsuario = useAuthStore((state) => state.actualizarUsuario);
  const { mostrar } = useToast();
  const [subiendo, setSubiendo] = useState(false);

  const manejarArchivo = async (archivo: File | undefined) => {
    if (!archivo) return;
    setSubiendo(true);
    try {
      const { data } = await usuarioService.actualizarFoto(archivo);
      if (data.data?.url) {
        actualizarUsuario({ fotoPerfil: data.data.url });
        mostrar('Foto de perfil actualizada exitosamente.', 'success');
      }
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos subir la imagen.'), 'error');
    } finally {
      setSubiendo(false);
    }
  };

  return (
    <div className="flex items-center gap-md">
      <div className="flex h-20 w-20 items-center justify-center overflow-hidden rounded-full bg-bg-alt">
        {usuario?.fotoPerfil ? (
          <img src={usuario.fotoPerfil} alt="Tu foto de perfil" className="h-full w-full object-cover" />
        ) : (
          <UserIcon size={32} className="text-text-secondary" aria-hidden="true" />
        )}
      </div>
      <label className="text-label font-medium text-secondary hover:text-secondary-hover">
        {subiendo ? 'Subiendo...' : 'Cambiar foto'}
        <input
          type="file"
          accept="image/*"
          className="hidden"
          disabled={subiendo}
          onChange={(evento) => {
            manejarArchivo(evento.target.files?.[0]);
            evento.target.value = '';
          }}
        />
      </label>
    </div>
  );
}
