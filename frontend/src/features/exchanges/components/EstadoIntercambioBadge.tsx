import { Ban, CheckCheck, CheckCircle2, Clock, Hourglass, XCircle } from 'lucide-react';
import type { EstadoIntercambio } from '../types/intercambio.types';

// Badges de estado de intercambio (UI.md SS7.3); nunca solo color (UX.md SS7).
const estilos: Record<EstadoIntercambio, string> = {
  Pendiente: 'bg-info/10 text-info',
  Aceptado: 'bg-primary-soft text-primary',
  PendienteConfirmacion: 'bg-bg-warm text-warning',
  Completado: 'bg-success/10 text-success',
  Rechazado: 'bg-error/10 text-error',
  Cancelado: 'bg-bg-alt text-text-secondary',
};

const iconos: Record<EstadoIntercambio, typeof CheckCircle2> = {
  Pendiente: Clock,
  Aceptado: CheckCircle2,
  PendienteConfirmacion: Hourglass,
  Completado: CheckCheck,
  Rechazado: XCircle,
  Cancelado: Ban,
};

const etiquetas: Record<EstadoIntercambio, string> = {
  Pendiente: 'Pendiente',
  Aceptado: 'Aceptado',
  PendienteConfirmacion: 'Por confirmar',
  Completado: 'Completado',
  Rechazado: 'Rechazado',
  Cancelado: 'Cancelado',
};

export default function EstadoIntercambioBadge({ estado }: { estado: EstadoIntercambio }) {
  const Icono = iconos[estado];
  return (
    <span
      className={`inline-flex items-center gap-xs rounded-full px-sm py-xs text-caption font-medium ${estilos[estado]}`}
    >
      <Icono size={14} aria-hidden="true" />
      {etiquetas[estado]}
    </span>
  );
}
