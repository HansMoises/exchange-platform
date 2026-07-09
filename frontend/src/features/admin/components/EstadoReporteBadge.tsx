import { Ban, CheckCheck, Clock, Eye } from 'lucide-react';
import { ESTADOS_REPORTE_POR_INDICE } from '../types/admin.types';

type EstadoReporte = (typeof ESTADOS_REPORTE_POR_INDICE)[number];

const estilos: Record<EstadoReporte, string> = {
  Pendiente: 'bg-info/10 text-info',
  EnRevision: 'bg-bg-warm text-warning',
  Resuelto: 'bg-success/10 text-success',
  Descartado: 'bg-bg-alt text-text-secondary',
};

const iconos: Record<EstadoReporte, typeof Clock> = {
  Pendiente: Clock,
  EnRevision: Eye,
  Resuelto: CheckCheck,
  Descartado: Ban,
};

export default function EstadoReporteBadge({ estadoIndice }: { estadoIndice: number }) {
  const estado = ESTADOS_REPORTE_POR_INDICE[estadoIndice];
  const Icono = iconos[estado];
  return (
    <span
      className={`inline-flex items-center gap-xs rounded-full px-sm py-xs text-caption font-medium ${estilos[estado]}`}
    >
      <Icono size={14} aria-hidden="true" />
      {estado}
    </span>
  );
}
