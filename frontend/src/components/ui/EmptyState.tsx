import type { ReactNode } from 'react';
import { Inbox } from 'lucide-react';
import Button from './Button';

interface EmptyStateProps {
  mensaje: string;
  accion?: string;
  onAccion?: () => void;
  icono?: ReactNode;
}

// Mensaje amable + accion sugerida, nunca solo un vacio silencioso (UX.md SS6, SS8).
export default function EmptyState({ mensaje, accion, onAccion, icono }: EmptyStateProps) {
  return (
    <div className="flex animate-fade-in flex-col items-center gap-md p-xl text-center">
      <div
        className="flex h-16 w-16 items-center justify-center rounded-full bg-bg-alt text-text-secondary"
        aria-hidden="true"
      >
        {icono ?? <Inbox size={32} strokeWidth={1.5} />}
      </div>
      <p className="text-body text-text-secondary">{mensaje}</p>
      {accion && onAccion && (
        <Button variant="primario" onClick={onAccion}>
          {accion}
        </Button>
      )}
    </div>
  );
}
