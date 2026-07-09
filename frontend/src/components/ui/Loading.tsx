interface LoadingProps {
  lineas?: number;
  className?: string;
}

// Skeleton: nunca pantalla en blanco mientras carga (UX.md SS6).
export default function Loading({ lineas = 3, className = '' }: LoadingProps) {
  return (
    <div className={`flex flex-col gap-sm ${className}`} role="status" aria-label="Cargando">
      {Array.from({ length: lineas }).map((_, indice) => (
        <div
          key={indice}
          className="h-4 animate-pulse rounded-sm bg-bg-alt"
          style={{ width: indice === lineas - 1 ? '70%' : '100%' }}
        />
      ))}
      <span className="sr-only">Cargando...</span>
    </div>
  );
}
