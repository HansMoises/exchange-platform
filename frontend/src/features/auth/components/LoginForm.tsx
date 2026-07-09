import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { useToast } from '../../../hooks/useToast';
import { useAuthStore } from '../../../stores/authStore';
import { obtenerMensajeError } from '../../../utils/apiError';
import { authService } from '../services/authService';
import { loginSchema, type LoginForm as LoginFormData } from '../schemas/auth.schemas';

export default function LoginForm() {
  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);
  const { mostrar } = useToast();
  const [enviando, setEnviando] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({ resolver: zodResolver(loginSchema) });

  const onSubmit = async (datos: LoginFormData) => {
    setEnviando(true);
    try {
      const { data } = await authService.iniciarSesion(datos);
      if (data.data) {
        login(data.data);
        mostrar('Inicio de sesion exitoso.', 'success');
        navigate('/dashboard');
      }
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'Credenciales invalidas.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex w-full max-w-sm flex-col gap-md" noValidate>
      <Input
        label="Correo electronico"
        type="email"
        autoComplete="email"
        error={errors.email?.message}
        {...register('email')}
      />
      <Input
        label="Contrasena"
        type="password"
        autoComplete="current-password"
        error={errors.password?.message}
        {...register('password')}
      />
      <Link to="/forgot-password" className="text-body-s text-secondary hover:text-secondary-hover">
        Olvide mi contrasena
      </Link>
      <Button type="submit" variant="primario" disabled={enviando}>
        {enviando ? 'Ingresando...' : 'Iniciar sesion'}
      </Button>
      <p className="text-body-s text-text-secondary">
        No tienes cuenta?{' '}
        <Link to="/register" className="text-secondary hover:text-secondary-hover">
          Registrate
        </Link>
      </p>
    </form>
  );
}
