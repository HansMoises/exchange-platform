import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import Select from '../../../components/ui/Select';
import { useDepartamentos, useDistritos, useProvincias } from '../../geo/hooks/useGeoSelects';
import { useToast } from '../../../hooks/useToast';
import { useAuthStore } from '../../../stores/authStore';
import { obtenerMensajeError } from '../../../utils/apiError';
import { perfilSchema, type PerfilForm as PerfilFormData } from '../schemas/perfil.schemas';
import { usuarioService } from '../services/usuarioService';
import type { PerfilUsuarioDto } from '../types/perfil.types';

interface ProfileFormProps {
  perfil: PerfilUsuarioDto;
}

export default function ProfileForm({ perfil }: ProfileFormProps) {
  const { mostrar } = useToast();
  const actualizarUsuario = useAuthStore((state) => state.actualizarUsuario);
  const [enviando, setEnviando] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<PerfilFormData>({
    resolver: zodResolver(perfilSchema),
    defaultValues: {
      nombres: perfil.nombres,
      apellidos: perfil.apellidos,
      telefono: perfil.telefono,
      departamentoId: perfil.departamentoId,
      provinciaId: perfil.provinciaId,
      distritoId: perfil.distritoId,
    },
  });

  const departamentoId = watch('departamentoId');
  const provinciaId = watch('provinciaId');

  const departamentos = useDepartamentos();
  const provincias = useProvincias(departamentoId || undefined);
  const distritos = useDistritos(provinciaId || undefined);

  const onSubmit = async (datos: PerfilFormData) => {
    setEnviando(true);
    try {
      await usuarioService.actualizarPerfil(datos);
      actualizarUsuario({ nombres: datos.nombres, apellidos: datos.apellidos });
      mostrar('Perfil actualizado exitosamente.', 'success');
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos actualizar tu perfil.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex w-full max-w-lg flex-col gap-md" noValidate>
      <Input label="Nombres" error={errors.nombres?.message} {...register('nombres')} />
      <Input label="Apellidos" error={errors.apellidos?.message} {...register('apellidos')} />
      <Input label="Telefono" error={errors.telefono?.message} {...register('telefono')} />

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
        {enviando ? 'Guardando...' : 'Guardar cambios'}
      </Button>
    </form>
  );
}
