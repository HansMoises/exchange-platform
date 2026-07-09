import { useState } from 'react';
import { ImagePlus, X } from 'lucide-react';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { imageService } from '../services/imageService';

interface ImageUploaderProps {
  value: string[];
  onChange: (urls: string[]) => void;
  error?: string;
}

const MAX_IMAGENES = 5;

// Sube 1-5 imagenes con vista previa inmediata (UX.md SS5.1, RN-011/012).
export default function ImageUploader({ value, onChange, error }: ImageUploaderProps) {
  const [subiendo, setSubiendo] = useState(false);
  const { mostrar } = useToast();

  const manejarArchivos = async (archivos: FileList | null) => {
    if (!archivos || archivos.length === 0) return;
    const espacioDisponible = MAX_IMAGENES - value.length;
    if (espacioDisponible <= 0) {
      mostrar('Ya alcanzaste el maximo de 5 imagenes.', 'warning');
      return;
    }

    const seleccionados = Array.from(archivos).slice(0, espacioDisponible);
    setSubiendo(true);
    try {
      const urls = await Promise.all(
        seleccionados.map(async (archivo) => {
          const { data } = await imageService.subir(archivo);
          return data.data?.url;
        }),
      );
      onChange([...value, ...urls.filter((url): url is string => !!url)]);
    } catch (error_) {
      mostrar(obtenerMensajeError(error_, 'No pudimos subir la imagen.'), 'error');
    } finally {
      setSubiendo(false);
    }
  };

  const quitar = (url: string) => onChange(value.filter((u) => u !== url));

  return (
    <div className="flex flex-col gap-sm">
      <span className="text-label font-medium text-text">Imagenes (1 a 5)</span>
      <div className="grid grid-cols-3 gap-sm sm:grid-cols-5">
        {value.map((url) => (
          <div key={url} className="relative aspect-square overflow-hidden rounded-md border border-text-secondary/20">
            <img src={url} alt="" className="h-full w-full object-cover" />
            <button
              type="button"
              onClick={() => quitar(url)}
              aria-label="Quitar imagen"
              className="absolute right-1 top-1 flex min-h-6 min-w-6 items-center justify-center rounded-sm bg-error text-white"
            >
              <X size={14} aria-hidden="true" />
            </button>
          </div>
        ))}
        {value.length < MAX_IMAGENES && (
          <label
            className={`flex aspect-square cursor-pointer flex-col items-center justify-center gap-xs rounded-md border border-dashed border-text-secondary/40 text-text-secondary hover:border-primary hover:text-primary ${
              subiendo ? 'pointer-events-none opacity-50' : ''
            }`}
          >
            <ImagePlus size={24} aria-hidden="true" />
            <span className="text-caption">{subiendo ? 'Subiendo...' : 'Agregar'}</span>
            <input
              type="file"
              accept="image/*"
              multiple
              className="hidden"
              disabled={subiendo}
              onChange={(evento) => {
                manejarArchivos(evento.target.files);
                evento.target.value = '';
              }}
            />
          </label>
        )}
      </div>
      {error && (
        <p role="alert" className="text-body-s text-error">
          {error}
        </p>
      )}
    </div>
  );
}
