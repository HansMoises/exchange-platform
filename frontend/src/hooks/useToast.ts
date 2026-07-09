import { useToastStore } from '../stores/toastStore';

export function useToast() {
  const mostrar = useToastStore((state) => state.mostrar);
  return { mostrar };
}
