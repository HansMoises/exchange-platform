import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { cambiarPasswordSchema, type CambiarPasswordForm as CambiarPasswordFormData } from '../schemas/perfil.schemas';
import { usuarioService } from '../services/usuarioService';

export default function ChangePasswordForm() {
  const { mostrar } = useToast();
  const [enviando, setEnviando] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CambiarPasswordFormData>({ resolver: zodResolver(cambiarPasswordSchema) });

  const onSubmit = async (datos: CambiarPasswordFormData) => {
    setEnviando(true);
    try {
      await usuarioService.cambiarPassword(datos);
      mostrar('Contrasena actualizada exitosamente.', 'success');
      reset();
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos actualizar tu contrasena.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex w-full max-w-sm flex-col gap-md" noValidate>
      <Input
        label="Contrasena actual"
        type="password"
        autoComplete="current-password"
        error={errors.passwordActual?.message}
        {...register('passwordActual')}
      />
      <Input
        label="Contrasena nueva"
        type="password"
        autoComplete="new-password"
        error={errors.passwordNuevo?.message}
        {...register('passwordNuevo')}
      />
      <Input
        label="Confirmar contrasena nueva"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register('confirmPassword')}
      />
      <Button type="submit" variant="secundario" disabled={enviando}>
        {enviando ? 'Guardando...' : 'Cambiar contrasena'}
      </Button>
    </form>
  );
}
