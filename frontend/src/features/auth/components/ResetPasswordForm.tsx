import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useSearchParams } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { authService } from '../services/authService';
import { resetPasswordSchema, type ResetPasswordForm as ResetPasswordFormData } from '../schemas/auth.schemas';

export default function ResetPasswordForm() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { mostrar } = useToast();
  const [enviando, setEnviando] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordFormData>({ resolver: zodResolver(resetPasswordSchema) });

  const onSubmit = async (datos: ResetPasswordFormData) => {
    setEnviando(true);
    try {
      await authService.restablecerPassword({ token, ...datos });
      mostrar('Contrasena actualizada exitosamente.', 'success');
      navigate('/login');
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'El enlace ha expirado. Solicita uno nuevo.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  if (!token) {
    return <p className="max-w-sm text-body text-error">El enlace de recuperacion no es valido.</p>;
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex w-full max-w-sm flex-col gap-md" noValidate>
      <Input
        label="Nueva contrasena"
        type="password"
        autoComplete="new-password"
        error={errors.password?.message}
        {...register('password')}
      />
      <Input
        label="Confirmar nueva contrasena"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register('confirmPassword')}
      />
      <Button type="submit" variant="primario" disabled={enviando}>
        {enviando ? 'Guardando...' : 'Restablecer contrasena'}
      </Button>
    </form>
  );
}
