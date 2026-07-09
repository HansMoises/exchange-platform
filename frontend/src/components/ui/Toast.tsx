import { AlertTriangle, CheckCircle2, Info, X, XCircle } from 'lucide-react';
import { useToastStore, type ToastVariant } from '../../stores/toastStore';

// Confirmacion breve tras una accion exitosa (UX.md SS6 "Exito").
const estilos: Record<ToastVariant, string> = {
  success: 'bg-success text-white',
  error: 'bg-error text-white',
  warning: 'bg-warning text-white',
  info: 'bg-info text-white',
};

const iconos: Record<ToastVariant, typeof CheckCircle2> = {
  success: CheckCircle2,
  error: XCircle,
  warning: AlertTriangle,
  info: Info,
};

export default function ToastContainer() {
  const toasts = useToastStore((state) => state.toasts);
  const cerrar = useToastStore((state) => state.cerrar);

  if (toasts.length === 0) return null;

  return (
    <div
      className="fixed bottom-md right-md z-50 flex flex-col gap-sm"
      role="region"
      aria-live="polite"
      aria-label="Notificaciones"
    >
      {toasts.map((toast) => {
        const Icono = iconos[toast.variant];
        return (
          <div
            key={toast.id}
            className={`flex animate-slide-up items-center gap-sm rounded-md px-md py-sm shadow-elevated ${estilos[toast.variant]}`}
          >
            <Icono size={20} aria-hidden="true" />
            <span className="text-body-s">{toast.mensaje}</span>
            <button
              type="button"
              onClick={() => cerrar(toast.id)}
              aria-label="Cerrar notificacion"
              className="ml-sm flex min-h-11 min-w-11 items-center justify-center rounded-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white"
            >
              <X size={16} aria-hidden="true" />
            </button>
          </div>
        );
      })}
    </div>
  );
}
