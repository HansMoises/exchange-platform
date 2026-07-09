import type { ButtonHTMLAttributes } from 'react';

export type ButtonVariant = 'primario' | 'secundario' | 'texto' | 'peligro';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
}

// Variantes y estados segun UI.md SS7.1 (evolucionado a nivel premium: sombra,
// resplandor de marca y micro-interaccion de presionado).
const variantClasses: Record<ButtonVariant, string> = {
  primario: 'bg-primary text-white shadow-soft hover:bg-primary-hover hover:shadow-glow',
  secundario: 'border border-primary/25 bg-bg text-primary hover:border-primary hover:bg-primary-soft',
  texto: 'bg-transparent text-secondary hover:bg-bg-alt hover:text-secondary-hover',
  peligro: 'bg-error text-white shadow-soft hover:opacity-90',
};

export default function Button({ variant = 'primario', className = '', disabled, ...props }: ButtonProps) {
  return (
    <button
      type="button"
      disabled={disabled}
      className={`inline-flex min-h-[44px] items-center justify-center gap-sm rounded-md px-md py-sm text-label font-medium transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-50 disabled:active:scale-100 ${variantClasses[variant]} ${className}`}
      {...props}
    />
  );
}
