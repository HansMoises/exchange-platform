import { Ban, CheckCircle2, Clock, Repeat } from 'lucide-react';
import type { EstadoObjeto } from '../types/objeto.types';

// Badges de estado de objeto (UI.md SS7.3); nunca solo color (UX.md SS7).
const estilos: Record<EstadoObjeto, string> = {
  Disponible: 'bg-primary-soft text-primary',
  Reservado: 'bg-bg-warm text-warning',
  Intercambiado: 'bg-bg-alt text-text-secondary',
  Suspendido: 'bg-error/10 text-error',
  Eliminado: 'bg-bg-alt text-text-secondary',
};

const iconos: Record<EstadoObjeto, typeof CheckCircle2> = {
  Disponible: CheckCircle2,
  Reservado: Clock,
  Intercambiado: Repeat,
  Suspendido: Ban,
  Eliminado: Ban,
};

export default function EstadoObjetoBadge({ estado }: { estado: EstadoObjeto }) {
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
