import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { authService } from '../services/authService';
import { forgotPasswordSchema, type ForgotPasswordForm as ForgotPasswordFormData } from '../schemas/auth.schemas';

export default function ForgotPasswordForm() {
  const { mostrar } = useToast();
  const [enviando, setEnviando] = useState(false);
  const [enviado, setEnviado] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormData>({ resolver: zodResolver(forgotPasswordSchema) });

  const onSubmit = async (datos: ForgotPasswordFormData) => {
    setEnviando(true);
    try {
      await authService.olvidarPassword(datos);
      setEnviado(true);
    } catch (error) {
      mostrar(obtenerMensajeError(error), 'error');
    } finally {
      setEnviando(false);
    }
  };

  if (enviado) {
    return (
      <p className="max-w-sm text-body text-text">Si el correo esta registrado, recibiras instrucciones en breve.</p>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex w-full max-w-sm flex-col gap-md" noValidate>
      <Input
        label="Correo electronico"
        type="email"
        autoComplete="email"
        error={errors.email?.message}
        {...register('email')}
      />
      <Button type="submit" variant="primario" disabled={enviando}>
        {enviando ? 'Enviando...' : 'Enviar instrucciones'}
      </Button>
      <Link to="/login" className="text-body-s text-secondary hover:text-secondary-hover">
        Volver a iniciar sesion
      </Link>
    </form>
  );
}
