import { create } from 'zustand';

export type ToastVariant = 'success' | 'error' | 'warning' | 'info';

export interface ToastItem {
  id: string;
  mensaje: string;
  variant: ToastVariant;
}

interface ToastState {
  toasts: ToastItem[];
  mostrar: (mensaje: string, variant?: ToastVariant) => void;
  cerrar: (id: string) => void;
}

const DURACION_MS = 4000;

export const useToastStore = create<ToastState>((set) => ({
  toasts: [],

  mostrar: (mensaje, variant = 'info') => {
    const id = crypto.randomUUID();
    set((state) => ({ toasts: [...state.toasts, { id, mensaje, variant }] }));
    setTimeout(() => {
      set((state) => ({ toasts: state.toasts.filter((toast) => toast.id !== id) }));
    }, DURACION_MS);
  },

  cerrar: (id) => set((state) => ({ toasts: state.toasts.filter((toast) => toast.id !== id) })),
}));
