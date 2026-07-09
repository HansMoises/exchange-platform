import { forwardRef, useId, type InputHTMLAttributes } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  helperText?: string;
}

// Label asociado + error vinculado por aria-describedby (UX.md SS7).
const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helperText, id, className = '', ...props }, ref) => {
    const autoId = useId();
    const inputId = id ?? autoId;
    const errorId = `${inputId}-error`;
    const helperId = `${inputId}-helper`;

    return (
      <div className="flex flex-col gap-xs">
        <label htmlFor={inputId} className="text-label font-medium text-text">
          {label}
        </label>
        <input
          ref={ref}
          id={inputId}
          aria-invalid={!!error}
          aria-describedby={error ? errorId : helperText ? helperId : undefined}
          className={`min-h-[44px] rounded-md border bg-bg px-md py-sm text-body text-text shadow-soft transition-all duration-200 placeholder:text-text-secondary focus-visible:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${
            error ? 'border-error' : 'border-text-secondary/25 hover:border-text-secondary/40'
          } ${className}`}
          {...props}
        />
        {error && (
          <p id={errorId} role="alert" className="text-body-s text-error">
            {error}
          </p>
        )}
        {!error && helperText && (
          <p id={helperId} className="text-body-s text-text-secondary">
            {helperText}
          </p>
        )}
      </div>
    );
  },
);

Input.displayName = 'Input';

export default Input;
