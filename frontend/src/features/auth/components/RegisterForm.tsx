import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import Select from '../../../components/ui/Select';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { useDepartamentos, useDistritos, useProvincias } from '../../geo/hooks/useGeoSelects';
import { authService } from '../services/authService';
import { registroSchema, type RegistroForm as RegistroFormData } from '../schemas/auth.schemas';

export default function RegisterForm() {
  const navigate = useNavigate();
  const { mostrar } = useToast();
  const [enviando, setEnviando] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<RegistroFormData>({
    resolver: zodResolver(registroSchema),
    defaultValues: { departamentoId: 0, provinciaId: 0, distritoId: 0 },
  });

  const departamentoId = watch('departamentoId');
  const provinciaId = watch('provinciaId');

  const departamentos = useDepartamentos();
  const provincias = useProvincias(departamentoId || undefined);
  const distritos = useDistritos(provinciaId || undefined);

  const onSubmit = async (datos: RegistroFormData) => {
    setEnviando(true);
    try {
      await authService.registrar(datos);
      mostrar('Cuenta creada exitosamente.', 'success');
      navigate('/login');
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos crear tu cuenta.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex w-full max-w-md flex-col gap-md" noValidate>
      <Input label="Nombres" error={errors.nombres?.message} {...register('nombres')} />
      <Input label="Apellidos" error={errors.apellidos?.message} {...register('apellidos')} />
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
        autoComplete="new-password"
        error={errors.password?.message}
        {...register('password')}
      />
      <Input
        label="Confirmar contrasena"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register('confirmPassword')}
      />
      <Input label="Telefono" placeholder="987654321" error={errors.telefono?.message} {...register('telefono')} />

      <Select
        label="Departamento"
        error={errors.departamentoId?.message}
        {...register('departamentoId', {
          valueAsNumber: true,
          onChange: () => {
            setValue('provinciaId', 0);
            setValue('distritoId', 0);
          },
        })}
      >
        <option value={0}>Selecciona un departamento</option>
        {departamentos.map((departamento) => (
          <option key={departamento.id} value={departamento.id}>
            {departamento.nombre}
          </option>
        ))}
      </Select>

      <Select
        label="Provincia"
        error={errors.provinciaId?.message}
        disabled={!departamentoId}
        {...register('provinciaId', {
          valueAsNumber: true,
          onChange: () => setValue('distritoId', 0),
        })}
      >
        <option value={0}>Selecciona una provincia</option>
        {provincias.map((provincia) => (
          <option key={provincia.id} value={provincia.id}>
            {provincia.nombre}
          </option>
        ))}
      </Select>

      <Select
        label="Distrito"
        error={errors.distritoId?.message}
        disabled={!provinciaId}
        {...register('distritoId', { valueAsNumber: true })}
      >
        <option value={0}>Selecciona un distrito</option>
        {distritos.map((distrito) => (
          <option key={distrito.id} value={distrito.id}>
            {distrito.nombre}
          </option>
        ))}
      </Select>

      <Button type="submit" variant="primario" disabled={enviando}>
        {enviando ? 'Creando cuenta...' : 'Crear cuenta'}
      </Button>
      <p className="text-body-s text-text-secondary">
        Ya tienes cuenta?{' '}
        <Link to="/login" className="text-secondary hover:text-secondary-hover">
          Inicia sesion
        </Link>
      </p>
    </form>
  );
}
