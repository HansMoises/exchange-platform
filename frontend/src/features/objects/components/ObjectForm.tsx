import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import Select from '../../../components/ui/Select';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { useCategorias, useDepartamentos, useDistritos, useProvincias } from '../../geo/hooks/useGeoSelects';
import { objetoSchema, type ObjetoFormBase } from '../schemas/objeto.schemas';
import { objetoService } from '../services/objetoService';
import type { ObjetoDto } from '../types/objeto.types';
import ImageUploader from './ImageUploader';

interface ObjectFormProps {
  objetoId?: string;
  valoresIniciales?: ObjetoDto;
}

const MIN_IMAGENES = 1;
const MAX_IMAGENES = 5;

// Un solo formulario para publicar/editar. La edicion no incluye imagenes
// porque el backend real (ActualizarObjetoCommand) no las admite todavia.
export default function ObjectForm({ objetoId, valoresIniciales }: ObjectFormProps) {
  const esEdicion = !!objetoId;
  const navigate = useNavigate();
  const { mostrar } = useToast();
  const [enviando, setEnviando] = useState(false);
  const [imagenesUrl, setImagenesUrl] = useState<string[]>(
    valoresIniciales?.imagenes.map((imagen) => imagen.url) ?? [],
  );
  const [errorImagenes, setErrorImagenes] = useState<string>();

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<ObjetoFormBase>({
    resolver: zodResolver(objetoSchema),
    defaultValues: {
      titulo: valoresIniciales?.titulo ?? '',
      descripcion: valoresIniciales?.descripcion ?? '',
      categoriaId: valoresIniciales?.categoriaId ?? 0,
      condicionFisica: valoresIniciales?.condicionFisica ?? undefined,
      departamentoId: valoresIniciales?.departamentoId ?? 0,
      provinciaId: valoresIniciales?.provinciaId ?? 0,
      distritoId: valoresIniciales?.distritoId ?? 0,
    },
  });

  const departamentoId = watch('departamentoId');
  const provinciaId = watch('provinciaId');

  const categorias = useCategorias();
  const departamentos = useDepartamentos();
  const provincias = useProvincias(departamentoId || undefined);
  const distritos = useDistritos(provinciaId || undefined);

  const onSubmit = async (datos: ObjetoFormBase) => {
    if (!esEdicion) {
      if (imagenesUrl.length < MIN_IMAGENES) {
        setErrorImagenes('Debes subir al menos 1 imagen.');
        return;
      }
      if (imagenesUrl.length > MAX_IMAGENES) {
        setErrorImagenes('Maximo 5 imagenes.');
        return;
      }
    }
    setErrorImagenes(undefined);
    setEnviando(true);
    try {
      if (esEdicion) {
        await objetoService.actualizar(objetoId, datos);
        mostrar('Objeto actualizado exitosamente.', 'success');
        navigate(`/objects/${objetoId}`);
      } else {
        const { data } = await objetoService.crear({ ...datos, imagenesUrl });
        mostrar('Objeto publicado exitosamente.', 'success');
        navigate(`/objects/${data.data?.id}`);
      }
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos guardar el objeto.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex w-full max-w-lg flex-col gap-md" noValidate>
      <Input label="Titulo" error={errors.titulo?.message} {...register('titulo')} />

      <div className="flex flex-col gap-xs">
        <label htmlFor="descripcion" className="text-label font-medium text-text">
          Descripcion
        </label>
        <textarea
          id="descripcion"
          rows={4}
          aria-invalid={!!errors.descripcion}
          className={`rounded-md border px-md py-sm text-body text-text focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 ${
            errors.descripcion ? 'border-error' : 'border-text-secondary/40'
          }`}
          {...register('descripcion')}
        />
        {errors.descripcion && (
          <p role="alert" className="text-body-s text-error">
            {errors.descripcion.message}
          </p>
        )}
      </div>

      <Select
        label="Categoria"
        error={errors.categoriaId?.message}
        {...register('categoriaId', { valueAsNumber: true })}
      >
        <option value={0}>Selecciona una categoria</option>
        {categorias.map((categoria) => (
          <option key={categoria.id} value={categoria.id}>
            {categoria.nombre}
          </option>
        ))}
      </Select>

      <Select
        label="Condicion fisica"
        error={errors.condicionFisica?.message}
        {...register('condicionFisica')}
        defaultValue=""
      >
        <option value="" disabled>
          Selecciona la condicion
        </option>
        <option value="Nuevo">Nuevo</option>
        <option value="Bueno">Bueno</option>
        <option value="Regular">Regular</option>
      </Select>

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

      {!esEdicion && <ImageUploader value={imagenesUrl} onChange={setImagenesUrl} error={errorImagenes} />}

      <Button type="submit" variant="primario" disabled={enviando}>
        {enviando ? 'Guardando...' : esEdicion ? 'Guardar cambios' : 'Publicar objeto'}
      </Button>
    </form>
  );
}
