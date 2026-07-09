import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import Button from '../../../components/ui/Button';
import { useToast } from '../../../hooks/useToast';
import { obtenerMensajeError } from '../../../utils/apiError';
import { reporteSchema, type ReporteForm as ReporteFormData } from '../schemas/reporte.schemas';
import { reporteService } from '../services/reporteService';
import type { EntidadTipoReporte } from '../types/reporte.types';

interface ReportFormProps {
  entidadTipo: EntidadTipoReporte;
  entidadId: string;
  onExito?: () => void;
}

const MOTIVOS: { valor: ReporteFormData['motivo']; etiqueta: string }[] = [
  { valor: 'ContenidoInapropiado', etiqueta: 'Contenido inapropiado' },
  { valor: 'Fraude', etiqueta: 'Fraude' },
  { valor: 'Spam', etiqueta: 'Spam' },
  { valor: 'InformacionFalsa', etiqueta: 'Informacion falsa' },
  { valor: 'Otro', etiqueta: 'Otro' },
];

// Reportar objeto/usuario (UC-040, RN-040/042).
export default function ReportForm({ entidadTipo, entidadId, onExito }: ReportFormProps) {
  const { mostrar } = useToast();
  const [enviando, setEnviando] = useState(false);
  const [enviado, setEnviado] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReporteFormData>({ resolver: zodResolver(reporteSchema) });

  const onSubmit = async (datos: ReporteFormData) => {
    setEnviando(true);
    try {
      await reporteService.crear({ entidadTipo, entidadId, ...datos });
      mostrar('Reporte enviado exitosamente.', 'success');
      setEnviado(true);
      onExito?.();
    } catch (error) {
      mostrar(obtenerMensajeError(error, 'No pudimos enviar tu reporte.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  if (enviado) {
    return <p className="text-body-s text-text-secondary">Gracias, revisaremos tu reporte.</p>;
  }

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="flex flex-col gap-sm rounded-lg bg-bg p-md shadow-card"
      noValidate
    >
      <div className="flex flex-col gap-xs">
        <label htmlFor="motivo-reporte" className="text-label font-medium text-text">
          Motivo
        </label>
        <select
          id="motivo-reporte"
          aria-invalid={!!errors.motivo}
          className={`min-h-[44px] rounded-md border bg-bg px-md py-sm text-body text-text focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 ${
            errors.motivo ? 'border-error' : 'border-text-secondary/40'
          }`}
          defaultValue=""
          {...register('motivo')}
        >
          <option value="" disabled>
            Selecciona un motivo
          </option>
          {MOTIVOS.map((motivo) => (
            <option key={motivo.valor} value={motivo.valor}>
              {motivo.etiqueta}
            </option>
          ))}
        </select>
        {errors.motivo && (
          <p role="alert" className="text-body-s text-error">
            {errors.motivo.message}
          </p>
        )}
      </div>

      <div className="flex flex-col gap-xs">
        <label htmlFor="descripcion-reporte" className="text-label font-medium text-text">
          Descripcion (opcional)
        </label>
        <textarea
          id="descripcion-reporte"
          rows={3}
          maxLength={500}
          className="rounded-md border border-text-secondary/40 px-md py-sm text-body text-text focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
          {...register('descripcion')}
        />
        {errors.descripcion && (
          <p role="alert" className="text-body-s text-error">
            {errors.descripcion.message}
          </p>
        )}
      </div>

      <Button type="submit" variant="peligro" disabled={enviando}>
        {enviando ? 'Enviando...' : 'Enviar reporte'}
      </Button>
    </form>
  );
}
